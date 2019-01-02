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
		const int RQUEUE_FIRST = 1;
		const int RQUEUE_MASK = 2500;		// End of opaque queue
		const int RQUEUE_OPAQUE = 2501;		// Start of transparent queue
		const int RQUEUE_LAST = 5000;
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
		/// If true, transparent objects will draw on top of vignette.<br />
		/// Disables z-rejection optimisation.
		/// </summary>
		[Tooltip("If ticked, transparent objects will draw on top of vignette.\nDisables z-rejection optimisation.")]
		public bool drawBeforeTransparent;
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
		Material _irisMatZ, _irisMatOuter, _irisMatInner;
		Mesh _irisMesh;
		Dictionary<Renderer, MeshFilter> _maskObjects = new Dictionary<Renderer, MeshFilter>();
		Stack<Mesh> _skinnedMeshPool = new Stack<Mesh>();
		Stack<Mesh> _skinnedMeshesRendering = new Stack<Mesh>();
		bool _wasUsingMask=false;
		List<Object> _toDestroy = new List<Object>();
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
			try {
				if (r is SkinnedMeshRenderer){
					_maskObjects.Add(r, null);
				} else {
					_maskObjects.Add(r, r.gameObject.GetComponent<MeshFilter>());
				}
			} catch (System.ArgumentException){
				Debug.LogErrorFormat(r, "Renderer {0} has already been added to the VRTP mask", r.name);
			}
			if (includeChildren){
				var childen = r.GetComponentsInChildren<Renderer>();
				for (int i=0; i<childen.Length; ++i){
					AddObjectToMask(childen[i], true);
				}
			}
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
			_irisMatZ = new Material(Shader.Find(PATH_SHADERS + PATH_SHADER + "Z"));
			_irisMatOuter = new Material(Shader.Find(PATH_SHADERS + PATH_SHADER + "Outer"));
			_irisMatInner = new Material(Shader.Find(PATH_SHADERS + PATH_SHADER + "Inner"));
			_toDestroy.Add(_irisMesh);
			_toDestroy.Add(_irisMatZ);
			_toDestroy.Add(_irisMatOuter);
			_toDestroy.Add(_irisMatInner);

			_cam = GetComponent<Camera>();

			_propColor = Shader.PropertyToID(PROP_COLOR);
			_propSkybox = Shader.PropertyToID(PROP_SKYBOX);

			_globPropStencilMask = Shader.PropertyToID(GLOBAL_PROP_STENCILMASK);
			_globPropStencilRef = Shader.PropertyToID(GLOBAL_PROP_STENCILREF);
			_globPropStencilBias = Shader.PropertyToID(GLOBAL_PROP_STENCILBIAS);
		}
		void OnDestroy(){
			foreach (var o in _toDestroy){
				Destroy(o);
			}
			_toDestroy.Clear();
			foreach (var m in _skinnedMeshPool){
				Destroy(m);
			}
			_skinnedMeshPool.Clear();
			foreach (var m in _skinnedMeshesRendering){
				Destroy(m);
			}
			_skinnedMeshesRendering.Clear();
		}
		#endregion

		void LateUpdate () {
			if (useMask){
				Shader.SetGlobalInt(_globPropStencilRef, stencilReference);
				Shader.SetGlobalInt(_globPropStencilMask, stencilMask);
				Shader.SetGlobalFloat(_globPropStencilBias, -stencilBias);
			}

			float motion = CalculateMotion(Time.deltaTime);
			_irisMatZ.SetFloat(_propFxInner, motion);
			_irisMatZ.SetFloat(_propFxOuter, motion - effectFeather);
			_irisMatZ.SetColor(_propColor, (!drawSkybox || applyColorToBackground) ? effectColor : Color.white);

			// Find a layer the camera will render
			int camLayer = 0;
			for (int i=0; i<32; ++i){
				camLayer = 1 << i;
				if ((_cam.cullingMask & camLayer) != 0){
					break;
				}
			}

			// Submit outer iris opaque pass
			// Select material depending on masking
			// Immediately after opaque queue, or first in background queue
			DrawIris(_irisMatOuter, 0, 1, camLayer);
			
			// Submit mask objects
			// Fill pool
			while (_skinnedMeshesRendering.Count > 0){
				_skinnedMeshPool.Push(_skinnedMeshesRendering.Pop());
			}
			if (useMask){
				Material mat = stencilMat;
				mat.renderQueue = RQUEUE_MASK;
				foreach (var a in _maskObjects){
					Mesh mesh = null;
					if (a.Value != null){
						// Static mesh renderer
						mesh = a.Value.sharedMesh;
					} else {
						// Skinned mesh renderer
						var s = (a.Key as SkinnedMeshRenderer);
						if (s != null){
							// Grab from pool
							mesh = _skinnedMeshPool.Pop();
							s.BakeMesh(mesh);
							_skinnedMeshesRendering.Push(mesh);
						}
						// Otherwise can't mask :(
					}

					Matrix4x4 matrix = a.Key.transform.localToWorldMatrix;
					
					for (int i=0; i<a.Value.sharedMesh.subMeshCount; ++i){
						Graphics.DrawMesh(mesh, matrix, mat, camLayer, _cam, i, null, false, false, false);
					}
				}
			}

			// Submit inner iris alpha blended pass
			// Select material depending on masking
			// Immediately after opaque queue, or absolute last
			DrawIris(_irisMatInner, 1, RQUEUE_LAST, camLayer);
		}
		DrawIris(Material m, int submesh, int opaqueQueue, int camLayer){
			// Select pass to use (toggles stencil usage)
			m.SetShaderPassEnabled("Masked", useMask);
			m.SetShaderPassEnabled("Unmasked", !useMask);

			if (drawBeforeTransparent){
				// Draw immediately after opaque objects
				m.renderQueue = RQUEUE_OPAQUE;
			} else if (useMask){
				// Draw after everything to allow masked objects to fill stencil
				m.renderQueue = RQUEUE_LAST;
			} else {
				// Draw opaque in specified order
				m.renderQueue = opaqueQueue;
			}
			
			if (drawSkybox){
				m.SetTexture(_propSkybox, effectSkybox);
				m.EnableKeyword(KEYWORD_SKYBOX);
			} else {
				m.SetTexture(_propSkybox, null);
				m.DisableKeyword(KEYWORD_SKYBOX);
			}
			Graphics.DrawMesh(_irisMesh, Matrix4x4.identity, m, camLayer, _cam, submesh, null, false, false, false);
		}

		void OnPreRender(){
			UpdateEyeMatrices();
			ApplyEyeMatrices(_irisMatZ);
		}
	}
}