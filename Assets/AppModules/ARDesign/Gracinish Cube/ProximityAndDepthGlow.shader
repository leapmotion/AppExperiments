Shader "Virtual Materials/AR Experiments/Proximity And Depth Glow"
{
	Properties
	{
    _HandGradient ("Hand Proximity Gradient", 2D) = "white" {}
    _HandMapping ("Hand Proximity Mapping", Vector) = (0, 0.04, 1, 0)
    _DepthGradient ("Depth Gradient", 2D) = "white" {}
    _WorldDepthMapping ("World Depth Mapping", Vector) = (0.4, 1, 0.3, 0.7)
    _ObjectDepthAdditionMapping ("Object Depth Addition Map", Vector) = (-1, 1, -0.3, 0.3)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "DisableBatching"="True" }
		LOD 100

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
        float4 worldVertex  : TEXCOORD0;
        float4 headObjDist  : TEXCOORD1;
        float  objVertDepthOffset_objToHeadAxis : TEXCOORD2;
			};

      // Public Material Properties
      float     _Alpha;
      sampler2D _HandGradient;
			float4    _HandMapping;
      sampler2D _DepthGradient;
      float4    _WorldDepthMapping;
      float4    _ObjectDepthAdditionMapping;

			v2f vert (appdata v)
			{
        // Apply gradient based on distance from fingertips.
        float4 worldVertex = mul(unity_ObjectToWorld, v.vertex);
        
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
        o.worldVertex = worldVertex;

        float4 objCenter = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
        float3 objToHead = objCenter - _WorldSpaceCameraPos;
        float objToHeadLength = length(objToHead);

        o.objVertDepthOffset_objToHeadAxis
          = dot(o.worldVertex - objCenter, objToHead / objToHeadLength);

        o.headObjDist = objToHeadLength;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
        float depthValue = Leap_Map4(i.headObjDist, _WorldDepthMapping);
        depthValue += Leap_Map4(i.objVertDepthOffset_objToHeadAxis,
                                _ObjectDepthAdditionMapping);
        
        float4 depthColor = tex2D(_DepthGradient, float2(depthValue, 0));
        float4 handColor = evalProximityColor(i.worldVertex, _HandGradient, _HandMapping);
        float4 color = depthColor + handColor;
        return color;
			}
			ENDCG
		}
	}
  Fallback "Diffuse"
}
