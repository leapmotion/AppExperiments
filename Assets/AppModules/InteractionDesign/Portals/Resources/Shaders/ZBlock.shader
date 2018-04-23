Shader "Hidden/ZBlock" {
	Properties { 
		_Mask ("Stencil Mask", Float) = 0
	}
	SubShader {
		Tags { "Queue"="Overlay" "RenderType"="Opaque" }

		Pass {
			ColorMask 0
			Offset -1, -1
			ZTest On
			ZWrite On

			Stencil {
				Ref [_Mask]
				ReadMask 3
				WriteMask 3
				Comp equal
				Pass DecrWrap
		        ZFail DecrWrap
			}
		}
	}
}
