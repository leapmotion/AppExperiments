Shader "Skybox/HSV Gradient Skybox" {
	Properties {
		_SkyColor("Sky Color", Color) = (0.37, 0.52, 0.73, 0)
		_HorizonColor("Horizon Color", Color) = (0.89, 0.96, 1, 0)
    _HorizonOffset("Horizon Offset", Float) = 0.0
    _Sharpness("Sharpness Multiplier", Range(0.2, 4)) = 1.0
	}
	SubShader {
		Tags{ "RenderType" = "Skybox" "Queue" = "Background" }
	  Pass
		{
			ZWrite Off
			Cull Off
			Fog { Mode Off }

			CGPROGRAM

        #pragma target 2.0
			  #pragma fragmentoption ARB_precision_hint_fastest
			  #pragma vertex vert
			  #pragma fragment frag

        #include "UnityCG.cginc"

		    struct appdata {
		      float4 position : POSITION;
		      float3 texcoord : TEXCOORD0;
	      };

	      struct v2f {
		      float4 position : SV_POSITION;
		      float3 texcoord : TEXCOORD0;
	      };

	      half3 _SkyColor;
	      half3 _HorizonColor;
        half _HorizonOffset;
        half _Sharpness;

        // HSV / RGB Functions -- http://www.chilliant.com/rgb2hsv.html
        
        float Epsilon = 1e-10;

        float3 RGBtoHCV(in float3 RGB) {
          // Based on work by Sam Hocevar and Emil Persson
          float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
          float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
          float C = Q.x - min(Q.w, Q.y);
          float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
          return float3(H, C, Q.x);
        }

        float3 HueToRGB(in float H) {
          float R = abs(H * 6 - 3) - 1;
          float G = 2 - abs(H * 6 - 2);
          float B = 2 - abs(H * 6 - 4);
          return saturate(float3(R, G, B));
        }

        float3 HSVtoRGB(in float3 HSV) {
          float3 RGB = HueToRGB(HSV.x);
          return ((RGB - 1) * HSV.y + 1) * HSV.z;
        }

        float3 RGBtoHSV(in float3 RGB) {
          float3 HCV = RGBtoHCV(RGB);
          float S = HCV.y / (HCV.z + Epsilon);
          return float3(HCV.x, S, HCV.z);
        }

        // ------

	      v2f vert(appdata v) {
		      v2f o;
		      o.position = UnityObjectToClipPos(v.position);
		      o.texcoord = v.texcoord;
		      return o;
	      }

	      half4 frag(v2f i) : COLOR {
          float3 v = normalize(i.texcoord);
          half3 color = HSVtoRGB(lerp(RGBtoHSV(_HorizonColor), RGBtoHSV(_SkyColor), smoothstep(0, 1, (v.y + _HorizonOffset) * _Sharpness)));
          return half4(color.x, color.y, color.z, 1);
	      }

			ENDCG
		}
	}
}