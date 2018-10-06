using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Sigtrap.VrTunnellingPro.Editors {
	[CustomEditor(typeof(TunnellingMobile), true)]
	public class VrTunnellingMobileEditor : VrTunnellingProEditorBase {
		override protected string HEADER_LOGO_NAME {get {return "VrTunnellingProMobileLogo";}}
		SerializedProperty _pDrawSkybox;
		SerializedProperty _pUseMask, _pStencilRef, _pStencilMask, _pStencilBias;
		TunnellingMobile _tm;

		GUIContent _gcApplyColor = new GUIContent("Apply Color", "Apply Effect Color to Skybox");

		protected override void CacheProperties(){
			_pDrawSkybox = serializedObject.FindProperty("drawSkybox");
			_pUseMask = serializedObject.FindProperty("useMask");
			_pStencilRef = serializedObject.FindProperty("stencilReference");
			_pStencilMask = serializedObject.FindProperty("stencilMask");
			_pStencilBias = serializedObject.FindProperty("stencilBias");
			_tm = (TunnellingMobile)target;
		}

		protected override void DrawSettings(){
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.PropertyField(_pDrawSkybox);
			if (_tm.drawSkybox){
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(_pApplyColorToBkg, _gcApplyColor);
				EditorGUILayout.PropertyField(_pFxSkybox);
				--EditorGUI.indentLevel;
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.PropertyField(_pUseMask);
			if (_tm.useMask){
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(_pStencilRef);
				EditorGUILayout.PropertyField(_pStencilMask);
				EditorGUILayout.PropertyField(_pStencilBias);
				EditorGUILayout.HelpBox("Mask may stress drawcalls and fillrate.", MessageType.Warning);
				--EditorGUI.indentLevel;
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.Space();

			DrawMotionSettings();
		}
	}
}
