using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sigtrap.VrTunnellingPro {
	/// <summary>
	/// Mobile-friendly tunnelling effect for Universal Render Pipeline.<br />
	/// This script does not use post-processing. Limited to color, skybox and simple mask modes.
	/// </summary>
	public class TunnellingMobileURP : TunnellingMobileBase {
		/// <summary>
		/// Singleton instance.<br />
		/// Refers to a <see cref="TunnellingMobileURP"/> effect.<br />
		/// Will not refer to a <see cref="Tunnelling"/> or <see cref="TunnellingOpaque"/> effect.
		/// </summary>
		public static TunnellingMobileURP instance { get; private set; }

        protected override void Awake(){
            base.Awake();

			if (instance != null){
				Debug.LogWarning("More than one VrTunnellingPro instance detected - tunnelling will work properly but singleton instance may not be the one you expect.");
			}
			instance = this;
        }
        protected override void OnEnable(){
            base.OnEnable();

			RenderPipelineManager.beginCameraRendering -= BeforeRender;
			RenderPipelineManager.beginCameraRendering += BeforeRender;
        }
		void OnDisable(){
			RenderPipelineManager.beginCameraRendering -= BeforeRender;
		}
        
		void BeforeRender(ScriptableRenderContext context, Camera cam){
			if (cam != _cam){
				return;
			}

			UpdateEyeMatrices();
			ApplyEyeMatrices(_irisMatOuter);
			ApplyEyeMatrices(_irisMatInner);
		}
	}
}