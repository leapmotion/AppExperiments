// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LeapMotion/Passthrough/UnlitImageSurface"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
		_ZMultiplier("Depth Multiplier", Range(0,10)) = 1.0
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			//#pragma multi_compile_fog

			#pragma multi_compile LEAP_FORMAT_IR LEAP_FORMAT_RGB
			#include "Assets/LeapMotion/Core/Resources/LeapCG.cginc"
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			uniform float _LeapGlobalColorSpaceGamma;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Cutoff;
			float _ZMultiplier;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float3 hand = LeapGetUVColor(float4(i.uv.x, i.uv.y, 1, 1));
				fixed4 col = float4(hand, hand.r);
				clip(hand.r - (i.vertex.z * _ZMultiplier) - _Cutoff);
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
