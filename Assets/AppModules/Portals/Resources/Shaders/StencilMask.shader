Shader "Hidden/StencilMask" {
	Properties { 
		_Mask ("Stencil Mask", Float) = 0
	}
	SubShader {
		Tags { "Queue"="Background" "RenderType"="Opaque" }

		Pass {
			ColorMask 0
			Offset -1, -1
			ZWrite Off

			Stencil {
				Ref [_Mask]
		        ReadMask 3
				WriteMask 3
				Comp equal
				Pass IncrWrap
		        ZFail IncrWrap
			}
		}
	}
}
