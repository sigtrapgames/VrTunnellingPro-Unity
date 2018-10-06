using UnityEngine;
using System.Collections;

namespace Sigtrap.VrTunnellingPro {
	/// <summary>
	/// The main Tunnelling effect.<br />
	/// This script uses post-processing.
	/// </summary>
	public class Tunnelling : TunnellingImageBase {
		protected override UnityEngine.Rendering.CameraEvent _maskCmdEvt {
			get {return UnityEngine.Rendering.CameraEvent.BeforeImageEffects;}
		}

		void OnRenderImage(RenderTexture src, RenderTexture dest){
			Draw(src, dest);
		}
	}
}