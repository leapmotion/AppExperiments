Shader "PlanarReflections/Reflection" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
    _BumpMap ("Bumpmap", 2D) = "bump" {}
    _Noise("Noise", 2D) = "white" {}
    _ReflectionTex("Reflection", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
    _Bunpiness ("Bumpiness", Float) = 0.1
    _MinBumpiness ("Min Bumpiness", Float) = 0.1
    _AlphaPow  ("Alpha pow", Float) = 2
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard
		#pragma target 3.0

		sampler2D _MainTex;
    sampler2D _BumpMap;
    sampler2D _Noise;
    sampler2D _ReflectionTex;

		struct Input {
			float2 uv_MainTex;
      float2 uv_BumpMap;
      float2 uv_Noise;
      float4 screenPos;
      float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
    half _AlphaPow;
    half _Bunpiness;
    half _MinBumpiness;
		fixed4 _Color;

    float4 getReflectionColorFromNoise(Input IN, float2 offset) {
      fixed2 noise = tex2D(_Noise, IN.uv_Noise + offset).xy - float2(0.5, 0.5);
      fixed2 initialScreenUv = IN.screenPos.xy / IN.screenPos.w;
      fixed offset1 = tex2D(_ReflectionTex, initialScreenUv).a;
      fixed2 secondScreenUv = initialScreenUv + offset1 * _Bunpiness * noise;
      fixed offset2 = tex2D(_ReflectionTex, secondScreenUv).a;
      fixed finalOffset = max(_MinBumpiness, min(offset2, offset1));

      fixed2 finalScreenUv = initialScreenUv + finalOffset * _Bunpiness * noise;
      fixed4 reflectionColor = tex2D(_ReflectionTex, finalScreenUv);

      return reflectionColor;
    }

    float4 getReflectionColorFromNoiseSS(Input IN) {
      float4 c0 = getReflectionColorFromNoise(IN, _Time.yy * float2(5.2, 9.2));
      float4 c1 = getReflectionColorFromNoise(IN, _Time.yy * float2(-4.6, -5.6));
      float4 c2 = getReflectionColorFromNoise(IN, _Time.yy * float2(-8.1, 3.5));
      float4 c3 = getReflectionColorFromNoise(IN, _Time.yy * float2(3.8, 6.2));
      return (c0 + c1 + c2 + c3) * 0.25;
    }

    float4 getReflectionColorFromMips(Input IN) {
      fixed2 screenUv = IN.screenPos.xy / IN.screenPos.w;

      fixed offset0 = tex2D(_ReflectionTex, screenUv).a;
      fixed offset1 = tex2D(_ReflectionTex, screenUv + float2(+0.05, +0.05)).a;
      fixed offset2 = tex2D(_ReflectionTex, screenUv + float2(-0.05, +0.05)).a;
      fixed offset3 = tex2D(_ReflectionTex, screenUv + float2(+0.05, -0.05)).a;
      fixed offset4 = tex2D(_ReflectionTex, screenUv + float2(-0.05, -0.05)).a;

      fixed offset = min(offset0, min(min(offset1, offset2), min(offset3, offset4)));

      return tex2Dlod(_ReflectionTex, float4(screenUv, 0, _Bunpiness * offset));
    }

    float4 getReflectionColorFromNormal(Input IN, float2 offset) {
      fixed2 initialScreenUv = IN.screenPos.xy / IN.screenPos.w;
      fixed offset1 = tex2D(_ReflectionTex, initialScreenUv).a;

      offset1 = pow(offset1, 2);

      fixed2 secondScreenUv = initialScreenUv + offset1 * _Bunpiness * offset;
      fixed offset2 = tex2D(_ReflectionTex, secondScreenUv).a;

      offset2 = pow(offset2, 2);

      fixed finalOffset = max(_MinBumpiness, min(offset2, offset1));

      fixed2 finalScreenUv = initialScreenUv + finalOffset * _Bunpiness * offset;
      fixed4 reflectionColor = tex2D(_ReflectionTex, finalScreenUv);

      return reflectionColor;
    }

		void surf (Input IN, inout SurfaceOutputStandard o) {
      //o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));

      //fixed4 reflectionColor = tex2D(_ReflectionTex, IN.screenPos.xy / IN.screenPos.w);

      //fixed4 reflectionColor = getReflectionColorFromNoise(IN, float2(0, 0));

      //fixed4 reflectionColor = getReflectionColorFromMips(IN);

      fixed4 reflectionColor = getReflectionColorFromNoiseSS(IN);

      //float2 screenUv = IN.screenPos.xy / IN.screenPos.w;
      //screenUv += o.Normal.xy * _Bunpiness;
     // fixed4 reflectionColor = tex2D(_ReflectionTex, screenUv);

      //float4 reflectionColor = getReflectionColorFromNormal(IN, o.Normal.xy);

      /*
      float4 screenUv = float4(IN.screenPos.xy / IN.screenPos.w, 0, _Bunpiness);
      fixed4 reflectionColor = tex2Dlod(_ReflectionTex, screenUv);
      */

			fixed4 objectColor = tex2D (_MainTex, IN.uv_MainTex) * _Color;
      objectColor.a = pow(objectColor.a, _AlphaPow);

      fixed3 finalColor = lerp(float3(0,0,0), objectColor.rgb, objectColor.a);

      o.Albedo = finalColor;
			o.Metallic = _Metallic;
      o.Emission = lerp(reflectionColor.rgb, float3(0, 0, 0), objectColor.a);
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
