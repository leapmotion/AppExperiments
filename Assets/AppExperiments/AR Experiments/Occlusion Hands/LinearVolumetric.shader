// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Linear Volumetric Light" {
	Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _Power ("Brightness", Float) = 1
    _DensityA ("Density A", Vector) = (0,0,0,1)
    _DensityB ("Density B", Vector) = (0,0,0,1)
	}

  CGINCLUDE
  #include "UnityCG.cginc"

  uniform sampler2D _CameraDepthTexture;

  struct vert_in {
    float4 vertex : POSITION;
  };

  struct fragment_input{
	float4 position : SV_POSITION;
    float4 screenPos : TEXCOORD0;
    float4 worldPos : TEXCOORD1;
    float cameraDensity : TEXCOORD2;
  };

  uniform float4 _DensityA;
  uniform float4 _DensityB;

  float density(float3 position){
    float3 pa = position - _DensityA.xyz;
    float3 ba = _DensityB.xyz - _DensityA.xyz;
    float t = dot(pa, ba) / dot(ba, ba);
    return lerp(_DensityA.w, _DensityB.w, t);
  }

  fragment_input vert(vert_in v) {
	fragment_input o;
	o.position = UnityObjectToClipPos(v.vertex);
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    o.screenPos = ComputeScreenPos(o.position); 
    o.cameraDensity = density(_WorldSpaceCameraPos);
	return o;
  }

  uniform float4 _Color;
  uniform float _Power;

	float4 frag(fragment_input input) : COLOR {
    float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(input.screenPos)));

    float distanceToCamera = min(sceneZ, input.screenPos.z);
    float3 fragmentPos = _WorldSpaceCameraPos + (input.worldPos.xyz - _WorldSpaceCameraPos) * distanceToCamera / input.screenPos.w;

    float light = (density(fragmentPos) + input.cameraDensity) * distanceToCamera * 0.5;

    //light = clamp(light, 0, 200);

	return light * _Color * _Power;
  }

  ENDCG

  SubShader {
    Tags {"Queue"="Transparent"}

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
  } 

  Fallback Off
}
