// SQRT via;
//https://github.com/michaldrobot/ShaderFastLibs/blob/master/ShaderFastMathLib.h
inline float sqrtIEEEIntApproximation(float inX, const int inSqrtConst) {
  int x = asint(inX);
  x = inSqrtConst + (x >> 1);
  return asfloat(x);
}

// Derived from batch testing
#define IEEE_INT_SQRT_CONST_NR0 0x1FBD1DF5

//
// Using 0 Newton Raphson iterations
// Relative error : < 0.7% over full
// Precise format : ~small float
// 1 ALU
//  
inline float fastSqrtNR0(float inX) {
  float  xRcp = sqrtIEEEIntApproximation(inX, IEEE_INT_SQRT_CONST_NR0);
  return xRcp;
}

inline float celLightFromLightAmount(int numShadeSteps, float lightAmount) {
  float rawCelLight = floor(max(lightAmount, 0) * numShadeSteps) / numShadeSteps + 0.2;
  return fastSqrtNR0(rawCelLight) + 0.1;
}

float4 celShadedColor(int numShadeSteps, float lightAmount, float4 color) {
  float celLight = celLightFromLightAmount(numShadeSteps, lightAmount);
  float4 albedo = color;
  float additive = saturate(((max(color, 0.3) * (1 - celLight)) * 0.2) - 0.2);
  float additive2 = celLight * 0.2;
  return albedo * celLight + additive + additive2;
}