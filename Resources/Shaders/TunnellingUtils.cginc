#ifndef VRTP_UTILS_INCLUDED
#define VRTP_UTILS_INCLUDED

#define CLIP_FAR 1
#if defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) || defined(SHADER_API_VULKAN)	
	#define CLIP_SCREEN CLIP_FAR
#else
	#define CLIP_SCREEN 1-UNITY_NEAR_CLIP_VALUE
#endif

samplerCUBE _Skybox;
float4x4 _EyeProjection[2];
float4x4 _EyeToWorld[2];

struct appdata {
	float4 vertex : POSITION;
#if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
	half2 uv : TEXCOORD0;
#else
	float2 uv : TEXCOORD0;
#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

inline float4 screenCoords(float2 uv){
	float2 c = (uv - 0.5) * 2;
	float4 vPos = mul(_EyeProjection[unity_StereoEyeIndex], float4(c, CLIP_SCREEN, 1));
	vPos.xyz /= vPos.w;
	return vPos;
}
inline fixed3 sampleSkybox(float4 vPos){
	float3 dir = normalize(mul(_EyeToWorld[unity_StereoEyeIndex], vPos).xyz);
	return texCUBE(_Skybox, dir).rgb;
}
#endif