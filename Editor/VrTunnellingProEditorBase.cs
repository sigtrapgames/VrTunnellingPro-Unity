using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Sigtrap.VrTunnellingPro.Editors {
	public abstract class VrTunnellingProEditorBase : Editor {
		protected string URL_DOCS = "http://www.sigtrapgames.com/VrTunnellingPro/html/index.html";
		protected abstract string HEADER_LOGO_NAME {get;}

		#region Label Consts
		const string MOTION_STRENGTH = "Strength";
		const string MOTION_MIN = "Min";
		const string MOTION_MAX = "Max";
		const string MOTION_SMOOTH = "Smoothing";

		const string MOTION_AV = "Angular Velocity";
		const string MOTION_LA = "Acceleration";
		const string MOTION_LV = "Velocity";

		const string MOTION_AV_UNITS = "Degrees per Second";
		const string MOTION_LA_UNITS = "Meters per Second squared";
		const string MOTION_LV_UNITS = "Meters per Second";

		const string MOTION_STR_FMT = "How much {0} contibutes to effect.";
		const string MOTION_MIN_FMT = "Ignore {0} below this.\n[{1}]";
		const string MOTION_MAX_FMT = "Clamp {0} effect contribution above this.\n[{1}]";
		const string MOTION_SMOOTH_FMT = "Smoothing time for {0}.\n0 for no smoothing";
		#endregion

		protected class SectionToggle {
			public class PropInfo {
				string _propName;
				SerializedProperty _property;
				System.Func<SerializedProperty, bool> _getActive;
				public bool active {
					get {
						if (_property == null) return false;
						if (_getActive != null) return _getActive(_property);
						return _property.boolValue;
					}
				}

				public PropInfo(string propName, System.Func<SerializedProperty, bool> getActive){
					_propName = propName;
					_getActive = getActive;
					_property = null;
				}
				public PropInfo(string propName){
					_propName = propName;
					_getActive = null;
					_property = null;
				}

				public void Init(SerializedObject so){
					_property = so.FindProperty(_propName);
				}

				public static implicit operator PropInfo(string propName){
					return new PropInfo(propName);
				}
			}

			// 1: force open
			// 0: depend on property bool value
			// -1: force closed
			int _active = 0;
			public bool active {
				get {
					switch (_active){
						case 1:
							return true;
						case -1:
							return false;
						case 0:
							foreach (var p in _props){
								if (p.active) return true;
							}
							return false;
					}
					return false;
				}
				set {
					_active = value ? 1 : -1;
				}
			}
			PropInfo[] _props;

			public SectionToggle(params PropInfo[] props){
				_props = props;
			}

			public static void Init(SerializedObject serializedObject, params SectionToggle[] toggles){
				foreach (var t in toggles){
					foreach (var p in t._props){
						p.Init(serializedObject);
					}				
				}
			}

			public static implicit operator bool(SectionToggle st){
				return st.active;
			}
		}

		#region SerializedProperties
		#region Effect settings
		AutoProperty _pTarget = new AutoProperty("motionTarget");
		AutoProperty _pFxColor = new AutoProperty("effectColor");
		AutoProperty _pFxCover = new AutoProperty("effectCoverage");
		AutoProperty _pFxFeather = new AutoProperty("effectFeather");
		protected AutoProperty _pFxSkybox = new AutoProperty("effectSkybox");
		protected AutoProperty _pApplyColorToBkg = new AutoProperty("applyColorToBackground");
		#endregion
		#region Angular velocity motion settings
		AutoProperty _pAvUse = new AutoProperty("useAngularVelocity", "Angular Velocity");
		AutoProperty _pAvStr = new AutoProperty("angularVelocityStrength");
		AutoProperty _pAvMin = new AutoProperty("angularVelocityMin");
		AutoProperty _pAvMax = new AutoProperty("angularVelocityMax");
		AutoProperty _pAvSmooth = new AutoProperty("angularVelocitySmoothing");
		#endregion
		#region Linear acceleration motion settings
		AutoProperty _pLaUse = new AutoProperty("useAcceleration", "Linear Acceleration");
		AutoProperty _pLaStr = new AutoProperty("accelerationStrength");
		AutoProperty _pLaMin = new AutoProperty("accelerationMin");
		AutoProperty _pLaMax = new AutoProperty("accelerationMax");
		AutoProperty _pLaSmooth = new AutoProperty("accelerationSmoothing");
		#endregion
		#region Linear velocity motion settings
		AutoProperty _pLvUse = new AutoProperty("useVelocity", "Linear Velocity");
		AutoProperty _pLvStr = new AutoProperty("velocityStrength");
		AutoProperty _pLvMin = new AutoProperty("velocityMin");
		AutoProperty _pLvMax = new AutoProperty("velocityMax");
		AutoProperty _pLvSmooth = new AutoProperty("velocitySmoothing");
		#endregion

		#region Motion Effects
		#region Counter-motion settings
		AutoProperty _pCounterMotion = new AutoProperty("useCounterMotion", "Cage Counter-Motion");
		AutoProperty _pCounterRotStr = new AutoProperty("counterRotationStrength", "Rotation Strength");
		AutoProperty _pCounterRotAxs = new AutoProperty("counterRotationPerAxis", "Rotation Per-Axis");
		#endregion
		AutoProperty _pMotionEffectTarget = new AutoProperty("motionEffectTarget");
		#region Artificial tilt settings
		AutoProperty _pArtTilt = new AutoProperty("useArtificialTilt", "Artificial Tilt");
		AutoProperty _pArtTiltStr = new AutoProperty("tiltStrength");
		AutoProperty _pArtTiltMax = new AutoProperty("tiltMaxAngles");
		AutoProperty _pArtTiltSmooth = new AutoProperty("tiltSmoothTime");
		#endregion
		#region Framerate division settings
		AutoProperty _pDivFps = new AutoProperty("framerateDivision");
		AutoProperty _pDivTrans = new AutoProperty("divideTranslation");
		AutoProperty _pDivRot = new AutoProperty("divideRotation");
		#endregion
		#endregion
		#endregion

		#region Labels
		// Angular velocity labels
		GUIContent _gcAvStr, _gcAvMin, _gcAvMax, _gcAvSmooth;
		// Linear acceleration labels
		GUIContent _gcLaStr, _gcLaMin, _gcLaMax, _gcLaSmooth;
		// Linear velocity labels
		GUIContent _gcLvStr, _gcLvMin, _gcLvMax, _gcLvSmooth;
		#endregion

		#region State
		TunnellingBase _tb;
		Texture _headerLogo;

		static protected bool _showEffectSettings = true;
		static SectionToggle _showMotionDetection = new SectionToggle("useAngularVelocity", "useAcceleration", "useVelocity");
		static SectionToggle _showMotionEffects = new SectionToggle(
			"useCounterMotion", "useArtificialTilt", 
			new SectionToggle.PropInfo("framerateDivision", (x)=>{return x.intValue>1;})
		);
		#endregion

		#region Debug
		protected bool _showDebug = false;
		FieldInfo _fiDebugForceOn, _fiDebugForceVal;
		FieldInfo _fiDebugMotion, _fiDebugAv, _fiDebugLa, _fiDebugLv;
		float _debugAvMax, _debugLaMax, _debugLvMax;
		const string DEBUG_MOTION_FORMAT = "{0:0.0000}";

		GUIContent _gcDebugLabel = new GUIContent("Debug / Diagnostics", "Editor-only debug info.");
		GUIContent _gcDebugForceOn = new GUIContent("Force Effect", "Use a constant value for the effect rather than motion.");
		GUIContent _gcDebugMotion = new GUIContent("Motion Analyzer", "Show calculated motion data to help fine-tune settings.");

		GUIContent _gcDebugAvLabel = new GUIContent("Angular", "Current Angular Velocity [Degrees per Second]");
		GUIContent _gcDebugLaLabel = new GUIContent("Accel", "Current Acceleration [Meters per Second squared]");
		GUIContent _gcDebugLvLabel = new GUIContent("Vel", "Current Velocity [Meters per Second]");

		GUIContent _gcDebugMotionResetAll = new GUIContent("Reset All", "Reset max values");
		GUIContent _gcDebugMotionResetBtn = new GUIContent("Reset", "Reset max value");
		#endregion

		protected abstract void CacheProperties();
		protected abstract void DrawSettings();
		protected virtual void DrawDebugOptions(){}

		protected void InitAps(params AutoProperty[] ap){
			foreach (var a in ap){
				a.Init(serializedObject);
			}
		}

		protected void OnEnable(){
			_tb = (TunnellingBase)target;
			_headerLogo = VrtpEditorUtils.LoadTexture(HEADER_LOGO_NAME);

			InitAps(
				_pTarget, _pFxColor, _pFxCover, _pFxFeather, _pFxSkybox, _pApplyColorToBkg,
				_pAvUse,_pAvStr, _pAvMin, _pAvMax, _pAvSmooth,
				_pLaUse, _pLaStr, _pLaMin, _pLaMax, _pLaSmooth,
				_pLvUse, _pLvStr, _pLvMin, _pLvMax, _pLvSmooth,
				_pCounterMotion, _pCounterRotStr, _pCounterRotAxs,
				_pMotionEffectTarget,
				_pArtTilt, _pArtTiltStr, _pArtTiltMax, _pArtTiltSmooth,
				_pDivFps, _pDivTrans, _pDivRot
			);

			SectionToggle.Init(serializedObject, _showMotionDetection, _showMotionEffects);

			#region Motion detection labels
			_gcAvStr = new GUIContent(MOTION_STRENGTH, string.Format(MOTION_STR_FMT, MOTION_AV));
			_gcLaStr = new GUIContent(MOTION_STRENGTH, string.Format(MOTION_STR_FMT, MOTION_LA));
			_gcLvStr = new GUIContent(MOTION_STRENGTH, string.Format(MOTION_STR_FMT, MOTION_LV));

			_gcAvMin = new GUIContent(MOTION_MIN, string.Format(MOTION_MIN_FMT, MOTION_AV, MOTION_AV_UNITS));
			_gcLaMin = new GUIContent(MOTION_MIN, string.Format(MOTION_MIN_FMT, MOTION_LA, MOTION_LA_UNITS));
			_gcLvMin = new GUIContent(MOTION_MIN, string.Format(MOTION_MIN_FMT, MOTION_LV, MOTION_LV_UNITS));

			_gcAvMax = new GUIContent(MOTION_MAX, string.Format(MOTION_MAX_FMT, MOTION_AV, MOTION_AV_UNITS));
			_gcLaMax = new GUIContent(MOTION_MAX, string.Format(MOTION_MAX_FMT, MOTION_LA, MOTION_LA_UNITS));
			_gcLvMax = new GUIContent(MOTION_MAX, string.Format(MOTION_MAX_FMT, MOTION_LV, MOTION_LV_UNITS));

			_gcAvSmooth = new GUIContent(MOTION_SMOOTH, string.Format(MOTION_SMOOTH_FMT, MOTION_AV));
			_gcLaSmooth = new GUIContent(MOTION_SMOOTH, string.Format(MOTION_SMOOTH_FMT, MOTION_LA));
			_gcLvSmooth = new GUIContent(MOTION_SMOOTH, string.Format(MOTION_SMOOTH_FMT, MOTION_LV));
			#endregion

			_fiDebugMotion = typeof(TunnellingBase).GetField("_debugMotionCalculations", BindingFlags.Instance | BindingFlags.NonPublic);
			_fiDebugAv = typeof(TunnellingBase).GetField("_debugAv", BindingFlags.Instance | BindingFlags.NonPublic);
			_fiDebugLa = typeof(TunnellingBase).GetField("_debugLa", BindingFlags.Instance | BindingFlags.NonPublic);
			_fiDebugLv = typeof(TunnellingBase).GetField("_debugLv", BindingFlags.Instance | BindingFlags.NonPublic);
			_fiDebugForceOn = typeof(TunnellingBase).GetField("_debugForceOn", BindingFlags.Instance | BindingFlags.NonPublic);
			_fiDebugForceVal = typeof(TunnellingBase).GetField("_debugForceValue", BindingFlags.Instance | BindingFlags.NonPublic);

			CacheProperties();

			EditorApplication.update += OnEditorUpdate;
		}
		void OnDisable(){
			EditorApplication.update -= OnEditorUpdate;
		}

		void OnEditorUpdate(){
			Repaint();
		}
		public override void OnInspectorGUI(){
			// Draw header
			EditorGUI.BeginChangeCheck();

			VrtpEditorUtils.DrawImage(_headerLogo, 77, new Vector2(0,4));

			EditorGUILayout.Space();
			VrtpStyles.BeginSectionBox(); {
				EditorGUILayout.PropertyField(_pTarget);
				if (_tb.motionTarget == null){
					EditorGUILayout.HelpBox("No motion target specified!", MessageType.Error);
				} else if (_tb.motionTarget == _tb.transform){
					EditorGUILayout.HelpBox("Motion Target generally shouldn't be the HMD", MessageType.Warning);
				}
			} VrtpStyles.EndSectionBox();

			VrtpStyles.BeginSectionBox(); {
				++EditorGUI.indentLevel;
				_showEffectSettings = EditorGUILayout.Foldout(_showEffectSettings, "Effect Settings", VrtpStyles.sectionFoldout);
				--EditorGUI.indentLevel;

				if (_showEffectSettings) {
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(_pFxColor);
					EditorGUILayout.PropertyField(_pFxCover);
					EditorGUILayout.PropertyField(_pFxFeather);
				}
			} VrtpStyles.EndSectionBox();

			// Draw content
			DrawSettings();

			// Finalise
			if (EditorGUI.EndChangeCheck()) {
				serializedObject.ApplyModifiedProperties();
			}

			#region Debug
			VrtpStyles.BeginSectionBox(); {
				++EditorGUI.indentLevel;
				EditorGUILayout.BeginHorizontal(); {
					_showDebug = EditorGUILayout.Foldout(_showDebug, _gcDebugLabel, VrtpStyles.sectionFoldout);
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("About VRTP", EditorStyles.miniButton)){
						VrtpAboutWindow.Open();
					}
					if (GUILayout.Button("Open Manual", EditorStyles.miniButton)){
						Application.OpenURL(URL_DOCS);
					}
				} EditorGUILayout.EndHorizontal();
				--EditorGUI.indentLevel;

				if (_showDebug) {
					bool forceOn = (bool)_fiDebugForceOn.GetValue(_tb);
					float forceValue = (float)_fiDebugForceVal.GetValue(_tb);
					EditorGUI.BeginChangeCheck();
					forceOn = EditorGUILayout.ToggleLeft(_gcDebugForceOn, forceOn, VrtpStyles.sectionHeader);
					if (forceOn){
						++EditorGUI.indentLevel;
						forceValue = EditorGUILayout.Slider("Strength", forceValue, 0, 1);
						--EditorGUI.indentLevel;
					}
					if (EditorGUI.EndChangeCheck()){
						_fiDebugForceOn.SetValue(_tb, forceOn);
						_fiDebugForceVal.SetValue(_tb, forceValue);
					}

					bool showMotionDebug = (bool)_fiDebugMotion.GetValue(_tb);
					EditorGUI.BeginChangeCheck();
					showMotionDebug = EditorGUILayout.ToggleLeft(_gcDebugMotion, showMotionDebug, VrtpStyles.sectionHeader);
					if (EditorGUI.EndChangeCheck()) {
						_fiDebugMotion.SetValue(_tb, showMotionDebug);
						// Reset peak motion data each time toggled
						_debugAvMax = _debugLaMax = _debugLvMax = 0;
					}
					if (showMotionDebug) {
						++EditorGUI.indentLevel;
						float currentAv, currentLa, currentLv;
						currentAv = currentLa = currentLv = 0;
						if (Application.isPlaying) {
							#region Get motion data
							currentAv = (float)_fiDebugAv.GetValue(_tb);
							currentLa = (float)_fiDebugLa.GetValue(_tb);
							currentLv = (float)_fiDebugLv.GetValue(_tb);
							_debugAvMax = Mathf.Max(currentAv, _debugAvMax);
							_debugLaMax = Mathf.Max(currentLa, _debugLaMax);
							_debugLvMax = Mathf.Max(currentLv, _debugLvMax);
							#endregion
						} else {
							GUI.enabled = false;
							_debugAvMax = _debugLaMax = _debugLvMax = 0;
						}

						#region Draw
						GUILayoutOption[] options = new GUILayoutOption[]{ GUILayout.Width(72) };

						EditorGUILayout.BeginHorizontal(); {
							EditorGUILayout.LabelField("Motion", EditorStyles.boldLabel, options);
							EditorGUILayout.LabelField("Current", EditorStyles.boldLabel, options);
							EditorGUILayout.LabelField("Max", EditorStyles.boldLabel, options);
							if (GUILayout.Button(_gcDebugMotionResetAll, options)){
								_debugAvMax = _debugLaMax = _debugLvMax = 0;
							}
						} EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(); {
							EditorGUILayout.LabelField(_gcDebugAvLabel, options);
							EditorGUILayout.LabelField(string.Format(DEBUG_MOTION_FORMAT, currentAv), options);
							EditorGUILayout.LabelField(string.Format(DEBUG_MOTION_FORMAT, _debugAvMax), options);
							if (GUILayout.Button(_gcDebugMotionResetBtn, options)) _debugAvMax = 0;
						} EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(); {
							EditorGUILayout.LabelField(_gcDebugLaLabel, options);
							EditorGUILayout.LabelField(string.Format(DEBUG_MOTION_FORMAT, currentLa), options);
							EditorGUILayout.LabelField(string.Format(DEBUG_MOTION_FORMAT, _debugLaMax), options);
							if (GUILayout.Button(_gcDebugMotionResetBtn, options)) _debugLaMax = 0;
						} EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(); {
							EditorGUILayout.LabelField(_gcDebugLvLabel, options);
							EditorGUILayout.LabelField(string.Format(DEBUG_MOTION_FORMAT, currentLv), options);
							EditorGUILayout.LabelField(string.Format(DEBUG_MOTION_FORMAT, _debugLvMax), options);
							if (GUILayout.Button(_gcDebugMotionResetBtn, options)) _debugLvMax = 0;
						} EditorGUILayout.EndHorizontal();
						#endregion
						GUI.enabled = true;
						--EditorGUI.indentLevel;
					}
					DrawDebugOptions();
				}
			} VrtpStyles.EndSectionBox();

			#endregion
		}
		protected void DrawMotionSettings(){
			#region Detection
			VrtpStyles.BeginSectionBox(); {
				++EditorGUI.indentLevel;
				_showMotionDetection.active = EditorGUILayout.Foldout(_showMotionDetection, "Motion Detection", VrtpStyles.sectionFoldout);
				--EditorGUI.indentLevel;
				if (_showMotionDetection) {
					EditorGUILayout.Space();

					#region Angular Velocity
					VrtpStyles.BeginChildBox(); {
						VrtpEditorUtils.ToggleProperty(_pAvUse, _pAvUse.content, VrtpStyles.sectionHeader);
						if (_tb.useAngularVelocity) {
							++EditorGUI.indentLevel;
							EditorGUILayout.PropertyField(_pAvStr, _gcAvStr);
							EditorGUILayout.PropertyField(_pAvMin, _gcAvMin);
							EditorGUILayout.PropertyField(_pAvMax, _gcAvMax);
							EditorGUILayout.PropertyField(_pAvSmooth, _gcAvSmooth);
							--EditorGUI.indentLevel;
						}
					} VrtpStyles.EndChildBox();
					#endregion

					#region Linear Acceleration
					VrtpStyles.BeginChildBox(); {
						VrtpEditorUtils.ToggleProperty(_pLaUse, _pLaUse.content, VrtpStyles.sectionHeader);
						if (_tb.useAcceleration) {
							++EditorGUI.indentLevel;
							EditorGUILayout.PropertyField(_pLaStr, _gcLaStr);
							EditorGUILayout.PropertyField(_pLaMin, _gcLaMin);
							EditorGUILayout.PropertyField(_pLaMax, _gcLaMax);
							EditorGUILayout.PropertyField(_pLaSmooth, _gcLaSmooth);
							--EditorGUI.indentLevel;
						}
					} VrtpStyles.EndChildBox();
					#endregion

					#region Linear Velocity
					VrtpStyles.BeginChildBox(); {
						VrtpEditorUtils.ToggleProperty(_pLvUse, _pLvUse.content, VrtpStyles.sectionHeader);
						if (_tb.useVelocity) {
							++EditorGUI.indentLevel;
							EditorGUILayout.PropertyField(_pLvStr, _gcLvStr);
							EditorGUILayout.PropertyField(_pLvMin, _gcLvMin);
							EditorGUILayout.PropertyField(_pLvMax, _gcLvMax);
							EditorGUILayout.PropertyField(_pLvSmooth, _gcLvSmooth);
							--EditorGUI.indentLevel;
						}
					} VrtpStyles.EndChildBox();
					#endregion
				}
			} VrtpStyles.EndSectionBox();
			#endregion

			#region Effects
			VrtpStyles.BeginSectionBox(); {
				++EditorGUI.indentLevel;
				_showMotionEffects.active = EditorGUILayout.Foldout(_showMotionEffects, "Motion Effects", VrtpStyles.sectionFoldout);
				--EditorGUI.indentLevel;
				if (_showMotionEffects){
					EditorGUILayout.Space();

					#region Counter-motion
					VrtpStyles.BeginChildBox(); {
						VrtpEditorUtils.ToggleProperty(_pCounterMotion, null, VrtpStyles.sectionHeader);
						if (_pCounterMotion.p.boolValue){
							++EditorGUI.indentLevel;
							DrawCounterMotionSettings();
							--EditorGUI.indentLevel;
						}
					} VrtpStyles.EndChildBox();
					#endregion

					EditorGUILayout.Separator();
					EditorGUILayout.PropertyField(_pMotionEffectTarget);
					if (_tb.motionEffectTarget == null && _tb.usingMotionEffectTarget){
						EditorGUILayout.HelpBox("No motion effect target specified!", MessageType.Error);
					}

					#region Auto-tilt
					VrtpStyles.BeginChildBox(); {
						VrtpEditorUtils.ToggleProperty(_pArtTilt, null, VrtpStyles.sectionHeader);
						if (_pArtTilt.p.boolValue){
							++EditorGUI.indentLevel;
							EditorGUILayout.PropertyField(_pArtTiltStr);
							EditorGUILayout.PropertyField(_pArtTiltMax);
							EditorGUILayout.PropertyField(_pArtTiltSmooth);
							--EditorGUI.indentLevel;
						}
					} VrtpStyles.EndChildBox();
					#endregion

					VrtpStyles.BeginChildBox(); {
						EditorGUILayout.PropertyField(_pDivFps);
						if (_pDivFps.p.intValue > 1){
							EditorGUILayout.PropertyField(_pDivTrans);
							EditorGUILayout.PropertyField(_pDivRot);
						}
					} VrtpStyles.EndChildBox();
				}
			} VrtpStyles.EndSectionBox();
			#endregion
		}
		protected virtual void DrawCounterMotionSettings(){
			VrtpEditorUtils.PropertyField(_pCounterRotStr);
			VrtpEditorUtils.PropertyField(_pCounterRotAxs);
		}
	}

	public class VrtpAboutWindow : EditorWindow {
		static VrtpAboutWindow _i;
		Texture _headerLogo;

		public static void Open(){
			if (_i == null){
				_i = ScriptableObject.CreateInstance<VrtpAboutWindow>();
				_i.minSize = _i.maxSize = new Vector2(320, 160);
				_i.titleContent = new GUIContent("About VRTP v" + VrTunnellingPro.TunnellingBase.VRTP_VERSION);
				_i._headerLogo = VrtpEditorUtils.LoadTexture(VrTunnellingProImageEditor.LOGO_NAME);
			}
			_i.ShowUtility();
		}

		void OnGUI(){
			VrtpEditorUtils.DrawImage(_headerLogo, 77, Vector2.zero, false);
			EditorGUILayout.LabelField("VR Tunnelling Pro");
			EditorGUILayout.LabelField("Version " + VrTunnellingPro.TunnellingBase.VRTP_VERSION);
			EditorGUILayout.LabelField("Copyright 2018 Sigtrap Ltd");
			EditorGUILayout.LabelField("All Rights Reserved");
		}
	}
}