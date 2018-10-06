using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Sigtrap.VrTunnellingPro.Editors {
	public abstract class VrTunnellingProEditorBase : Editor {
		protected string URL_DOCS = "http://www.sigtrapgames.com/VrTunnellingPro/html/index.html";
		protected abstract string HEADER_LOGO_NAME {get;}

		#region SPs
		private SerializedProperty _pTarget;
		protected SerializedProperty _pFxColor;
		protected SerializedProperty _pFxCover;
		protected SerializedProperty _pFxFeather;
		protected SerializedProperty _pFxSkybox;
		protected SerializedProperty _pApplyColorToBkg;

		protected SerializedProperty _pAvUse;
		protected SerializedProperty _pAvStr;
		protected SerializedProperty _pAvMin;
		protected SerializedProperty _pAvMax;
		protected SerializedProperty _pAvSmooth;

		protected SerializedProperty _pLaUse;
		protected SerializedProperty _pLaStr;
		protected SerializedProperty _pLaMin;
		protected SerializedProperty _pLaMax;
		protected SerializedProperty _pLaSmooth;

		protected SerializedProperty _pLvUse;
		protected SerializedProperty _pLvStr;
		protected SerializedProperty _pLvMin;
		protected SerializedProperty _pLvMax;
		protected SerializedProperty _pLvSmooth;
		#endregion

		#region Labels
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

		static readonly GUIContent _gcAvLabel = new GUIContent(MOTION_AV, "Add angular velocity to effect strength?\nHelps players with average sim-sickness.");
		static readonly GUIContent _gcLaLabel = new GUIContent(MOTION_LA, "Add linear acceleration to effect strength?\nHelps players with above-average sim-sickness.");
		static readonly GUIContent _gcLvLabel = new GUIContent(MOTION_LV, "Add linear velocity to effect strength?\nHelps players with strong sim-sickness.");

		GUIContent _gcAvStr, _gcAvMin, _gcAvMax, _gcAvSmooth;
		GUIContent _gcLaStr, _gcLaMin, _gcLaMax, _gcLaSmooth;
		GUIContent _gcLvStr, _gcLvMin, _gcLvMax, _gcLvSmooth;
		#endregion

		TunnellingBase _tb;
		Texture _headerLogo;

		static protected bool _showEffectSettings = true;
		static bool _showMotionSettings = true;

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

		protected void OnEnable(){
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

			_pTarget = serializedObject.FindProperty("motionTarget");

			_pFxColor = serializedObject.FindProperty("effectColor");
			_pFxCover = serializedObject.FindProperty("effectCoverage");
			_pFxFeather = serializedObject.FindProperty("effectFeather");
			_pFxSkybox = serializedObject.FindProperty("effectSkybox");
			_pApplyColorToBkg = serializedObject.FindProperty("applyColorToBackground");

			_pAvUse = serializedObject.FindProperty("useAngularVelocity");
			_pAvStr = serializedObject.FindProperty("angularVelocityStrength");
			_pAvMin = serializedObject.FindProperty("angularVelocityMin");
			_pAvMax = serializedObject.FindProperty("angularVelocityMax");
			_pAvSmooth = serializedObject.FindProperty("angularVelocitySmoothing");

			_pLaUse = serializedObject.FindProperty("useAcceleration");
			_pLaStr = serializedObject.FindProperty("accelerationStrength");
			_pLaMin = serializedObject.FindProperty("accelerationMin");
			_pLaMax = serializedObject.FindProperty("accelerationMax");
			_pLaSmooth = serializedObject.FindProperty("accelerationSmoothing");

			_pLvUse = serializedObject.FindProperty("useVelocity");
			_pLvStr = serializedObject.FindProperty("velocityStrength");
			_pLvMin = serializedObject.FindProperty("velocityMin");
			_pLvMax = serializedObject.FindProperty("velocityMax");
			_pLvSmooth = serializedObject.FindProperty("velocitySmoothing");

			_tb = (TunnellingBase)target;

			_headerLogo = VrTunnellingProEditorUtils.LoadTexture(HEADER_LOGO_NAME);

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

			VrTunnellingProEditorUtils.DrawImage(_headerLogo, 77, new Vector2(0,4));

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(_pTarget);
			if (_tb.motionTarget == null){
				EditorGUILayout.HelpBox("No motion target specified!", MessageType.Error);
			} else if (_tb.motionTarget == _tb.transform){
				EditorGUILayout.HelpBox("Motion Target generally shouldn't be the HMD", MessageType.Warning);
			}

			_showEffectSettings = EditorGUILayout.Foldout(_showEffectSettings, "Effect Settings");
			if (_showEffectSettings) {
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				EditorGUILayout.PropertyField(_pFxColor);
				EditorGUILayout.PropertyField(_pFxCover);
				EditorGUILayout.PropertyField(_pFxFeather);
				EditorGUILayout.EndVertical();
			}

			// Draw content
			EditorGUILayout.Space();
			DrawSettings();

			// Finalise
			if (EditorGUI.EndChangeCheck()) {
				serializedObject.ApplyModifiedProperties();
			}

			#region Debug
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			++EditorGUI.indentLevel;
			EditorGUILayout.BeginHorizontal();
			{
				_showDebug = EditorGUILayout.Foldout(_showDebug, _gcDebugLabel);
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("About VRTP", EditorStyles.miniButton)){
					VrtpAboutWindow.Open();
				}
				if (GUILayout.Button("Open Manual", EditorStyles.miniButton)){
					Application.OpenURL(URL_DOCS);
				}
			}
			EditorGUILayout.EndHorizontal();
			if (_showDebug) {
				bool forceOn = (bool)_fiDebugForceOn.GetValue(_tb);
				float forceValue = (float)_fiDebugForceVal.GetValue(_tb);
				EditorGUI.BeginChangeCheck();
				forceOn = EditorGUILayout.ToggleLeft(_gcDebugForceOn, forceOn);
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
				showMotionDebug = EditorGUILayout.ToggleLeft(_gcDebugMotion, showMotionDebug);
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
					GUILayoutOption[] options = new GUILayoutOption[]{ GUILayout.Width(96) };

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Motion", EditorStyles.boldLabel, options);
					EditorGUILayout.LabelField("Current", EditorStyles.boldLabel, options);
					EditorGUILayout.LabelField("Max", EditorStyles.boldLabel, options);
					if (GUILayout.Button(_gcDebugMotionResetAll, options)){
						_debugAvMax = _debugLaMax = _debugLvMax = 0;
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(_gcDebugAvLabel, options);
					EditorGUILayout.LabelField(string.Format(DEBUG_MOTION_FORMAT, currentAv), options);
					EditorGUILayout.LabelField(string.Format(DEBUG_MOTION_FORMAT, _debugAvMax), options);
					if (GUILayout.Button(_gcDebugMotionResetBtn, options)) _debugAvMax = 0;
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(_gcDebugLaLabel, options);
					EditorGUILayout.LabelField(string.Format(DEBUG_MOTION_FORMAT, currentLa), options);
					EditorGUILayout.LabelField(string.Format(DEBUG_MOTION_FORMAT, _debugLaMax), options);
					if (GUILayout.Button(_gcDebugMotionResetBtn, options)) _debugLaMax = 0;
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(_gcDebugLvLabel, options);
					EditorGUILayout.LabelField(string.Format(DEBUG_MOTION_FORMAT, currentLv), options);
					EditorGUILayout.LabelField(string.Format(DEBUG_MOTION_FORMAT, _debugLvMax), options);
					if (GUILayout.Button(_gcDebugMotionResetBtn, options)) _debugLvMax = 0;
					EditorGUILayout.EndHorizontal();
					#endregion
					GUI.enabled = true;
					--EditorGUI.indentLevel;
				}
				DrawDebugOptions();
			}
			--EditorGUI.indentLevel;	
			EditorGUILayout.EndVertical();

			#endregion
		}
		protected void DrawMotionSettings(){
			_showMotionSettings = EditorGUILayout.Foldout(_showMotionSettings, "Motion Settings");
			if (_showMotionSettings) {
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				_pAvUse.boolValue = EditorGUILayout.ToggleLeft(_gcAvLabel, _pAvUse.boolValue);
				if (_tb.useAngularVelocity) {
					++EditorGUI.indentLevel;
					EditorGUILayout.PropertyField(_pAvStr, _gcAvStr);
					EditorGUILayout.PropertyField(_pAvMin, _gcAvMin);
					EditorGUILayout.PropertyField(_pAvMax, _gcAvMax);
					EditorGUILayout.PropertyField(_pAvSmooth, _gcAvSmooth);
					--EditorGUI.indentLevel;
				}
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				_pLaUse.boolValue = EditorGUILayout.ToggleLeft(_gcLaLabel, _pLaUse.boolValue);
				if (_tb.useAcceleration) {
					++EditorGUI.indentLevel;
					EditorGUILayout.PropertyField(_pLaStr, _gcLaStr);
					EditorGUILayout.PropertyField(_pLaMin, _gcLaMin);
					EditorGUILayout.PropertyField(_pLaMax, _gcLaMax);
					EditorGUILayout.PropertyField(_pLaSmooth, _gcLaSmooth);
					--EditorGUI.indentLevel;
				}
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				_pLvUse.boolValue = EditorGUILayout.ToggleLeft(_gcLvLabel, _pLvUse.boolValue);
				if (_tb.useVelocity) {
					++EditorGUI.indentLevel;
					EditorGUILayout.PropertyField(_pLvStr, _gcLvStr);
					EditorGUILayout.PropertyField(_pLvMin, _gcLvMin);
					EditorGUILayout.PropertyField(_pLvMax, _gcLvMax);
					EditorGUILayout.PropertyField(_pLvSmooth, _gcLvSmooth);
					--EditorGUI.indentLevel;
				}
				EditorGUILayout.EndVertical();
			}
		}
	}

	public class VrtpAboutWindow : EditorWindow {
		static VrtpAboutWindow _i;
		Texture _headerLogo;

		public static void Open(){
			if (_i == null){
				_i = new VrtpAboutWindow();
				_i.minSize = _i.maxSize = new Vector2(320, 160);
				_i.titleContent = new GUIContent("About VRTP v" + VrTunnellingPro.TunnellingBase.VRTP_VERSION);
				_i._headerLogo = VrTunnellingProEditorUtils.LoadTexture(VrTunnellingProImageEditor.LOGO_NAME);
			}
			_i.ShowUtility();
		}

		void OnGUI(){
			VrTunnellingProEditorUtils.DrawImage(_headerLogo, 77, Vector2.zero, false);
			EditorGUILayout.LabelField("VR Tunnelling Pro");
			EditorGUILayout.LabelField("Version " + VrTunnellingPro.TunnellingBase.VRTP_VERSION);
			EditorGUILayout.LabelField("Copyright 2018 Sigtrap Ltd");
			EditorGUILayout.LabelField("All Rights Reserved");
		}
	}
}