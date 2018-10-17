Shader "Hidden/TraceDistanceFields" {
  
  Properties { }

  CGINCLUDE
  
  struct vert_in {
    float4 vertex : POSITION;
  };

  struct frag_in {
    float4 position : SV_POSITION;
    float3 worldPosition : TEXCOORD0;
  };

  frag_in vert(vert_in v) {
    frag_in frag;
    frag.position = v.position;
    frag.normal = v.normal;
  }

  uniform float4 u_sphereOps[32];

  float sqr_dist_sphere(float3 pos, float4 sphere) {
    float3 s = pos - sphere;
    return s.x * s.x + s.y * s.y + s.z * s.z;
  }

  float4 frag(frag_in f) : COLOR {
    float3 camPos = _WorldSpaceCameraPos.xyz;
    float3 ray = normalize(f.worldPosition - _WorldSpaceCameraPos.xyz);
    float3 step = ray;
    for (int i = 0; i < 32; i++) {
      float sqrDistance = 10000000;
      for (int j = 0; j < 32; j++) {
        float4 sphere = u_sphereOps[j];
        sqrDistance = min(sqrDistance, sqr_dist_sphere(step, sphere));
      }
      float distance = sqrt(sqrDistance);
      if (distance < 0.01) {
        
      }
    }
  }

  ENDCG

  SubShader {
    // No culling or depth
    Cull Off ZWrite Off ZTest Always
    Blend SrcAlpha [_BlendType]

    Pass {

    }

  }

}