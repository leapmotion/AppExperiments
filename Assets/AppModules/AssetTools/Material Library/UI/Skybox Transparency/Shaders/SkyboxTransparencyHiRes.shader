Shader "Custom/SkyboxTransparencyHiRes" {
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
      float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed _LOD;

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
			float3 cameraToObj = worldPos - _WorldSpaceCameraPos;

			o.cubeColor = texCUBElod(_Cube, float4(cameraToObj, _LOD));

      o.worldPos = worldPos;
		}

    //// w0, w1, w2, and w3 are the four cubic B-spline basis functions
    //float w0(float a)
    //{
    //  return (1.0 / 6.0)*(a*(a*(-a + 3.0) - 3.0) + 1.0);
    //}

    //float w1(float a)
    //{
    //  return (1.0 / 6.0)*(a*a*(3.0*a - 6.0) + 4.0);
    //}

    //float w2(float a)
    //{
    //  return (1.0 / 6.0)*(a*(a*(-3.0*a + 3.0) + 3.0) + 1.0);
    //}

    //float w3(float a)
    //{
    //  return (1.0 / 6.0)*(a*a*a);
    //}

    //// g0 and g1 are the two amplitude functions
    //float g0(float a)
    //{
    //  return w0(a) + w1(a);
    //}

    //float g1(float a)
    //{
    //  return w2(a) + w3(a);
    //}

    //// h0 and h1 are the two offset functions
    //float h0(float a)
    //{
    //  return -1.0 + w1(a) / (w0(a) + w1(a));
    //}

    //float h1(float a)
    //{
    //  return 1.0 + w3(a) / (w2(a) + w3(a));
    //}

    //float4 texture_bicubic(sampler2D tex, float2 uv, float4 texelSize)
    //{
    //  uv = uv*texelSize.zw + 0.5;
    //  float2 iuv = floor(uv);
    //  float2 fuv = frac(uv);

    //  float g0x = g0(fuv.x);
    //  float g1x = g1(fuv.x);
    //  float h0x = h0(fuv.x);
    //  float h1x = h1(fuv.x);
    //  float h0y = h0(fuv.y);
    //  float h1y = h1(fuv.y);

    //  float2 p0 = (float2(iuv.x + h0x, iuv.y + h0y) - 0.5) * texelSize.xy;
    //  float2 p1 = (float2(iuv.x + h1x, iuv.y + h0y) - 0.5) * texelSize.xy;
    //  float2 p2 = (float2(iuv.x + h0x, iuv.y + h1y) - 0.5) * texelSize.xy;
    //  float2 p3 = (float2(iuv.x + h1x, iuv.y + h1y) - 0.5) * texelSize.xy;

    //  return g0(fuv.y) * (g0x * tex2D(tex, p0) +
    //    g1x * tex2D(tex, p1)) +
    //    g1(fuv.y) * (g0x * tex2D(tex, p2) +
    //      g1x * tex2D(tex, p3));
    //}

    //void convert_xyz_to_cube_uv(float x, float y, float z,
    //                            out int index, out float u, out float v)
    //{
    //  float absX = abs(x);
    //  float absY = abs(y);
    //  float absZ = abs(z);

    //  int isXPositive = x > 0 ? 1 : 0;
    //  int isYPositive = y > 0 ? 1 : 0;
    //  int isZPositive = z > 0 ? 1 : 0;

    //  float maxAxis, uc, vc;

    //  // POSITIVE X
    //  if (isXPositive && absX >= absY && absX >= absZ) {
    //    // u (0 to 1) goes from +z to -z
    //    // v (0 to 1) goes from -y to +y
    //    maxAxis = absX;
    //    uc = -z;
    //    vc = y;
    //    index = 0;
    //  }
    //  // NEGATIVE X
    //  if (!isXPositive && absX >= absY && absX >= absZ) {
    //    // u (0 to 1) goes from -z to +z
    //    // v (0 to 1) goes from -y to +y
    //    maxAxis = absX;
    //    uc = z;
    //    vc = y;
    //    index = 1;
    //  }
    //  // POSITIVE Y
    //  if (isYPositive && absY >= absX && absY >= absZ) {
    //    // u (0 to 1) goes from -x to +x
    //    // v (0 to 1) goes from +z to -z
    //    maxAxis = absY;
    //    uc = x;
    //    vc = -z;
    //    index = 2;
    //  }
    //  // NEGATIVE Y
    //  if (!isYPositive && absY >= absX && absY >= absZ) {
    //    // u (0 to 1) goes from -x to +x
    //    // v (0 to 1) goes from -z to +z
    //    maxAxis = absY;
    //    uc = x;
    //    vc = z;
    //    index = 3;
    //  }
    //  // POSITIVE Z
    //  if (isZPositive && absZ >= absX && absZ >= absY) {
    //    // u (0 to 1) goes from -x to +x
    //    // v (0 to 1) goes from -y to +y
    //    maxAxis = absZ;
    //    uc = x;
    //    vc = y;
    //    index = 4;
    //  }
    //  // NEGATIVE Z
    //  if (!isZPositive && absZ >= absX && absZ >= absY) {
    //    // u (0 to 1) goes from +x to -x
    //    // v (0 to 1) goes from -y to +y
    //    maxAxis = absZ;
    //    uc = -x;
    //    vc = y;
    //    index = 5;
    //  }

    //  // Convert range from -1 to 1 to 0 to 1
    //  u = 0.5f * (uc / maxAxis + 1.0f);
    //  v = 0.5f * (vc / maxAxis + 1.0f);
    //}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

      float3 cameraToObj = IN.worldPos - _WorldSpaceCameraPos;
      IN.cubeColor = texCUBElod(_Cube, float4(cameraToObj, _LOD));

			o.Albedo = lerp(IN.cubeColor, c.rgb, c.a);
			o.Emission = o.Albedo * (1 - c.a);
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
