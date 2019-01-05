using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sigtrap.VrTunnellingPro {
	/// <summary>
	/// Preset for <see cref="TunnellingMobile"/>.<br />
	/// Apply using <see cref="TunnellingMobile.ApplyPreset"/>.<br />
	/// Create and modify via Unity editor.
	/// </summary>
	[CreateAssetMenu(menuName=  "VrTunnellingPro/Tunnelling Mobile Preset", order = 1)]
	public class TunnellingPresetMobile : TunnellingPresetBase {
		[SerializeField]
		bool _drawSkybox;
		[SerializeField]
		bool _overrideDrawSkybox = true;
		[SerializeField]
		bool _drawBeforeTransparent;
		[SerializeField]
		bool _overrideDrawBeforeTransparent = true;
		[SerializeField]
		bool _useMask = false;
		[SerializeField]
		bool _overrideUseMask = true;
		[SerializeField][Range(0,255)]
		int _stencilReference = 1;
		[SerializeField]
		bool _overrideStencilReference = true;
		[SerializeField][Range(0,255)]
		int _stencilMask = 255;
		[SerializeField]
		bool _overrideStencilMask = true;
		[SerializeField][Range(0,10)]
		int _stencilBias = 1;
		[SerializeField]
		bool _overrideStencilBias = true;

		public bool drawSkybox {get {return _drawSkybox;}}
		public bool overrideDrawSkybox {get {return _overrideDrawSkybox || overrideAll;}}
		public bool drawBeforeTransparent {get {return _drawBeforeTransparent;}}
		public bool overrideDrawBeforeTransparent {get {return _overrideDrawBeforeTransparent;}}
		public bool useMask {get {return _useMask;}}
		public bool overrideUseMask {get {return _overrideUseMask;}}
		public int stencilReference {get {return _stencilReference;}}
		public bool overrideStencilReference {get {return _overrideStencilReference;}}
		public int stencilMask {get {return _stencilMask;}}
		public bool overrideStencilMask {get {return _overrideStencilMask;}}
		public int stencilBias {get {return _stencilBias;}}
		public bool overrideStencilBias {get {return _overrideStencilBias;}}
	}
}