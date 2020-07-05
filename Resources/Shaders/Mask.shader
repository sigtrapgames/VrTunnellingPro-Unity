Shader "Hidden/VrTunnellingPro/Mask"{
	Properties {}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100
		Offset -0.1, -1
		Pass {
			Colormask R
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "TunnellingMaskUtils.cginc"
			ENDCG
		}
	}
}
