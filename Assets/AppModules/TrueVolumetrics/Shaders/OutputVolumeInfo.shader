Shader "TrueVolumetrics/OutputVolumeInfo" {
  Properties { }

  CGINCLUDE
  #include "UnityCG.cginc"
  uniform sampler2D _CameraDepthTexture;

  struct vert_in {
    float4 vertex : POSITION;
  };

  struct fragment_input{
    float4 position : SV_POSITION;
    float4 screenPos : TEXCOORD0;
  };

  fragment_input vert(vert_in v) {
    fragment_input o;
    o.position = UnityObjectToClipPos(v.vertex);
    o.screenPos = ComputeScreenPos(o.position);
    return o;
  }

  float4 frag(fragment_input input, float face : VFACE) : COLOR {
    float sceneZ = LinearEyeDepth(
      SAMPLE_DEPTH_TEXTURE_PROJ(
        _CameraDepthTexture, UNITY_PROJ_COORD(input.screenPos)));
    float distanceToCamera = min(sceneZ, input.screenPos.w);
    return face < 0 ? distanceToCamera : -distanceToCamera;
  }

  ENDCG

  SubShader {
    Tags {"Queue"="Transparent"}

    Pass {
      Cull Off
      ZWrite Off
      ZTest Off
      Blend One One

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
    }
  } 

  Fallback Off
}
