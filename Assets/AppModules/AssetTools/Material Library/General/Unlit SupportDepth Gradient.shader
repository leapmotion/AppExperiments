Shader "Unlit/Unlit SupportDepth Gradient"
{
  Properties
  {
    _MainTex("Texture", 2D) = "white" {}
    _Color("Color", Color) = (1, 1, 1, 1)
    _MaskColor("Mask Color", Color) = (0, 0, 0, 0)
    _GradXMap("Gradient X Map", Vector) = (0.1, 1, 0, 1)
    _GradYMap("Gradient Y Map", Vector) = (0.1, 1, 0, 1)
    _GradMask("Gradient Mask (alpha: blend)", 2D) = "white" {}
  }
  SubShader
  {
    Tags{ "RenderType" = "Opaque" }
    LOD 100

    Pass
    {
      CGPROGRAM

      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_fog // make fog work

      #include "UnityCG.cginc"
      #include "Assets/AppModules/TodoUMward/Shader Hand Data/Resources/HandData.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f
      {
        float2 uv : TEXCOORD0;
        UNITY_FOG_COORDS(1)
        float4 vertex : SV_POSITION;
        float4 objVert : TEXCOORD1;
      };

      sampler2D _MainTex;
      float4 _MainTex_ST;
      float4 _Color;
      float4 _MaskColor;
      float4 _GradXMap;
      float4 _GradYMap;
      sampler2D _GradMask;

      v2f vert(appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        UNITY_TRANSFER_FOG(o,o.vertex);
        o.objVert = v.vertex;
        return o;
      }

      fixed4 frag(v2f i) : SV_Target
      {
        // sample the texture
        fixed4 col = tex2D(_MainTex, i.uv);
        // apply fog
        UNITY_APPLY_FOG(i.fogCoord, col);

        // Apply color multiply.
        fixed4 preMaskColor = col * _Color;
        fixed4 fullMaskColor = col * _MaskColor;
    
        // Apply multiplicative gradient mask.
        fixed4 gradColor = Leap_EvalGradientWithMap(i.objVert.x, _GradMask, _GradXMap);
        fixed3 applyColor = gradColor.xyz;
        float xAlpha = gradColor.a;

        float3 oldColor = gradColor.xyz;
        gradColor = Leap_EvalGradientWithMap(i.objVert.y, _GradMask, _GradYMap);
        applyColor = lerp(applyColor.xyz, gradColor.xyz, gradColor.a);
        float yAlpha = gradColor.a;

        applyColor = lerp(preMaskColor, fullMaskColor, saturate(xAlpha + yAlpha));

        col = float4(applyColor.x, applyColor.y, applyColor.z, 1);

        return col;
      }

      ENDCG
    }
  }
  Fallback "Diffuse"
}
