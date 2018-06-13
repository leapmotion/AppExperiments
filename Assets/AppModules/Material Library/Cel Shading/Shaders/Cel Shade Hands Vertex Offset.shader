Shader "LeapMotion/Examples/Mobile/Cel Shade Gray Hand Vertex Offset" {
	Properties {
    _Color ("Color", Color) = (0.5, 0.5, 0.5, 0.5)
    [MaterialToggle] _isLeftHand("Is Left Hand?", Int) = 0
  }
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 80
		
    Pass {
		  CGPROGRAM
		    #pragma vertex vert
        #pragma fragment frag
        #pragma fragmentoption ARB_precision_hint_fastest
		    #pragma target 2.0

        #include "UnityCG.cginc"
        #include "Assets/LeapMotion/Core/Resources/LeapCG.cginc"
        #include "CelShading.cginc"

        struct vertInput {
          float4 pos : POSITION;
          half4 normal : NORMAL;
        };

        struct fragInput {
          float4 pos : SV_POSITION;
          float litAmount : TEXCOORD0;
          float4 color : TEXCOORD1;
        };

        float4 _Color;
        int _isLeftHand;

        fragInput vert(in vertInput vertIn) {
          fragInput f;

          vertIn.pos = LeapGetLateVertexPos(vertIn.pos, _isLeftHand);
          f.pos = UnityObjectToClipPos(vertIn.pos);

          half3 lightDir = UnityWorldSpaceLightDir(vertIn.pos);

          half litAmount = max(0, (dot(UnityObjectToWorldDir(vertIn.normal), lightDir) + 0.7) / 2);
          f.litAmount = litAmount;

          f.color = _Color;

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
