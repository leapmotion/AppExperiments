Shader "Custom/FloorRings" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_Ring("Ring Color", Color) = (0, 0, 0, 0)
		_Fog("Fog Color", Color) = (1, 1, 1, 1)
		_FogStart ("Fog Start", Float) = 0
		_FogEnd   ("Fog End", Float) = 0
		_RingSpacing("Ring Spacing", Float) = 1
		_RingWidth ("Ring Width", Float) = 1
	    _RingSmooth ("Ring Smooth", Float) = 1
		_RingFactor ("Ring Factor", Float) = 1
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		Stencil{
			Ref[_PortalMask]
			ReadMask 3
			Comp equal
		}
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows finalcolor:mycolor
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		fixed4 _Ring;
		float _RingSpacing;
		float _RingWidth;
		float _RingFactor;
		float _RingSmooth;

		fixed4 _Fog;
		float _FogStart;
		float _FogEnd;

		void mycolor(Input IN, SurfaceOutputStandard o, inout fixed4 color)
		{
			float distFromCamera = length(IN.worldPos - _WorldSpaceCameraPos);
			float fogFactor = smoothstep(_FogStart, _FogEnd, distFromCamera);

			color = lerp(color, _Fog, fogFactor);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

			float dist = length(IN.worldPos.xz);
			float smooth = _RingSmooth + dist * dist * _RingFactor;
			float ring = smoothstep(_RingWidth - smooth, _RingWidth + smooth, sin(dist / _RingSpacing));

			c = lerp(c, _Ring, ring);


			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
