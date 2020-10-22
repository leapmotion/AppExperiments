Shader "Shape"
{
    SubShader
    {
		CGINCLUDE
 		#include "UnityCG.cginc"
  
		#include "Includes/bitops.cginc"
		#include "Includes/hashes.cginc"
		#include "Includes/fields.cginc"
		#include "Includes/curves.cginc"
		#include "Includes/derivatives.cginc"
		#include "Includes/spherical_fibonacci.cginc"
 
		struct ps_input 
		{
            float4 position 				: TEXCOORD0;
			float size	 					: PSIZE;
			float4 center					: TEXCOORD1;
			float4 color 					: COLOR;
        };

		#include "Assets/AppModules/TodoUMward/Shader Hand Data/Resources/HandData.cginc"

//		RWStructuredBuffer<float4>   		_Position; 
	//	RWStructuredBuffer<float4>   		_Color; 
	//	RWStructuredBuffer<float4>   		_Hand; 

		#define FILL 			(_Fill == 1)
 		#define GRID 			(_Grid == 1)
		#define SHAPE_SPHERE 	(_Shape == 0)
		#define SHAPE_CUBE 		(_Shape == 1)		
		#define SHAPE_FFT		(_Shape == 2)	
		#define SHAPE_PATH		(_Shape == 3)
 
		#define 	FREQUENCIES 1000

		float		_FFT[FREQUENCIES];

		int 		_Shape;
		float 		_PointSize;
		float 		_Fog;
		float 		_Curvature;
		float 		_Density;
		bool 		_Fill;
		bool 		_Grid;
		bool 		_Hand_Interaction;
		float4x4	_Transform;
		float4 		_Color;

		float 		_Count;
		float4 		_ControlPoint[8];


		//hue saturation value colorspace
		float3 hsv(float h, float s,float v)
		{
			return lerp(float3(1., 1., 1.), clamp((abs(frac(h+float3(3.,2.,1.)/3.)*6.-3.)-1.),0.,1.),s)*v;
		}


		//perlin value noise
		float perlin(float2 uv)
		{
			float2 f	= floor(uv);
			float2 c	= f + 1.;
			
			uv 			= frac(uv);
			uv			= uv * uv * (3.-2. * uv);

			return lerp(lerp(hash_merge(f.x, f.y), hash_merge(f.x, c.y), uv.y), lerp(hash_merge(c.x, f.y), hash_merge(c.x, c.y), uv.y), uv.x);
		}


		//fractal brownian motion (perlin noise fractal)
		float fbm(float2 uv) 
		{
			float n = 0.;
			float f = 1.5;
			float a = .45;
			for (int i = 0; i < 4; i++) 
			{
				n 	+= perlin(uv * f) * a;	 	
				f 	*= 2.; 
				a	*= .5;
			}

			return n;
		}


		//path
		float N_i_1 (in float t, in float i)
		{
			if(t < 2.)
			{
			 	return 1.;
			} 
			return step(i, t) * step(t,i+1.0);
		}

		float N_i_2 (in float t, in float i)
		{
			return
				N_i_1(t, i)       * (t - i) +
				N_i_1(t, i + 1.0) * (i + 2.0 - t);
		}

		float N_i_3 (in float t, in float i)
		{
			return
				N_i_2(t, i)       * (t - i) / 2.0 +
				N_i_2(t, i + 1.0) * (i + 3.0 - t) / 2.0;
		}

		float N_i_4 (in float t, in float i)
		{
			return
				N_i_3(t, i)       * (t - i) / 3.0 +
				N_i_3(t, i + 1.0) * (i + 4.0 - t) / 3.0;
		}


		float3 Path(in float t)
		{						
			return
				_ControlPoint[0].xyz * N_i_4(t, 0.0) +
				_ControlPoint[1].xyz * N_i_4(t, 1.0) +
				_ControlPoint[2].xyz * N_i_4(t, 2.0) +
				_ControlPoint[3].xyz * N_i_4(t, 3.0) +
				_ControlPoint[4].xyz * N_i_4(t, 4.0) +
				_ControlPoint[5].xyz * N_i_4(t, 5.0) +
				_ControlPoint[6].xyz * N_i_4(t, 6.0) +
				_ControlPoint[7].xyz * N_i_4(t, 7.0);   
		}



		float max_component (float3 v) 
		{
		return max(max(v.x, v.y), v.z);
		}


		float3 ceil_max_component (float3 v) 
		{
			float maxima = max_component(v);
			v *= float3(v.x == maxima, v.y == maxima, v.z == maxima);
			return ceil(v);
		}


		float2x2 rmat(float t)
		{
			float c = cos(t);
			float s = sin(t);
			return float2x2(c, s, -s, c);	
		}


		uint deinterlace9(in uint x)
		{		
			x = x & 0x09249249; 
			x = (x | (x >>  2)) & 0x03248649; 
			x = (x | (x >>  2)) & 0x00e181c3; 
			x = (x | (x >>  4)) & 0x000f801f; 
			return x;
		}


		float3 decode9(in uint x)
		{				
			return float3(deinterlace9(x), deinterlace9(x >> 1), deinterlace9(x >> 2));
		}


		float3 zucconi_rainbow(float x) //modified
		{
			x				= max(x, .00000001);
			const float3 cs = float3(3.54541723, 2.86670055, 2.29421995);
			const float3 xs = float3(0.69548916, 0.49416934, 0.28269708);
			const float3 ys = float3(0.02320775, 0.15936245, 0.53520021);

			float3 s		= cs * (x - xs);
			s 				= 1. - s * s;
	
			return clamp(s - ys, float3(0., 0., 0.), float3(1.9, 1.9, 1.9));	
		}

		float map_hand(float3 position)
		{
			return sqrt(Leap_SqrDistToHand(position));
		}

		float4 derive_hand_with_curvature(float3 position , float epsilon)
		{
			float2 offset 	= float2(epsilon, -epsilon);
			float4 simplex 	= float4(0., 0., 0., 0.);
	 
			simplex.x 		= map_hand(position + offset.xyy);
			simplex.y 		= map_hand(position + offset.yyx);
			simplex.z 		= map_hand(position + offset.yxy);
			simplex.w 		= map_hand(position + offset.xxx);
	
			float4 gradient = float4(0., 0., 0., 0.);
			gradient.xyz	= offset.xyy * simplex.x + offset.yyx * simplex.y + offset.yxy * simplex.z + offset.xxx * simplex.w;
			gradient.w		= .25/epsilon*(dot(simplex, float4(1., 1., 1., 1.)) - 4. * map_hand(position));
	
			return gradient;
		}

		ps_input vert (uint id : SV_VertexID, out float4 vertex : SV_POSITION)//, out float psize : PSIZE)
		{
            ps_input o;
			float inverse_count					= 1./float(_Count);
			float interpolant					= id * inverse_count;
			
			float id_hash 						= fmod(float(id) * 65537.,32.) * (1./32.);
			
			float3 position  					= float3(0., 0., 0.);
			float4 color 						= _Color;

			if(SHAPE_SPHERE)
			{

				int count 					= _Count;
				int x 						= knuth_hash((257+65537*id)%count,	count);
				int y 						= knuth_hash(id | x * id, 		count);
				int z 						= knuth_hash(id | y * id, 		count);
				position					= hash(float3(x,y,z) * (1./count)) * 4. - 2.;
	
				float3 normal 				= normalize(position);

				if(GRID)
				{
					position 				= normal;
					float ring 				= 1.-(id%_Density)/_Density;
					float axis 				= floor(interpolant * 3);
					float theta 			= frac(interpolant * 3.) * TAU;
					float2x2 rotation 		= rmat(theta);
					
					position 				= axis == 0 ? float3(mul(float2(ring, 0.), rotation), 0.).xyz : position;
					position 				= axis == 1 ? float3(mul(float2(ring, 0.), rotation), 0.).xzy : position;
					position	 			= axis == 2 ? float3(mul(float2(ring, 0.), rotation), 0.).zxy : position;	
				}
				else
				{					
					position.xy 			= mul(position.xy, rmat(x));
					position.xz 			= mul(position.xz, rmat(y));
					position.yz 			= mul(position.yz, rmat(z));
					position 				*= 1.;

					position 				= interpolant > _Curvature ?  position * _Density : position;
				}
				
			
				float3 bounds 				= abs(normal);
				float3 bounded_position		= clamp(position, -bounds, bounds);
				position 					= bounded_position;
				

				bool offset 				= id % 3; ;

				if(FILL)
				{

				}
				else
				{
					position 				= normalize(position);
				}
				position 					*= .5;
			}
			else if(SHAPE_CUBE)
			{					 
				float density 				= pow(_Count, 1./3.);
				float sparsity 				= 1./density;

				position					= fmod(id * float3(1., sparsity, sparsity * sparsity), density) * sparsity;
				int count 					= _Count;
				int x 						= knuth_hash((257+65537*id)%count,	count);
				int y 						= knuth_hash(id | x * id, 			count);
				int z 						= knuth_hash(id | y * id, 			count);
				position					= hash(float3(x,y,z)/count);
				

				if(GRID)
				{					
					float divisions 			= max(ceil(_Density), 1.);

					position.xz					= id % 3 == 0 ? round(position.xz * divisions)/divisions : position.xz;
					position.xy					= id % 3 == 1 ? round(position.xy * divisions)/divisions : position.xy;
					position.yz					= id % 3 == 2 ? round(position.yz * divisions)/divisions : position.yz;
				}

				if(!FILL)
				{		
					float side 					= floor(interpolant * 6);
					position 					= side == 0 ? float3(0., position.y, position.z) : position;
					position 					= side == 1 ? float3(position.x, 0., position.z) : position;
					position 					= side == 2 ? float3(position.x, position.y, 0.) : position;
					position 					= side == 3 ? float3(1., position.y, position.z) : position;
					position 					= side == 4 ? float3(position.x, 1., position.z) : position;
					position 					= side == 5 ? float3(position.x, position.y, 1.) : position;
				}

				position 						-= .5;
			}
			else if(SHAPE_FFT)
			{
				
				position.x						= float(id)/_Count;
				position.z						= hash(position.x);
				position.xz						= position.zx;
				int index						= floor((position.x) * 1000.);
			
				position.y						= _FFT[index] * _Curvature;
				position.z						-= .5;
				position.z						*= 1.-clamp(position.y*_PointSize,0., 1.);
				color.xyz						= (zucconi_rainbow(position.y * position.y * 4.) + .005) * .75;
				position.xy						-= .5;
			}	
			if(SHAPE_PATH)
			{
				position						= Path(interpolant * 8.);
			}
		

			if(_Hand_Interaction)
			{
				float3 displacement					= position;
				position							*= .65;
				float3 shape_normal					= normalize(position.xyz);
				float4 world_position				= mul(unity_ObjectToWorld, mul(_Transform, float4(position, 1.)));
			
				
				float distance_to_hand				= map_hand(world_position);
				float distance_to_shape_origin		= map_hand(world_position-shape_normal*.5);
				float4 hand_gradient				= derive_hand_with_curvature(world_position, .0125);
				float3 hand_normal					= mul(unity_WorldToObject, float4(normalize(hand_gradient.xyz),1.)).xyz;
				float curvature						= hand_gradient.w;
				float pressure						= pow(log(.65/distance_to_hand) * .25, 4.5);
				float threshold						= .025;
				float falloff						= abs(threshold-distance_to_hand);
			
				
				if(distance_to_hand <= threshold)
				{	
					displacement					-= hand_normal * falloff * 12.8 * distance_to_shape_origin;
					displacement					-= shape_normal * falloff * 6.4;
				}
				
				position							= mul(unity_WorldToObject, float4(displacement.xyz,1.)).xyz;			
				color.xyz							= zucconi_rainbow(pressure) * 1.25;
				color.xyz							+= clamp((.5+color.xyz) * float(abs(curvature) > .6), 0.125, .75);
			}

		
			position 							= mul(_Transform, float4(position, 1.)).xyz;
			float depth 						= distance(_WorldSpaceCameraPos, position);
			float fog 							= 1.0 - smoothstep(512., 1024., depth);

			//float3 direction					= UNITY_MATRIX_IT_MV[2].xyz;

              
		 	// color.xyz 							*= .5+rsqrt(depth);
			// color.w 							= clamp(color.w - depth * (1./45.), 0., 1.);
	
			float size 							= _PointSize;

			vertex								= UnityObjectToClipPos(float4(position, 1.));			
			o.position							= float4(position, 1.);
			float scale 						= 1.;
    		float4 uv							= 1./o.position;// float4((float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5, o.vertex.zw);
		
				
			float3 direction					= UNITY_MATRIX_IT_MV[2].xyz;
			float3 direction_to_position		= direction;//(position-_WorldSpaceCameraPos);
			
			
			//    float4 uvwt         = UNITY_PROJ_COORD(float4(i.uvgrab.x, i.uvgrab.y, i.uvgrab.z, i.uvgrab.w));
			float point_scale					= 64. * (size/vertex.w);
		//	psize 								= min(point_scale, 64.);

			o.center 							= ComputeScreenPos(o.position); 
						
			o.color				 				= color;
			o.color.w							= _Color.w;		
			o.size								= point_scale;
			return o;
        }

		
        float4 point_fragment (ps_input i, UNITY_VPOS_TYPE vpos : VPOS) : SV_Target
  		{		
			float4 center		= i.center;
		
			float2 screen_uv	= vpos.xy/_ScreenParams.xy;
			
			float2 point_uv		= vpos.xy/center.z;
			point_uv			-= (center.xy * _ScreenParams.xy)/center.w/center.z;						

			uint id 			= i.position.w;
 			float2 sprite_uv	= point_uv + .5;
			
			if (length(point_uv) > .5)
			{ 
			//	discard;
			}
         
			return i.color;
        }
 		ENDCG


		Tags 
		{ 
			"Queue"					= "Background"
			"IgnoreProjector" 		= "True" 
			"DisableBatching" 		= "True" 
			"ForceNoShadowCasting" 	= "True"
		}
	
		Blend 		SrcAlpha One
		ZWrite 		Off
		ZTest 		Off
		Cull 		Off
		
        Pass
        {
			Lighting Off

            CGPROGRAM
			#pragma target 4.5
			#pragma fragmentoption ARB_precision_hint_nicest
            #pragma vertex vert
            #pragma fragment point_fragment
            ENDCG 
        }
    }
}