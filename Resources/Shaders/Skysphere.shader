Shader "Hidden/VrTunnellingPro/Skysphere" {
	Properties {
		_Skybox ("Texture", CUBE) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Background" }
		LOD 100
		Cull Off
		ZWrite Off
		Colormask RGB

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float3 uv : TEXCOORD0;
			};

			struct v2f {
				float3 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			samplerCUBE _Skybox;
			fixed3 _Color;
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(float4(v.vertex.xyz, 0));
				o.vertex.z = o.vertex.w;
				o.uv = v.vertex.xyz;
				return o;
			}
			
			fixed3 frag (v2f i) : SV_Target {
				return texCUBE(_Skybox, i.uv).rgb * _Color;
			}
			ENDCG
		}
	}
}
