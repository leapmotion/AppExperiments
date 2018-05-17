Shader "Hidden/DistanceCompositeEffect" {
  Properties {
    _MainTex ("Texture", 2D) = "white" {}
  }
  SubShader {
    Cull Off ZWrite Off ZTest Always
    Blend SrcAlpha OneMinusSrcAlpha

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      
      #include "UnityCG.cginc"

      struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
      };

      v2f vert (appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
      }
      
      sampler2D _MainTex;

      //This is just an example composite to visualize the distance
      //field, it's not actually part of the algorithm
      fixed4 frag(v2f i) : SV_Target {
        float4 curr = tex2D(_MainTex, i.uv);

        //Grab the signed distance out of the result
        //Squared distance is stored in the z component
        float dist = sqrt(curr.z);

        //Calculate a nice stripe effect based on distance
        float alpha = smoothstep(0.8, 1, sin(140 * dist));

        float3 color;

        //Calculate color based on inside/outside flag
        //w is non-zero if we are inside
        if (curr.w) {
          color = 0.05 * float3(0, 0, 0.6) / dist;
        }
        else {
          color = 0.1 * float3(0, 1, 0) / dist;
        }

        return float4(color, alpha);
      }
      ENDCG
    }
  }
}
