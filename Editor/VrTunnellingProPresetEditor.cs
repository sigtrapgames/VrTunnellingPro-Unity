using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Sigtrap.VrTunnellingPro.Editors {
	[CustomEditor(typeof(TunnellingPreset)), CanEditMultipleObjects]
	public class VrTunnellingProPresetEditor : VrTunnellingProPresetEditorBase {
		protected override string HEADER_LOGO_NAME {get {return "VrTunnellingProPresetLogo";}}

		protected SerializedPropertyPair _pEffectOverlay = new SerializedPropertyPair("_effectOverlay");
		SerializedPropertyPair _pBackgroundMode = new SerializedPropertyPair("_backgroundMode");

		SerializedPropertyPair _pCageDownsample = new SerializedPropertyPair("_cageDownsample");
		SerializedPropertyPair _pCageAa = new SerializedPropertyPair("_cageAntiAliasing");
		SerializedPropertyPair _pCageUpdate = new SerializedPropertyPair("_cageUpdateEveryFrame");

		SerializedPropertyPair _pCageFogDensity = new SerializedPropertyPair("_cageFogDensity");
		SerializedPropertyPair _pCageFogPower = new SerializedPropertyPair("_cageFogPower");
		SerializedPropertyPair _pCageFogBlend = new SerializedPropertyPair("_cageFogBlend");

		SerializedPropertyPair _pMaskMode = new SerializedPropertyPair("_maskMode");

		SerializedPropertyPair _pBlurDownsample = new SerializedPropertyPair("_blurDownsample");
		SerializedPropertyPair _pBlurDistance = new SerializedPropertyPair("_blurDistance");
		SerializedPropertyPair _pBlurPasses = new SerializedPropertyPair("_blurPasses");
		SerializedPropertyPair _pBlurSamples = new SerializedPropertyPair("_blurSamples");

		SerializedPropertyPair _pCounterVelocityMode = new SerializedPropertyPair("_counterVelocityMode");
		SerializedPropertyPair _pCounterVelocityResetDistance = new SerializedPropertyPair("_counterVelocityResetDistance");
		SerializedPropertyPair _pCounterVelocityResetTime = new SerializedPropertyPair("_counterVelocityResetTime");
		SerializedPropertyPair _pCounterVelocityStrength = new SerializedPropertyPair("_counterVelocityStrength");
		SerializedPropertyPair _pCounterVelocityPerAxis = new SerializedPropertyPair("_counterVelocityPerAxis");

		SerializedPropertyPair _pIrisZRejection = new SerializedPropertyPair("_irisZRejection");

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
			InitSpps(
				_pEffectOverlay, _pBackgroundMode, _pCageDownsample, _pCageAa, _pCageUpdate,
				_pCageFogDensity, _pCageFogPower, _pCageFogBlend, _pMaskMode,
				_pBlurDownsample, _pBlurDistance, _pBlurPasses, _pBlurSamples,
				_pCounterVelocityMode, _pCounterVelocityResetDistance, _pCounterVelocityResetTime,
				_pCounterVelocityStrength, _pCounterVelocityPerAxis, _pIrisZRejection
			);
		}
		protected override void DrawSettings(){
			_showEffectSettings = EditorGUILayout.Foldout(_showEffectSettings, "Effect Settings", VrtpStyles.sectionFoldout);
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
			_showCageSettings = EditorGUILayout.Foldout(_showCageSettings, "Cage Settings", VrtpStyles.sectionFoldout);
			if (_showCageSettings) {
				++EditorGUI.indentLevel;
				DrawProperty(_pCageDownsample, false, _gcDownsample);
				DrawProperty(_pCageAa, false, _gcMsaa);
				DrawProperty(_pCageUpdate, false, _gcUpdate);
				--EditorGUI.indentLevel;
			}

			EditorGUILayout.Space();
			_showFogSettings = EditorGUILayout.Foldout(_showFogSettings, "Cage Fog", VrtpStyles.sectionFoldout);
			if (_showFogSettings) {
				++EditorGUI.indentLevel;
				DrawProperty(_pCageFogDensity, false, _gcDensity);
				DrawProperty(_pCageFogPower, false, _gcPower);
				DrawProperty(_pCageFogBlend, false, _gcBlend);
				--EditorGUI.indentLevel;
			}

			EditorGUILayout.Space();
			_showBlurSettings = EditorGUILayout.Foldout(_showBlurSettings, "Blur Settings", VrtpStyles.sectionFoldout);
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

		protected override void DrawCounterMotionSettings(){
			base.DrawCounterMotionSettings();
			EditorGUILayout.Space();
			DrawProperty(_pCounterVelocityMode);
			DrawProperty(_pCounterVelocityResetDistance);
			DrawProperty(_pCounterVelocityResetTime);
			DrawProperty(_pCounterVelocityStrength);
			DrawProperty(_pCounterVelocityPerAxis);
		}
	}
}