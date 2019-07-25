#define HASHES

#define FLOAT_HASH_RING	1234.5677490234375			//b -—█———█——█——██—█——█—█——█———█—█—█ //f 1234.5677490234375
#define UINT_HASH_RING	2654435769					//b █——████———██—███—████——██—███——█ //i 2654435769

#ifndef PHI
	#define PHI 		((sqrt(5.)+1.)/2.)
#endif

uint knuth_hash(uint x, uint p) 
{
    const uint ring = UINT_HASH_RING;  			
	return (UINT_HASH_RING * x % p);// clamp((x ^ ring) % p, 0, p);
}


float hash(float x)
{
	x = sin(x) * FLOAT_HASH_RING; 						//b -—█———█——█——██—█——█—█——█———█—█—█ //f 1234.5677490234375
	return frac(frac(x) * x);	
}


float3 hash(float3 v)
{	
	v = frac(v+sin(v.yzx)) * FLOAT_HASH_RING; 			//b -—█———█——█——██—█——█—█——█———█—█—█ //f 1234.5677490234375
	
	return frac(float3(frac(v.x-frac(v.y-v.z)), frac(v.y-frac(v.z-v.x)), frac(v.z-frac(v.x-v.y))) * v);
}


float hash_merge(float x, float y) 
{ 
	return frac(sin(x*FLOAT_HASH_RING)*y-FLOAT_HASH_RING); 
}


float2 hash2(float2 uv) 
{
	uv = sin(uv)*FLOAT_HASH_RING;
	return frac(sin(uv*uv.yx));
}

/*
float3 hash3(float3 uvw) 
{
	float3x3 m = float3x3(108.0, -35.7, -93.9, -75.3, 82.0, 39.7, -67.29, 91.0, -18.7);
	return frac(sin(mul(m, sin(mul(uvw, m))*FLOAT_HASH_RING)));	
}
*/
