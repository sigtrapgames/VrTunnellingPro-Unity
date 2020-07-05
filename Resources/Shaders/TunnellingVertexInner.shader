Shader "Hidden/VrTunnellingPro/TunnellingVertexInner" {
	Properties {
		_Color ("Color", Color) = (0,0,0,1)
		_Effect ("Effect", Range(0,1)) = 0.5
	}
	SubShader {
		// Queue set in script
		Tags { "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100
		ZTest Always
		ZWrite Off
		Cull Off

		Pass {
			Stencil {
				Ref [_VRTP_Stencil_Ref]
				ReadMask [_VRTP_Stencil_Mask]
				Comp [_VRTP_Stencil_Comp]
			}

			CGPROGRAM
			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ TUNNEL_SKYBOX
			
			#include "UnityCG.cginc"
			#include "TunnellingVertexUtils.cginc"

			fixed4 frag (v2f v) : SV_Target {
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(v);
				return fragBody(v, v.a);
			}
			ENDCG
		}
	}
}
