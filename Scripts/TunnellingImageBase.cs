using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

namespace Sigtrap.VrTunnellingPro {
	/// <summary>
	/// Base class for non-mobile effects (<see cref="Tunnelling"/> and <see cref="TunnellingOpaque"/>).<br />
	/// These effects use full screen post-processing.<br />
	/// This class implements all core functionality. The only difference between <see cref="Tunnelling"/> and <see cref="TunnellingOpaque"/> is drawing after *everything* or after *opaque geometry*.
	/// </summary>
	public abstract class TunnellingImageBase : TunnellingBase {
		#region Consts
		const string KEYWORD_MASK = "TUNNEL_MASK";
		const string KEYWORD_CONSTANT = "TUNNEL_CONSTANT";
		const string KEYWORD_INVERT = "TUNNEL_INVERT_MASK";
		const string KEYWORD_BLUR = "TUNNEL_BLUR";
		const string PATH_TUNNELSHADER = "Tunnelling";
		const string PATH_MASKSHADER = "Mask";
		const string PATH_WINDOWSHADER = "Window";
		const string PATH_COPYSHADER = "BlitA";
		const string PATH_BLURSHADER = "SeparableBlur";
		const string PATH_IRISSHADER = "TunnellingVertexZ";
		const string PATH_SKYSPHERESHADER = "Skysphere";
		const string PATH_SKYSPHEREMESH = "Skysphere";
		const string PROP_FEATHER = "_Feather";
		const string PROP_BKGRT = "_BkgTex";
		const string PROP_MASKRT = "_MaskTex";
		const string PROP_BLUR = "_Blur";
		const string PROP_OVERLAY = "_Overlay";

		const string PROP_BLUR_OFFSETS = "_Offsets";

		const float BLUR_OFFSET_1 = 1.33333333f;
		static readonly float[] BLUR_OFFSETS_9 = new float[]{1.38461538f, 3.23076923f};
		static readonly float[] BLUR_OFFSETS_13 = new float[]{1.41176470f, 3.29411764f, 5.17647058f};

		/// <summary>
		/// Global shader parameter for world-to-cage vertex matrix.<br />
		/// Used in counter-motion triplanar shaders.
		/// </summary>
		public const string GLOBAL_PROP_WORLD2CAGE = "_VRTP_WorldToCage";
		/// <summary>
		/// Global shader parameter for world-to-cage normals matrix.<br />
		/// Used in counter-motion triplanar shaders.
		/// </summary>
		public const string GLOBAL_PROP_WORLD2CAGE_NORMAL = "_VRTP_WorldToCageNormal";
		/// <summary>
		/// Global shader parameter for relative position offset between cage and world.
		/// In other words, how far has player artificially locomoted from reference point?
		/// Used in counter-motion triplanar shaders.
		/// </summary>
		public const string GLOBAL_PROP_CAGEPOS = "_VRTP_CagePos";
		#endregion

		public enum BlurKernel {
			/// <summary>
			/// Blur uses a 5x5 sample kernel
			/// </summary>
			FIVE = 0,
			/// <summary>
			/// Blur uses a 9x9 sample kernel
			/// </summary>
			NINE = 1,
			/// <summary>
			/// Blur uses a 13x13 sample kernel
			/// </summary>
			THIRTEEN = 2
		}
		public enum CounterVelocityMode {
			/// <summary>
			/// No counter-velocity effect
			/// </summmary>
			OFF,
			/// <summary>
			/// Counter-velocity effect uses Cage Counter Motion triplanar shaders
			/// </summary>
			SHADER,
			/// <summary>
			/// Counter-vloecity effect moves cage
			/// </summary>
			REAL
		}

		/// <summary>
		/// Singleton instance.<br />
		/// Will refer to either a <see cref="Tunnelling"/> or <see cref="TunnellingOpaque"/> effect.<br />
		/// Will not refer to a <see cref="TunnellingMobile"/> effect.
		/// </summary>
		public static TunnellingImageBase instance { get; private set; }

		#region Fields & Properties
		#region Serialized
		#region Effect
		/// <summary>
		/// Determines what is rendered in the vignette effect.
		/// </summary>
		[Tooltip("Determines what is rendered in the vignette effect.")]
		public BackgroundMode backgroundMode = BackgroundMode.COLOR;
		/// <summary>
		/// Allows a persistent overlay of the effect across the entire view.<br />
		/// Applies to SKYBOX, CAGE_COLOUR, CAGE_SKYBOX modes.
		/// </summary>
		[Range(0f,1f)][Tooltip("Allows a persistent overlay of the effect across the entire view.")]
		public float effectOverlay = 0;
		#endregion

