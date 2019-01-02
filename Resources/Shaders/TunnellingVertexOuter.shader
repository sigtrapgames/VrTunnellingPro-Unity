Shader "Hidden/VrTunnellingPro/TunnellingVertexOuter" {
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

		CGINCLUDE
		#pragma target 2.0
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"
		#include "TunnellingVertexUtils.cginc"

		fixed3 _Color;
		fixed4 frag (v2f v) : SV_Target {
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(v);
			#if TUNNEL_SKYBOX
				float4 vPos = screenCoords(v.sPos);
				return sampleSkybox(vPos) * _Color;
			#else
				return _Color;
			#endif
		}
		ENDCG

		Pass {
			Name "Unmasked"
			
			CGPROGRAM	
			#pragma multi_compile __ TUNNEL_SKYBOX
			ENDCG
		}
		Pass {
			Name "Masked"
			Stencil {
				Ref [_VRTP_Stencil_Ref]
				ReadMask [_VRTP_Stencil_Mask]
				Comp NotEqual
			}

			CGPROGRAM
			#pragma multi_compile __ TUNNEL_SKYBOX
			ENDCG
		}
	}
}
