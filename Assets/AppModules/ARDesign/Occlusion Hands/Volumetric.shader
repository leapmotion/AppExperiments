// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Volumetric Light/Uniform Density" {
	Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _Power ("Brightness", Float) = 1
	_InflationAmount ("Inflation Amount", Float) = 0.005
	[MaterialToggle] _isLeftHand("Is Left Hand?", Int) = 0
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
	    int _isLeftHand;
	fragment_input vert(vert_in v) {
		fragment_input o;
		v.vertex = LeapGetLateVertexPos(v.vertex, _isLeftHand); // late-latch support
		o.position = UnityObjectToClipPos(v.vertex + float4(_InflationAmount * v.normal, 0));
		o.screenPos = ComputeScreenPos(o.position);
		return o;
	}

  uniform float4 _Color;
  uniform float _Power;



  float4 frag(fragment_input input) : COLOR {
    float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(input.screenPos)));
    float distanceToCamera = min(sceneZ, input.screenPos.w);
	return distanceToCamera * -1.0 * _Power;
  }

  ENDCG

  SubShader {
    Tags {"Queue"="Transparent"}

    Pass{
      Cull Back
      ZWrite Off
      ZTest Off
      BlendOp RevSub
      Blend One One

      CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
      ENDCG
    }

    Pass{
      Cull Front
      ZWrite Off
      ZTest Off
      BlendOp Add
      Blend One One

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
    }
  } 

  Fallback Off
}
