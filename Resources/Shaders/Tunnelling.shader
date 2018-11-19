Shader "Hidden/VrTunnellingPro/Tunnelling" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (0,0,0,1)
		_Effect ("Effect Strength", Float) = 0
		_Feather ("Feather", Float) = 0.1
		_BkgTex ("Background Texture", 2D) = "white" {}
		_MaskTex ("Mask Texture", 2D) = "white" {}
		_Skybox ("Skybox", Cube) = "" {}
	}

	CGINCLUDE
	#pragma vertex vert
	#pragma multi_compile __ TUNNEL_BKG
	#pragma multi_compile __ TUNNEL_MASK
	#pragma multi_compile __ TUNNEL_CONSTANT
	#pragma multi_compile __ TUNNEL_INVERT_MASK
	#pragma multi_compile __ TUNNEL_SKYBOX
	#pragma multi_compile __ TUNNEL_OVERLAY
	#include "UnityCG.cginc"
	#include "TunnellingUtils.cginc"

	struct appdata {
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};
	struct v2f {			
		float4 vertex : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	v2f vert (appdata v) {
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}
	
	sampler2D _MainTex;
	float4 _MainTex_ST;
	fixed4 _Color;
	sampler2D _BkgTex;
	sampler2D _MaskTex;
	float _FxInner;
	float _FxOuter;
	float _Overlay;

	fixed3 frag (v2f i) : SV_Target {
		float2 uv = UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST);
		fixed3 col = tex2D(_MainTex, uv).rgb;
		float4 coords = screenCoords(i.uv);
		fixed4 bkg;

		// Sample cage/skybox
		#if TUNNEL_BKG
			// Sample cage/blur RT
			// Don't do skybox - cage will include it already if needed
			bkg = tex2D(_BkgTex, uv);
			bkg.rgb *= _Color.rgb;

			// If CAGE_ONLY use rt alpha
			// Otherwise use 1
			#if !TUNNEL_OVERLAY				
				bkg.a = 1;
			#endif
		#elif TUNNEL_SKYBOX
			// Sample skybox cubemap
			bkg.rgb = sampleSkybox(coords);
			bkg.rgb *= _Color.rgb;
			bkg.a = 1;
		#else
			// Just use color
			bkg.rgb = _Color.rgb;
			bkg.a = 1;
		#endif

		// Sample mask
		#if TUNNEL_MASK
			bkg.a *= tex2D(_MaskTex, uv).r;
		#endif

		// Apply color alpha at the end as final factor
		fixed a = bkg.a * _Color.a;

		// Invert mask for portal mode
		#if TUNNEL_INVERT_MASK
			a = 1-a;
		#endif

		// Calculate radial blend factor r
		float radius = length(coords.xy / (_ScreenParams.xy/2)) / 2;
		float fxMin = (1 - _FxInner);
		float fxMax = (1 - _FxOuter);
		float r = max(_Overlay, saturate((radius - fxMin) / (fxMax - fxMin)));

		#if TUNNEL_CONSTANT
			// Add constant windows/portals to vignette
			return lerp(col, bkg.rgb, saturate(r+a));
		#else
			// Blend result based on alpha and vignette
			return lerp(col, bkg.rgb, min(r,a));
		#endif
	}
	fixed4 frag4(v2f i) : SV_Target {
		return fixed4(frag(i), 1);
	}
	ENDCG

	// Regular subshader
	SubShader {
		Cull Off
		ZWrite Off
		ZTest Always
		Colormask RGB

		Pass {
			CGPROGRAM
			#pragma fragment frag
			#pragma exclude_renderers metal
			ENDCG
		}
	}
	// Metal subshader - RGBA
	SubShader {
		Cull Off
		ZWrite Off
		ZTest Always

		Pass {
			CGPROGRAM
			#pragma fragment frag4
			#pragma only_renderers metal
			ENDCG
		}
	}
}