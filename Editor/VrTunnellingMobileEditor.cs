using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Sigtrap.VrTunnellingPro.Editors {
	[CustomEditor(typeof(TunnellingMobile), true)]
	public class VrTunnellingMobileEditor : VrTunnellingProEditorBase {
		override protected string HEADER_LOGO_NAME {get {return "VrTunnellingProMobileLogo";}}
		AutoProperty _pDrawSkybox = new AutoProperty("drawSkybox");
		AutoProperty _pUseMask = new AutoProperty("useMask");
		AutoProperty _pStencilRef = new AutoProperty("stencilReference");
		AutoProperty _pStencilMask = new AutoProperty("stencilMask");
		AutoProperty _pStencilBias = new AutoProperty("stencilBias");
		TunnellingMobile _tm;

		GUIContent _gcApplyColor = new GUIContent("Apply Color", "Apply Effect Color to Skybox");

		protected override void CacheProperties(){
			InitAps(_pDrawSkybox, _pUseMask, _pStencilRef, _pStencilMask, _pStencilBias);
			_tm = (TunnellingMobile)target;
		}

		protected override void DrawSettings(){
			VrtpStyles.BeginSectionBox(); {
				VrtpEditorUtils.ToggleProperty(_pDrawSkybox, null, VrtpStyles.sectionHeader);
				if (_tm.drawSkybox){
					++EditorGUI.indentLevel;
					EditorGUILayout.PropertyField(_pApplyColorToBkg, _gcApplyColor);
					EditorGUILayout.PropertyField(_pFxSkybox);
					--EditorGUI.indentLevel;
				}
			} VrtpStyles.EndSectionBox();

			VrtpStyles.BeginSectionBox(); {
				VrtpEditorUtils.ToggleProperty(_pUseMask, null, VrtpStyles.sectionHeader);
				if (_tm.useMask){
					++EditorGUI.indentLevel;
					EditorGUILayout.PropertyField(_pStencilRef);
					EditorGUILayout.PropertyField(_pStencilMask);
					EditorGUILayout.PropertyField(_pStencilBias);
					EditorGUILayout.HelpBox("Mask may stress drawcalls and fillrate.", MessageType.Warning);
					--EditorGUI.indentLevel;
				}
			} VrtpStyles.EndSectionBox();

			DrawMotionSettings();
		}
	}
}
