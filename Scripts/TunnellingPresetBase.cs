using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sigtrap.VrTunnellingPro {
	/// <summary>
	/// Base class for effect presets.
	/// </summary>
	public abstract class TunnellingPresetBase : ScriptableObject {
		//! @cond
		[System.Serializable]
		public class MotionSettings {
			[SerializeField]
			bool _use;
			[SerializeField][Range(0f,TunnellingBase.MOTION_STRENGTH_MAX)]
			float _strength = 1;
			[SerializeField]
			float _min;
			[SerializeField]
			float _max;
			[SerializeField]
			float _smoothing;

			public bool use {get {return _use;}}
			public float strength {get {return _strength;}}
			public float min {get {return _min;}}
			public float max {get {return _max;}}
			public float smoothing {get {return _smoothing;}}
		}
		//! @endcond

		[SerializeField]
		bool _overrideAll = false;
		protected bool overrideAll {get {return _overrideAll;}}

		#region Basics
		[SerializeField][Range(0f,1f)]
		float _effectCoverage = 0.75f;
		public float effectCoverage {get {return _effectCoverage;}}
		[SerializeField]
		bool _overrideEffectCoverage=true;
		public bool overrideEffectCoverage {get {return _overrideEffectCoverage || overrideAll;}}
		[SerializeField]
		Color _effectColor = Color.black;
		public Color effectColor {get {return _effectColor;}}
		[SerializeField]
		bool _overrideEffectColor = true;
		public bool overrideEffectColor {get {return _overrideEffectColor || overrideAll;}}
		[SerializeField][Range(0f,TunnellingBase.FEATHER_MAX)]
		float _effectFeather = 0.1f;
		public float effectFeather {get {return _effectFeather;}}
		[SerializeField]
		bool _overrideEffectFeather=true;
		public bool overrideEffectFeather {get {return _overrideEffectFeather || overrideAll;}}
		[SerializeField]
		bool _applyColorToBackground;
		public bool applyColorToBackground {get {return _applyColorToBackground;}}
		[SerializeField]
		bool _overrideApplyColorToBackground = true;
		public bool overrideApplyColorToBackground {get {return _overrideApplyColorToBackground || overrideAll;}}
		[SerializeField]
		Cubemap _skybox;
		public Cubemap skybox {get {return _skybox;}}
		[SerializeField]
		bool _overrideSkybox = true;
		public bool overrideSkybox {get {return _overrideSkybox || overrideAll;}}
		#endregion

		#region Motion Detection
		[SerializeField]
		MotionSettings _angularVelocity;
		public MotionSettings angularVelocity {get {return _angularVelocity;}}
		[SerializeField]
		bool _overrideAngularVelocity = true;
		public bool overrideAngularVelocity {get {return _overrideAngularVelocity || overrideAll;}}
		[SerializeField]
		MotionSettings _acceleration;
		public MotionSettings acceleration {get {return _acceleration;}}
		[SerializeField]
		bool _overrideAcceleration = true;
		public bool overrideAcceleration {get {return _overrideAcceleration || overrideAll;}}
		[SerializeField]
		MotionSettings _velocity;
		public MotionSettings velocity {get {return _velocity;}}
		[SerializeField]
		bool _overrideVelocity = true;
		public bool overrideVelocity {get {return _overrideVelocity || overrideAll;}}
		#endregion

		#region Motion Effects
		#region Counter Motion
		[SerializeField]
		bool _useCounterMotion = false;
		public bool useCounterMotion {get {return _useCounterMotion;}}
		[SerializeField]
		bool _overrideUseCounterMotion = true;
		public bool overrideUseCounterMotion {get {return _overrideUseCounterMotion || overrideAll;}}

		[SerializeField][Range(0, TunnellingBase.COUNTER_STRENGTH_MAX)]
		float _counterRotationStrength = 1f;
		public float counterRotationStrength {get {return _counterRotationStrength;}}
		[SerializeField]
		bool _overrideCounterRotationStrength = true;
		public bool overrideCounterRotationStrength {get {return _overrideCounterRotationStrength || overrideAll;}}
		[SerializeField]
		Vector3 _counterRotationPerAxis = Vector3.one;
		public Vector3 counterRotationPerAxis {get {return _counterRotationPerAxis;}}
		[SerializeField]
		bool _overrideCounterRotationPerAxis = true;
		public bool overrideCounterRotationPerAxis {get {return _overrideCounterRotationPerAxis || overrideAll;}}
		#endregion

		#region Artificial Tilt
		[SerializeField]
		bool _useArtificialTilt = false;
		public bool useArtificialTilt {get {return _useArtificialTilt;}}
		[SerializeField]
		bool _overrideUseArtificialTilt = true;
		public bool overrideUseArtificialTilt {get {return _overrideUseArtificialTilt || overrideAll;}}
		#endregion

		#region Framerate Division
		[SerializeField][Range(0, TunnellingBase.FPSDIV_MAX)]
		int _framerateDivision = 1;
		public int framerateDivision {get {return _framerateDivision;}}
		[SerializeField]
		bool _overrideFramerateDivision = true;
		public bool overrideFramerateDivision {get {return _overrideFramerateDivision || overrideAll;}}
		[SerializeField]
		bool _divideTranslation = true;
		public bool divideTranslation {get {return _divideTranslation;}}
		[SerializeField]
		bool _overrideDivideTranslation = true;
		public bool overrideDivideTranslation {get {return _overrideDivideTranslation || overrideAll;}}
		[SerializeField]
		bool _divideRotation = true;
		public bool divideRotation {get {return _divideRotation;}}
		[SerializeField]
		bool _overrideDivideRotation = true;
		public bool overrideDividerotation {get {return _overrideDivideRotation || overrideAll;}}
		#endregion
		#endregion

		#region Force Effect
		[SerializeField]
		TunnellingBase.ForceVignetteMode _forceVignetteMode;
		public TunnellingBase.ForceVignetteMode forceVignetteMode {get {return _forceVignetteMode;}}
		[SerializeField]
		bool _overrideForceVignetteMode = true;
		public bool overrideForceVignetteMode {get {return _overrideForceVignetteMode || overrideAll;}}
		[SerializeField][Range(0f,1f)]
		float _forceVignetteValue = 0;
		public float forceVignetteValue {get {return _forceVignetteValue;}}
		[SerializeField]
		bool _overrideForceVignetteValue = true;
		public bool overrideForceVignetteValue {get {return _overrideForceVignetteValue || overrideAll;}}
		#endregion
	}
}