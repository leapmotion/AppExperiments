using UnityEngine;

public class ProceduralShape : MonoBehaviour
{
    public ShapeConfiguration                   configuration;
    private Material _material                  = null;
    public Material material                    { get { return _material; } }
    private Camera[] _camera                    = null;
    private bool[] _has_command_buffer          = null;
    private GameObject[] _control_object        = null;
// CommandBuffer[] _command_buffer             = null;

    void Start() 
    {
        configuration                           = configuration == null ? ScriptableObject.CreateInstance<ShapeConfiguration>() : configuration;
        _material                               = new Material(Shader.Find("Shape"));          
            
        _camera                                 = GameObject.FindObjectsOfType<Camera>();
        _has_command_buffer                     = new bool[_camera.Length];
        // _command_buffer                         = new CommandBuffer[_camera.Length];

        for(int i = 0; i < _camera.Length; i++)
        {               
            _has_command_buffer[i]              = false;
        }

          CreateControlObjects();
}

    void Update()
    { 
        for(int i = 0; i < _control_object.Length; i++)
        {
            configuration.control_point[i].x = _control_object[i].transform.position.x;   
            configuration.control_point[i].y = _control_object[i].transform.position.y;   
            configuration.control_point[i].z = _control_object[i].transform.position.z;   
        }

        if (_material.shader != null) 
        {
          SetUniforms();
        }
    }


    void OnRenderObject() 
    {
        if (_material.shader != null)
        {
            _material.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Points, Mathf.Clamp(configuration.point_count, 1, (int)Mathf.Pow(2, 24)));
        }
    }

        
    void SetUniforms()
    {   
        _material.SetMatrix("_Transform", Matrix4x4.TRS(gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.localScale));
        _material.SetFloat("_Count", configuration.point_count);                   
        _material.SetFloat("_PointSize", configuration.point_size);
        _material.SetInt("_Grid", configuration.grid ? 1 : 0);
        _material.SetFloat("_Curvature", configuration.curvature);
        _material.SetFloat("_Density", configuration.density);
        _material.SetColor("_Color", configuration.color);     
        _material.SetInt("_Fill", configuration.fill ? 1 : 0);   
        _material.SetInt("_Shape", (int)configuration.shape);
        _material.SetInt("_Hand_Interaction", configuration.hand_interaction ? 1 : 0);
        _material.SetVectorArray("_ControlPoint", configuration.control_point); 
    }

        
    void CreateControlObjects()
    {
        _control_object                         = new GameObject[configuration.control_point.Length];
        for(int i = 0; i < _control_object.Length; i++)
        {
            _control_object[i]                      = new GameObject();
            _control_object[i].name                 = "Shape Control Point " + i.ToString();
            _control_object[i].transform.position   = configuration.control_point[i];
            _control_object[i].transform.SetParent(gameObject.transform);
        }    
    }


    void DestroyControlObjects()
    {
        for(int i = 0; i < _control_object.Length; i++)
        {
            if(_control_object[i] != null)
            {
                GameObject.Destroy(_control_object[i]);
            }
        }   
    }
}
