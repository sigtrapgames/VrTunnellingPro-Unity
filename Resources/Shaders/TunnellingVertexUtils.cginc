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
	
	// Reproject centre - only required for OpenVR
	#if !defined(SHADER_API_GLES) && !defined(SHADER_API_GLES3)
		float fac = saturate(c.r + c.g);	// Ignore corner verts
		o.vertex.x -= _EyeOffset[unity_StereoEyeIndex*2] * fac;
		o.vertex.y += _EyeOffset[(unity_StereoEyeIndex*2)+1] * fac;
	#endif

	o.sPos = ComputeNonStereoScreenPos(o.vertex);
	o.a = c.b;
	return o;
}