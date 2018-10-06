using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sigtrap.VrTunnellingPro {
	/// <summary>
	/// Mobile-friendly tunnelling effect.<br />
	/// This script does not use post-processing. Limited to color, skybox and simple mask modes.
	/// </summary>
	public class TunnellingMobile : TunnellingBase {
		#region Consts
		/// <summary>
		/// Global shader parameter controlling mobile masking stencil buffer value.
		/// </summary>
		public const string GLOBAL_PROP_STENCILREF = "_VRTP_Stencil_Ref";
		/// <summary>
		/// Global shader parameter controlling mobile masking stencil buffer read/write mask.
		/// </summary>
		public const string GLOBAL_PROP_STENCILMASK = "_VRTP_Stencil_Mask";
		/// <summary>
		/// Global shader parameter controlling mobile masking stencil buffer z-bias.
		/// </summary>
		public const string GLOBAL_PROP_STENCILBIAS = "_VRTP_Stencil_Bias";

		const string PATH_SHADER = "TunnellingVertex";
		const string PATH_STENCILSHADER = "TunnellingMobileStencil";
		const CameraEvent CEVENT_FX = CameraEvent.BeforeImageEffects;
		const CameraEvent CEVENT_Z = CameraEvent.BeforeForwardOpaque;
		#endregion

		#region Static
		/// <summary>
		/// Singleton instance.<br />
		/// Refers to a <see cref="TunnellingMobile"/> effect.<br />
		/// Will not refer to a <see cref="Tunnelling"/> or <see cref="TunnellingOpaque"/> effect.
		/// </summary>
		public static TunnellingMobile instance { get; private set; }

		static Material _stencilMat;
		public static Material stencilMat {
			get {
				if (_stencilMat == null){
					_stencilMat = new Material(Shader.Find(PATH_SHADERS + PATH_STENCILSHADER));
				}
				return _stencilMat;
			}
		}
		#endregion

		#region Serialized Fields
		/// <summary>
		/// Draw skybox over vignette instead of solid color.
		/// </summary>
		[Tooltip("Draw skybox over vignette instead of solid color.")]
		public bool drawSkybox;
		/// <summary>
		/// Use stencil mask to exclude objects from vignette?<br />
		/// Can stress drawcalls and fillrate.
		/// </summary>
		[Tooltip("Use stencil mask to exclude objects from vignette?\nCan stress drawcalls and fillrate.")]
		public bool useMask;
		/// <summary>
		/// Pixels with this value in the stencil buffer will be masked.
		/// </summary>
		[Tooltip("Pixels with this value in the stencil buffer will be masked.")]
		[Range(0,255)]
		public int stencilReference=1;
		/// <summary>
		/// Write- and read-mask for stencil buffer.<br />
		/// If in doubt, leave at 255.
		/// </summary>
		[Tooltip("Write- and read-mask for stencil buffer.\nIf in doubt, leave at 255.")]
		[Range(0,255)]
		public int stencilMask=255;
		/// <summary>
		/// Offset Z on mask to avoid z-fighting.
		/// </summary>
		[Tooltip("Offset Z on mask to avoid z-fighting.")]
		[Range(0, 10)]
		public float stencilBias=1;
		#endregion

		#region Internal Fields
		int _propColor, _propSkybox;
		int _globPropStencilRef, _globPropStencilMask, _globPropStencilBias;
		Material _irisMat;
		Mesh _irisMesh;
		CommandBuffer _zCmd, _fxCmd, _maskCmd;
		List<Renderer> _maskObjects = new List<Renderer>();
		bool _wasUsingMask=false;
		#endregion

		#region Public Methods
		/// <summary>
		/// Override some or all settings using a TunnellingPreset asset.
		/// </summary>
		public void ApplyPreset(TunnellingPresetMobile p){
			ApplyPresetBase(p);

			if (p.overrideDrawSkybox){
				drawSkybox = p.drawSkybox;
			}
			if (p.overrideUseMask){
				useMask = p.useMask;
			}
			if (p.overrideStencilReference){
				stencilReference = p.stencilReference;
			}
			if (p.overrideStencilMask){
				stencilMask = p.stencilMask;
			}
			if (p.overrideStencilBias){
				stencilBias = p.stencilBias;
			}
		}
		/// <summary>
		/// Start using object to mask tunnelling effect.
		/// </summary>
		/// <param name="r">Renderer to add to mask.</param>
		/// <param name="includeChildren">If set to <c>true</c> include children.</param>
		public void AddObjectToMask(Renderer r, bool includeChildren=false){
			_maskObjects.Add(r);
			if (includeChildren){
				var childen = r.GetComponentsInChildren<Renderer>();
				for (int i=0; i<childen.Length; ++i){
					_maskObjects.Add(childen[i]);
				}
			}
			// Clear and repopulate commands
			ResetMaskCommandBuffer();
		}
		/// <summary>
		/// Stop using object to mask tunnelling effect.
		/// </summary>
		/// <param name="r">Renderer to remove from mask.</param>
		/// <param name="includeChildren">If set to <c>true</c> include children.</param>
		public void RemoveObjectFromMask(Renderer r, bool includeChildren=false){
			_maskObjects.Remove(r);
			if (includeChildren){
				var childen = r.GetComponentsInChildren<Renderer>();
				for (int i=0; i<childen.Length; ++i){
					_maskObjects.Remove(childen[i]);
				}
			}
			// Clear and repopulate commands
			ResetMaskCommandBuffer();
		}
		#endregion

		#region Lifecycle
		protected override void Awake(){
			base.Awake();

			if (instance != null){
				Debug.LogWarning("More than one VrTunnellingPro instance detected - tunnelling will work properly but singleton instance may not be the one you expect.");
			}
			instance = this;

			_irisMesh = Resources.Load<Mesh>(PATH_MESHES + PATH_IRISMESH);
			_irisMat = new Material(Shader.Find(PATH_SHADERS + PATH_SHADER));

			_zCmd = new CommandBuffer();
			_zCmd.name = "VrTunnellingPro Mobile Z-Reject";
			// Draw outer iris opaque for z-rejection
			_zCmd.DrawMesh(_irisMesh, Matrix4x4.identity, _irisMat, 0, 0);

			_fxCmd = new CommandBuffer();
			_fxCmd.name = "VrTunnellingPro Mobile Effect";
			ResetEffectCommandBuffer(useMask);

			_maskCmd = new CommandBuffer();
			_maskCmd.name = "VrTunnellingPro Mobile Mask";

			_cam = GetComponent<Camera>();

			_propColor = Shader.PropertyToID(PROP_COLOR);
			_propSkybox = Shader.PropertyToID(PROP_SKYBOX);

			_globPropStencilMask = Shader.PropertyToID(GLOBAL_PROP_STENCILMASK);
			_globPropStencilRef = Shader.PropertyToID(GLOBAL_PROP_STENCILREF);
			_globPropStencilBias = Shader.PropertyToID(GLOBAL_PROP_STENCILBIAS);
		}
		protected override void OnEnable(){
			base.OnEnable();
			SetCommandBuffers(useMask);
		}
		void OnDisable(){
			UnsetCommandBuffers(useMask);
		}
		void OnDestroy(){
			Destroy(_irisMat);
			_zCmd.Dispose();
			_zCmd = null;
			_fxCmd.Dispose();
			_fxCmd = null;
			_maskCmd.Dispose();
			_maskCmd = null;
			_irisMesh = null;
		}
		#endregion

		void ResetEffectCommandBuffer(bool mask){
			_fxCmd.Clear();
			// Draw outer iris opaque
			_fxCmd.DrawMesh(_irisMesh, Matrix4x4.identity, _irisMat, 0, mask ? 3 : 1);
			// Draw inner iris alpha blended
			_fxCmd.DrawMesh(_irisMesh, Matrix4x4.identity, _irisMat, 1, mask ? 4 : 2);
		}
		void ResetMaskCommandBuffer(){
			_maskCmd.Clear();
			FillMaskBuffer(_maskCmd, _maskObjects, stencilMat);
		}
		void SetCommandBuffers(bool mask){
			if (mask){				
				_cam.AddCommandBuffer(CEVENT_FX, _maskCmd);
			} else {
				_cam.AddCommandBuffer(CEVENT_Z, _zCmd);
			}
			_cam.AddCommandBuffer(CEVENT_FX, _fxCmd);
		}
		void UnsetCommandBuffers(bool mask){
			if (mask){
				_cam.RemoveCommandBuffer(CEVENT_FX, _maskCmd);
			} else {
				_cam.RemoveCommandBuffer(CEVENT_Z, _zCmd);
			}
			_cam.RemoveCommandBuffer(CEVENT_FX, _fxCmd);
		}

		void LateUpdate () {
			if (_wasUsingMask != useMask){
				// Remove all current buffers (using current mask state)
				UnsetCommandBuffers(_wasUsingMask);
				// Set up mask buffer from list of mask objects
				ResetMaskCommandBuffer();
				// Set up effect buffer to use / not use mask
				ResetEffectCommandBuffer(useMask);
				// Add buffers (using new mask state)
				SetCommandBuffers(useMask);
				_wasUsingMask = useMask;
			}
			
			if (useMask){
				Shader.SetGlobalInt(_globPropStencilRef, stencilReference);
				Shader.SetGlobalInt(_globPropStencilMask, stencilMask);
				Shader.SetGlobalFloat(_globPropStencilBias, -stencilBias);
			}

			float motion = CalculateMotion(Time.deltaTime);
			_irisMat.SetFloat(_propFxInner, motion);
			_irisMat.SetFloat(_propFxOuter, motion - effectFeather);
			_irisMat.SetColor(_propColor, (!drawSkybox || applyColorToBackground) ? effectColor : Color.white);

			if (drawSkybox){
				_irisMat.SetTexture(_propSkybox, effectSkybox);
				_irisMat.EnableKeyword(KEYWORD_SKYBOX);
			} else {
				_irisMat.SetTexture(_propSkybox, null);
				_irisMat.DisableKeyword(KEYWORD_SKYBOX);
			}
		}

		void OnPreRender(){
			UpdateEyeMatrices();
			ApplyEyeMatrices(_irisMat);
		}
	}
}