Shader "Hidden/VrTunnellingPro/SeparableBlur" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Blur ("Blur Radius", Float) = 1
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always

		CGINCLUDE
		#include "UnityCG.cginc"

		uniform float4 _Offsets[3];

		UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
		float4 _MainTex_ST;
		float2 _MainTex_TexelSize;
		float _Blur;

		struct appdata {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		#define TAPSTRUCT(t,o)			\
		struct tap##t {					\
			float4 vertex : SV_POSITION;\
			float2 uv : TEXCOORD;		\
			float2 uvs[o] : TEXCOORD1;	\
			UNITY_VERTEX_OUTPUT_STEREO	\
		};

		TAPSTRUCT(5,2)
		TAPSTRUCT(9,4)
		TAPSTRUCT(13,6)

		#if !SHADER_API_GLES
			#define UNROLL [unroll]
		#else
			#define UNROLL
		#endif

		#define TAPVERT(t,d,o,s)											\
		tap##t vert##t##d (appdata v){										\
			tap##t output;													\
			UNITY_SETUP_INSTANCE_ID(v);										\
			UNITY_INITIALIZE_OUTPUT(tap##t, output);						\
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);					\
			output.vertex = UnityObjectToClipPos(v.vertex);					\
			output.uv = UnityStereoScreenSpaceUVAdjust(v.uv, _MainTex_ST);	\
			float2 ofst;													\
			UNROLL for (int i=0; i<o; ++i){									\
				ofst = _Offsets[i].s;										\
				output.uvs[(2*i)] = saturate(output.uv + ofst);				\
				output.uvs[(2*i)+1] = saturate(output.uv - ofst);			\
			}																\
			return output;													\
		}

		TAPVERT(5,x,1,xy)
		TAPVERT(5,y,1,zw)
		TAPVERT(9,x,2,xy)
		TAPVERT(9,y,2,zw)
		TAPVERT(13,x,3,xy)
		TAPVERT(13,y,3,zw)

		fixed4 frag5 (tap5 t) : SV_Target {
			fixed3 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, t.uv).rgb * 0.29411764;
			col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, t.uvs[0]).rgb * 0.35294117;
			col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, t.uvs[1]).rgb * 0.35294117;
			return fixed4(col,1);
		}
		fixed4 frag9 (tap9 t) : SV_Target {
			fixed3 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, t.uv).rgb * 0.22702702;
			col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, t.uvs[0]).rgb * 0.31621621;
			col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, t.uvs[1]).rgb * 0.31621621;
			col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, t.uvs[2]).rgb * 0.07027027;
			col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, t.uvs[3]).rgb * 0.07027027;
			return fixed4(col,1);
		}
		fixed4 frag13 (tap13 t) : SV_Target {
			fixed3 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, t.uv).rgb * 0.19648255;
			col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, t.uvs[0]).rgb * 0.29690696;
			col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, t.uvs[1]).rgb * 0.29690696;
			col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, t.uvs[2]).rgb * 0.09447039;
			col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, t.uvs[3]).rgb * 0.09447039;
			col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, t.uvs[4]).rgb * 0.01038136;
			col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, t.uvs[5]).rgb * 0.01038136;
			return fixed4(col,1);
		}
		ENDCG

		Pass {
			CGPROGRAM
			#pragma vertex vert5x
			#pragma fragment frag5
			#pragma multi_compile_instancing
			ENDCG
		}
		Pass {
			CGPROGRAM
			#pragma vertex vert5y
			#pragma fragment frag5
			#pragma multi_compile_instancing
			ENDCG
		}
		Pass {
			CGPROGRAM
			#pragma vertex vert9x
			#pragma fragment frag9
			#pragma multi_compile_instancing
			ENDCG
		}
		Pass {
			CGPROGRAM
			#pragma vertex vert9y
			#pragma fragment frag9
			#pragma multi_compile_instancing
			ENDCG
		}
		Pass {
			CGPROGRAM
			#pragma vertex vert13x
			#pragma fragment frag13
			#pragma multi_compile_instancing
			ENDCG
		}
		Pass {
			CGPROGRAM
			#pragma vertex vert13y
			#pragma fragment frag13
			#pragma multi_compile_instancing
			ENDCG
		}
	}
}