		#region Cage
		[SerializeField][Tooltip("Root of objects to render as 'cage'.\n> No lighting! Use Unlit materials\n> Parent to camera's parent, to be 'static'")]
		private GameObject _cageParent;
		/// <summary>
		/// Render cage at half or quarter resolution.<br />
		/// 1: Half res. 2: Quarter res.
		/// </summary>
		[Range(0,2)]
		public int cageDownsample = 0;
		/// <summary>
		/// Manually set cage MSAA, or AUTO to follow quality settings.
		/// </summary>
		public MSAA cageAntiAliasing = MSAA.AUTO;
		/// <summary>
		/// FALSE: Cache cage objects in OnEnable.<br />
		///     CAN move/modify objects<br />
		///     CANNOT add/destroy objects<br />
		/// TRUE: Refresh objects each frame.<br />
		///     There will be some GC alloc each Update.<br />
		///     Consider calling UpdateCage() manually.<br />
		/// </summary>
		public bool cageUpdateEveryFrame=false;
		#endregion

		#region Fog
		/// <summary>
		/// Density of fog for materials using Cage Fogged shaders
		/// </summary>
		[Range(0.001f, 0.2f)]
		public float cageFogDensity = 0.01f;
		/// <summary>
		/// Fog falloff for materials using Cage Fogged shaders
		/// </summary>
		[Range(1,5)]
		public float cageFogPower = 2;
		/// <summary>
		/// Fog blending for materials using Cage Fogged shaders. 
		/// 0: No fog. 1: 100% fog.
		/// </summary>
		[Range(0f,1f)]
		public float cageFogBlend = 1;
		#endregion

		#region Mask
		public MaskMode maskMode = MaskMode.OFF;
		/* MSAA mismatch between camera and cmd buffer results in no drawing - appears to be unity issue
		[Tooltip("Manually set mask MSAA, or AUTO to follow quality settings.")]
		public MSAA maskAntiAliasing = MSAA.AUTO;
		private MSAA _lastMaskMsaa = MSAA.AUTO;*/
		#endregion

		#region Blur
		/// <summary>
		/// Downsample before blurring.
		/// Higher is faster and blurrier.
		/// </summary>
		[Range(0,4)]
		public int blurDownsample = 3;
		/// <summary>
		/// Blur radius.
		/// </summary>
		[Range(1f,5f)]
		public float blurDistance = 3;
		/// <summary>
		/// Blur passes.
		/// Higher is slower but blurrier.
		/// </summary>
		[Range(1,5)]
		public int blurPasses = 3;
		/// <summary>
		/// How many samples to use per pixel per pass?
		/// </summary>
		public BlurKernel blurSamples;
		#endregion

		/// <summary>
		/// At start of rendering, fill Z buffer where effect will be to save fillrate on drawing world.<br />
		/// Disabled with blur or masking.
		/// </summary>
		[Tooltip("At start of rendering, fill Z buffer where effect will be to save fillrate on drawing world.\nDisabled with blur or masking.")]
		public bool irisZRejection = true;

		#region Counter-Motion
		/// <summary>
		/// Choose counter-velocity effect implementation, or disable.<br />
		/// SHADER mode requires cage objects to use Cage Counter Motion shaders.
		/// </summary>
		public CounterVelocityMode counterVelocityMode = CounterVelocityMode.OFF;
		/// <summary>
		/// If <see cref="counterVelocityMode"/> is REAL, reset cage position and rotation when it has travelled this far.<br />
		/// If <= 0, no distance-based reset.
		/// </summary>
		[Tooltip("Reset cage after this distance.\nSet 0 for no distance-based reset.")]
		public float counterVelocityResetDistance = 0;
		/// <summary>
		/// If <see cref="counterVelocityMode"/> is REAL, reset cage position and rotation after this time.<br />
		/// If <= 0, no time-based reset.
		/// </summary>
		[Tooltip("Reset cage after this time.\nSet 0 for no distance-based reset.")]
		public float counterVelocityResetTime = 0;
		/// <summary>
		/// Scale counter-velocity relative to <see cref="motionTarget"/> motion.
		/// </summary>
		[Range(0f,COUNTER_STRENGTH_MAX)][Tooltip("Scale counter-velocity relative to Motion Target velocity.")]
		public float counterVelocityStrength = 1f;
		/// <summary>
		/// Scale counter-velocity on individual axes.<br />
		/// Multiplied by <see cref="counterVelocityStrength"/>.
		/// </summary>
		[Tooltip("Scale counter-velocity on individual axes.\nMultiplied by Counter Velocity Strength.")]
		public Vector3 counterVelocityPerAxis = Vector3.one;
		#endregion
		#endregion

