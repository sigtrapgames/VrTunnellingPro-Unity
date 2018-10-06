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
		[SerializeField]
		TunnellingBase.BackgroundMode _backgroundMode = TunnellingBase.BackgroundMode.COLOR;
		[SerializeField]
		bool _overrideBackgroundMode=true;

		[SerializeField][Range(0,2)]
		int _cageDownsample = 0;
		[SerializeField]
		bool _overrideCageDownsample=true;
		[SerializeField]
		TunnellingBase.MSAA _cageAntiAliasing = TunnellingBase.MSAA.AUTO;
		[SerializeField]
		bool _overrideCageAntiAliasing=true;
		[SerializeField]
		bool _cageUpdateEveryFrame = false;
		[SerializeField]
		bool _overrideCageUpdateEveryFrame = true;

		[SerializeField][Range(0.001f, 0.2f)]
		float _cageFogDensity = 0.05f;
		[SerializeField]
		bool _overrideCageFogDensity = true;
		[SerializeField][Range(1,5)]
		float _cageFogPower = 2;
		[SerializeField]
		bool _overrideCageFogPower = true;
		[SerializeField][Range(0,1)]
		float _cageFogBlend = 1;
		[SerializeField]
		bool _overrideCageFogBlend = true;

		[SerializeField]
		TunnellingBase.MaskMode _maskMode = TunnellingBase.MaskMode.OFF;
		[SerializeField]
		bool _overrideMaskMode = true;

		[SerializeField][Range(0,4)]
		int _blurDownsample = 3;
		[SerializeField]
		bool _overrideBlurDownsample = true;
		[SerializeField][Range(1,5)]
		float _blurDistance = 3;
		[SerializeField]
		bool _overrideBlurDistance = true;
		[SerializeField][Range(1,5)]
		int _blurPasses = 3;
		[SerializeField]
		bool _overrideBlurPasses = true;
		[SerializeField]
		TunnellingImageBase.BlurKernel _blurSamples;
		[SerializeField]
		bool _overrideBlurSamples = true;

		[SerializeField]
		bool _irisZRejection = true;
		[SerializeField]
		bool _overrideIrisZRejection = true;

		public TunnellingBase.BackgroundMode backgroundMode {get {return _backgroundMode;}}
		public bool overrideBackgroundMode {get {return _overrideBackgroundMode || overrideAll;}}

		public int cageDownsample {get {return _cageDownsample;}}
		public bool overrideCageDownsample {get {return _overrideCageDownsample || overrideAll;}}
		public TunnellingBase.MSAA cageAntiAliasing {get {return _cageAntiAliasing;}}
		public bool overrideCageAntiAliasing {get {return _overrideCageAntiAliasing || overrideAll;}}
		public bool cageUpdateEveryFrame {get {return _cageUpdateEveryFrame;}}
		public bool overrideCageUpdateEveryFrame {get {return _overrideCageUpdateEveryFrame || overrideAll;}}

		public float cageFogDensity {get {return _cageFogDensity;}}
		public bool overrideCageFogDensity {get {return _overrideCageFogDensity || overrideAll;}}
		public float cageFogPower {get {return _cageFogPower;}}
		public bool overrideCageFogPower {get {return _overrideCageFogPower || overrideAll;}}
		public float cageFogBlend {get {return _cageFogBlend;}}
		public bool overrideCageFogBlend {get {return _overrideCageFogBlend || overrideAll;}}

		public TunnellingBase.MaskMode maskMode {get {return _maskMode;}}
		public bool overrideMaskMode {get {return _overrideMaskMode || overrideAll;}}

		public int blurDownsample {get {return _blurDownsample;}}
		public bool overrideBlurDownsample {get {return _overrideBlurDownsample || overrideAll;}}
		public float blurDistance {get {return _blurDistance;}}
		public bool overrideBlurDistance {get {return _overrideBlurDistance || overrideAll;}}
		public int blurPasses {get {return _blurPasses;}}
		public bool overrideBlurPasses {get {return _overrideBlurPasses || overrideAll;}}
		public TunnellingImageBase.BlurKernel blurSamples {get {return _blurSamples;}}
		public bool overrideBlurSamples {get {return _overrideBlurSamples || overrideAll;}}

		public bool irisZRejection {get {return _irisZRejection;}}
		public bool overrideIrisZRejection {get {return _overrideIrisZRejection || overrideAll;}}
	}
}