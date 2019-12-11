Shader "Hidden/VrTunnellingPro/Window"{
	Properties {}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100
		ZTest Always
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