		#region Graphics
		/// <summary>
		/// Is effect currently using/rendering a mask?
		/// </summary>
		public bool usingMask {get {return maskMode != MaskMode.OFF;}}
		/// <summary>
		/// Is effect currently using/rendering the cage?
		/// </summary>
		public bool usingCage {
			get {
				return backgroundMode == BackgroundMode.CAGE_COLOR || 
				backgroundMode == BackgroundMode.CAGE_SKYBOX ||
				backgroundMode == BackgroundMode.CAGE_ONLY;
			}}
		private bool _usingCageRt {get {return usingCage || backgroundMode == BackgroundMode.BLUR;}}
		private bool _canDrawIris {
			get {
				return !usingMask && backgroundMode != BackgroundMode.BLUR && 
				backgroundMode != BackgroundMode.CAGE_ONLY && effectColor.a == 1;
			}
		}
		private bool _drawIris {get {return irisZRejection && _canDrawIris;}}

		protected abstract CameraEvent _maskCmdEvt { get; }

		private CommandBuffer _maskCmd;

		private Material _matTunnel;
		private Material _matMask;
		private Material _matWindow;
		private Material _matCopyAlpha;
		private Material _matBlur;
		private Material _matSkysphere;
		private Mesh _meshSkysphere;

		private MeshRenderer[] _cageMrs;
		private List<MeshFilter> _cageMfs = new List<MeshFilter>();
		private List<Renderer> _maskObjects = new List<Renderer>();

		private RenderTexture _cageRt;
		private RenderTexture _maskRt;
		private RenderTexture _blurRt0, _blurRt1;
		private int _rtX, _rtY, _rtA;

		private CommandBuffer _irisCmd;
		private Material _matIris;
		private Mesh _meshIris;

		Vector4[] _blurOffsets = new Vector4[3];
		#endregion

		#region State
		private MaskMode _lastMaskMode = MaskMode.OFF;
		private MSAA _lastCageMsaa = MSAA.AUTO;
		private bool _camHasMaskBuffer = false;
		private int _lastCageDownsample = 0;
		private int _lastBlurDownsample = 0;
		private float _lastBlurRadius = 0;
		private BlurKernel _lastBlurKernel;
		private bool _wasDrawingIrisEarly = false;
		private bool _camHasIrisBuffer = false;
		private CounterVelocityMode _lastCvMode = CounterVelocityMode.OFF;
		private Vector3 _cmPos, _cageInitialPosLocal;
		private float _timeToResetCounterVelocity;
		#endregion

		#region Shader property IDs
		private int _propColor, _propBkgRt, _propMaskRt, _propSkybox, _propOverlay;
		private int _propBlur, _propBlurOffsets;
		private int _globPropWorld2Cage, _globPropWorld2CageNormal, _globPropCagePos;
		#endregion
		#endregion

