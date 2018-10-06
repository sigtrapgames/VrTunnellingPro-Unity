Shader "Hidden/VrTunnellingPro/Skybox" {
	Properties {}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Cull Off
		ZWrite Off
		LOD 100
		Pass {
			Colormask RGB
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "TunnellingUtils.cginc"

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 sPos : TEXCOORD;
			};

			v2f vert (float4 v : POSITION) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v);
				o.sPos = ComputeScreenPos(o.vertex);
				return o;
			}
			fixed3 frag (v2f v) : SV_Target {
				return sampleSkybox(screenCoords(v.sPos));
			}
			ENDCG
		}
	}
}
