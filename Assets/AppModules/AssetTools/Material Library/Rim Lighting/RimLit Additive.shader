Shader "Custom/RimLit Additive" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
    _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
	}
	SubShader {
		Tags { "Queue"="Transparent-10" "RenderType"="Transparent" }
    ZWrite Off
    Blend One One
		LOD 200

		CGPROGRAM

    fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
    {
      fixed4 c;
      c.rgb = s.Albedo;
      c.a = s.Alpha;
      return c;
    }

		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf NoLighting noforwardadd

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
      float3 worldNormal;
      float3 viewDir;
		};

		fixed4 _Color;
    fixed4 _RimColor;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Alpha = c.a;

      float toCameraAmount = 1 - dot(IN.worldNormal, IN.viewDir);
      toCameraAmount *= 1.1;
      toCameraAmount *= toCameraAmount;
      toCameraAmount = saturate(toCameraAmount);
      o.Emission = _RimColor.rgb * _RimColor.a * toCameraAmount;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
