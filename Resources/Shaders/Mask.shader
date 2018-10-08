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
			#include "UnityCG.cginc"

			float4 vert (float4 v : POSITION) : SV_POSITION {
				return UnityObjectToClipPos(v);
			}
			fixed frag () : SV_Target {
				return 0;
			}
			ENDCG
		}
	}
}
