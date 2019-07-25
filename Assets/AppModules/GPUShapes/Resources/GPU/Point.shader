Shader "Point"
{
    SubShader
    {  
		CGINCLUDE

 		#include "UnityCG.cginc"
	
		//#include "Includes/buffers.cginc"
		struct PointStruct
		{			
			float4 position;
			float4 color;
		};	

		StructuredBuffer<PointStruct>	_Point; 



		struct ps_input 
		{           
			float4 color	: COLOR;
			float4 position	: TEXCOORD0;
			float4 center	: TEXCOORD1;
			uint id			: TEXCOORD2;
        };


		uniform float   	_Count;
		uniform float 		_PointSize;
		uniform float4 		_ColorTint;     
	

		ps_input vertex_point (uint id : SV_VertexID, out float4 vertex : SV_POSITION, out float psize : PSIZE)
        {		
			float4 position 		= _Point[id].position;
			float4 color 			= float4(1., 1., 1., 1.);//_Point[id].color;
			float4 world_position	= mul(unity_ObjectToWorld, position);
		
		//	color 					*= _ColorTint;
		
			vertex					= UnityObjectToClipPos(position);
			psize					= 64;// min(_PointSize/vertex.w, 64.);

			ps_input o;
			o.id					= id;			
			o.position				= world_position;
			o.color					= color;			
 			o.center				= ComputeScreenPos(vertex); 			
			o.center.z				= psize;

			return o;
        }


		float4 fragment_point (ps_input i, UNITY_VPOS_TYPE vpos : VPOS) : SV_Target
  		{		
			float4 center		= i.center;
			float3 position		= i.position.xyz;
			
			float2 point_uv		= vpos.xy/center.z - (center.xy * _ScreenParams.xy)/center.w/center.z;
			
			if(length(point_uv) > .5)
			{ 
				//discard;
			}
         
			float2 screen_uv	= vpos.xy/_ScreenParams.xy;
 			float2 sprite_uv	= point_uv + .5;
			  
			float4 color		= i.color;

			return color;
        }
 		ENDCG


		Tags 
		{ 
			"LightMode" 			= "Off"
			"Queue"					= "Transparent"
			"IgnoreProjector" 		= "True" 
			"DisableBatching" 		= "False" 
			"ForceNoShadowCasting" 	= "True"
		}


		Blend 		[_SrcBlend] [_DestBlend]
		ZWrite 		[_ZWrite]
		ZTest 		[_ZTest]
		Cull 		[_Cull]
		ColorMask	[_ColorMask]
		
		Pass
        {
			Lighting Off

            CGPROGRAM
			#pragma target 			4.5
			#pragma fragmentoption 	ARB_precision_hint_fastest
            #pragma vertex 			vertex_point
            #pragma fragment 		fragment_point
            ENDCG 
        }
    }
}