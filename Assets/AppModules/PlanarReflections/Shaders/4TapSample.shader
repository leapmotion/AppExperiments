// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/4TapSample" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
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
      half4 _MainTex_TexelSize;
      half _TapRadius;

			fixed4 frag (v2f i) : SV_Target {
        float2 uv = i.uv;
        float2 duv = _MainTex_TexelSize.xy * _TapRadius;
				fixed4 col0 = 0.25 * tex2D(_MainTex, uv + duv * float2(1, 1));
        fixed4 col1 = 0.25 * tex2D(_MainTex, uv + duv * float2(-1, 1));
        fixed4 col2 = 0.25 * tex2D(_MainTex, uv + duv * float2(1, -1));
        fixed4 col3 = 0.25 * tex2D(_MainTex, uv + duv * float2(-1, -1));
        return (col0 + col1 + col2 + col3);
			}
			ENDCG
		}
	}
}
