Shader "LeapMotion/Examples/Lines/LineVertexColor" {
	Properties { }
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100
    Cull Off

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct vertdata {
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct fragdata {
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};
			
			fragdata vert (vertdata v) {
				fragdata o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (fragdata i) : SV_Target {
				fixed4 col = i.color;
				return col;
			}
			ENDCG
		}
	}
}