		#region Lifecycle
		protected override void Awake(){
			base.Awake();

			if (instance != null){
				Debug.LogWarning("More than one VrTunnellingPro instance detected - tunnelling will work properly but singleton instance may not be the one you expect.");
			}
			instance = this;

			_propColor = Shader.PropertyToID(PROP_COLOR);
			_propBkgRt = Shader.PropertyToID(PROP_BKGRT);
			_propMaskRt = Shader.PropertyToID(PROP_MASKRT);
			_propSkybox = Shader.PropertyToID(PROP_SKYBOX);
			_propOverlay = Shader.PropertyToID(PROP_OVERLAY);
			_propBlur = Shader.PropertyToID(PROP_BLUR);
			_propBlurOffsets = Shader.PropertyToID(PROP_BLUR_OFFSETS);

			_globPropWorld2Cage = Shader.PropertyToID(GLOBAL_PROP_WORLD2CAGE);
			_globPropWorld2CageNormal = Shader.PropertyToID(GLOBAL_PROP_WORLD2CAGE_NORMAL);
			_globPropCagePos = Shader.PropertyToID(GLOBAL_PROP_CAGEPOS);

			_matTunnel = new Material(Shader.Find(PATH_SHADERS + PATH_TUNNELSHADER));
			_matMask = new Material(Shader.Find(PATH_SHADERS + PATH_MASKSHADER));
			_matWindow = new Material(Shader.Find(PATH_SHADERS + PATH_WINDOWSHADER));
			_matCopyAlpha = new Material(Shader.Find(PATH_SHADERS + PATH_COPYSHADER));
			_matBlur = new Material(Shader.Find(PATH_SHADERS + PATH_BLURSHADER));
			_matSkysphere = new Material(Shader.Find(PATH_SHADERS + PATH_SKYSPHERESHADER));
			_meshSkysphere = Resources.Load<Mesh>(PATH_MESHES + PATH_SKYSPHEREMESH);

			_meshIris = Resources.Load<Mesh>(PATH_MESHES + PATH_IRISMESH);
			_matIris = new Material(Shader.Find(PATH_SHADERS + PATH_IRISSHADER));
			_irisCmd = new CommandBuffer();
			_irisCmd.name = "VrTunnellingPro Z-Reject Iris";
			_irisCmd.DrawMesh(_meshIris, Matrix4x4.identity, _matIris, 0, 0);

			UpdateKeywords();

			if (_cageParent != null) {
				_cageParent.SetActive(false);
				_cageInitialPosLocal = _cageParent.transform.localPosition;
			}
		}
		protected override void OnEnable(){
			base.OnEnable();
			UpdateCage();

			// Add command buffers back to camera, but only if we've already set everything up properly
			if (usingMask && _maskCmd != null){
				ToggleMaskCommandBuffer(true);
			}
			if (_drawIris && _irisCmd != null){
				ToggleIrisCommandBuffer(true);
			}
		}
		void OnDisable(){
			// Remove command buffers to prevent unnecessary rendering while disabled
			if (_camHasMaskBuffer && _maskCmd != null){
				ToggleMaskCommandBuffer(false);
			}
			if (_camHasIrisBuffer && _drawIris){
				ToggleIrisCommandBuffer(false);
			}
		}
		void OnDestroy(){
			Destroy(_matTunnel);
			Destroy(_matMask);
			Destroy(_matCopyAlpha);
			Destroy(_matBlur);
			Destroy(_matSkysphere);

			if (_maskCmd != null) {
				ToggleMaskCommandBuffer(false);
				_maskCmd.Dispose();
			}
			if (_irisCmd != null) {
				ToggleIrisCommandBuffer(false);
				_irisCmd.Dispose();
			}

			ReleaseRT(ref _cageRt);
			ReleaseRT(ref _maskRt);
			ReleaseRT(ref _blurRt0);
			ReleaseRT(ref _blurRt1);

			if (instance == this) {
				instance = null;
			}
		}
		void ReleaseRT(ref RenderTexture rt){
			if (rt != null){
				rt.Release();
				rt = null;
			}
		}
		#endregion

		#region Public Methods
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

		/// <summary>
		/// Updates cached objects to render in cage mode.
		/// Use if adding/removing children from Cage Parent.
		/// No need to call if moving/modifying/toggling existing children.
		/// </summary>
		public void UpdateCage(){
			if (_cageParent == null){
				_cageMrs = new MeshRenderer[0];
				return;
			}
			_cageMrs = _cageParent.GetComponentsInChildren<MeshRenderer>(true);
			_cageMfs.Clear();
			for (int i=0; i<_cageMrs.Length; ++i){
				MeshFilter mf = _cageMrs[i].GetComponent<MeshFilter>();
				if (mf == null){
					throw new MissingReferenceException("Tunnelling cage requires a MeshFilter for every active MeshRenderer under Cage Parent!\nNo MeshFilter found on "+_cageMrs[i].name);
				}
				_cageMfs.Add(mf);
			}
		}

		public override void ResetCounterMotion(){
			base.ResetCounterMotion();
			_cmPos = Vector3.zero;
			if (_cageParent != null){
				_cageParent.transform.localPosition = _cageInitialPosLocal;
			}
		}

