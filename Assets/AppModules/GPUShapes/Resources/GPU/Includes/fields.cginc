#define FIELDS

float sphere(float3 position, float radius)
{
	return length(position)-radius; 
}


float sphere(float2 position, float radius)
{
	return length(position)-radius;
}


float cube(float3 p, float3 s)
{
	float3 d = abs(p) - s;
	return min(max(d.x,max(d.y,d.z)),0.0) + length(max(d,0.0));
}


float cube(float2 position, float2 scale)
{
	float2 vertex 	= abs(position) - scale;
	float2 edge 	= max(vertex, 0.);
	float interior	= max(vertex.x, vertex.y);
	return min(interior, 0.) + length(edge);
}


float torus( float3 p, float2 t )
{
	float2 q = float2(length(p.xz)-t.x, p.y);
	return length(q)-t.y;
}


float torus(float2 position, float2 radius)
{
	return abs(abs(length(position)-radius.x)-radius.y);
}


float cylinder(float3 p, float l, float r)
{
	return max(abs(p.y-l)-l, length(p.xz)-r);
}


float cone(float3 p, float l, float2 r)
{
	float m = 1.-(p.y*.5)/l;
	return max(length(p.xz)-lerp(r.y, r.x, m), abs(p.y-l)-l);
}


float simplex(float2 position, float scale)
{		
	position.y	*= .57735026837;
	
	float3 edge	= float3(0., 0., 0.);
	edge.x		= position.y + position.x;
	edge.y		= position.x - position.y;
	edge.z		= position.y + position.y;
	edge		*= .866025405;
	
	return max(edge.x, max(-edge.y, -edge.z))-scale * .57735026837;
}


float hex( float3 p, float2 h )
{
	float3 q = abs(p);
	return max(q.z-h.y,max(q.x+q.y * .57735026837, q.y * 1.1547)-h.x);
}


float tri( float3 p, float2 h )
{
	float3 q = abs(p);
	return max(q.z - h.y, max(q.x * .866025 + p.y * .5, -p.y) - h.x * .5);
}


float2 project(float2 position, float2 a, float2 b)
{
	float2 q	= b - a;	
	float u 	= dot(position - a, q)/dot(q, q);
	u 			= clamp(u, 0., 1.);

	return lerp(a, b, u);
}


float projection( float3 p, float3 a, float3 b, float r)
{
	float3 pa = p - a;
	float3 ba = b - a;
	float h = clamp(dot(pa, ba) / dot(ba, ba), 0., 1.);
	
	return length(pa - ba * h) - r;
}


float projection(float2 position, float2 a, float2 b)
{
	return distance(position, project(position, a, b));
}


float frame(float3 p, float3 r, float w)
{
	p		= abs(p)-r;
	float x = max(p.x, p.y);
	float y = max(p.x, p.z);
	float z = max(p.y, p.z);
					
	return max(max(max(x, y), z) - w, -min(min(x, y),z));
}

//
//float lattice(float3 position, float scale, float width) 
//{
//	position 	= fmod(position, scale) - scale * .5;
//	position 	= max(-abs(position), -position - scale);
//	float x		= max(position.x, position.y);
//	float y 	= max(position.x, position.z);
//	float z 	= max(position.y, position.z);
//					
//	return max(max(max(x, y), z), -min(min(x, y),z)) - width;
//}


// cube-centered lattice (cubic symmetry), 6 directions
float lattice_cc(float3 p) 
{
    float3 o = p*p;    
    float s = sqrt(o.x+o.y);
    s = min(s, sqrt(o.x+o.z));
    s = min(s, sqrt(o.y+o.z));
    return s;
}


// face-centered lattice (rhombic dodecahedral symmetry), 12 directions
float lattice_fcc(float3 p) 
{
    float3 o = abs(p);
    float3 q = o / 2.0;
    float s = length(float3(o.xy - (q.x + q.y), o.z));
    s = min(s, length(float3(o.xz - (q.x + q.z), o.y)));
    s = min(s, length(float3(o.yz - (q.y + q.z), o.x)));
    return s;
}


// body-centered lattice (octahedral symmetry), 8 directions
float lattice_bcc(float3 p) 
{
    float3 o = abs(p);    
    return length( o - (o.x+o.y+o.z) / 3.0 );
}
