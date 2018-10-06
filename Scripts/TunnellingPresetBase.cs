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
			[SerializeField][Range(0f,2f)]
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

		[SerializeField][Range(0f,1f)]
		float _effectCoverage = 0.75f;
		[SerializeField]
		bool _overrideEffectCoverage=true;
		[SerializeField]
		Color _effectColor = Color.black;
		[SerializeField]
		bool _overrideEffectColor = true;
		[SerializeField][Range(0f,0.5f)]
		float _effectFeather = 0.1f;
		[SerializeField]
		bool _overrideEffectFeather=true;
		[SerializeField]
		bool _applyColorToBackground;
		[SerializeField]
		bool _overrideApplyColorToBackground = true;
		[SerializeField]
		Cubemap _skybox;
		[SerializeField]
		bool _overrideSkybox = true;

		[SerializeField]
		MotionSettings _angularVelocity;
		[SerializeField]
		bool _overrideAngularVelocity = true;
		[SerializeField]
		MotionSettings _acceleration;
		[SerializeField]
		bool _overrideAcceleration = true;
		[SerializeField]
		MotionSettings _velocity;
		[SerializeField]
		bool _overrideVelocity = true;

		protected bool overrideAll {get {return _overrideAll;}}

		public float effectCoverage {get {return _effectCoverage;}}
		public bool overrideEffectCoverage {get {return _overrideEffectCoverage || overrideAll;}}
		public Color effectColor {get {return _effectColor;}}
		public bool overrideEffectColor {get {return _overrideEffectColor || overrideAll;}}
		public float effectFeather {get {return _effectFeather;}}
		public bool overrideEffectFeather {get {return _overrideEffectFeather || overrideAll;}}
		public bool applyColorToBackground {get {return _applyColorToBackground;}}
		public bool overrideApplyColorToBackground {get {return _overrideApplyColorToBackground || overrideAll;}}
		public Cubemap skybox {get {return _skybox;}}
		public bool overrideSkybox {get {return _overrideSkybox || overrideAll;}}

		public MotionSettings angularVelocity {get {return _angularVelocity;}}
		public bool overrideAngularVelocity {get {return _overrideAngularVelocity || overrideAll;}}
		public MotionSettings acceleration {get {return _acceleration;}}
		public bool overrideAcceleration {get {return _overrideAcceleration || overrideAll;}}
		public MotionSettings velocity {get {return _velocity;}}
		public bool overrideVelocity {get {return _overrideVelocity || overrideAll;}}
	}
}