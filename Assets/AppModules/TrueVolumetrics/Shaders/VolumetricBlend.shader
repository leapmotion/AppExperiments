Shader "LeapMotion/TrueVolumetrics/VolumetricBlend" {
  Properties {
    [HideInInspector]
    _MainTex ("Volume Info", 2D)    = "white" {}
    _Color   ("Color",   Color) = (0,0,0,0)
    _Scale   ("Scale",   Float) = 1
    _Density ("Density", Range(0, 1)) = 1

    [KeywordEnum(Exponential, Linear)] _Blend ("Blend Mode", Float) = 0
    [Enum(Additive,1,AlphaBlended,10)] _BlendType ("Blend Type", Float) = 1
  }
  SubShader
  {
    // No culling or depth
    Cull Off ZWrite Off ZTest Always
    Blend SrcAlpha [_BlendType]

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile _BLEND_EXPONENTIAL _BLEND_LINEAR

      #include "UnityCG.cginc"

      struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
      };

      v2f vert (appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
      }
      
      sampler2D _MainTex;
      float4 _Color;
      float _Power;
      float _Density;
      float _Scale;

      fixed4 frag (v2f i) : SV_Target {
        float distance = tex2D(_MainTex, i.uv).r;

#if _BLEND_EXPONENTIAL
        float opacity = saturate((1 - pow(1 - _Density, _Scale * distance)));
#endif

#if _BLEND_LINEAR
        float opacity = distance * _Scale;
#endif

        return fixed4(_Color.rgb, opacity);
      }
      ENDCG
    }
  }
}
