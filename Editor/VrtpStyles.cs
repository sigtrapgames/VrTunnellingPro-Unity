using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Sigtrap.VrTunnellingPro.Editors {
	public static class VrtpStyles {
		static GUIStyle _sectionFoldout = null;
		public static GUIStyle sectionFoldout {
			get {
				if (_sectionFoldout == null){
					_sectionFoldout = new GUIStyle(EditorStyles.foldout);
					_sectionFoldout.fontStyle = FontStyle.Bold;
					SetTextColors(_sectionFoldout, headerTxtCol, accentTxtCol);
				}
				return _sectionFoldout;
			}
		}

		static GUIStyle _childFoldout = null;
		public static GUIStyle childFoldout {
			get {
				if (_childFoldout == null){
					_childFoldout = new GUIStyle(EditorStyles.foldout);
				}
				return _childFoldout;
			}
		}

		static GUIStyle _sectionBox = null;
		public static GUIStyle sectionBox {
			get {
				if (_sectionBox == null){
					_sectionBox = new GUIStyle(EditorStyles.helpBox);
				}
				return _sectionBox;
			}
		}

		static GUIStyle _childBox = null;
		public static GUIStyle childBox {
			get {
				if (_childBox == null){
					_childBox = new GUIStyle(EditorStyles.helpBox);
				}
				return _childBox;
			}
		}

		static GUIStyle _sectionHeader = null;
		public static GUIStyle sectionHeader {
			get {
				if (_sectionHeader == null){
					_sectionHeader = new GUIStyle(EditorStyles.label);
					_sectionHeader.fontStyle = FontStyle.Bold;
				}
				return _sectionHeader;
			}
		}

		public static Color headerTxtCol {
			get {
				return EditorGUIUtility.isProSkin ? new Color(1f, 0.4f, 0.1f) : new Color(0.8f, 0.25f, 0f);
			}
		}
		public static Color accentTxtCol {
			get {
				return EditorGUIUtility.isProSkin ? new Color(1f, 0.8f, 0) : new Color(1f, 1f, 0);
			}
		}

		public static Color secBkgCol {
			get {
				return EditorGUIUtility.isProSkin ? new Color(0.1f, 0.6f, 0.7f) : new Color(0.26f, 0.5f, 0.6f);
			}
		}
		public static Color secTxtCol {
			get {
				return EditorGUIUtility.isProSkin ? Color.red : new Color(0.8f, 0.4f, 0.4f);
			}
		}
		public static Color childBkgCol {
			get {
				return EditorGUIUtility.isProSkin ? Color.white : secBkgCol * 0.5f;
			}
		}
		public static Color childTxtCol {
			get {
				return EditorGUIUtility.isProSkin ? Color.red : new Color(0.8f, 0.4f, 0.4f);
			}
		}

		static void SetTextColors(GUIStyle style, Color main, Color accent){
			style.normal.textColor = style.onNormal.textColor = main;
			style.focused.textColor = style.hover.textColor = style.onActive.textColor =
			style.onFocused.textColor = style.onHover.textColor = accent;
		}

		public static void BeginSectionBox(){
			Color gbc = GUI.backgroundColor;
			GUI.backgroundColor = secBkgCol;
			EditorGUILayout.BeginVertical(sectionBox);
			GUI.backgroundColor = gbc;
		}
		public static void EndSectionBox(){
			EditorGUILayout.EndVertical();
		}

		public static void BeginChildBox(){
			Color gbc = GUI.backgroundColor;
			GUI.backgroundColor = childBkgCol;
			EditorGUILayout.BeginVertical(childBox);
			GUI.backgroundColor = gbc;
		}
		public static void EndChildBox(){
			EditorGUILayout.EndVertical();
		}
	}
}
