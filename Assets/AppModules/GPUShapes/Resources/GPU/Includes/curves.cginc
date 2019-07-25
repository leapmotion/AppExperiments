#define CURVES

// #define PI			(4.*atan(1.))
// #define TAU			(8.*atan(1.))
// #define PHI 		((sqrt(5.)+1.)/2.)
// #define R3  		sqrt(3.)
// #define E  			2.718281828 //laziness

// float smooth(float x)
// {
// 	return x*x*(3.0-2.0*x);
// }

float linstep(float a, float b, float x)
{
    return clamp((x-a)/(b-a),0.,1.);
}


float expstep(float a, float b, float x)
{
    return exp(-b*pow(a,x));
}


float impulse(float a, float x )
{
    float h = a*x;
    return h*exp(1.-h);
}


float cubicpulse(float a, float b, float x)
{
    x = abs(x - a);
    if(x > b) return 0.0;
    x /= b;
    return x;//1. - smooth(x);
}


float parabola(float a, float b, float x)
{
	x=2.*clamp((x-a)/(b-a),0.,1.)-1.;
	return 1.-x*x;
}


float gain(float g, float t)
{
	float n = step(.5, t);
	float p = (1. / g - 2.) * (1. - 2. * t);
	return (1. - n) * t / (p + 1.) + n * (p - t) / (p - 1.);
}


float boxstep(float a, float b, float t)
{
	return clamp((t - a)/(b - a), 0., 1.);
}


float logistic(float x, float y)
{	
	return 1./(1. + pow(2.718281828, -y * (x - .5)));
}


float normalized_logistic(float x)
{
	return clamp(logistic(x, 2.718281828 * 5.), 0., 1.);
}


float witch(float x, float y)
{
	return ((y*y*y)*8.)/((x*x)+4.*(y*y));
}


float normalized_witch(float x)
{
	return 1./(x*x+1.);
}


float smoothmin(float a, float b, float x)
{
	return -(log(exp(x*-a)+exp(x*-b))/x);
}


float3 smoothmin(float3 a, float3 b, float x)
{
	return float3(smoothmin(a.x, b.x, x),smoothmin(a.y, b.y, x),smoothmin(a.z, b.z, x));
}


float smoothmax(float a, float b, float x)
{
	return log(exp(x*a)+exp(x*b))/x;
}

float3 smoothmax(float3 a, float3 b, float x)
{
	return float3(smoothmax(a.x, b.x, x),smoothmax(a.y, b.y, x),smoothmax(a.z, b.z, x));
}


// float smoothmin(float a, float b, float r) 
// {
//     float e = max(r-abs(a - b), 0.0) * .01;
//     return min(a, b)-e*e*0.25/r;
// }


// float smoothmax(float a, float b, float r) 
// {
//     float e = max(r-abs(b - a), 0.0) * .01;
//     return max(a, b)+e*e*0.25/r;
// }


float round(float x ) 
{ 
	return floor(x+0.5); 
}


float wave(float x)
{
	bool p  = frac(x*.5)<.5;
	x		= frac(x)*2.;
	x 		*= 2.-x;
	x 		*= 1.-abs(1.-x)*.25;
	return  p ? x : -x;
}


float fmod_lerp(float a, float b, float r)
{    
	a 		= abs( a - b - 1. ) < abs( a + b ) ? a - 1. 	: a;
	a 		= abs( a - b + 1. ) < abs( a - b ) ? a + 1. 	: a;
	r 		= r < .25 && abs(a - b) > r ? r * 1./abs(a - b) : r; //forced convergence for small interpolants
	return frac(lerp(a, b, r));
}


float unit_atan2(in float x, in float y)
{
	return atan2(x, y) * .159154943+.5;
}


float guassian(float x, float k)
{
	return normalize(float2(k, tan(x * atan(1.) * 2.))).x;
}