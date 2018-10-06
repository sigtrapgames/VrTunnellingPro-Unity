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
			public SerializedPropertyPair(SerializedObject s, string baseName){
				p = s.FindProperty(baseName);
				b = s.FindProperty("_override" + baseName.ToUpper()[1] + baseName.Substring(2));
			}
		}

		protected static readonly GUIContent _gcNull = new GUIContent();
		static readonly GUIContent _gcOverride = new GUIContent("Override?", "Settings without this box ticked will be ignored when using ApplyPreset().");
		static readonly GUIContent _gcOverrideAll = new GUIContent("All", "Override all settings (doesn't wipe individual overrides).");

		SerializedProperty _pOverrideAll;
		protected SerializedPropertyPair _pEffectCoverage;
		protected SerializedPropertyPair _pEffectColor;
		protected SerializedPropertyPair _pEffectFeather;
		protected SerializedPropertyPair _pApplyColor;
		protected SerializedPropertyPair _pSkybox;
		SerializedPropertyPair _pAv;
		SerializedPropertyPair _pLa;
		SerializedPropertyPair _pLv;

		Texture _headerLogo;

		bool _overrideAll;

		void OnEnable(){
			_pOverrideAll = serializedObject.FindProperty("_overrideAll");

			_pEffectCoverage = new SerializedPropertyPair(serializedObject, "_effectCoverage");
			_pEffectColor = new SerializedPropertyPair(serializedObject, "_effectColor");
			_pEffectFeather = new SerializedPropertyPair(serializedObject, "_effectFeather");
			_pApplyColor = new SerializedPropertyPair(serializedObject, "_applyColorToBackground");
			_pSkybox = new SerializedPropertyPair(serializedObject, "_skybox");
			_pAv = new SerializedPropertyPair(serializedObject, "_angularVelocity");
			_pLa = new SerializedPropertyPair(serializedObject, "_acceleration");
			_pLv = new SerializedPropertyPair(serializedObject, "_velocity");

			_headerLogo = Resources.Load<Texture>(HEADER_LOGO_PATH + HEADER_LOGO_NAME);

			CacheProperties();
		}

		protected abstract void CacheProperties();
		protected abstract void DrawSettings();

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
			DrawProperty(_pAv, true);
			DrawProperty(_pLa, true);
			DrawProperty(_pLv, true);
		}

		public override void OnInspectorGUI(){
			EditorGUI.BeginChangeCheck();

			VrTunnellingProEditorUtils.DrawImage(_headerLogo, 77, new Vector2(0,-4));

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
