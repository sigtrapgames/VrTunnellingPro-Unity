#ifndef VRTP_CAGE_FOG
#define VRTP_CAGE_FOG

#define VRTP_FOG_COORDS(idx) float2 vrtpFogCoords : TEXCOORD##idx;
#define VRTP_TRANSFER_FOG(o,outpos) o.vrtpFogCoords = (outpos).zw
#define VRTP_APPLY_FOG(col,i) col = applyFog(col, i.vrtpFogCoords)

uniform float _VRTP_Cage_FogBlend;
uniform float _VRTP_Cage_FogPower;
uniform float _VRTP_Cage_FogDensity;
uniform fixed3 _VRTP_Cage_FogColor;

inline fixed3 applyFog(fixed3 col, float2 vrtpFogCoords){
	float power = pow((vrtpFogCoords.x/vrtpFogCoords.y) / _VRTP_Cage_FogDensity, _VRTP_Cage_FogPower);
	float t = exp(-power);
	return lerp(col, _VRTP_Cage_FogColor, t*_VRTP_Cage_FogBlend);
}
#endif