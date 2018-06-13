Shader "Custom/Cel Shade Gray Particles" {
	Properties {
    _Lerp        ("Prev to Curr", Range(0, 1))      = 1
		_Color       ("Color", Color)                   = (1,1,1,1)
    _Size        ("Size", Range(0, 0.5))            = 0.01
    _TrailLength ("Trail Length", Range(0, 10000))  = 1000
    _Brightness  ("Brightness", Float)              = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
    Pass {
      CGPROGRAM
      #pragma multi_compile COLOR_SPECIES COLOR_SPECIES_MAGNITUDE COLOR_VELOCITY
      #pragma multi_compile _ ENABLE_INTERPOLATION

      #pragma vertex vert
      #pragma fragment frag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 2.0

      sampler2D _PrevPos;
      sampler2D _CurrPos;
      sampler2D _PrevVel;
      sampler2D _CurrVel;

      #include "UnityCG.cginc"

      sampler2D _MainTex;
      sampler2D _Velocity;

      struct vertInput {
        float4 pos : POSITION;
        float4 normal : NORMAL;
        float4 texcoord : TEXCOORD0;
        float4 color : COLOR;
      };

      struct fragInput {
        float4 pos : SV_POSITION;
        float4 color : TEXCOORD0;
        float litAmount : TEXCOORD1;
      };

      half _Lerp;
      float4 _Colors[32];
      float _Size;
      float _TrailLength;
      float _Brightness;

      fragInput vert(in vertInput v) {
  #ifdef ENABLE_INTERPOLATION
        float4 particle = lerp(tex2Dlod(_PrevPos, v.texcoord), tex2Dlod(_CurrPos, v.texcoord), _Lerp);
  #else
        float4 particle = tex2Dlod(_CurrPos, v.texcoord);
  #endif
        float4 velocity = tex2Dlod(_CurrVel, v.texcoord);
        velocity.xyz *= velocity.w;

        float dir = saturate(-dot(normalize(velocity.xyz), normalize(v.pos.xyz)) - 0.2);
        v.pos.xyz -= velocity.xyz * dir * _TrailLength * (1/max(_Size, 0.001)) * 0.001;

        v.pos.xyz *= _Size;
        v.pos.xyz += particle.xyz;

        #ifdef COLOR_SPECIES
              v.color = _Colors[(int)particle.w];
        #endif
          
        #ifdef COLOR_VELOCITY
              v.color.rgb = abs(velocity.xyz) * _Brightness;
        #endif
          
        #ifdef COLOR_SPECIES_MAGNITUDE
              v.color = _Colors[(int)particle.w] * length(velocity.xyz) * _Brightness;
        #endif


        // Frag data for cel-shader.

        fragInput f;

        f.pos = UnityObjectToClipPos(v.pos);

        half3 lightDir = UnityWorldSpaceLightDir(v.pos);
        half litAmount = max(0, (dot(UnityObjectToWorldDir(v.normal), lightDir) + 0.7) / 2);
        f.litAmount = litAmount;

        f.color = v.color;

        return f;
      }

      #include "CelShading.cginc"
      #define CEL_SHADE_STEPS 4
      fixed4 frag(in fragInput fragIn) : SV_Target {
        return celShadedColor(CEL_SHADE_STEPS, fragIn.litAmount, fragIn.color);
      }
      ENDCG
    }
	}

}
