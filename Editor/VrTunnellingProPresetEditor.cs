using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Sigtrap.VrTunnellingPro.Editors {
	[CustomEditor(typeof(TunnellingPreset))]
	public class VrTunnellingProPresetEditor : VrTunnellingProPresetEditorBase {
		protected override string HEADER_LOGO_NAME {get {return "VrTunnellingProPresetLogo";}}

		SerializedPropertyPair _pBackgroundMode;

		SerializedPropertyPair _pCageDownsample;
		SerializedPropertyPair _pCageAa;
		SerializedPropertyPair _pCageUpdate;

		SerializedPropertyPair _pCageFogDensity;
		SerializedPropertyPair _pCageFogPower;
		SerializedPropertyPair _pCageFogBlend;

		SerializedPropertyPair _pMaskMode;

		SerializedPropertyPair _pBlurDownsample;
		SerializedPropertyPair _pBlurDistance;
		SerializedPropertyPair _pBlurPasses;
		SerializedPropertyPair _pBlurSamples;

		SerializedPropertyPair _pIrisZRejection;

		static readonly GUIContent _gcDownsample = new GUIContent("Downsample");
		static readonly GUIContent _gcMsaa = new GUIContent("MSAA");
		static readonly GUIContent _gcUpdate = new GUIContent("Update Every Frame");
		static readonly GUIContent _gcDensity = new GUIContent("Density");
		static readonly GUIContent _gcPower = new GUIContent("Power");
		static readonly GUIContent _gcBlend = new GUIContent("Strength");
		static readonly GUIContent _gcDistance = new GUIContent("Distance");
		static readonly GUIContent _gcPasses = new GUIContent("Passes");
		static readonly GUIContent _gcSamples = new GUIContent("Samples");

		static bool _showEffectSettings = true;
		static bool _showCageSettings = true;
		static bool _showFogSettings = true;
		static bool _showBlurSettings = true;

		protected override void CacheProperties(){
			_pBackgroundMode = new SerializedPropertyPair(serializedObject, "_backgroundMode");

			_pCageDownsample = new SerializedPropertyPair(serializedObject, "_cageDownsample");
			_pCageAa = new SerializedPropertyPair(serializedObject, "_cageAntiAliasing");
			_pCageUpdate = new SerializedPropertyPair(serializedObject, "_cageUpdateEveryFrame");

			_pCageFogDensity = new SerializedPropertyPair(serializedObject, "_cageFogDensity");
			_pCageFogPower = new SerializedPropertyPair(serializedObject, "_cageFogPower");
			_pCageFogBlend = new SerializedPropertyPair(serializedObject, "_cageFogBlend");

			_pMaskMode = new SerializedPropertyPair(serializedObject, "_maskMode");

			_pBlurDownsample = new SerializedPropertyPair(serializedObject, "_blurDownsample");
			_pBlurDistance = new SerializedPropertyPair(serializedObject, "_blurDistance");
			_pBlurPasses = new SerializedPropertyPair(serializedObject, "_blurPasses");
			_pBlurSamples = new SerializedPropertyPair(serializedObject, "_blurSamples");

			_pIrisZRejection = new SerializedPropertyPair(serializedObject, "_irisZRejection");
		}
		protected override void DrawSettings(){
			_showEffectSettings = EditorGUILayout.Foldout(_showEffectSettings, "Effect Settings", EditorStyles.boldFont);
			if (_showEffectSettings) {
				++EditorGUI.indentLevel;
				DrawProperty(_pEffectColor);
				DrawProperty(_pEffectCoverage);
				DrawProperty(_pEffectFeather);
				DrawProperty(_pApplyColor);
				DrawProperty(_pSkybox);

				DrawProperty(_pBackgroundMode);
				DrawProperty(_pIrisZRejection);
				--EditorGUI.indentLevel;
			}

			EditorGUILayout.Space();
			_showCageSettings = EditorGUILayout.Foldout(_showCageSettings, "Cage Settings", EditorStyles.boldFont);
			if (_showCageSettings) {
				++EditorGUI.indentLevel;
				DrawProperty(_pCageDownsample, false, _gcDownsample);
				DrawProperty(_pCageAa, false, _gcMsaa);
				DrawProperty(_pCageUpdate, false, _gcUpdate);
				--EditorGUI.indentLevel;
			}

			EditorGUILayout.Space();
			_showFogSettings = EditorGUILayout.Foldout(_showFogSettings, "Cage Fog", EditorStyles.boldFont);
			if (_showFogSettings) {
				++EditorGUI.indentLevel;
				DrawProperty(_pCageFogDensity, false, _gcDensity);
				DrawProperty(_pCageFogPower, false, _gcPower);
				DrawProperty(_pCageFogBlend, false, _gcBlend);
				--EditorGUI.indentLevel;
			}

			EditorGUILayout.Space();
			_showBlurSettings = EditorGUILayout.Foldout(_showBlurSettings, "Blur Settings", EditorStyles.boldFont);
			if (_showBlurSettings) {
				++EditorGUI.indentLevel;
				DrawProperty(_pBlurDownsample, false, _gcDownsample);
				DrawProperty(_pBlurDistance, false, _gcDistance);
				DrawProperty(_pBlurPasses, false, _gcPasses);
				DrawProperty(_pBlurSamples, false, _gcSamples);
				--EditorGUI.indentLevel;
			}

			EditorGUILayout.Space();
			DrawProperty(_pMaskMode, true);

			EditorGUILayout.Space();
			DrawMotionSettings();
		}
	}
}