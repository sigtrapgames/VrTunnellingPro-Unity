///////////////////////////////////////////////////////////////
//     Copyright 2018 Sigtrap Ltd. All rights reserved.      //
//  www.sigtrapgames.com/VrTunnellingPro @sigtrapgames.com   //
///////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sigtrap.VrTunnellingPro {
	/// <summary>
	/// Base class for all Tunnelling effects.<br />
	/// Calculates motion parameters and contains shared settings.
	/// </summary>
	public abstract class TunnellingBase : MonoBehaviour {
		#region Version Info
		//! @cond
		public const string VRTP_VERSION = "1.0.1";
		public const int VRTP_VERSION_MAJOR = 1;
		public const int VRTP_VERSION_MINOR = 0;
		public const int VRTP_VERSION_PATCH = 1;
		//! @endcond
		#endregion

		#region Enums
		/// <summary>
		/// Determines what is drawn over the periphery in the vignetted region.
		/// </summary>
		public enum BackgroundMode {
			/// <summary>
			/// Vignette uses a simple color.
			/// </summary>
			COLOR,
			/// <summary>
			/// Vignette uses a skybox.
			/// </summary>
			SKYBOX,
			/// <summary>
			/// Vignette reveals a 3D sub-scene to provide a static "cage" for static reference.
			/// Cage scene has a solid backgorund color.
			/// </summary>
			CAGE_COLOR,
			/// <summary>
			/// Vignette reveals a 3D sub-scene to provide a static "cage" for static reference.
			/// Cage scene has a skybox background.
			/// </summary>
			CAGE_SKYBOX,
			/// <summary>
			/// Vignette blurs, rather than replaces, the periphery.
			/// </summary>
			BLUR
		}
		/// <summary>
		/// Determines whether and how objects can mask the vignette.
		/// </summary>
		public enum MaskMode {
			/// <summary>
			/// No masking used.
			/// </summary>
			OFF, 
			/// <summary>
			/// Vignette is excluded by mask objects.
			/// </summary>
			MASK, 
			/// <summary>
			/// Background is always shown except through mask objects.
			/// </summary>
			WINDOW, 
			/// <summary>
			/// Background is always shown but only through mask objects - inverted WINDOW mode.
			/// </summary>
			PORTAL
		}
		/// <summary>
		/// Antialiasing modes for cage RenderTexture.
		/// </summary>
		public enum MSAA {
			/// <summary>
			/// Take MSAA setting from quality settings.
			/// </summary>
			AUTO,
			/// <summary>
			/// Don't use MSAA.
			/// </summary>
			OFF,
			/// <summary>
			/// 2x MSAA
			/// </summary>
			X2,
			/// <summary>
			/// 4x MSAA
			/// </summary>
			X4,
			/// <summary>
			/// 8x MSAA
			/// </summary>
			X8
		}
		#endregion

		#region Consts
		/// <summary>
		/// Global shader parameter controlling fog color in shaders using CageFog.cginc.
		/// </summary>
		public const string GLOBAL_PROP_FOGCOLOR = "_VRTP_Cage_FogColor";
		/// <summary>
		/// Global shader parameter controlling fog density in shaders using CageFog.cginc.
		/// </summary>
		public const string GLOBAL_PROP_FOGDENSITY = "_VRTP_Cage_FogDensity";
		/// <summary>
		/// Global shader parameter controlling fog falloff in shaders using CageFog.cginc.
		/// </summary>
		public const string GLOBAL_PROP_FOGPOWER = "_VRTP_Cage_FogPower";
		/// <summary>
		/// Global shader parameter controlling fog strength in shaders using CageFog.cginc.
		/// </summary>
		public const string GLOBAL_PROP_FOGBLEND = "_VRTP_Cage_FogBlend";

		protected const string PATH_SHADERS = "Hidden/VrTunnellingPro/";
		protected const string PATH_MESHES = "Meshes/";
		protected const string PATH_IRISMESH = "Iris";
		protected const string PROP_OUTER = "_FxOuter";
		protected const string PROP_INNER = "_FxInner";
		protected const string PROP_COLOR = "_Color";
		protected const string PROP_SKYBOX = "_Skybox";
		protected const string PROP_EYEPRJ = "_EyeProjection";
		protected const string PROP_EYEMAT = "_EyeToWorld";
		protected const string PROP_EYEOFF = "_EyeOffset";
		protected const string KEYWORD_BKG = "TUNNEL_BKG";
		protected const string KEYWORD_SKYBOX = "TUNNEL_SKYBOX";

		#if UNITY_PS4
		const float COVERAGE_MIN = 0.40f;
		#else
		const float COVERAGE_MIN = 0.65f;
		#endif
		#endregion

		#region Fields
		#region Shared Settings
		/// <summary>
		/// Motion calculated using this Transform. Generally shouldn't use HMD.
		/// </summary>
		[Tooltip("Motion calculated using this Transform.\n> Generally shouldn't use HMD")]
		public Transform motionTarget;

		#region Effect
		/// <summary>
		/// Colour of vignette. Alpha is blend factor.<br />
		/// If <see cref="applyColorToBackground"/> true, will affect background of advanced modes.
		/// </summary>
		[Tooltip("Colour of vignette.\n> Alpha is blend factor.")]
		public Color effectColor = Color.black;
		/// <summary>
		/// Maximum screen coverage.
		/// </summary>
		[Range(0f,1f)][Tooltip("Maximum screen coverage.")]
		public float effectCoverage = 0.75f;
		/// <summary>
		/// Feather around cut-off as fraction of screen.
		/// </summary>
		[Range(0f, 0.5f)][Tooltip("Feather around cut-off as fraction of screen.")]
		public float effectFeather = 0.1f;
		/// <summary>
		/// Skybox to apply to vignette.<br />
		/// Requires skybox mode to be enabled (varies depending upon effect).
		/// </summary>
		public Cubemap effectSkybox;
		/// <summary>
		/// Depending upon effect mode, causes <see cref="effectColor"/> to affect background.<br />
		/// Basic vignette mode always uses <see cref="effectColor"/>.
		/// </summary>
		public bool applyColorToBackground = false;
		#endregion

		#region AV
		/// <summary>
		/// Add angular velocity to effect strength? Helps players with regular sim-sickness.
		/// </summary>
		public bool useAngularVelocity = true;
		/// <summary>
		/// Strength contributed to effect by angular velocity.
		/// </summary>
		[Range(0,2f)]
		public float angularVelocityStrength = 1;
		/// <summary>
		/// No effect contribution below this angular velocity. Degrees per second
		/// </summary>
		public float angularVelocityMin = 0;
		/// <summary>
		/// Clamp effect contribution above this angular velocity. Degrees per second
		/// </summary>
		public float angularVelocityMax = 180;
		/// <summary>
		/// Smoothing time for angular velocity calculation. 0 for no smoothing
		/// </summary>
		public float angularVelocitySmoothing = 0.15f;
		#endregion

		#region LA
		/// <summary>
		/// Add linear acceleration to effect strength? Helps players with moderate sim-sickness.
		/// </summary>
		public bool useAcceleration = false;
		/// <summary>
		/// Strength contributed to effect by linear acceleration.
		/// </summary>
		[Range(0,2f)]
		public float accelerationStrength = 1;
		/// <summary>
		/// No effect contribution below this acceleration. Metres per second squared
		/// </summary>
		public float accelerationMin;
		/// <summary>
		/// Clamp effect contribution above this acceleration. Metres per second squared
		/// </summary>
		public float accelerationMax;
		/// <summary>
		/// Smoothing time for acceleration calculation. 0 for no smoothing
		/// </summary>
		public float accelerationSmoothing=0.15f;
		#endregion

		#region LV
		/// <summary>
		/// Add translation velocity to effect strength? Helps players with strong sim-sickness.
		/// </summary>
		public bool useVelocity = false;
		/// <summary>
		/// Strength contributed to effect by linear velocity.
		/// </summary>
		[Range(0,2f)]
		public float velocityStrength = 1;
		/// <summary>
		/// No effect contribution below this velocity. Metres per second
		/// </summary>
		public float velocityMin;
		/// <summary>
		/// Clamp effect contribution above this velocity. Metres per second
		/// </summary>
		public float velocityMax;
		/// <summary>
		/// Smoothing time for velocity calculation. 0 for no smoothing
		/// </summary>
		public float velocitySmoothing=0.15f;
		#endregion
		#endregion

		#region Motion
		private Vector3 _lastFwd;
		private Vector3 _lastPos;
		private float _lastSpeed;
		#endregion

		#region Smoothing
		private float _avSmoothed;
		private float _avSlew;
		private float _velSmoothed;
		private float _velSlew;
		private float _accelSmoothed;
		private float _accelSlew;
		#endregion

		#region Shader Properties
		protected int _propFxInner, _propFxOuter;
		private int _propEyeProjection, _propEyeToWorld, _propEyeOffset;
		protected int _globPropFogColor, _globPropFogDensity, _globPropFogPower, _globPropFogBlend;
		Matrix4x4[] _eyeToWorld = new Matrix4x4[2];
		Matrix4x4[] _eyeProjection = new Matrix4x4[2];
		Vector4 _eyeOffset = new Vector4();
		#endregion

		#region Misc
		protected Camera _cam;
		protected bool _hasDrawnThisFrame = false;
		#endregion
		#endregion

		#if UNITY_EDITOR
		#pragma warning disable 0414
		bool _debugForceOn = false;
		float _debugForceValue = 0;

		bool _debugMotionCalculations = false;
		float _debugAv, _debugLa, _debugLv;
		#pragma warning restore 0414
		#endif

		protected virtual void Awake(){
			_cam = GetComponent<Camera>();

			_propFxOuter = Shader.PropertyToID(PROP_OUTER);
			_propFxInner = Shader.PropertyToID(PROP_INNER);

			_propEyeProjection = Shader.PropertyToID(PROP_EYEPRJ);
			_propEyeToWorld = Shader.PropertyToID(PROP_EYEMAT);
			_propEyeOffset = Shader.PropertyToID(PROP_EYEOFF);

			_globPropFogColor = Shader.PropertyToID(GLOBAL_PROP_FOGCOLOR);
			_globPropFogDensity = Shader.PropertyToID(GLOBAL_PROP_FOGDENSITY);
			_globPropFogPower = Shader.PropertyToID(GLOBAL_PROP_FOGPOWER);
			_globPropFogBlend = Shader.PropertyToID(GLOBAL_PROP_FOGBLEND);
		}
		protected virtual void OnEnable(){
			// Prevent effect thinking we've instantly teleported from 0,0,0
			ResetMotion();
		}

		protected void ApplyPresetBase(TunnellingPresetBase p){
			if (p.overrideEffectCoverage) effectCoverage = p.effectCoverage;
			if (p.overrideEffectColor) effectColor = p.effectColor;
			if (p.overrideEffectFeather) effectFeather = p.effectFeather;
			if (p.overrideApplyColorToBackground) applyColorToBackground = p.applyColorToBackground;
			if (p.overrideSkybox) effectSkybox = p.skybox;

			if (p.overrideAngularVelocity){
				useAngularVelocity = p.angularVelocity.use;
				angularVelocityStrength = p.angularVelocity.strength;
				angularVelocityMin = p.angularVelocity.min;
				angularVelocityMax = p.angularVelocity.max;
				angularVelocitySmoothing = p.angularVelocity.smoothing;
			}

			if (p.overrideAcceleration){
				useAcceleration = p.acceleration.use;
				accelerationStrength = p.acceleration.strength;
				accelerationMin = p.acceleration.min;
				accelerationMax = p.acceleration.max;
				accelerationSmoothing = p.acceleration.smoothing;
			}

			if (p.overrideVelocity){
				useVelocity = p.velocity.use;
				velocityStrength = p.velocity.strength;
				velocityMin = p.velocity.min;
				velocityMax = p.velocity.max;
				velocitySmoothing = p.velocity.smoothing;
			}
		}

		protected void FillMaskBuffer(UnityEngine.Rendering.CommandBuffer cb, List<Renderer> rs, Material m){
			for (int i=0; i<rs.Count; ++i){
				Renderer r = rs[i];
				if (r is MeshRenderer) {
					MeshFilter mf = r.GetComponent<MeshFilter>();
					if (!mf) continue;
					int sms = mf.sharedMesh.subMeshCount;
					for (int j = 0; j < sms; ++j) {
						cb.DrawRenderer(r, m, j);
					}
				} else {
					cb.DrawRenderer(r, m);
				}
			}
		}

		protected void ResetMotion(){
			_lastFwd = motionTarget.forward;
			_lastPos = motionTarget.position;
			_lastSpeed = 0;
			_avSmoothed = _avSlew = _velSmoothed = _velSlew = _accelSmoothed = _accelSlew = 0;
		}
		float RemapRadius(float radius){
			return Mathf.Lerp(COVERAGE_MIN, 1, radius);
		}
		protected float CalculateMotion(float dT){
			dT = Mathf.Max(dT, 0.000001f);
			float fx = 0;	// Total effect strength from all motion types

			#if UNITY_EDITOR	// Debug motion calculations
			// Cache current settings
			bool wasUsingAv = useAngularVelocity;
			bool wasUsingLa = useAcceleration;
			bool wasUsingLv = useVelocity;
			float prevAvStr = angularVelocityStrength;
			float prevLaStr = accelerationStrength;
			float prevLvStr = velocityStrength;

			// Turn on all motion calculations
			if (_debugMotionCalculations){
				// Disable contributions of unused motion types
				if (!useAngularVelocity) angularVelocityStrength = 0;
				if (!useAcceleration) accelerationStrength = 0;
				if (!useVelocity) velocityStrength = 0;	
			
				useAngularVelocity = useAcceleration = useVelocity = true;
			}
			#endif

			// Rotation
			Vector3 fwd = motionTarget.forward;
			if (useAngularVelocity) {
				float av = Vector3.Angle(_lastFwd, fwd) / dT;

				#if UNITY_EDITOR
				_debugAv = av;
				#endif

				// Check for divide-by-zero
				if (Mathf.Approximately(angularVelocityMax, angularVelocityMin)) {
					av = 0;
				} else {
					av = (av - angularVelocityMin) / (angularVelocityMax - angularVelocityMin);
				}
				_avSmoothed = Mathf.SmoothDamp(_avSmoothed, av, ref _avSlew, angularVelocitySmoothing);
				fx += _avSmoothed * angularVelocityStrength;
			}

			// Velocity
			float vel = 0;
			if (useVelocity || useAcceleration){
				vel = Vector3.Distance(motionTarget.position, _lastPos) / dT;

				#if UNITY_EDITOR
				_debugLv = vel;
				#endif
			}
			if (useVelocity) {
				_velSmoothed = Mathf.SmoothDamp(_velSmoothed, vel, ref _velSlew, velocitySmoothing);
				float lm = 0;

				// Check for divide-by-zero
				if (!Mathf.Approximately(velocityMax, velocityMin)){
					lm = Mathf.Clamp01((_velSmoothed - velocityMin) / (velocityMax - velocityMin));
				}
				fx += lm * velocityStrength;
			}

			// Acceleration
			if (useAcceleration) {
				float accel = Mathf.Abs(vel - _lastSpeed) / dT;	// Use unsmoothed vel to keep smoothing independent

				#if UNITY_EDITOR
				_debugLa = accel;
				#endif

				// Check for divide-by-zero
				if (!Mathf.Approximately(accelerationMax, accelerationMin)) {
					accel = Mathf.Clamp01((accel - accelerationMin) / (accelerationMax - accelerationMin));
				}
				_accelSmoothed = Mathf.SmoothDamp(_accelSmoothed, accel, ref _accelSlew, accelerationSmoothing);
				fx += _accelSmoothed * accelerationStrength;
			};

			// Clamp and scale final effect strength
			fx = RemapRadius(fx) * RemapRadius(effectCoverage);

			// Cache current motion params for next frame
			_lastFwd = fwd;
			_lastPos = motionTarget.position;
			_lastSpeed = vel;

			#if UNITY_EDITOR
			// Restore motion settings
			if (_debugMotionCalculations){
				useAngularVelocity = wasUsingAv;
				useAcceleration = wasUsingLa;
				useVelocity = wasUsingLv;

				angularVelocityStrength = prevAvStr;
				accelerationStrength = prevLaStr;
				velocityStrength = prevLvStr;
			}

			// Use forced value
			if (_debugForceOn){
				return Mathf.Clamp01(RemapRadius(_debugForceValue) * RemapRadius(effectCoverage));
			}
			#endif

			return Mathf.Clamp01(fx);
		}
		protected void UpdateEyeMatrices(){
			Matrix4x4 local;
			#if UNITY_2017_2_OR_NEWER
			if (UnityEngine.XR.XRSettings.enabled) {
			#else
			if (UnityEngine.VR.VRSettings.enabled) {
			#endif
				local = _cam.transform.parent.worldToLocalMatrix;
			} else {
				local = Matrix4x4.identity;
			}

			_eyeProjection[0] = _cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
			_eyeProjection[1] = _cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
			_eyeProjection[0] = GL.GetGPUProjectionMatrix(_eyeProjection[0], true).inverse;
			_eyeProjection[1] = GL.GetGPUProjectionMatrix(_eyeProjection[1], true).inverse;

			#if !UNITY_ANDROID && !UNITY_STANDALONE_OSX
			// Reverse y for D3D, PS4, XB1, Metal
			var api = SystemInfo.graphicsDeviceType;
			if (
				api != UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 &&
				api != UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2 &&
				api != UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore &&
				api != UnityEngine.Rendering.GraphicsDeviceType.Vulkan
			){
				_eyeProjection[0][1, 1] *= -1f;
				_eyeProjection[1][1, 1] *= -1f;
			}
			#endif
			
			CorrectEyeMatrices(_eyeProjection, _eyeToWorld);
			
			_eyeToWorld[0] = _cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
			_eyeToWorld[1] = _cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);

			_eyeToWorld[0] = local * _eyeToWorld[0].inverse;
			_eyeToWorld[1] = local * _eyeToWorld[1].inverse;

			_eyeOffset[0] = _eyeProjection[0].m03;
			_eyeOffset[1] = _eyeProjection[0].m13;
			_eyeOffset[2] = _eyeProjection[1].m03;
			_eyeOffset[3] = _eyeProjection[1].m13;
		}
		protected virtual void CorrectEyeMatrices(Matrix4x4[] eyePrj, Matrix4x4[] eyeToWorld){}
		protected void ApplyEyeMatrices(Material m){
			m.SetMatrixArray(_propEyeProjection, _eyeProjection);
			m.SetMatrixArray(_propEyeToWorld, _eyeToWorld);
			m.SetVector(_propEyeOffset, _eyeOffset);
		}
	}
}