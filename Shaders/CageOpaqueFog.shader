Shader "VrTunnellingPro/Cage/Opaque Fogged" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Brightness ("Brightness", Float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry" }
		LOD 100

		Pass {
			CGPROGRAM
			#pragma vertex vertFog
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "CageCommon.cginc"
			
			fixed4 frag (v2fFog i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv) * _Color * _Brightness;
				VRTP_APPLY_FOG(col.rgb, i);
				return col;
			}
			ENDCG
		}
	}
}
