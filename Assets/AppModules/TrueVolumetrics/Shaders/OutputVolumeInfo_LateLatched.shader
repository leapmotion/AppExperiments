Shader "TrueVolumetrics/OutputVolumeInfo_LateLatched" {
  Properties { 
		_InflationAmount("Inflation Amount", Float) = 0.005
    [Enum(Left,1,Right,0)]
    _IsLeft ("Handedness", Float) = 0.0
    [Enum(Yes,1,No,0)]
    _IsTracked ("Is Tracked", Float) = 0.0
  }

  CGINCLUDE
  #include "UnityCG.cginc"
  #include "Assets/LeapMotion/Core/Resources/LeapCG.cginc"
  uniform sampler2D _CameraDepthTexture;

  struct vert_in {
    float4 vertex : POSITION;
		float3 normal : NORMAL;
  };

  struct fragment_input{
    float4 position : SV_POSITION;
    float4 screenPos : TEXCOORD0;
  };

	float _InflationAmount;
  float _IsLeft;
  float _IsTracked;

  fragment_input vert(vert_in v) {
    fragment_input o;
    v.vertex = LeapGetLateVertexPos(v.vertex, _IsLeft); // late-latch support
    o.position = UnityObjectToClipPos(v.vertex + float4(_InflationAmount * v.normal, 0));
    o.screenPos = ComputeScreenPos(o.position);
    return o;
  }

  float4 frag(fragment_input input, float face : VFACE) : COLOR {
    float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(input.screenPos)));
    
    if (_IsTracked <= 0) input.screenPos.w = sceneZ;
    
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
