Shader "Unlit/Color FakeLit StrengthAdjust" {
	Properties {
    _Color("Color", Color) = (1, 1, 1, 1)
    _LitStrength("Lit Strength", Range(0, 1)) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
        float4 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
        float4 normal : NORMAL;
				float4 vertex : SV_POSITION;
			};
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
        o.normal = v.normal;
        o.uv = v.uv;
				return o;
			}

      float4 _Color;
      float _LitStrength;
			
			fixed4 frag (v2f i) : SV_Target {
        fixed4 color = fixed4(1,1,1,1);

        float litAmount = dot(normalize(i.normal.xyz), normalize(float3(1, 1.3, 0)));
        color = litAmount * 0.25 + color;

        color *= _Color;

        color = lerp(_Color, color, _LitStrength);

				return color;
			}
			ENDCG
		}
	}
}
