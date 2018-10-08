using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sigtrap.VrTunnellingPro {
	/// <summary>
	/// Preset for <see cref="Tunnelling"/> and <see cref="TunnellingOpaque"/>.<br />
	/// Apply using <see cref="Tunnelling.ApplyPreset"/>.<br />
	/// Create and modify via Unity editor.
	/// </summary>
	[CreateAssetMenu(menuName = "VrTunnellingPro/Tunnelling Preset", order = 0)]
	public class TunnellingPreset : TunnellingPresetBase {
		#region Basic
		[SerializeField][Range(0f,1f)]
		float _effectOverlay = 0f;
		public float effectOverlay {get {return _effectOverlay;}}
		[SerializeField]
		bool _overrideEffectOverlay = false;
		public bool overrideEffectOverlay {get {return _overrideEffectOverlay;}}
		#endregion

		#region Background
		[SerializeField]
		TunnellingBase.BackgroundMode _backgroundMode = TunnellingBase.BackgroundMode.COLOR;
		public TunnellingBase.BackgroundMode backgroundMode {get {return _backgroundMode;}}
		[SerializeField]
		bool _overrideBackgroundMode=true;
		public bool overrideBackgroundMode {get {return _overrideBackgroundMode || overrideAll;}}

		#region Cage
		[SerializeField][Range(0,2)]
		int _cageDownsample = 0;
		public int cageDownsample {get {return _cageDownsample;}}
		[SerializeField]
		bool _overrideCageDownsample=true;
		public bool overrideCageDownsample {get {return _overrideCageDownsample || overrideAll;}}

		[SerializeField]
		TunnellingBase.MSAA _cageAntiAliasing = TunnellingBase.MSAA.AUTO;
		public TunnellingBase.MSAA cageAntiAliasing {get {return _cageAntiAliasing;}}
		[SerializeField]
		bool _overrideCageAntiAliasing=true;
		public bool overrideCageAntiAliasing {get {return _overrideCageAntiAliasing || overrideAll;}}

		[SerializeField]
		bool _cageUpdateEveryFrame = false;
		public bool cageUpdateEveryFrame {get {return _cageUpdateEveryFrame;}}
		[SerializeField]
		bool _overrideCageUpdateEveryFrame = true;
		public bool overrideCageUpdateEveryFrame {get {return _overrideCageUpdateEveryFrame || overrideAll;}}

		#endregion

		#region Cage Fog
		[SerializeField][Range(0.001f, 0.2f)]
		float _cageFogDensity = 0.05f;
		public float cageFogDensity {get {return _cageFogDensity;}}
		[SerializeField]
		bool _overrideCageFogDensity = true;
		public bool overrideCageFogDensity {get {return _overrideCageFogDensity || overrideAll;}}

		[SerializeField][Range(1,5)]
		float _cageFogPower = 2;
		public float cageFogPower {get {return _cageFogPower;}}
		[SerializeField]
		bool _overrideCageFogPower = true;
		public bool overrideCageFogPower {get {return _overrideCageFogPower || overrideAll;}}

		[SerializeField][Range(0,1)]
		float _cageFogBlend = 1;
		public float cageFogBlend {get {return _cageFogBlend;}}
		[SerializeField]
		bool _overrideCageFogBlend = true;
		public bool overrideCageFogBlend {get {return _overrideCageFogBlend || overrideAll;}}
		#endregion
		#endregion

		#region Masking
		[SerializeField]
		TunnellingBase.MaskMode _maskMode = TunnellingBase.MaskMode.OFF;
		public TunnellingBase.MaskMode maskMode {get {return _maskMode;}}
		[SerializeField]
		bool _overrideMaskMode = true;
		public bool overrideMaskMode {get {return _overrideMaskMode || overrideAll;}}
		#endregion

		#region Blur
		[SerializeField][Range(0,4)]
		int _blurDownsample = 3;
		public int blurDownsample {get {return _blurDownsample;}}
		[SerializeField]
		bool _overrideBlurDownsample = true;
		public bool overrideBlurDownsample {get {return _overrideBlurDownsample || overrideAll;}}

		[SerializeField][Range(1,5)]
		float _blurDistance = 3;
		public float blurDistance {get {return _blurDistance;}}
		[SerializeField]
		bool _overrideBlurDistance = true;
		public bool overrideBlurDistance {get {return _overrideBlurDistance || overrideAll;}}

		[SerializeField][Range(1,5)]
		int _blurPasses = 3;
		public int blurPasses {get {return _blurPasses;}}
		[SerializeField]
		bool _overrideBlurPasses = true;
		public bool overrideBlurPasses {get {return _overrideBlurPasses || overrideAll;}}

		[SerializeField]
		TunnellingImageBase.BlurKernel _blurSamples;
		public TunnellingImageBase.BlurKernel blurSamples {get {return _blurSamples;}}
		[SerializeField]
		bool _overrideBlurSamples = true;
		public bool overrideBlurSamples {get {return _overrideBlurSamples || overrideAll;}}
		#endregion

		#region Counter Velocity
		[SerializeField]
		TunnellingImageBase.CounterVelocityMode _counterVelocityMode;
		public TunnellingImageBase.CounterVelocityMode counterVelocityMode {get {return _counterVelocityMode;}}
		[SerializeField]
		bool _overrideCounterVelocityMode = true;
		public bool overrideCounterVelocityMode {get {return _overrideCounterVelocityMode || overrideAll;}}

		[SerializeField]
		float _counterVelocityResetDistance;
		public float counterVelocityResetDistance {get {return _counterVelocityResetDistance;}}
		[SerializeField]
		bool _overrideCounterVelocityResetDistance=true;
		public bool overrideCounterVelocityResetDistance {get {return _overrideCounterVelocityResetDistance || overrideAll;}}
		
		[SerializeField]
		float _counterVelocityResetTime;
		public float counterVelocityResetTime {get {return _counterVelocityResetTime;}}
		[SerializeField]
		bool _overrideCounterVelocityResetTime=true;
		public bool overrideCounterVelocityResetTime {get {return _overrideCounterVelocityResetTime || overrideAll;}}

		[SerializeField][Range(0, TunnellingBase.COUNTER_STRENGTH_MAX)]
		float _counterVelocityStrength = 1f;
		public float counterVelocityStrength {get {return _counterVelocityStrength;}}
		[SerializeField]
		bool _overrideCounterVelocityStrength = true;
		public bool overrideCounterVelocityStrength {get {return _overrideCounterVelocityStrength || overrideAll;}}

		[SerializeField]
		Vector3 _counterVelocityPerAxis = Vector3.one;
		public Vector3 counterVelocityPerAxis {get {return _counterVelocityPerAxis;}}
		[SerializeField]
		bool _overrideCounterVelocityPerAxis = true;
		public bool overrideCounterVelocityPerAxis {get {return _overrideCounterVelocityPerAxis || overrideAll;}}
		#endregion

		#region Optimisation
		[SerializeField]
		bool _irisZRejection = true;
		public bool irisZRejection {get {return _irisZRejection;}}
		[SerializeField]
		bool _overrideIrisZRejection = true;
		public bool overrideIrisZRejection {get {return _overrideIrisZRejection || overrideAll;}}
		#endregion
	}
}