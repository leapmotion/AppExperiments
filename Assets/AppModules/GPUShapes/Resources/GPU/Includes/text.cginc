#define SPRITES

float extract_bit(float n, float e)
{
	return frac(n/exp2(e+1.));
}


float sprite(float n, float2 p)
{
	p = floor(p);
	float bounds = float(all(bool2(p.x < 3., p.y < 5.)) && all(bool2(p.x >= 0., p.y >= 0.)));
	return extract_bit(n, (2. - p.x) + 3. * p.y) * bounds;
}

				
float digit(float n, float2 p)
{
         if(n == 0.) { return sprite(31599., p); }
	else if(n == 1.) { return sprite( 9362., p); }
	else if(n == 2.) { return sprite(29671., p); }
	else if(n == 3.) { return sprite(29391., p); }
	else if(n == 4.) { return sprite(23497., p); }
	else if(n == 5.) { return sprite(31183., p); }
	else if(n == 6.) { return sprite(31215., p); }
	else if(n == 7.) { return sprite(29257., p); }
	else if(n == 8.) { return sprite(31727., p); }
	else             { return sprite(31695., p); }
}

				
float print(float n, float2 position)
{	
	float offset	= 4.;
	float result	= 0.;
	position.x 		-= 4.*(log10(max(n, 1.)));		
	for(int i = 0; i < 8; i++)
	{
		float place = pow(10., float(i));
		
		if(n >= place || i == 0)
		{
			result	 	+= digit(floor(fmod(floor(n/place)+.5, 10.)), position);		
			position.x	+= 4.;
		}
		else
		{
			break;
		}
		
	}
	return floor(result+.5);
}