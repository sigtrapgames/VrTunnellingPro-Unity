using UnityEngine;
using System.Collections;

namespace Sigtrap.VrTunnellingPro {
	/// <summary>
	/// A variant of the Tunnelling effect that draws after opaque geometry but before transparent and UI elements.<br />
	/// This script uses post-processing.
	/// </summary>
	public class TunnellingOpaque : TunnellingImageBase {
		protected override UnityEngine.Rendering.CameraEvent _maskCmdEvt {
			get {return UnityEngine.Rendering.CameraEvent.BeforeImageEffectsOpaque;}
		}

		[ImageEffectOpaque]
		void OnRenderImage(RenderTexture src, RenderTexture dest){
			Draw(src, dest);
		}
	}
}