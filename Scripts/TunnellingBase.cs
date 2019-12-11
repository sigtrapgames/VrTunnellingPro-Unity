/////////////////////////////////////////////////////////////
//  Copyright 2018-2019 Sigtrap Ltd. All rights reserved.  //
//   www.sigtrapgames.com/VrTunnellingPro @sigtrapgames    //
/////////////////////////////////////////////////////////////

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
		public const string VRTP_VERSION = "1.2.1";
		public const int VRTP_VERSION_MAJOR = 1;
		public const int VRTP_VERSION_MINOR = 2;
		public const int VRTP_VERSION_PATCH = 1;
		public const string VRTP_VERSION_BETA = "";
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
			/// Vignette reveals a 3D sub-scene to provide a static "cage" for static reference.
			/// Cage scene has a transparent background and is composited on top of the main scene.
			/// </summary>
			CAGE_ONLY,
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
		protected const string KEYWORD_BKG = "TUNNEL_BKG";
		protected const string KEYWORD_SKYBOX = "TUNNEL_SKYBOX";
		protected const string KEYWORD_OVERLAY = "TUNNEL_OVERLAY";

		#region Ranges
		public const float FEATHER_MAX = 0.5f;
		public const float MOTION_STRENGTH_MAX = 2f;
		public const float COUNTER_STRENGTH_MAX = 2f;
		public const int FPSDIV_MAX = 60;		
		#endregion

		// Fudge coverage to useful values depending on platform
		#if UNITY_PS4
		const float COVERAGE_MIN = 0.40f;
		#else
		const float COVERAGE_MIN = 0.65f;
		#endif
		#endregion

		#region Shader Properties
		protected int _propFxInner, _propFxOuter;
		private int _propEyeProjection, _propEyeToWorld;
		protected int _globPropFogColor, _globPropFogDensity, _globPropFogPower, _globPropFogBlend;
		
		Matrix4x4[] _eyeToWorld = new Matrix4x4[2];
		Matrix4x4[] _eyeProjection = new Matrix4x4[2];
		#endregion

		#region Fields
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
		[Range(0f, FEATHER_MAX)][Tooltip("Feather around cut-off as fraction of screen.")]
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

		#region Motion Detection
		#region AV
		/// <summary>
		/// Add angular velocity to effect strength? Helps players with regular sim-sickness.
		/// </summary>
		[Tooltip("Add angular velocity to effect strength?\nHelps players with average sim-sickness.")]
		public bool useAngularVelocity = true;
		/// <summary>
		/// Strength contributed to effect by angular velocity.
		/// </summary>
		[Range(0,MOTION_STRENGTH_MAX)]
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
		[Tooltip("Add linear acceleration to effect strength?\nHelps players with above-average sim-sickness.")]
		public bool useAcceleration = false;
		/// <summary>
		/// Strength contributed to effect by linear acceleration.
		/// </summary>
		[Range(0,MOTION_STRENGTH_MAX)]
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
		public float accelerationSmoothing = 0.15f;
		#endregion

		#region LV
		/// <summary>
		/// Add translation velocity to effect strength? Helps players with strong sim-sickness.
		/// </summary>
		[Tooltip("Add linear velocity to effect strength?\nHelps players with strong sim-sickness.")]
		public bool useVelocity = false;
		/// <summary>
		/// Strength contributed to effect by linear velocity.
		/// </summary>
		[Range(0,MOTION_STRENGTH_MAX)]
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
		public float velocitySmoothing = 0.15f;
		#endregion
		#endregion

		#region Motion Effects
		/// <summary>
		/// Transform used for artificial tilt and framerate division.<br />
		/// Should be a child of Motion Target and a parent of the HMD.<br />
		/// Target's transform should not be modified by anything else.
		/// </summary>
		[Tooltip("Transform used for artificial tilt and framerate division.\nShould be below Motion Target in hierarchy.\nTarget's transform should not be modified by anything else.")]
		public Transform motionEffectTarget = null;
		/// <summary>
		/// Indicates whether the <see cref="motionEffectTarget"/> is currently being used for motion effects.
		/// </summary>
		public bool usingMotionEffectTarget {
			get {return useArtificialTilt || framerateDivision > 1;}
		}
		
		#region Counter Motion
		/// <summary>
		/// Counter-motion moves/rotates cage/skybox opposite to <see cref="motionTarget"/> motion.
		/// </summary>
		[Tooltip("Move/rotate cage/skybox opposite to Motion Target motion.")]
		public bool useCounterMotion = false;
		/// <summary>
		/// Scale counter-rotation relative to <see cref="motionTarget"/> motion.
		/// </summary>
		[Range(0f,COUNTER_STRENGTH_MAX)][Tooltip("Scale counter-rotation relative to Motion Target rotation.")]
		public float counterRotationStrength = 1f;
		/// <summary>
		/// Scale counter-rotation on individual axes.<br />
		/// Multiplied by <see cref="counterVelocityStrength"/>.<br />
		/// X: Pitch, Y: Yaw, Z: Roll.
		/// </summary>
		[Tooltip("Scale counter-rotation on individual axes.\nX: Pitch, Y: Yaw, Z: Roll")]
		public Vector3 counterRotationPerAxis = Vector3.up;
		#endregion

		#region Artificial Tilt
		/// <summary>
		/// Artifically tilt the camera when moving to simulate acceleration forces.
		/// </summary>
		[Tooltip("Artifically tilt the camera when moving to simulate acceleration forces.")]
		public bool useArtificialTilt = false;
		/// <summary>
		/// Strength of artificial tilt relative to linear acceleration.<br />
		/// Degrees per (metres per second squared).
		/// </summary>
		[Tooltip("Strength of artificial tilt relative to linear acceleration.")]
		public float tiltStrength = 0f;
		/// <summary>
		/// Maximum artificial tilt in degrees. Zero for no clamp.<br />
		/// X: Pitch, Y: Roll.
		/// </summary>
		[Tooltip("Maximum artificial tilt in degrees. Zero for no clamp.\nX: Pitch, Y: Roll.")]
		public Vector2 tiltMaxAngles = 5*Vector2.one;
		/// <summary>
		/// Smooth out tilt over this time.
		/// </summary>
		[Range(0f, 0.5f)][Tooltip("Smooth out tilt over this time.")]
		public float tiltSmoothTime = 0f;
		#endregion

		#region Framerate Division
		/// <summary>
		/// Divide VR framerate by this number while keeping HMD and cage at device framerate.<br />
		/// Forces stepped motion of <see cref="motionEffectTarget"/> rather than changing rendering.<br />
		/// </summary>
		[Range(1,FPSDIV_MAX)][Tooltip("Divide VR framerate by this number while keeping HMD and cage at device framerate.")]
		public int framerateDivision = 1;
		/// <summary>
		/// Apply <see cref="framerateDivision"/> division to linear motion.
		/// </summary>
		public bool divideTranslation = true;
		/// <summary>
		/// Apply <see cref="framerateDivision"/> division to rotation.
		/// </summary>
		public bool divideRotation = true;
		#endregion
		#endregion

		#region State
		#region Motion
		private Quaternion _lastRot;
		private Vector3 _lastFwd;
		private Vector3 _lastPos;
		private float _lastSpeed;
		private Vector3 _lastVel;
		protected Quaternion _cmRot;

		// Motion smoothing
		private float _avSmoothed, _avSlew, _speedSmoothed, _speedSlew, _accelSmoothed, _accelSlew;

		#region Motion Effects
		private Transform _prevMotionEffectTarget = null;
		
		#region Artificial Tilt
		private bool _prevUseTilt = false;
		private Vector3 _tiltAccelSmoothed, _tiltAccelSlew;
		private Angle3 _tiltInit;
		#endregion

		#region Framerate Division
		private int _lastFpsDivision = 1;
		private Vector3 _mfxTgtLocalPosInit;
		private Quaternion _mfxTgtLocalRotInit;
		#endregion
		#endregion
		#endregion

		#region Misc
		protected Camera _cam;
		protected bool _hasDrawnThisFrame = false;
		#endregion

		#region Debug
		#if UNITY_EDITOR
		#pragma warning disable 0414
		bool _debugForceOn = false;
		float _debugForceValue = 0;
		bool _debugMotionCalculations = false;
		float _debugAv, _debugLa, _debugLv;
		#pragma warning restore 0414
		#endif
		#endregion
		#endregion
		#endregion

		protected virtual void Awake(){
			_cam = GetComponent<Camera>();

			_propFxOuter = Shader.PropertyToID(PROP_OUTER);
			_propFxInner = Shader.PropertyToID(PROP_INNER);

			_propEyeProjection = Shader.PropertyToID(PROP_EYEPRJ);
			_propEyeToWorld = Shader.PropertyToID(PROP_EYEMAT);

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

			if (p.overrideUseCounterMotion) useCounterMotion = p.useCounterMotion;
			if (p.overrideCounterRotationPerAxis) counterRotationPerAxis = p.counterRotationPerAxis;

			if (p.overrideUseArtificialTilt) useArtificialTilt = p.useArtificialTilt;
			
			if (p.overrideFramerateDivision) framerateDivision = p.framerateDivision;
			if (p.overrideDivideTranslation) divideTranslation = p.divideTranslation;
			if (p.overrideDividerotation) divideRotation = p.divideRotation;
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
			_avSmoothed = _avSlew = _speedSmoothed = _speedSlew = _accelSmoothed = _accelSlew = 0;

			ResetCounterMotion();
		}
		/// <summary>
		/// Reset cage and skybox orientation/offset when using counter motion.
		/// </summary>
		public virtual void ResetCounterMotion(){
			_cmRot = Quaternion.identity;
			_lastRot = motionTarget.rotation;
		}
		float RemapRadius(float radius){
			return Mathf.Lerp(COVERAGE_MIN, 1, radius);
		}
		Vector3 _fpsPosition;
		Quaternion _fpsRotation;
		
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

				av = Mathf.InverseLerp(angularVelocityMin, angularVelocityMax, av);
				_avSmoothed = Mathf.SmoothDamp(_avSmoothed, av, ref _avSlew, angularVelocitySmoothing);
				fx += _avSmoothed * angularVelocityStrength;
			}

			// Velocity
			float speed = 0;
			Vector3 dPos = motionTarget.position - _lastPos;
			Vector3 vel = dPos / dT;
			if (useVelocity || useAcceleration){
				speed = vel.magnitude;

				#if UNITY_EDITOR
				_debugLv = speed;
				#endif
			}
			if (useVelocity) {
				_speedSmoothed = Mathf.SmoothDamp(_speedSmoothed, speed, ref _speedSlew, velocitySmoothing);
				float lm = 0;

				// Check for divide-by-zero
				if (!Mathf.Approximately(velocityMax, velocityMin)){
					lm = Mathf.Clamp01((_speedSmoothed - velocityMin) / (velocityMax - velocityMin));
				}
				fx += lm * velocityStrength;
			}

			// Acceleration
			if (useAcceleration) {
				// Use unsmoothed vel to keep smoothing independent
				float accel = Mathf.Abs(speed - _lastSpeed) / dT;

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

			#region Motion Effects
			#region Artificial Tilt
			// Check if target has changed or bool has been toggled
			if ((useArtificialTilt != _prevUseTilt) || (motionEffectTarget != _prevMotionEffectTarget)){
				// Reset smoothing params
				_prevUseTilt = useArtificialTilt;
				_tiltAccelSlew = _tiltAccelSmoothed = Vector3.zero;
				// Ensure previous tilt target gets its rotation reset
				if (_prevMotionEffectTarget != null){
					_prevMotionEffectTarget.localEulerAngles = _tiltInit.eulerAcute;
				}
				// Reset tilt offset
				if (motionEffectTarget != null){
					_tiltInit = new Angle3(motionEffectTarget.localEulerAngles);
					_mfxTgtLocalPosInit = motionEffectTarget.localPosition;
					_mfxTgtLocalRotInit = motionEffectTarget.localRotation;
				} else {
					_tiltInit = Vector3.zero;
				}
				// Cache new target
				_prevMotionEffectTarget = motionEffectTarget;
			}
			// Apply tilt
			if (useArtificialTilt && motionEffectTarget != null){
				Vector3 accTgt = motionTarget.InverseTransformDirection(vel - _lastVel) / dT;
				// Convert to log_e
				for (int i=0; i<3; ++i){
					float acc = accTgt[i];
					float sign = Mathf.Sign(acc);
					acc = Mathf.Abs(acc);
					acc = Mathf.Log(acc + 1);
					accTgt[i] = acc * sign;
				}
				_tiltAccelSmoothed = Vector3.SmoothDamp(_tiltAccelSmoothed, accTgt, ref _tiltAccelSlew, tiltSmoothTime, 1000, dT);

				Vector3 tilt = new Angle3(_tiltAccelSmoothed.z * tiltStrength, 0, _tiltAccelSmoothed.x * tiltStrength).eulerAcute;
				if (tiltMaxAngles.sqrMagnitude > 0){
					tilt = new Vector3(
						Mathf.Clamp(tilt.x, -tiltMaxAngles.x, tiltMaxAngles.x), 0,
						Mathf.Clamp(tilt.z, -tiltMaxAngles.y, tiltMaxAngles.y)
					);
				}
				motionEffectTarget.localEulerAngles = (_tiltInit + tilt).eulerAcute;
			}
			#endregion

			#region Framerate Division
			bool updateFps = false;
			if (framerateDivision > 1 && (divideTranslation || divideRotation)){
				if (Time.frameCount % framerateDivision == 0){
					updateFps = true;
				} else {
					motionEffectTarget.position = _fpsPosition;
					motionEffectTarget.rotation = _fpsRotation;
				}
			}
			// Check for settings change
			if (_lastFpsDivision != framerateDivision){
				updateFps = true;
				_lastFpsDivision = framerateDivision;
			}
			if (updateFps){
				if (divideTranslation){
					motionEffectTarget.localPosition = _mfxTgtLocalPosInit;
					_fpsPosition = motionEffectTarget.position;
				}
				if (divideRotation){
					motionEffectTarget.localRotation = _mfxTgtLocalRotInit;
					_fpsRotation = motionEffectTarget.rotation;
				}
			}
			#endregion

			if (useCounterMotion){
				UpdateCounterMotion(dPos, Quaternion.Inverse(motionTarget.rotation) * _lastRot);
			}
			#endregion
			
			// Cache current motion params for next frame
			_lastFwd = fwd;
			_lastPos = motionTarget.position;
			_lastSpeed = speed;
			_lastVel = vel;
			_lastRot = motionTarget.rotation;

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
		
		public static Vector3 SmoothDampAngle(Vector3 current, Vector3 target, ref Vector3 vel, float smoothTime, float maxSpeed, float dT){
			float vx = vel.x;
			float vy = vel.y;
			float vz = vel.z;
			Vector3 result;
			result.x = Mathf.SmoothDampAngle(current.x, target.x, ref vx, smoothTime, maxSpeed, dT);
			result.y = Mathf.SmoothDampAngle(current.y, target.y, ref vy, smoothTime, maxSpeed, dT);
			result.z = Mathf.SmoothDampAngle(current.z, target.z, ref vz, smoothTime, maxSpeed, dT);
			vel.x = vx;
			vel.y = vy;
			vel.z = vz;
			return result;
		}

		protected virtual void UpdateCounterMotion(Vector3 deltaPos, Quaternion deltaRot){
			if (counterRotationStrength > 0){
				_cmRot = GetCounterRotationDelta(deltaRot) * _cmRot;
			}
		}
		protected Quaternion GetCounterRotationDelta(Quaternion deltaRot){
			Vector3 ea = new Angle3(deltaRot.eulerAngles).eulerAcute;
			ea.x *= counterRotationPerAxis.x;
			ea.y *= counterRotationPerAxis.y;
			ea.z *= counterRotationPerAxis.z;
			ea *= counterRotationStrength;
			return Quaternion.Euler(ea);
		}
		protected void UpdateEyeMatrices(){
			Matrix4x4 local;
			#if UNITY_2017_2_OR_NEWER
			if (UnityEngine.XR.XRSettings.enabled) {
			#else
			if (UnityEngine.VR.VRSettings.enabled) {
			#endif
				local = motionTarget.worldToLocalMatrix;
			} else {
				local = Matrix4x4.identity;
			}

			// Rotate local matrix by counter motion for skybox
			#if UNITY_2017_1_OR_NEWER
			Matrix4x4 r = Matrix4x4.Rotate(_cmRot);
			#else
			Matrix4x4 r = Matrix4x4.TRS(Vector3.zero, _cmRot, Vector3.one);
			#endif

			local = r * local;

			_eyeProjection[0] = _cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
			_eyeProjection[1] = _cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
			_eyeProjection[0] = GL.GetGPUProjectionMatrix(_eyeProjection[0], true).inverse;
			_eyeProjection[1] = GL.GetGPUProjectionMatrix(_eyeProjection[1], true).inverse;

			// Reverse y for D3D, PS4, XB1, Metal
			// Don't reverse on OSX or Android (but do if in-editor and build target is Android)
			#if (!UNITY_STANDALONE_OSX && !UNITY_ANDROID) || UNITY_EDITOR_WIN 
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
		}
		protected virtual void CorrectEyeMatrices(Matrix4x4[] eyePrj, Matrix4x4[] eyeToWorld){}
		protected void ApplyEyeMatrices(Material m){
			m.SetMatrixArray(_propEyeProjection, _eyeProjection);
			m.SetMatrixArray(_propEyeToWorld, _eyeToWorld);
		}
	}
	
	/// <summary>
	/// Expresses a float as an angle.
	/// </summary>
	public struct Angle {
		public enum AngleType {
			/// <summary>
			/// Angle expressed from 0 to 360.
			/// <para />E.g. -10 => 350
			/// <para />E.g. 10 => 10
			/// <para />E.g. 580 => -140
			/// </summary>
			ANGLE,
			/// <summary>
			/// Angle expressed from -180 to +180.
			/// <para />E.g. -10 => -10
			/// <para />E.g. 10 => 10
			/// <para />E.g. 580 => 220
			/// </summary>
			ACUTE,
			/// <summary>
			/// Angle expressed from ±(180 to 360).
			/// <para />E.g. -10 => 350
			/// <para />E.g. 10 => -350
			/// <para />E.g. 580 => 220
			/// </summary>
			REFLEX
		}
		
		float _angle;
		
		#region Accessors
		/// <summary>
		/// Angle expressed from 0 to 360.
		/// <para />E.g. -10 => 350
		/// <para />E.g. 10 => 10
		/// <para />E.g. 580 => -140
		/// </summary>
		public float angle {
			get {return _angle;}
		}
		/// <summary>
		/// Angle expressed from -180 to +180.
		/// <para />E.g. -10 => -10
		/// <para />E.g. 10 => 10
		/// <para />E.g. 580 => 220
		/// </summary>
		public float acute {
			get {return ToAngle(_angle, AngleType.ACUTE);}
		}
		/// <summary>
		/// Angle expressed from ±(180 to 360).
		/// <para />E.g. -10 => 350
		/// <para />E.g. 10 => -350
		/// <para />E.g. 580 => 220
		/// </summary>
		public float reflex {
			get {return ToAngle(_angle, AngleType.REFLEX);}
		}
		#endregion

		#region Ctors
		public Angle(float a){
			_angle = ToAngle(a, AngleType.ANGLE);
		}
		public Angle(Angle a) : this(a._angle){}
		#endregion

		#region Conversion
		/// <summary>
		/// Get angle expressed as an AngleType.
		/// </summary>
		public float ToFloat(AngleType type){
			return ToAngle(_angle, type);
		}
		/// <summary>
		/// Convert a float to an angle expressed as an AngleType.
		/// </summary>
		public static float ToAngle(float a, AngleType type){
			a = Mathf.Repeat(a, 360f);
			switch (type){
				case AngleType.ACUTE:
					if (a > 180f){
						a -= 360f;
					}
					break;
				case AngleType.REFLEX:
					if (a < 180f){
						a -= 360f;
					}
					break;
			}
			return a;
		}
		public override string ToString(){
			return string.Format("{0:0.0}",angle);
		}
		#endregion

		#region Operators
		#region Angle Angle
		public static bool operator <(Angle a, Angle b){
			return a._angle < b._angle;
		}
		public static bool operator >(Angle a, Angle b){
			return a._angle > b._angle;
		}
		public static bool operator <=(Angle a, Angle b){
			return a._angle <= b._angle;
		}
		public static bool operator >=(Angle a, Angle b){
			return a._angle >= b._angle;
		}

		public static Angle operator +(Angle a, Angle b){
			return new Angle(a._angle + b._angle);
		}
		public static Angle operator -(Angle a, Angle b){
			return new Angle(a._angle - b._angle);
		}

		public static bool operator ==(Angle a, Angle b){
			return a._angle == b._angle;
		}
		public static bool operator !=(Angle a, Angle b){
			return !(a == b);
		}
		public override bool Equals(object obj){
			if (obj == null) return false;
			if (obj is Angle || obj is float){
				return (Angle)obj == this;
			}
			return false;
		}
		public override int GetHashCode(){
			return _angle.GetHashCode();
		}
		#endregion

		#region Angle Float
		public static bool operator <(Angle a, float b){
			return a._angle < ToAngle(b, AngleType.ANGLE);
		}
		public static bool operator >(Angle a, float b){
			return a._angle > ToAngle(b, AngleType.ANGLE);
		}
		public static bool operator <=(Angle a, float b){
			return a._angle <= ToAngle(b, AngleType.ANGLE);
		}
		public static bool operator >=(Angle a, float b){
			return a._angle >= ToAngle(b, AngleType.ANGLE);
		}

		public static Angle operator +(Angle a, float b){
			return a + new Angle(b);
		}
		public static Angle operator -(Angle a, float b){
			return a - new Angle(b);
		}
		public static Angle operator *(Angle a, float b){
			return new Angle(a._angle * b);
		}
		public static Angle operator /(Angle a, float b){
			return new Angle(a._angle / b);
		}
		#endregion

		#region Float Angle
		public static bool operator <(float a, Angle b){
			return ToAngle(a, AngleType.ANGLE) < b;
		}
		public static bool operator >(float a, Angle b){
			return ToAngle(a, AngleType.ANGLE) > b;
		}
		public static bool operator <=(float a, Angle b){
			return ToAngle(a, AngleType.ANGLE) <= b;
		}
		public static bool operator >=(float a, Angle b){
			return ToAngle(a, AngleType.ANGLE) >= b;
		}

		public static Angle operator +(float a, Angle b){
			return new Angle(a) + b;
		}
		public static Angle operator -(float a, Angle b){
			return new Angle(a) - b;
		}
		#endregion

		#region Casts
		public static implicit operator Angle(float f){
			return new Angle(f);
		}
		// No implicit cast to float - must specify expression
		#endregion
		#endregion
	}

	public struct Angle3 {
		public Angle x, y, z;

		#region Accessors
		/// <summary>
		/// Angles expressed from 0 to 360.
		/// <para />E.g. -10 => 350
		/// <para />E.g. 10 => 10
		/// <para />E.g. 580 => -140
		/// </summary>
		public Vector3 eulerAngles {
			get {return new Vector3(x.angle, y.angle, z.angle);}
		}
		/// <summary>
		/// Angles expressed from -180 to +180.
		/// <para />E.g. -10 => -10
		/// <para />E.g. 10 => 10
		/// <para />E.g. 580 => 220
		/// </summary>
		public Vector3 eulerAcute {
			get {return new Vector3(x.acute, y.acute, z.acute);}
		}
		/// <summary>
		/// Angles expressed from ±(180 to 360).
		/// <para />E.g. -10 => 350
		/// <para />E.g. 10 => -350
		/// <para />E.g. 580 => 220
		/// </summary>
		public Vector3 eulerReflex {
			get {return new Vector3(x.reflex, y.reflex, z.reflex);}
		}
		
		/// <summary>
		/// Resultant angle. Use Angle3.axis to get the associated axis.
		/// </summary>
		public Angle angle {
			get {
				Vector3 axis;
				Angle a;
				ToAxisAngle(out axis, out a);
				return a;
			}
		}
		/// <summary>
		/// Resultant axis. Use Angle3.angle to get the associated angle.
		/// </summary>
		public Vector3 axis {
			get {
				Vector3 a;
				Angle angle;
				ToAxisAngle(out a, out angle);
				return a;
			}
		}
		#endregion

		#region Ctors
		public Angle3(float x, float y, float z){
			this.x = x;
			this.y = y;
			this.z = z;
		}
		public Angle3(Angle x, Angle y, Angle z) : this(x.angle, y.angle, z.angle){}
		public Angle3(Vector3 euler) : this(euler.x, euler.y, euler.z){}
		public Angle3(Quaternion rotation) : this(rotation.eulerAngles){}
		#endregion

		#region Conversion
		/// <summary>
		/// Convert to axis angle representation.
		/// </summary>
		public void ToAxisAngle(out Vector3 axis, out Angle angle){
			float a;
			Quaternion.Euler(eulerAngles).ToAngleAxis(out a, out axis);
			angle = new Angle(a);
		}
		/// <summary>
		/// Get Euler angles expressed as specified AngleType.
		/// </summary>
		public Vector3 ToEuler(Angle.AngleType type){
			return new Vector3 (x.ToFloat(type), y.ToFloat(type), z.ToFloat(type));
		}
		public override string ToString(){
			return string.Format("({0},{1},{2})", x, y, z);
		}
		#endregion

		#region Operators
		public static Angle3 operator +(Angle3 a, Angle3 b){
			return new Angle3(
				a.x + b.x, a.y + b.y, a.z + b.z
			);
		}
		public static Angle3 operator -(Angle3 a, Angle3 b){
			return new Angle3(
				a.x - b.x, a.y - b.y, a.z - b.z
			);
		}
		public static Angle3 operator *(Angle3 a, float b){
			return new Angle3(
				a.x * b, a.y * b, a.z * b
			);
		}

		public static bool operator ==(Angle3 a, Angle3 b){
			return a.x == b.x && a.y == b.y && a.z == b.z;
		}
		public static bool operator !=(Angle3 a, Angle3 b){
			return !(a==b);
		}
		public override bool Equals(object obj){
			if (obj == null) return false;
			if (obj is Angle3 || obj is Vector3){
				return (Angle3)obj == this;
			}
			return false;
		}
		public override int GetHashCode(){
			return eulerAngles.GetHashCode();
		}

		public static implicit operator Angle3(Vector3 v){
			return new Angle3(v);
		}
		// No implicit cast from Angle3 to Vector3 - must specify expression
		#endregion
	}
}