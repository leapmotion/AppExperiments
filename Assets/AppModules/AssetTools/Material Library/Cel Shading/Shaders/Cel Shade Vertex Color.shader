Shader "LeapMotion/Examples/Mobile/Cel Shade (Vertex Color)" {
  Properties {
    _Tint("Tint", Color) = (1, 1, 1, 1)
  }
    SubShader {
    Tags { "RenderType" = "Opaque" }
    LOD 80

    Pass {
      CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma fragmentoption ARB_precision_hint_fastest
        #pragma target 2.0

        #include "UnityCG.cginc"
        #include "CelShading.cginc"

        struct vertInput {
          float4 pos : POSITION;
          half4  normal : NORMAL;
          float4 color : COLOR;
        };

        struct fragInput {
          float4 pos : SV_POSITION;
          float litAmount : TEXCOORD0;
          float4 color : TEXCOORD1;
        };

        float4 _Tint;

        fragInput vert(in vertInput vertIn) {
          fragInput f;

          f.pos = UnityObjectToClipPos(vertIn.pos);

          half3 lightDir = UnityWorldSpaceLightDir(vertIn.pos);

          half litAmount = max(0, (dot(UnityObjectToWorldDir(vertIn.normal), lightDir) + 0.7) / 2);
          f.litAmount = litAmount;

          f.color = _Tint * vertIn.color;

          return f;
        }

        #define CEL_SHADE_STEPS 4
        fixed4 frag(in fragInput fragIn) : SV_Target{
          return celShadedColor(CEL_SHADE_STEPS, fragIn.litAmount, fragIn.color);
        }
      ENDCG
    }

  }

}
