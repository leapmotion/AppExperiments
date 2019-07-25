#define ULAM

//maps coordinate p.xy to an ulam spiral number
float ulam_spiral(float2 p)
{
	float x = abs(p.x);
	float y	= abs(p.y);
	bool q	= x > y;
	
	x		= max(x, y);
	y		= q ? abs(p.x + p.y) : abs(p.x - p.y);
	y 		= y + 4. * (x * x);
	x 		*= 2.;
	
	if(abs(p.x) <= abs(p.y))
	{
		return p.y > 0. ? y - x : y + x;
	}
	else
	{
	 	return p.x > 0. ? y - x - x : y;
	}
}

//maps the ulam spiral number n to an xy coordinate
float2 inverse_ulam(float n)
{
	float r	= sqrt(floor(n));
	float s = 3. - frac(r) * 4.;	
	r		*= fmod(r, 2.) > 1. ? .5 : -.5;
	
	return s > 1. ? float2(r, 2. * r - r * s) : float2(r * s, r);
}