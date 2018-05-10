Shader "Unlit/Inflation Hand" {
  Properties {
    _Color   ("Color", Color) = (1,1,1,1)
    _InflationAmount ("Inflation Amount", Float) = 0.005
    [MaterialToggle] _isLeftHand("Is Left Hand?", Int) = 0
  }
  CGINCLUDE
  #include "UnityCG.cginc"
  #include "Assets/LeapMotion/Core/Resources/LeapCG.cginc"
  //#pragma fragmentoption ARB_precision_hint_fastest
  //#pragma target 3.0
  float4 _Color;
  float _InflationAmount;
  int _isLeftHand;
  struct appdata {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
  };
  struct v2f {
    float4 vertex : SV_POSITION;
  };
  v2f vert_inflation(appdata v) {
    v2f o;
    v.vertex = LeapGetLateVertexPos(v.vertex, _isLeftHand); // late-latch support
    o.vertex = UnityObjectToClipPos(v.vertex + float4(_InflationAmount * v.normal, 0));
    return o;
  }
  fixed4 frag(v2f i) : SV_Target{
    return _Color;
  }
  ENDCG
    SubShader {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
    Pass {
      Cull Back
      CGPROGRAM
      #pragma vertex vert_inflation
      #pragma fragment frag
      ENDCG
    }
    }
}