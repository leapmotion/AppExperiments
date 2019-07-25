#define BITOPS


uint set_last_bit(uint x, bool set)
{   
	return set ? x | 0x80000000 : x & 0x3FFFFFFF; 	
}


bool get_last_bit(uint x)
{   
	return x >> 31 == 1;
}


uint binary_gray(uint x)
{
	return (x >> 1) ^ x;
}


uint gray_binary(uint x)
{
	uint mask = x >> 1;
	
	for(int i = 0; i < 32; i++)
	{
		if(mask != 0)
		{
			x	 = x ^ mask;
			mask = mask >> 1;
		}		
	}
	return x;
}


uint pack4(uint x, uint y, uint z, uint w)
{
	return (x << 24) | (y << 16) | (z << 8) | w;
}


float4 unpack4(uint x)
{
	return float4(x >> 24, (x << 8) >> 24, (x << 16) >> 24, (x << 24) >> 24);
}


uint interlace3(uint x, uint i)
{
	// x = binary_gray(x);
	x = (x | (x << 10)) & 0x000003FF;  //b ——————————————————██████████ //i 1023
	x = (x | (x << 10)) & 0x000f801f;  //b ————————█████——————————█████ //i 1015839
	x = (x | (x <<  4)) & 0x00e181c3;  //b ————███————██——————███————██ //i 14778819
	x = (x | (x <<  2)) & 0x03248649;  //b ——██——█——█——█————██——█——█——█ //i 52725321
	x = (x | (x <<  2)) & 0x09249249;  //b █——█——█——█——█——█——█——█——█——█ //i 153391689
 
	return x << (int)i;
}


uint deinterlace3(uint x, uint i)
{		
	// x = binary_gray(x);
	x = (x   >> (int)i) & 0x09249249; 	//b █——█——█——█——█——█——█——█——█——█ //i 153391689
	x = (x | (x >>  2)) & 0x03248649; 	//b ——██——█——█——█————██——█——█——█ //i 52725321
	x = (x | (x >>  2)) & 0x00e181c3;  	//b ————███————██——————███————██ //i 14778819
	x = (x | (x >>  4)) & 0x000f801f;  	//b ————————█████——————————█████ //i 10158390
	x = (x | (x >> 10)) & 0x000003FF;  	//b ——————————————————██████████ //i 1023

	return x;
}


uint extract3(uint x, int i)
{
	//float3 = 24
	return (x >> (24 - 3 * (i + 1))) & 7;
}


uint shift3(uint x, int i)
{
	//float3 = 24
	return x << (24 - 3 * (i + 1));
}


uint encode3(uint3 p)
{
	return interlace3(p.x, 0) | interlace3(p.y, 1) | interlace3(p.z, 2);
}


float3 decode3(uint x)
{ 
	return float3(deinterlace3(x, 0), deinterlace3(x, 1), deinterlace3(x, 2));
}


uint3 dialate3(uint x)
{
	uint3 y = uint3(0, 0, 0);
	y.x 	= x & 0x09249249; 			//b ———█——█——█——█——█——█——█——█——█——█ //d 153391689
	y.y 	= x & 0x12492492; 			//b ——█——█——█——█——█——█——█——█——█——█— //d 306783378
	y.z 	= x & 0x24924924; 			//b —█——█——█——█——█——█——█——█——█——█—— //d 613566756
	return y;
}  


uint translate3(uint x, uint y)
{
	uint3 a = dialate3(x);
	uint3 b = dialate3(y);
	return (((b.x - a.x) & 0x49249249) | ((b.y - a.y) & 0x12492492) | ((b.z - a.z) & 0x24924924)); 
}