		/// <summary>
		/// Override some or all settings using a TunnellingPreset asset.
		/// </summary>
		public void ApplyPreset(TunnellingPreset p){
			ApplyPresetBase(p);

			if (p.overrideEffectOverlay) effectOverlay = p.effectOverlay;
			if (p.overrideBackgroundMode) backgroundMode = p.backgroundMode;

			if (p.overrideCageDownsample) cageDownsample = p.cageDownsample;
			if (p.overrideCageAntiAliasing) cageAntiAliasing = p.cageAntiAliasing;
			if (p.overrideCageUpdateEveryFrame) cageUpdateEveryFrame = p.cageUpdateEveryFrame;

			if (p.overrideCageFogDensity) cageFogDensity = p.cageFogDensity;
			if (p.overrideCageFogPower) cageFogPower = p.cageFogPower;
			if (p.overrideCageFogBlend) cageFogBlend = p.cageFogBlend;

			if (p.overrideMaskMode) maskMode = p.maskMode;
			if (p.overrideBlurDownsample) blurDownsample = p.blurDownsample;
			if (p.overrideBlurDistance) blurDistance = p.blurDistance;
			if (p.overrideBlurPasses) blurPasses = p.blurPasses;
			if (p.overrideBlurSamples) blurSamples = p.blurSamples;

			if (p.overrideCounterVelocityMode) counterVelocityMode = p.counterVelocityMode;
			if (p.overrideCounterVelocityResetDistance) counterVelocityResetDistance = p.counterVelocityResetDistance;
			if (p.overrideCounterVelocityResetTime) counterVelocityResetTime = p.counterVelocityResetTime;
			if (p.overrideCounterVelocityStrength) counterVelocityStrength = p.counterVelocityStrength;
			if (p.overrideCounterVelocityPerAxis) counterVelocityPerAxis = p.counterVelocityPerAxis;

			if (p.overrideIrisZRejection) irisZRejection = p.irisZRejection;
		}
		#endregion

		#region Tick
		void LateUpdate(){
			UpdateKeywords();

			// Shader properties
			float motion = CalculateMotion(Time.deltaTime);
			_matTunnel.SetFloat(_propFxInner, motion);
			_matTunnel.SetFloat(_propFxOuter, motion-effectFeather);
			_matTunnel.SetFloat(_propOverlay, effectOverlay);

			switch (backgroundMode){
				case BackgroundMode.COLOR:
					_matTunnel.SetColor(_propColor, effectColor);
					break;
				case BackgroundMode.CAGE_COLOR:
				case BackgroundMode.BLUR:
				case BackgroundMode.CAGE_SKYBOX:
				case BackgroundMode.SKYBOX:
					Color bkg = applyColorToBackground ? effectColor : Color.white;
					bkg.a = effectColor.a;
					_matTunnel.SetColor(_propColor, bkg);
					break;
			}

			switch (backgroundMode){
				case BackgroundMode.SKYBOX:
					_matTunnel.SetTexture(_propSkybox, effectSkybox);
					break;
				case BackgroundMode.CAGE_SKYBOX:
					_matSkysphere.SetTexture(_propSkybox, effectSkybox);
					break;
			}

			// Update cage objects if necessary
			if (usingCage && cageUpdateEveryFrame){
				UpdateCage();
			}

			if (_lastMaskMode != maskMode) {
				ResetMaskCommandBuffer();
			}

			if (_lastBlurKernel != blurSamples || !Mathf.Approximately(_lastBlurRadius, blurDistance)) {
				UpdateBlurKernel();
			}

			bool di = _drawIris;
			if (_wasDrawingIrisEarly != di){
				if (_camHasIrisBuffer && !di) {
					ToggleIrisCommandBuffer(false);
				} else if (!_camHasIrisBuffer && di){
					ToggleIrisCommandBuffer(true);
				}
				_wasDrawingIrisEarly = di;
			}
			if (di){
				float inner = motion*0.98f;	// Ensure iris is always a little bigger than image effect aperture
				_matIris.SetFloat(_propFxInner, inner);
				_matIris.SetFloat(_propFxOuter, inner-effectFeather);
			}

			// Check counter-velocity modes
			bool resetCm = false;
			if (_lastCvMode != counterVelocityMode){
				resetCm = true;
				_lastCvMode = counterVelocityMode;
			}
			if (counterVelocityMode == CounterVelocityMode.REAL){
				if (counterVelocityResetTime > 0 && Time.time >= _timeToResetCounterVelocity){
					resetCm = true;
					_timeToResetCounterVelocity = Time.time + counterVelocityResetTime;
				}
				if (counterVelocityResetDistance > 0 && _cmPos.sqrMagnitude >= (counterVelocityResetDistance * counterVelocityResetDistance)){
					resetCm = true;
				}
			}  
			if (resetCm){
				ResetCounterMotion();
			}

			// Fog
			Shader.SetGlobalFloat(_globPropFogDensity, cageFogDensity);
			Shader.SetGlobalFloat(_globPropFogPower, cageFogPower);
			Shader.SetGlobalFloat(_globPropFogBlend, cageFogBlend);
			Shader.SetGlobalColor(_globPropFogColor, effectColor);

			_hasDrawnThisFrame = false;	// Flag for once-per-frame things in Draw
		}

