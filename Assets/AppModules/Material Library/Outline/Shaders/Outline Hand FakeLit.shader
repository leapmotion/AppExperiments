﻿Shader "Unlit/Outline Hand FakeLit" {
  Properties {
    _Color   ("Color",         Color) = (1,1,1,1)
    _Outline ("Outline Color", Color) = (1,1,1,1)
    _Width   ("Outline Width", Float) = 0.002
    _LitCoeff ("Lighting Strength", Range(0, 1)) = 0.5
    [MaterialToggle] _isLeftHand("Is Left Hand?", Int) = 0
    [MaterialToggle] _reverseNormals("Reverse normals?", Int) = 0
  }

  CGINCLUDE
  #include "UnityCG.cginc"
  #include "Assets/Plugins/LeapMotion/Core/Resources/LeapCG.cginc"

  //#pragma fragmentoption ARB_precision_hint_fastest
  //#pragma target 3.0

  float4 _Color;
  float4 _Outline;
  float _Width;
  float _LitCoeff;
  int _isLeftHand;
  int _reverseNormals;

  struct appdata {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
  };

  struct v2f {
    float4 vertex : SV_POSITION;
  };
  struct v2f_normal {
    float4 vertex : SV_POSITION;
    float3 normal : NORMAL;
  };

  v2f_normal vert_model(appdata v) {
    v2f_normal o;
    v.vertex = LeapGetLateVertexPos(v.vertex, _isLeftHand); // late-latch support
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.normal = v.normal;
    if (_reverseNormals) o.normal = -o.normal;
    return o;
  }

  v2f vert_outline(appdata v) {
    v2f o;
    v.vertex = LeapGetLateVertexPos(v.vertex, _isLeftHand); // late-latch support
    o.vertex = UnityObjectToClipPos(v.vertex + float4(_Width * v.normal, 0));
    return o;
  }

  fixed4 frag_model(v2f_normal i) : SV_Target {
    fixed4 color = fixed4(1,1,1,1);
    float litAmount = dot(normalize(i.normal.xyz), normalize(float3(1, 1.3, 0)));
    color = litAmount * 0.25 + color;
    color *= _Color;

    color = lerp(_Color, color, _LitCoeff);

    return color;
  }

  fixed4 frag_outline(v2f i) : SV_Target{
    return _Outline;
  }

  ENDCG

	SubShader {
		Tags { "Queue"="Geometry" "RenderType"="Opaque" }
		LOD 80
    ZWrite On

    Pass {
      Cull Back

      CGPROGRAM
      #pragma vertex vert_model
      #pragma fragment frag_model
      ENDCG
    }

    Pass{
      Cull Front

      CGPROGRAM
      #pragma vertex vert_outline
      #pragma fragment frag_outline
      ENDCG
    }
	}
}


//Shader "Unlit/FakeLit Outline" {
//  Properties {
//    _Color   ("Color",         Color) = (1,1,1,1)
//    _Outline ("Outline Color", Color) = (1,1,1,1)
//    _Width   ("Outline Width", Float) = 0.01
//  }
//
//  CGINCLUDE
//  #include "UnityCG.cginc"
//  #pragma fragmentoption ARB_precision_hint_fastest
//  #pragma target 2.0
//
//  float4 _Color;
//  float4 _Outline;
//  float _Width;
//  int _isLeftHand;
//
//  struct appdata {
//    float4 vertex : POSITION;
//    float3 normal : NORMAL;
//  };
//
//  struct v2f_n {
//    float4 vertex : SV_POSITION;
//    float3 normal : NORMAL;
//  };
//
//  struct v2f {
//    float4 vertex : SV_POSITION;
//  };
//
//  v2f_n vert_rev_extrude(appdata v) {
//    v2f_n o;
//    o.vertex = UnityObjectToClipPos(v.vertex + float4(_Width * -1.0 * v.normal, 0));
//    o.normal = v.normal;
//    return o;
//  }
//
//  v2f vert_extrude(appdata v) {
//    v2f o;
//    o.vertex = UnityObjectToClipPos(v.vertex + float4(_Width * 0.0 * v.normal, 0));
//    return o;
//  }
//
//  fixed4 frag(v2f_n i) : SV_Target {
//    fixed4 color = fixed4(1,1,1,1);
//    float litAmount = dot(normalize(i.normal.xyz), normalize(float3(1, 1.3, 0)));
//    color = litAmount * 0.25 + color;
//    color *= _Color;
//    return color;
//  }
//
//  fixed4 frag_outline(v2f i) : SV_Target{
//    return _Outline;
//  }
//
//  ENDCG
//
//	SubShader {
//		Tags { "Queue"="Transparent" "RenderType"="Opaque" }
//		LOD 100
//
//    Pass {
//      Cull Back
//      ColorMask 0
//
//      CGPROGRAM
//      #pragma vertex vert_rev_extrude
//      #pragma fragment frag
//      ENDCG
//    }
//
//    Pass{
//      Cull Front
//
//      CGPROGRAM
//      #pragma vertex vert_extrude
//      #pragma fragment frag_outline
//      ENDCG
//    }
//	}
//}
