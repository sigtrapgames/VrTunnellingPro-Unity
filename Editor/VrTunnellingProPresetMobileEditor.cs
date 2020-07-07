using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Sigtrap.VrTunnellingPro.Editors {
	[CustomEditor(typeof(TunnellingPresetMobile)), CanEditMultipleObjects]
	public class VrTunnellingProPresetMobileEditor : VrTunnellingProPresetEditorBase {
		protected override string HEADER_LOGO_NAME {get {return "VrTunnellingProMobilePresetLogo";}}

		SerializedPropertyPair _pDrawSkybox = new SerializedPropertyPair("_drawSkybox");
		SerializedPropertyPair _pDrawBeforeTransparent = new SerializedPropertyPair("_drawBeforeTransparent");
		SerializedPropertyPair _pUseMask = new SerializedPropertyPair("_useMask");
		SerializedPropertyPair _pStencilReference = new SerializedPropertyPair("_stencilReference");
		SerializedPropertyPair _pStencilMask = new SerializedPropertyPair("_stencilMask");
		SerializedPropertyPair _pStencilBias = new SerializedPropertyPair("_stencilBias");

		static bool _showEffectSettings = true;
		static bool _showMaskSettings = true;

		protected override void CacheProperties(){
			InitSpps(_pDrawSkybox, _pDrawBeforeTransparent, _pUseMask, _pStencilReference, _pStencilMask, _pStencilBias);
		}
		protected override void DrawSettings(){
			EditorGUILayout.Space();

			_showEffectSettings = EditorGUILayout.Foldout(_showEffectSettings, "Effect Settings", VrtpStyles.sectionFoldout);
			if (_showEffectSettings){
				++EditorGUI.indentLevel;
				DrawProperty(_pEffectColor);
				DrawProperty(_pEffectCoverage);
				DrawProperty(_pEffectFeather);
				DrawProperty(_pApplyColor);
				DrawProperty(_pDrawSkybox);
				DrawProperty(_pSkybox);
				DrawProperty(_pDrawBeforeTransparent);
				--EditorGUI.indentLevel;
			}

			EditorGUILayout.Space();

			_showMaskSettings = EditorGUILayout.Foldout(_showMaskSettings, "Mask Settings", VrtpStyles.sectionFoldout);
			if (_showMaskSettings){
				++EditorGUI.indentLevel;
				DrawProperty(_pUseMask);
				DrawProperty(_pStencilReference);
				DrawProperty(_pStencilMask);
				DrawProperty(_pStencilBias);
				--EditorGUI.indentLevel;
			}

			EditorGUILayout.Space();
			DrawMotionSettings();
		}
	}
}