		void OnPreRender(){
			if (!_hasDrawnThisFrame) {
				if (_cageParent != null){
					Shader.SetGlobalMatrix(_globPropWorld2Cage, _cageParent.transform.worldToLocalMatrix);
					Shader.SetGlobalMatrix(_globPropWorld2CageNormal, _cageParent.transform.localToWorldMatrix.transpose);
					if (counterVelocityMode == CounterVelocityMode.SHADER){
						Shader.SetGlobalVector(_globPropCagePos, _cmPos);
					} else {
						Shader.SetGlobalVector(_globPropCagePos, Vector3.zero);
					}
				}
				UpdateEyeMatrices();
				ApplyEyeMatrices(_matTunnel);
				ApplyEyeMatrices(_matIris);
			}
		}

		protected void Draw(RenderTexture src, RenderTexture dest){
			if (!_hasDrawnThisFrame) {
				UpdateRenderTextures(src.width, src.height, src.antiAliasing);
			}

			if (usingMask || _usingCageRt){
				if (_usingCageRt) {
					// Set render target and blit bkg colour
					Graphics.SetRenderTarget(_cageRt);
					Color bkg = effectColor;
					bkg.a = backgroundMode == TunnellingBase.BackgroundMode.CAGE_ONLY ? 0 : 1;
					GL.Clear(true, true, bkg);
				}

				if (usingCage) {
					// Render skybox if necessary
					if (backgroundMode == BackgroundMode.CAGE_SKYBOX){
						_matSkysphere.SetPass(0);
						Graphics.DrawMeshNow(_meshSkysphere, motionTarget.localToWorldMatrix);
					}

					// Render cage objects
					for (int i = 0; i < _cageMrs.Length; ++i) {
						MeshRenderer mr = _cageMrs[i];
						if (mr.gameObject.activeSelf) {
							Mesh m = _cageMfs[i].sharedMesh;
							for (int j=0; j<m.subMeshCount; ++j){
								mr.sharedMaterials[j].SetPass(0);
								Graphics.DrawMeshNow(m, mr.localToWorldMatrix, j);
							}
						}
					}
				}

				if (backgroundMode == BackgroundMode.BLUR) {
					_matBlur.SetFloat(_propBlur, blurDistance);
					int blurPass = 2 * ((int)blurSamples);
					for (int i = 0; i < blurPasses; ++i) {
						// If first pass blit from source
						RenderTexture s = (i == 0) ? src : _blurRt0;
						Graphics.Blit(s, _blurRt1, _matBlur, blurPass);
						// If final pass blit straight to background
						RenderTexture d = (i != (blurPasses - 1)) ? _blurRt0 : _cageRt;
						Graphics.Blit(_blurRt1, d, _matBlur, blurPass+1);
					}
				}

				_matTunnel.SetTexture(_propBkgRt, _cageRt);
				_matTunnel.SetTexture(_propMaskRt, _maskRt);
			} else {
				_matTunnel.SetTexture(_propBkgRt, null);
			}

			Graphics.Blit(src, dest, _matTunnel);

			_hasDrawnThisFrame = true;	// Prevent once-per-frame things happening twice in multipass
		}
		#endregion

		#region Internal Methods
		protected override void CorrectEyeMatrices(Matrix4x4[] eyePrj, Matrix4x4[] eyeToWorld){
			// Hard-code far clip to 500
			eyePrj[0][3, 3] = 0.002f;
			eyePrj[1][3, 3] = 0.002f;
		}

		protected override void UpdateCounterMotion(Vector3 deltaPos, Quaternion deltaRot){
			base.UpdateCounterMotion(deltaPos, deltaRot);

			Quaternion q = GetCounterRotationDelta(deltaRot);
			_cageParent.transform.Rotate(-q.eulerAngles);

			if (counterVelocityStrength > 0){
				if (counterVelocityMode != CounterVelocityMode.OFF && counterVelocityStrength > 0){
					Vector3 d = new Vector3(
						deltaPos.x * counterVelocityPerAxis.x,
						deltaPos.y * counterVelocityPerAxis.y,
						deltaPos.z * counterVelocityPerAxis.z
					) * counterVelocityStrength;
					switch (counterVelocityMode){
						case CounterVelocityMode.SHADER:
							_cmPos += _cageParent.transform.InverseTransformVector(d);
							break;
						case CounterVelocityMode.REAL:
							_cmPos += motionTarget.transform.InverseTransformVector(d);
							_cageParent.transform.localPosition = _cmPos;
							break;
					}
				}
			}
		}

