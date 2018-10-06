#include "UnityCG.cginc"
#include "CageFog.cginc"

struct appdata {
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
};

struct v2f {
	float2 uv : TEXCOORD0;
	float4 vertex : SV_POSITION;
};

struct v2fFog {
	float2 uv : TEXCOORD0;
	float4 vertex : SV_POSITION;
	VRTP_FOG_COORDS(1)
};

sampler2D _MainTex;
float4 _MainTex_ST;
fixed4 _Color;
float _Brightness;
fixed4 _Fog;
float _FogPow;
float _FogFactor;

v2f vert (appdata v) {
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	return o;
}

v2fFog vertFog (appdata v){
	v2fFog o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	VRTP_TRANSFER_FOG(o, o.vertex);
	o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	return o;
}