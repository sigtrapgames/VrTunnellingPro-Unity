Shader "VrTunnellingPro/Cage/Transparent Fogged" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Brightness ("Brightness", Float) = 1
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vertFog
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "CageCommon.cginc"
			
			fixed4 frag (v2fFog i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				col.rgb *= _Brightness;
				fixed3 fogged = col.rgb;
				VRTP_APPLY_FOG(fogged, i);
				col.rgb = fogged;
				return col;
			}
			ENDCG
		}
	}
}