		void UpdateKeywords(){
			switch (maskMode){
				case MaskMode.OFF:
					_matTunnel.DisableKeyword(KEYWORD_CONSTANT);
					_matTunnel.DisableKeyword(KEYWORD_MASK);
					_matTunnel.DisableKeyword(KEYWORD_INVERT);
					break;
				case MaskMode.MASK:
					_matTunnel.DisableKeyword(KEYWORD_CONSTANT);
					_matTunnel.EnableKeyword(KEYWORD_MASK);
					_matTunnel.DisableKeyword(KEYWORD_INVERT);
					break;
				case MaskMode.WINDOW:
					_matTunnel.EnableKeyword(KEYWORD_CONSTANT);
					_matTunnel.EnableKeyword(KEYWORD_MASK);
					_matTunnel.DisableKeyword(KEYWORD_INVERT);
					break;
				case MaskMode.PORTAL:
					_matTunnel.EnableKeyword(KEYWORD_CONSTANT);
					_matTunnel.EnableKeyword(KEYWORD_MASK);
					_matTunnel.EnableKeyword(KEYWORD_INVERT);
					break;
			}
			switch (backgroundMode){
				case BackgroundMode.COLOR:
					_matTunnel.DisableKeyword(KEYWORD_BKG);
					_matTunnel.DisableKeyword(KEYWORD_SKYBOX);
					_matTunnel.DisableKeyword(KEYWORD_OVERLAY);
					break;
				case BackgroundMode.SKYBOX:
					_matTunnel.DisableKeyword(KEYWORD_BKG);
					_matTunnel.EnableKeyword(KEYWORD_SKYBOX);
					_matTunnel.DisableKeyword(KEYWORD_OVERLAY);
					break;
				case BackgroundMode.CAGE_COLOR:
				case BackgroundMode.CAGE_SKYBOX:
				case BackgroundMode.BLUR:
					_matTunnel.EnableKeyword(KEYWORD_BKG);
					_matTunnel.DisableKeyword(KEYWORD_SKYBOX);
					_matTunnel.DisableKeyword(KEYWORD_OVERLAY);
					break;
				case BackgroundMode.CAGE_ONLY:
					_matTunnel.EnableKeyword(KEYWORD_BKG);
					_matTunnel.DisableKeyword(KEYWORD_SKYBOX);
					_matTunnel.EnableKeyword(KEYWORD_OVERLAY);
					break;
			}
		}
		void UpdateBlurKernel(){
			if (_blurRt0 == null) return;
			Vector2 ts = _blurRt0.texelSize;
			switch(blurSamples){
				case BlurKernel.FIVE:
					_blurOffsets[0].x = ts.x * blurDistance * BLUR_OFFSET_1;
					_blurOffsets[0].w = ts.y * blurDistance * BLUR_OFFSET_1;
					break;
				case BlurKernel.NINE:
					for (int i = 0; i < 2; ++i) {
						_blurOffsets[i].x = ts.x * blurDistance * BLUR_OFFSETS_9[i];
						_blurOffsets[i].w = ts.y * blurDistance * BLUR_OFFSETS_9[i];
					}
					break;
				case BlurKernel.THIRTEEN:
					for (int i = 0; i < 3; ++i) {
						_blurOffsets[i].x = ts.x * blurDistance * BLUR_OFFSETS_13[i];
						_blurOffsets[i].w = ts.y * blurDistance * BLUR_OFFSETS_13[i];
					}
					break;
			}

			_matBlur.SetVectorArray(_propBlurOffsets, _blurOffsets);

			_lastBlurRadius = blurDistance;
			_lastBlurKernel = blurSamples;
		}
		int GetMsaa(MSAA m, int srcMsaa){
			int msaa = srcMsaa;
			switch (m){
				case MSAA.AUTO:
					break;
				case MSAA.OFF:
					msaa = 1;
					break;
				case MSAA.X2:
					msaa = 2;
					break;
				case MSAA.X4:
					msaa = 4;
					break;
				case MSAA.X8:
					msaa = 8;
					break;
			}
			return msaa;
		}
		void UpdateRenderTextures(int srcX, int srcY, int srcMsaa){
			bool changeRes = (srcX != _rtX || srcY != _rtY || srcMsaa != _rtA);
			bool updated = false;

			// Update background RT
			if (
				_usingCageRt &&
				(changeRes || _cageRt == null || _lastCageDownsample != cageDownsample || _lastCageMsaa != cageAntiAliasing)
			){
				if (_cageRt != null){
					_cageRt.Release();
				}

				int x = srcX / (cageDownsample+1);
				int y = srcY / (cageDownsample+1);
				#if UNITY_2017_2_OR_NEWER
				RenderTextureDescriptor cageRtd = new RenderTextureDescriptor(x, y, RenderTextureFormat.Default, 24);
				cageRtd.vrUsage = UnityEngine.XR.XRSettings.eyeTextureDesc.vrUsage;
				_cageRt = new RenderTexture(cageRtd);
				#else
				_cageRt = new RenderTexture(x, y, 24);
				#endif
				_cageRt.antiAliasing = GetMsaa(cageAntiAliasing, srcMsaa);
				_cageRt.name = "VTP Background";
				_cageRt.Create();

				_lastCageDownsample = cageDownsample;
				_lastCageMsaa = cageAntiAliasing;
				updated = true;
			}

			// Update mask RT
			if (usingMask && (changeRes || _maskRt == null /*|| _lastMaskMsaa != maskAntiAliasing*/)) {
				if (_maskRt != null){
					_maskRt.Release();
				}

				_maskRt = new RenderTexture(srcX, srcY, 16, RenderTextureFormat.R8);
				_maskRt.antiAliasing = srcMsaa; //GetMsaa(maskAntiAliasing, srcMsaa);
				_maskRt.name = "VTP Mask";
				_maskRt.Create();

				ResetMaskCommandBuffer();

				//_lastMaskMsaa = maskAntiAliasing;
				updated = true;
			}

			// Update blur RTs
			if (
				backgroundMode == BackgroundMode.BLUR &&
				(changeRes || _blurRt0 == null || _blurRt1 == null || _lastBlurDownsample != blurDownsample)
			){
				if (_blurRt0 != null){
					_blurRt0.Release();
				}
				if (_blurRt1 != null){
					_blurRt1.Release();
				}

				int x = srcX / (blurDownsample+1);
				int y = srcY / (blurDownsample+1);
				#if UNITY_2017_2_OR_NEWER
				RenderTextureDescriptor blurRtd = new RenderTextureDescriptor(x, y, RenderTextureFormat.Default, 0);
				blurRtd.vrUsage = UnityEngine.XR.XRSettings.eyeTextureDesc.vrUsage;
				_blurRt0 = new RenderTexture(blurRtd);
				_blurRt1 = new RenderTexture(blurRtd);
				#else
				_blurRt0 = new RenderTexture(x, y, 0);
				_blurRt1 = new RenderTexture(x, y, 0);
				#endif
				_blurRt0.name = "VTP Blur 0";
				_blurRt1.name = "VTP Blur 1";
				_blurRt0.Create();
				_blurRt1.Create();
				UpdateBlurKernel();
				_lastBlurDownsample = blurDownsample;
				updated = true;
			}

			if (updated){
				_rtX = srcX;
				_rtY = srcY;
				_rtA = srcMsaa;
			}
		}

