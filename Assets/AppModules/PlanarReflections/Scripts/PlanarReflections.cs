using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PlanarReflections : MonoBehaviour {

  [SerializeField]
  private Camera _targetCamera;

  [SerializeField]
  private Camera _reflectionCamera;

  [SerializeField]
  private Transform _reflectionAxis;

  [SerializeField]
  private int _resolution = 256;

  [SerializeField]
  private MsaaMode _msaa = MsaaMode.Mode_8;

  [SerializeField]
  private bool _useMips = false;

  [SerializeField]
  private float _offset = 0;

  [SerializeField]
  private float _distortDistance = 0.1f;
  public float distortDistance {
    get { return _distortDistance; }
    set { _distortDistance = value; }
  }

  [Header("Blur")]
  [SerializeField]
  private Shader _blurShader;

  [SerializeField]
  private int _itterations = 0;

  [SerializeField]
  private int _downsampleScale = 1;

  [Header("Noise")]
  [SerializeField]
  private int _noiseResolution = 128;

  private Camera.StereoscopicEye _currentEye;

  [Header("Runtime Textures")]
  [SerializeField]
  private RenderTexture _cameraTarget;

  [SerializeField]
  private RenderTexture _blurTex0;

  [SerializeField]
  private RenderTexture _blurTex1;

  private Renderer _renderer;
  private Material _downsampleMaterial;

  void Start() {
    _renderer = GetComponent<Renderer>();

    _reflectionCamera.enabled = false;

    _cameraTarget = new RenderTexture(_resolution, _resolution, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
    _cameraTarget.antiAliasing = (int)_msaa;
    _cameraTarget.useMipMap = _useMips;
    _cameraTarget.autoGenerateMips = false;
    _cameraTarget.filterMode = FilterMode.Bilinear;

    if (_itterations >= 1) {
      _downsampleMaterial = new Material(_blurShader);
      _downsampleMaterial.SetFloat("_TapRadius", 0.5f);

      _blurTex0 = getBlurTexture();
      if (_itterations > 1) {
        _blurTex1 = getBlurTexture();
      }

      if (_itterations % 2 == 0) {
        _renderer.material.SetTexture("_ReflectionTex", _blurTex1);
      } else {
        _renderer.material.SetTexture("_ReflectionTex", _blurTex0);
      }
    } else {
      _renderer.material.SetTexture("_ReflectionTex", _cameraTarget);
    }

    _reflectionCamera.targetTexture = _cameraTarget;

    Camera.onPreCull += onPreCullCallback;
    Camera.onPreRender += onPreRenderCallback;

    Texture2D noiseTex = new Texture2D(_noiseResolution, _noiseResolution,
      TextureFormat.ARGB32, mipChain: false, linear: true);

    Color32[] pixels = noiseTex.GetPixels32();
    for (int i = 0; i < pixels.Length; i++) {
      float length = Random.Range(0.0f, 128.0f);
      float angle = Random.Range(0.0f, Mathf.PI * 2.0f);

      float x = 127.0f + Mathf.Sin(angle) * length;
      float y = 127.0f + Mathf.Cos(angle) * length;

      byte r = (byte)Mathf.Clamp((int)x, 0, 255);
      byte g = (byte)Mathf.Clamp((int)y, 0, 255);

      Color32 c = new Color32(r, g, 0, 0);
      pixels[i] = c;
    }

    noiseTex.SetPixels32(pixels);
    noiseTex.Apply();

    GetComponent<Renderer>().material.SetTexture("_Noise", noiseTex);
  }

  void OnDestroy() {
    Camera.onPreCull -= onPreCullCallback;
    Camera.onPreRender -= onPreRenderCallback;
  }

  private RenderTexture getBlurTexture() {
    var tex = new RenderTexture(_resolution / _downsampleScale, _resolution / _downsampleScale, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
    tex.antiAliasing = 1;
    tex.filterMode = FilterMode.Bilinear;
    return tex;
  }

  private void setCameraMatrix(Camera.StereoscopicEye eye) {
    Matrix4x4 P = _targetCamera.GetStereoProjectionMatrix(eye);
    Matrix4x4 V = _targetCamera.GetStereoViewMatrix(eye);

    V *= calculateReflectionMatrix(calculatePlane());

    _reflectionCamera.projectionMatrix = P;
    _reflectionCamera.worldToCameraMatrix = V;

    Vector4 plane = calculateCameraSpacePlane(V, _reflectionAxis.position, _reflectionAxis.forward, 1, 0);
    P = _reflectionCamera.CalculateObliqueMatrix(plane);
    _reflectionCamera.projectionMatrix = P;
  }

  void LateUpdate() {
    Shader.SetGlobalVector("_ReflectionPlane", new Vector4(_reflectionAxis.position.y, _distortDistance, 0, 0));
  }

  private void onPreCullCallback(Camera camera) {
    if (camera != _targetCamera) return;

    _currentEye = Camera.StereoscopicEye.Left;
  }

  private void onPreRenderCallback(Camera camera) {
    if (camera != _targetCamera) return;

    setCameraMatrix(_currentEye);

    GL.invertCulling = true;
    _reflectionCamera.Render();
    GL.invertCulling = false;

    if (_useMips) {
      _cameraTarget.GenerateMips();
    }

    //int itLeft = _itterations;
    //if (itLeft > 0) {
    //  _downsampleMaterial.SetFloat("_TapRadius", 1);
    //  Graphics.Blit(_cameraTarget, _blurTex0, _downsampleMaterial);
    //  itLeft--;

    //  if (itLeft > 0) {
    //    _downsampleMaterial.SetFloat("_TapRadius", 0.5f);

    //    while (itLeft >= 2) {
    //      Graphics.Blit(_blurTex0, _blurTex1, _downsampleMaterial);
    //      Graphics.Blit(_blurTex1, _blurTex0, _downsampleMaterial);
    //    }

    //    if (itLeft > 0) {
    //      Graphics.Blit(_blurTex0, _blurTex1, _downsampleMaterial);
    //    }
    //  }
    //}

    _currentEye = Camera.StereoscopicEye.Right;
  }

  private Vector4 calculatePlane() {
    Plane plane = new Plane(_reflectionAxis.forward, _reflectionAxis.position + _reflectionAxis.forward * _offset);
    return new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
  }

  private static Vector4 calculateCameraSpacePlane(Matrix4x4 worldToCameraMatrix, Vector3 pos, Vector3 normal, float sideSign, float clipPlaneOffset) {
    Vector3 offsetPos = pos + normal * clipPlaneOffset;
    Vector3 cpos = worldToCameraMatrix.MultiplyPoint(offsetPos);
    Vector3 cnormal = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
    return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
  }

  private static Matrix4x4 calculateReflectionMatrix(Vector4 plane) {
    Matrix4x4 reflectionMat = new Matrix4x4();
    reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
    reflectionMat.m01 = (-2F * plane[0] * plane[1]);
    reflectionMat.m02 = (-2F * plane[0] * plane[2]);
    reflectionMat.m03 = (-2F * plane[3] * plane[0]);

    reflectionMat.m10 = (-2F * plane[1] * plane[0]);
    reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
    reflectionMat.m12 = (-2F * plane[1] * plane[2]);
    reflectionMat.m13 = (-2F * plane[3] * plane[1]);

    reflectionMat.m20 = (-2F * plane[2] * plane[0]);
    reflectionMat.m21 = (-2F * plane[2] * plane[1]);
    reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
    reflectionMat.m23 = (-2F * plane[3] * plane[2]);

    reflectionMat.m30 = 0F;
    reflectionMat.m31 = 0F;
    reflectionMat.m32 = 0F;
    reflectionMat.m33 = 1F;
    return reflectionMat;
  }

  public enum MsaaMode {
    Mode_1 = 1,
    Mode_2 = 2,
    Mode_4 = 4,
    Mode_8 = 8
  }
}
