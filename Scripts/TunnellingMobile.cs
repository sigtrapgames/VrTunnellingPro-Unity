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

		const string PROP_WRITEZ = "_WriteZ";
		const string PATH_SHADER = "TunnellingVertex";
		const string PATH_STENCILSHADER = "TunnellingMobileStencil";
		const CameraEvent CEVENT_FX = CameraEvent.BeforeImageEffects;
		const CameraEvent CEVENT_Z = CameraEvent.BeforeForwardOpaque;
		const int RQUEUE_FIRST = 1;
		const int RQUEUE_MASK = 2499;		// End of opaque queue
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

		/// <summary>
		/// If true, opaque portion of effect is drawn at start of frame, preventing overdraw by writing to z buffer.<br />
		/// Disabled by masking or drawing before transparent objects.
		/// </summary>
		public bool irisZRejectionEnabled {get {return !(useMask || drawBeforeTransparent);}}

		#region Internal Fields
		int _propColor, _propSkybox, _propWriteZ;
		int _globPropStencilRef, _globPropStencilMask, _globPropStencilBias;
		Material _irisMatOuter, _irisMatInner;
		Mesh _irisMesh;
		Dictionary<Renderer, MeshFilter> _maskObjects = new Dictionary<Renderer, MeshFilter>();
		Stack<Mesh> _skinnedMeshPool = new Stack<Mesh>();
		Stack<Mesh> _skinnedMeshesRendering = new Stack<Mesh>();
		List<Object> _toDestroy = new List<Object>();
		#endregion

		#region Non-alloc Helpers
		List<MeshRenderer> _tempMeshChildren = new List<MeshRenderer>();
		List<SkinnedMeshRenderer> _tempSkinnedMeshChildren = new List<SkinnedMeshRenderer>();
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
			if (p.overrideDrawBeforeTransparent){
				drawBeforeTransparent = p.drawBeforeTransparent;
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
			MeshFilter f = null;
			bool add = true;
			if (r is MeshRenderer){
				f = r.gameObject.GetComponent<MeshFilter>();
			} else if (!(r is SkinnedMeshRenderer)) {
				string err = "VRTP: Only MeshRenderers and SkinnedMeshRenderers can be masked.";
				if (includeChildren){
					err += " Any active MeshRenderer or SkinnedMeshRenderer children will still be masked.";
				}
				Debug.LogError(err, r);
				add = false;
			}

			if (add){
				try {
					_maskObjects.Add(r, f);	
				} catch (System.ArgumentException){
					string err = "VRTP: Renderer {0} has already been masked.";
					if (includeChildren){
						err += " Will still attempt to addd any active MeshRenderer or SkinnedMeshRenderer children to mask.";
					}
					Debug.LogErrorFormat(r, err, r.name);
				}
			}

			if (includeChildren){
				r.GetComponentsInChildren<MeshRenderer>(_tempMeshChildren);
				for (int i=0; i<_tempMeshChildren.Count; ++i){
					AddObjectToMask(_tempMeshChildren[i], true);
				}
				r.GetComponentsInChildren<SkinnedMeshRenderer>(_tempSkinnedMeshChildren);
				for (int i=0; i<_tempSkinnedMeshChildren.Count; ++i){
					AddObjectToMask(_tempSkinnedMeshChildren[i], true);
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
				// Remove inactive also, in case they've been deactivated after masking
				r.GetComponentsInChildren<MeshRenderer>(true, _tempMeshChildren);
				for (int i=0; i<_tempMeshChildren.Count; ++i){
					_maskObjects.Remove(_tempMeshChildren[i]);
				}
				r.GetComponentsInChildren<SkinnedMeshRenderer>(true, _tempSkinnedMeshChildren);
				for (int i=0; i<_tempSkinnedMeshChildren.Count; ++i){
					_maskObjects.Remove(_tempSkinnedMeshChildren[i]);
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

			_irisMesh = Instantiate<Mesh>(Resources.Load<Mesh>(PATH_MESHES + PATH_IRISMESH));
			_irisMatOuter = new Material(Shader.Find(PATH_SHADERS + PATH_SHADER + "Outer"));
			_irisMatInner = new Material(Shader.Find(PATH_SHADERS + PATH_SHADER + "Inner"));
			_toDestroy.Add(_irisMesh);
			_toDestroy.Add(_irisMatOuter);
			_toDestroy.Add(_irisMatInner);

			_cam = GetComponent<Camera>();

			_propColor = Shader.PropertyToID(PROP_COLOR);
			_propSkybox = Shader.PropertyToID(PROP_SKYBOX);
			_propWriteZ = Shader.PropertyToID(PROP_WRITEZ);

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
			// This should be emtpy, but just in case...
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
			_irisMatOuter.SetFloat(_propFxInner, motion);
			_irisMatInner.SetFloat(_propFxInner, motion);

			float outer = motion - effectFeather;
			_irisMatOuter.SetFloat(_propFxOuter, outer);
			_irisMatInner.SetFloat(_propFxOuter, outer);

			Color color = (!drawSkybox || applyColorToBackground) ? effectColor : Color.white;
			_irisMatOuter.SetColor(_propColor, color);
			_irisMatInner.SetColor(_propColor, color);

			Shader.SetGlobalInt("_VRTP_Stencil_Comp", useMask ? (int)CompareFunction.NotEqual : (int)CompareFunction.Always);

			// Disable z-write if allowing transparent to draw on top
			_irisMatOuter.SetFloat(_propWriteZ, drawBeforeTransparent ? 0 : 1);

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
			if (useMask){
				Material mat = stencilMat;
				mat.renderQueue = RQUEUE_MASK;
				foreach (var a in _maskObjects){
					// Ignore inactive
					if (!a.Key.enabled || !a.Key.gameObject.activeInHierarchy) continue;

					Mesh mesh = null;
					Matrix4x4 matrix = new Matrix4x4();
					bool canDrawMask = true;
					if (a.Value != null){
						// Static mesh renderer
						mesh = a.Value.sharedMesh;
						matrix = a.Key.transform.localToWorldMatrix;
					} else {
						// Skinned mesh renderer
						var s = (a.Key as SkinnedMeshRenderer);
						if (s != null){
							// Grab from pool
							mesh = _skinnedMeshPool.Pop();
							s.BakeMesh(mesh);
							_skinnedMeshesRendering.Push(mesh);
							// BakeMesh bakes scale, so render with unity scale
							matrix = Matrix4x4.TRS(s.transform.position, s.transform.rotation, Vector3.one);
						} else {
							// Otherwise can't mask :(
							canDrawMask = false;
						}
					}
					
					if (canDrawMask){
						for (int i=0; i<a.Value.sharedMesh.subMeshCount; ++i){
							Graphics.DrawMesh(mesh, matrix, mat, camLayer, _cam, i, null, false, false, false);
						}
					}
				}

				// Refill pool
				while (_skinnedMeshesRendering.Count > 0){
					_skinnedMeshPool.Push(_skinnedMeshesRendering.Pop());
				}
			}

			// Submit inner iris alpha blended pass
			// Select material depending on masking
			// Immediately after opaque queue, or absolute last
			DrawIris(_irisMatInner, 1, RQUEUE_LAST, camLayer);
		}
		void DrawIris(Material m, int submesh, int opaqueQueue, int camLayer){
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
			// Matrix is ignored in shader, but have to ensure mesh passes culling
			Vector3 pos = transform.position + (transform.forward * _cam.nearClipPlane * 1.01f);
			Graphics.DrawMesh(_irisMesh, pos, Quaternion.identity, m, camLayer, _cam, submesh, null, false, false, false);
		}

		void OnPreRender(){
			UpdateEyeMatrices();
			ApplyEyeMatrices(_irisMatOuter);
			ApplyEyeMatrices(_irisMatInner);
		}
	}
}