#ifndef VRTP_CAGE_COMMON
#define VRTP_CAGE_COMMON
#include "UnityCG.cginc"
#include "CageFog.cginc"

struct appdata {
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
};
struct appdata_triplanar {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
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
struct v2fTriplanar {
	float3 cagePos : TEXCOORD0;
	float3 cageNrm : NORMAL;
	float4 vertex : SV_POSITION;
};
struct v2fTriplanarFog {
	float3 cagePos : TEXCOORD0;
	float3 cageNrm : NORMAL;
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
float4x4 _VRTP_WorldToCage;
float4x4 _VRTP_WorldToCageNormal;
float3 _VRTP_CagePos;

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

v2fTriplanar vertTriplanar (appdata_triplanar v){
	v2fTriplanar o;
	o.vertex = UnityObjectToClipPos(v.vertex);

	float4x4 objToCage = mul(_VRTP_WorldToCage, unity_ObjectToWorld);
	o.cagePos = mul(objToCage, v.vertex);

	float3x3 objToCageNrm = (float3x3) mul(_VRTP_WorldToCageNormal, transpose(unity_WorldToObject));
	o.cageNrm = mul(objToCageNrm, v.normal);
	return o;
}

v2fTriplanarFog vertTriplanarFog (appdata_triplanar v){
	v2fTriplanarFog o;
	o.vertex = UnityObjectToClipPos(v.vertex);

	float4x4 objToCage = mul(_VRTP_WorldToCage, unity_ObjectToWorld);
	o.cagePos = mul(objToCage, v.vertex);

	float3x3 objToCageNrm = (float3x3) mul(_VRTP_WorldToCageNormal, transpose(unity_WorldToObject));
	o.cageNrm = mul(objToCageNrm, v.normal);

	VRTP_TRANSFER_FOG(o, o.vertex);
	return o; 
}

inline fixed4 planeColor(sampler2D t, float4 st, float2 uv, float b){
	return tex2D(t, uv * st.xy + st.zw) * b;
}
inline fixed4 triplanar(sampler2D t, float4 t_ST, float3 cagePos, float3 nrm){
	float3 blend = normalize(abs(nrm));
	blend /= dot(blend, 1);
	blend = normalize(pow(blend, 10));

	fixed4 x = planeColor(t, t_ST, cagePos.yz, blend.x);
	fixed4 y = planeColor(t, t_ST, cagePos.xz, blend.y);
	fixed4 z = planeColor(t, t_ST, cagePos.xy, blend.z);

	return x + y + z;
}
inline fixed4 triplanarCounterMotion(sampler2D t, float4 t_ST, float3 cagePos, float3 nrm){
	float3 blend = normalize(abs(nrm));
	blend /= dot(blend, 1);
	blend = normalize(pow(blend, 10));

	cagePos -= _VRTP_CagePos;
	fixed4 x = planeColor(t, t_ST, cagePos.yz, blend.x);
	fixed4 y = planeColor(t, t_ST, cagePos.xz, blend.y);
	fixed4 z = planeColor(t, t_ST, cagePos.xy, blend.z);

	return x + y + z;
}
#endif