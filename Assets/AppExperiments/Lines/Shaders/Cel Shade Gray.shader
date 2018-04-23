Shader "LeapMotion/Examples/Mobile/Cel Shade Gray" {
	Properties { }
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

        struct vertInput {
          float4 pos : POSITION;
          half4 normal : NORMAL;
        };

        struct fragInput {
          float4 pos : SV_POSITION;
          float litAmount : TEXCOORD0;
        };
        
        fragInput vert(in vertInput vertIn) {
          fragInput f;
          f.pos = UnityObjectToClipPos(vertIn.pos);
          half litAmount = max(0, dot(UnityObjectToWorldDir(vertIn.normal), half3(0.5566811, 0.6451192, 0.5233808)));
          f.litAmount = litAmount;
          return f;
        }
        
        #define NUM_SHADE_STEPS 3
        #define BASE_COLOR fixed4(0.6, 0.7, 0.8, 1)
        fixed4 frag(in fragInput fragIn) : SV_Target {
          return BASE_COLOR * floor(max(fragIn.litAmount, 0) * NUM_SHADE_STEPS) / NUM_SHADE_STEPS + 0.2;
        }
		  ENDCG
    }
	}
}
