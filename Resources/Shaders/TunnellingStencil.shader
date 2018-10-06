Shader "Hidden/VrTunnellingPro/TunnellingMobileStencil" {
	SubShader {
		ZWrite Off
		ZTest LEqual
		Offset -1, [_VRTP_Stencil_Bias]
		Colormask 0

		Pass {
			Stencil {
				Ref [_VRTP_Stencil_Ref]
				WriteMask [_VRTP_Stencil_Mask]
				Pass Replace
			}
		}
	}
}