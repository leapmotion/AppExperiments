// Allows 3D Text to be occluded by scene geometry.
Shader "Portal/Depth Friendly Font" {
  Properties {
    _PortalMask("Portal Mask", Float) = 0
    _MainTex("Font Texture", 2D) = "white" {}
    _Color("Text Color", Color) = (1,1,1,1)
  }

  SubShader {
    // "IgnoreProjector" = "True"
    Tags{ "Queue" = "Transparent+300"  "RenderType"="Transparent" }
    LOD 200

    Stencil {
      Ref[_PortalMask]
      ReadMask 3
      Comp equal
    }

    //AlphaTest Greater 0.5
    Cull Off
    Lighting Off
    ZWrite On
    Fog { Mode Off }
    Blend SrcAlpha OneMinusSrcAlpha

    Pass {
      Color[_Color]
      SetTexture[_MainTex]  {
        combine primary, texture * primary
      }
    }
  }
}