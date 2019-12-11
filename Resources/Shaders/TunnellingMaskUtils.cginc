#ifndef VRTP_MASKUTILS_INCLUDED
#define VRTP_MASKUTILS_INCLUDED
struct appdata {
	float4 vertex : POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct v2f {
	float4 vertex : SV_POSITION;
	uint eye : TEXCOORD;
	//UNITY_VERTEX_OUTPUT_STEREO
};

v2f vert (appdata i) {
	v2f o;
	UNITY_SETUP_INSTANCE_ID(i);
	UNITY_INITIALIZE_OUTPUT(v2f, o);
	//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

#if defined(UNITY_GET_INSTANCE_ID)
	o.eye = UNITY_GET_INSTANCE_ID(i);
#else
	o.eye = 0;
#endif
	
	o.vertex = UnityObjectToClipPos(i.vertex);
	return o;
}
fixed frag (v2f i) : SV_Target {
	//return 1-i.eye;
	return 0;
}
#endif