Shader "Hidden/VrTunnellingPro/TunnellingVertexZ" {
	Properties {
		_Color ("Color", Color) = (0,0,0,1)
		_Effect ("Effect", Range(0,1)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100
		ZTest Always
		ZWrite On
		Cull Off

		Pass {
			CGPROGRAM
			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "TunnellingVertexUtils.cginc"

			fixed4 frag (v2f v) : SV_Target {
				return 0;
			}
			ENDCG
		}
	}
}
