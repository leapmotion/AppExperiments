Shader "Custom/Standard RimLit RainbowNormals" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
    _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
      float3 worldNormal;
      float3 viewDir;
      float3 objectNormal;
		};

    #define RED_V   float3(0,  0,         -1.0)
    #define GREEN_V float3(0,  0.8660257,  0.5)
    #define BLUE_V  float3(0, -0.8660253,  0.5)
    #define WHITE_V float3(1,  0,          0)

    float4x4 rotationMatrix(float3 axis, float angle) {
      axis = normalize(axis);
      float s = sin(angle);
      float c = cos(angle);
      float oc = 1.0 - c;

      return float4x4(
        oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s, 0.0,
        oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s, 0.0,
        oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c, 0.0,
        0.0, 0.0, 0.0, 1.0);
    }

    void vert(inout appdata_full v, out Input o) {
      UNITY_INITIALIZE_OUTPUT(Input, o);

      float3 axis = WHITE_V;
      float angle = _Time.x * 8;
      float3 rotated = mul(rotationMatrix(axis, angle), v.normal);

      o.objectNormal = rotated;
    }

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
    fixed4 _RimColor;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;

      // Colorful normals.
      float redAmount = (dot(IN.objectNormal, RED_V) + 1) * 0.5;
      float greenAmount = (dot(IN.objectNormal, GREEN_V) + 1) * 0.5;
      float blueAmount = (dot(IN.objectNormal, BLUE_V) + 1) * 0.5;
      fixed3 extraColor = fixed3(redAmount, greenAmount, blueAmount);
      float whiteAmount = abs(dot(IN.objectNormal, WHITE_V));
      //extraColor = lerp(extraColor, float3(1, 1, 1), whiteAmount);
      o.Albedo *= extraColor;

      // Rim lighting.
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
