#define DERIVATIVES
float map(float3 position);

float4 derive_curvature(float3 position , float epsilon)
{
	float2 offset 	= float2(epsilon, -epsilon);
	float4 simplex 	= float4(0., 0., 0., 0.);
	 
	simplex.x 		= map(position + offset.xyy);
	simplex.y 		= map(position + offset.yyx);
	simplex.z 		= map(position + offset.yxy);
	simplex.w 		= map(position + offset.xxx);
	
	float4 gradient = float4(0., 0., 0., 0.);
	gradient.xyz	= offset.xyy * simplex.x + offset.yyx * simplex.y + offset.yxy * simplex.z + offset.xxx * simplex.w;
	gradient.w		= .25/epsilon*(dot(simplex, float4(1., 1., 1., 1.)) - 4. * map(position));
	
	return gradient;
}


float3 derive_gradient(float3 position , float epsilon)
{
	float2 offset 	= float2(epsilon, -epsilon);
//	float2 offset 	= float2(0., epsilon);
	float4 simplex 	= float4(0., 0., 0., 0.);
	 
	simplex.x 		= map(position + offset.xyy);
	simplex.y 		= map(position + offset.yyx);
	simplex.z 		= map(position + offset.yxy);
	simplex.w 		= map(position + offset.xxx);
	
	float3 gradient = float3(0., 0., 0.);
	gradient.xyz	= offset.xyy * simplex.x + offset.yyx * simplex.y + offset.yxy * simplex.z + offset.xxx * simplex.w;
	
	return gradient;
}
