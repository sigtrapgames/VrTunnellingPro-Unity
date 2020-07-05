using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Sigtrap.VrTunnellingPro.Editors {
	public abstract class VrTunnellingProPresetEditorBase : Editor {
		const string HEADER_LOGO_PATH = "Logos/";
		protected abstract string HEADER_LOGO_NAME {get;}

		protected class SerializedPropertyPair {
			public SerializedProperty p;
			public SerializedProperty b;
			public bool isReady {get {return p != null && b !=null;}}
			string _baseName;

			public SerializedPropertyPair(string baseName){
				_baseName = baseName;
			}
			public void Init(SerializedObject s){
				p = s.FindProperty(_baseName);
				b = s.FindProperty("_override" + _baseName.ToUpper()[1] + _baseName.Substring(2));
			}
		}

		protected static readonly GUIContent _gcNull = new GUIContent();
		static readonly GUIContent _gcOverride = new GUIContent("Override?", "Settings without this box ticked will be ignored when using ApplyPreset().");
		static readonly GUIContent _gcOverrideAll = new GUIContent("All", "Override all settings (doesn't wipe individual overrides).");

		SerializedProperty _pOverrideAll;
		protected SerializedPropertyPair _pEffectCoverage = new SerializedPropertyPair("_effectCoverage");
		protected SerializedPropertyPair _pEffectColor = new SerializedPropertyPair("_effectColor");
		protected SerializedPropertyPair _pEffectFeather = new SerializedPropertyPair("_effectFeather");
		protected SerializedPropertyPair _pApplyColor = new SerializedPropertyPair("_applyColorToBackground");
		protected SerializedPropertyPair _pSkybox = new SerializedPropertyPair("_skybox");
		SerializedPropertyPair _pAv = new SerializedPropertyPair("_angularVelocity");
		SerializedPropertyPair _pLa = new SerializedPropertyPair("_acceleration");
		SerializedPropertyPair _pLv = new SerializedPropertyPair("_velocity");

		protected SerializedPropertyPair _pCounterMotion = new SerializedPropertyPair("_useCounterMotion");
		protected SerializedPropertyPair _pCounterRotationStrength = new SerializedPropertyPair("_counterRotationStrength");
		protected SerializedPropertyPair _pCounterRotationPerAxis = new SerializedPropertyPair("_counterRotationPerAxis");

		protected SerializedPropertyPair _pArtificialTilt = new SerializedPropertyPair("_useArtificialTilt");
		protected SerializedPropertyPair _pFramerateDivision = new SerializedPropertyPair("_framerateDivision");
		protected SerializedPropertyPair _pDivideTranslation = new SerializedPropertyPair("_divideTranslation");
		protected SerializedPropertyPair _pDivideRotation = new SerializedPropertyPair("_divideRotation");

		protected SerializedPropertyPair _pForceVigMode = new SerializedPropertyPair("_forceVignetteMode");
		protected SerializedPropertyPair _pForceVigVal = new SerializedPropertyPair("_forceVignetteValue");

		Texture _headerLogo;

		bool _overrideAll;

		static bool _showMotionDetection = true;
		static bool _showCounterMotion = true;
		static bool _showArtificialTilt = true;
		static bool _showFramerateDivision = true;

		void OnEnable(){
			_headerLogo = Resources.Load<Texture>(HEADER_LOGO_PATH + HEADER_LOGO_NAME);
			_pOverrideAll = serializedObject.FindProperty("_overrideAll");
			InitSpps(
				_pEffectCoverage, _pEffectColor, _pEffectFeather, _pApplyColor, _pSkybox,
				_pAv, _pLa, _pLv,
				_pCounterMotion, _pCounterRotationStrength, _pCounterRotationPerAxis,
				_pArtificialTilt, _pFramerateDivision, _pDivideTranslation, _pDivideRotation,
				_pForceVigMode, _pForceVigVal
			);
			CacheProperties();
		}

		protected abstract void CacheProperties();
		protected abstract void DrawSettings();

		protected void InitSpps(params SerializedPropertyPair[] pairs){
			foreach (var p in pairs){
				p.Init(serializedObject);
			}
		}

		protected void DrawProperty(SerializedPropertyPair p, bool indentBool=false, GUIContent label=null){
			EditorGUILayout.BeginHorizontal();
			if (label == null) {
				EditorGUILayout.PropertyField(p.p, true);
			} else {
				EditorGUILayout.PropertyField(p.p, label, true);
			}
			if (indentBool){
				++EditorGUI.indentLevel;
			}
			if (_overrideAll){
				GUI.enabled = false;
			}
			EditorGUILayout.PropertyField(p.b, _gcNull, true, GUILayout.Width(32));
			GUI.enabled = true;
			if (indentBool){
				--EditorGUI.indentLevel;
			}
			EditorGUILayout.EndHorizontal();
		}
		protected void DrawMotionSettings(){
			_showMotionDetection = EditorGUILayout.Foldout(_showMotionDetection, "Motion Detection", VrtpStyles.sectionFoldout);
			if (_showMotionDetection){
				++EditorGUI.indentLevel;
				DrawProperty(_pAv, false);
				DrawProperty(_pLa, false);
				DrawProperty(_pLv, false);
				EditorGUILayout.Space();
				DrawProperty(_pForceVigMode);
				DrawProperty(_pForceVigVal);
				--EditorGUI.indentLevel;
			}

			_showCounterMotion = EditorGUILayout.Foldout(_showCounterMotion, "Counter Motion", VrtpStyles.sectionFoldout);
			if (_showCounterMotion){
				++EditorGUI.indentLevel;
				DrawCounterMotionSettings();
				--EditorGUI.indentLevel;
			}

			_showArtificialTilt = EditorGUILayout.Foldout(_showArtificialTilt, "Artificial Tilt", VrtpStyles.sectionFoldout);
			if (_showArtificialTilt){
				++EditorGUI.indentLevel;
				DrawProperty(_pArtificialTilt);
				--EditorGUI.indentLevel;
			}

			_showFramerateDivision = EditorGUILayout.Foldout(_showFramerateDivision, "Framerate Division", VrtpStyles.sectionFoldout);
			if (_showFramerateDivision){
				++EditorGUI.indentLevel;
				DrawProperty(_pFramerateDivision);
				DrawProperty(_pDivideTranslation);
				DrawProperty(_pDivideRotation);
				--EditorGUI.indentLevel;
			}
		}

		protected virtual void DrawCounterMotionSettings(){
			DrawProperty(_pCounterMotion);
			DrawProperty(_pCounterRotationStrength);
			DrawProperty(_pCounterRotationPerAxis);
		}

		public override void OnInspectorGUI(){
			EditorGUI.BeginChangeCheck();

			VrtpEditorUtils.DrawImage(_headerLogo, 77, new Vector2(0,-4));

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
			EditorGUILayout.LabelField(_gcOverride, EditorStyles.boldLabel, GUILayout.Width(68));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			++EditorGUI.indentLevel;
			EditorGUILayout.LabelField(_gcOverrideAll, GUILayout.Width(32));
			EditorGUILayout.PropertyField(_pOverrideAll, _gcNull, true, GUILayout.Width(32));
			--EditorGUI.indentLevel;
			_overrideAll = _pOverrideAll.boolValue;
			EditorGUILayout.EndHorizontal();

			DrawSettings();

			if (EditorGUI.EndChangeCheck()){
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
