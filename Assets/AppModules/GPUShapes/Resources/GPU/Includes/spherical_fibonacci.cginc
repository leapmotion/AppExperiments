#define SPHERICAL_FIBONACCI

#ifndef MADOPS
#include	"/Includes/madops.cginc"
#endif


#ifndef DEFINITIONS
#include	"/Includes/definitions.cginc"
#endif


float spherical_fibonacci(float3 p, float n) 
{
    float phi 	= min(atan2(p.y, p.x), PI), cosTheta = p.z;
    
    float k  	= max(2.0, floor( log(n * PI * sqrt(5.0) * (1.0 - cosTheta*cosTheta))/ log(PHI*PHI)));
    float Fk 	= pow(PHI, k)/sqrt(5.0);
    
    float2 F 	= float2( round(Fk), round(Fk * PHI) );

    float2 ka 	= -2.0*F/n;
    float2 kb 	= 2.0*PI*madfrac(F+1.0, PHI-1.0) - 2.0*PI*(PHI-1.0);    
    float2x2 iB = float2x2( ka.y, -ka.x, -kb.y, kb.x );
	iB 			/= (ka.y*kb.x - ka.x*kb.y);

    float2 c 	= floor(mul(iB, float2(phi, cosTheta - (1.0-1.0/n))));
    float d 	= 8.0;
    float j 	= 0.0;
    for(int s=0; s<4; s++) 
    {
        float2 uv				= float2(float(s-2*(s/2)), float(s/2) );
        
        float cosTheta			= dot(ka, uv + c) + (1.0-1.0/n);
        
        cosTheta 				= clamp(cosTheta, -1.0, 1.0)*2.0 - cosTheta;
        float i					= floor(n*0.5 - cosTheta*n*0.5);
        float phi				= 2.0*PI*madfrac(i, PHI-1.0);
        cosTheta				= 1.0 - (2.0*i + 1.0)/n;
        float sinTheta 			= sqrt(1.0 - cosTheta*cosTheta);
        
        float3 q 				= float3( cos(phi)*sinTheta, sin(phi)*sinTheta, cosTheta);
       
		float squaredDistance 	= dot(q-p, q-p);
        if (squaredDistance < d) 
        {
            d = squaredDistance;
            j = i;
        }
    }

    return j;
}


float3 inverse_spherical_fibonacci(float i, float n) 
{
    float phi 		= 2.0*PI*madfrac(i,PHI);
    float zi 		= 1.0 - (2.0*i+1.0)/n;
    float sinTheta 	= sqrt( 1.0 - zi*zi);

    return float3(cos(phi) * sinTheta, sin(phi) * sinTheta, zi);
}



float3 compact_sf_normal(float3 v)
{
	return float3(spherical_fibonacci(v, 256.)/256., 0., 0.);
}


float3 expand_sf_normal(float3 v)
{
	return normalize(inverse_spherical_fibonacci(floor(v.x*256.), 256.));
}