Shader "Custom/SkyboxTransparency" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	  _Cube("Cubemap", CUBE) = "" {}
		_LOD ("Cubemap LOD", Float) = 0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 3.0

		sampler2D _MainTex;
		samplerCUBE _Cube;

		struct Input {
			float2 uv_MainTex;
			float4 cubeColor;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed _LOD;

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			
			float3 worldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
			float3 cameraToObj = worldPos - _WorldSpaceCameraPos;

			o.cubeColor = texCUBElod(_Cube, float4(cameraToObj, _LOD));
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = lerp(IN.cubeColor, c.rgb, c.a);
			o.Emission = o.Albedo * (1 - c.a);
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
