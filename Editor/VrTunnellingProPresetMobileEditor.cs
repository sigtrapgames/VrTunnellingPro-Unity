using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Sigtrap.VrTunnellingPro.Editors {
	[CustomEditor(typeof(TunnellingPresetMobile))]
	public class VrTunnellingProPresetMobileEditor : VrTunnellingProPresetEditorBase {
		protected override string HEADER_LOGO_NAME {get {return "VrTunnellingProMobilePresetLogo";}}

		SerializedPropertyPair _pDrawSkybox;
		SerializedPropertyPair _pUseMask;
		SerializedPropertyPair _pStencilReference;
		SerializedPropertyPair _pStencilMask;
		SerializedPropertyPair _pStencilBias;

		protected override void CacheProperties(){
			_pDrawSkybox = new SerializedPropertyPair(serializedObject, "_drawSkybox");
			_pUseMask = new SerializedPropertyPair(serializedObject, "_useMask");
			_pStencilReference = new SerializedPropertyPair(serializedObject, "_stencilReference");
			_pStencilMask = new SerializedPropertyPair(serializedObject, "_stencilMask");
			_pStencilBias = new SerializedPropertyPair(serializedObject, "_stencilBias");
		}
		protected override void DrawSettings(){
			EditorGUILayout.Space();

			DrawProperty(_pEffectColor, true);
			DrawProperty(_pEffectCoverage, true);
			DrawProperty(_pEffectFeather, true);
			DrawProperty(_pApplyColor, true);
			DrawProperty(_pDrawSkybox, true);
			DrawProperty(_pSkybox, true);

			EditorGUILayout.Space();

			DrawProperty(_pUseMask, true);
			DrawProperty(_pStencilReference, true);
			DrawProperty(_pStencilMask, true);
			DrawProperty(_pStencilBias, true);

			EditorGUILayout.Space();
			DrawMotionSettings();
		}
	}
}