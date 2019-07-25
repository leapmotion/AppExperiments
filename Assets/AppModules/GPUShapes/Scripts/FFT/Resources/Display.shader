Shader "Display"
{
	Properties
	{
		_Scale 		("Scale", 		Range(0, 1)) = .99
		//_Speed 		("Speed", 		Range(0,32)) = 1.
		_Amplitude  ("Amplitude", 	Range(0, 1)) = 1.0
	}

	CGINCLUDE
	#define 	FREQUENCIES 1000


	float		_FFT[FREQUENCIES];
	float		_Frame;
	float		_Scale;
	float		_Speed;
	float		_Amplitude;


	sampler2D	_RenderTarget0;
	sampler2D	_RenderTarget1;
	sampler2D	_RenderTarget2;
	sampler2D	_RenderTarget3;


	struct Target
	{
		float4 buffer0 	: SV_Target0;
		float4 buffer1 	: SV_Target1;
		float4 buffer2 	: SV_Target2;
		float4 buffer3 	: SV_Target3;
	};


	struct appdata
	{
		float4 vertex 	: POSITION;
		float2 uv 		: TEXCOORD0;
	};


	struct v2f
	{
		float2 uv 		: TEXCOORD0;
		float4 vertex 	: SV_POSITION;
	};


	v2f vert (appdata v)
	{
		v2f o;
		o.vertex 	= v.vertex-.5;
		o.uv 		= v.uv;
		return o;
	}


	Target o;	
	Target frag_histogram (v2f i) : SV_Target
	{
		float2 uv 				= i.uv;
		float frequencies 		= float(FREQUENCIES);
		int bin 				= int(_Scale *  frequencies * uv.y);

		float width				= 1024;

		float histogram			= tex2D(_RenderTarget0, uv);

		float position			= floor(i.uv.x * width);
		float frame_position	= fmod(_Frame, width);
		if(position == frame_position)
		{	
			for(int i = 0; i < FREQUENCIES; i++)
			{				
				if(bin == i )
				{
					float next 		= _FFT[i-1];
					float current 	= _FFT[i];
					float prior 	= _FFT[i+1];

					float fft		= prior * .25 + current * .5 + next * .25;

					histogram 		*= 0.;
					histogram 		+= pow(1.+fft, _Amplitude * 512.) - 1.;
				}						
			}
		}

		float4 result 	= float4(histogram, histogram, histogram, 1.);

		float2 offset	= float2(1./width, 0.);

		o.buffer0 = result;
		o.buffer1 = result;//tex2D(_RenderTarget0, i.uv.xy - offset);
		o.buffer2 = result;
		o.buffer3 = result;

		return o;
	}

	float4 frag_blit (v2f i) : SV_Target
	{
		float width				= _ScreenParams.x;
		float2 offset			= float2(1./width, 0.);
		
		 return tex2D(_RenderTarget0, i.uv.xy);
	}
	ENDCG

	SubShader
	{

 		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
//		AlphaTest Off
		Cull Off 
//		ZWrite On 
//		ZTest Always
//		Lighting Off
		Blend Off
//		Blend SrcColor One

		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_nicest
			#pragma vertex vert
			#pragma fragment frag_histogram
			ENDCG
		}
		
		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_nicest
			#pragma vertex vert
			#pragma fragment frag_blit
			ENDCG
		}
	}

}
