Shader "Virtual Materials/Plath Unlit"
{
	Properties
	{
    [NoScaleOffset]
    _ProximityGradient ("Proximity Gradient", 2D) = "white" {}

    _ProximityMapping ("Map: DistMin, DistMax, GradMin, GradMax", Vector) = (0, 0.04, 1, 0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Stencil{
			Ref[_PortalMask]
			ReadMask 3
			Comp equal
		}

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
      #include "Assets/AppModules/TodoUMward/Shader Hand Data/Resources/HandData.cginc"

      // Vert / Frag Structs
			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
        float4 worldVertex : TECOORD0;
			};

      // Public Material Properties
      sampler2D _ProximityGradient;
			float4 _ProximityMapping;

			v2f vert (appdata v)
			{
        // Apply gradient based on distance from fingertips.
        float4 worldVertex = mul(unity_ObjectToWorld, v.vertex);
        
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
        o.worldVertex = worldVertex;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return evalProximityColor(i.worldVertex, _ProximityGradient, _ProximityMapping);
			}
			ENDCG
		}
	}
}