		void ResetMaskCommandBuffer(){
			if (_maskCmd == null){
				// If buffer doesn't exist, create it
				_maskCmd = new CommandBuffer();
				_maskCmd.name = "VrTunnellingPro Draw Mask Objects";
			}

			_maskCmd.Clear();

			// Set color target to our RT, clear, but keep z buffer
			_maskCmd.SetRenderTarget(
				new RenderTargetIdentifier(_maskRt), 
				new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget)
			);
			_maskCmd.ClearRenderTarget(false, true, Color.white);

			// Draw each listed mesh
			Material maskMatToUse = maskMode == MaskMode.WINDOW ? _matWindow : _matMask;
			FillMaskBuffer(_maskCmd, _maskObjects, maskMatToUse);

			// Update command buffer state
			if (isActiveAndEnabled) {
				if (_lastMaskMode == MaskMode.OFF && usingMask && !_camHasMaskBuffer){
					ToggleMaskCommandBuffer(true);
				} else if (_lastMaskMode != MaskMode.OFF && !usingMask && _camHasMaskBuffer){
					ToggleMaskCommandBuffer(false);
				}
				_lastMaskMode = maskMode;
			} else if (_camHasMaskBuffer){
				ToggleMaskCommandBuffer(false);
			}
		}
		void ToggleMaskCommandBuffer(bool on){
			if (on){
				_cam.AddCommandBuffer(_maskCmdEvt, _maskCmd);
				_camHasMaskBuffer = true;
			} else {
				_cam.RemoveCommandBuffer(_maskCmdEvt, _maskCmd);
				_camHasMaskBuffer = false;
			}
		}
		void ToggleIrisCommandBuffer(bool on){
			if (on) {
				_cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, _irisCmd);
				_camHasIrisBuffer = true;
			} else {
				_cam.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, _irisCmd);
				_camHasIrisBuffer = false;
			}
		}
		#endregion
	}
}
