#include "TunnellingUtils.cginc"

float _FxInner;
float _FxOuter;

struct v2f {
	float4 vertex : SV_POSITION;
	float2 sPos : TEXCOORD0;
	fixed a : TEXCOORD1;
	UNITY_VERTEX_OUTPUT_STEREO
};

v2f vert (float4 v : POSITION, fixed4 c : COLOR) {
	v2f o;
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	o.vertex = v;
	float p = (_FxInner * c.r) + (_FxOuter * c.g);
	o.vertex.xy = lerp(o.vertex.xy, 0, p);
	o.vertex.z = CLIP_NEAR;
	o.vertex.xy *= 2.4;

	// Reproject for asymmetric FOV
	o.vertex.xy += mul(UNITY_MATRIX_P, half4(0,0,-CLIP_NEAR,1));

	o.sPos = ComputeNonStereoScreenPos(o.vertex);
	o.a = c.b;
	return o;
}