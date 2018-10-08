using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Sigtrap.VrTunnellingPro.Editors {
	public static class VrtpEditorUtils {
		const string HEADER_LOGO_PATH = "Logos/";

		static Material __imageMat;
		static Material _imageMat {
			get {
				if (__imageMat == null){
					__imageMat = new Material(Shader.Find("Unlit/Transparent"));
				}
				return __imageMat;
			}
		}
		public static Texture LoadTexture(string name){
			return Resources.Load<Texture>(HEADER_LOGO_PATH + name);
		}
		public static void DrawImage(Texture image, int height, Vector2 offset, bool allowScroll=true){
			EditorGUILayout.BeginHorizontal();
			float vw = EditorGUIUtility.currentViewWidth;
			if (allowScroll){
				vw -= 16;
			}
			float offsetY = GUILayoutUtility.GetRect(0, 0).yMin + offset.y;
			float tw = (float)image.width * ((float)height/(float)image.height);
			float ratio = (tw / vw);
			if (ratio>1){
				// Scale logo to fit
				height = (int)(((float)height) / ratio);
			} else {
				// Left align
				vw *= ratio;
			}
			EditorGUI.DrawPreviewTexture(new Rect(offset.x,offsetY,vw,height), image, _imageMat, ScaleMode.ScaleToFit);
			// Force height
			EditorGUILayout.BeginVertical();
			GUILayout.Space(height+2);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}

		public static bool ToggleProperty(AutoProperty ap, GUIContent content=null, GUIStyle style=null){
			if (content == null){
				content = ap.content;
			}
			if (style != null){
				ap.p.boolValue = EditorGUILayout.ToggleLeft(content, ap.p.boolValue, style);
			} else {
				ap.p.boolValue = EditorGUILayout.ToggleLeft(content, ap.p.boolValue);
			}
			
			return ap.p.boolValue;
		}
		public static void PropertyField(AutoProperty ap, GUIContent content=null){
			if (content == null){
				content = ap.content;
			}
			EditorGUILayout.PropertyField(ap, content);
		}
	}

	public class AutoProperty {
		public string path {get; private set;}
		public SerializedProperty p {get; private set;}
		public GUIContent content {get; private set;}
		public bool isReady {get {return p != null;}}

		string _label = null;

		public AutoProperty(string path){
			this.path = path;
		}
		public AutoProperty(string path, string label) : this(path) {
			_label = label;
		}
		public void Init(SerializedObject so){
			p = so.FindProperty(path);
			content = p.GetGUIContent(_label);
		}

		public static implicit operator SerializedProperty(AutoProperty a){
			return a.p;
		}
	}

	public static class Ext_SerializedProperty {
		public static GUIContent GetGUIContent(this SerializedProperty sp, string overrideLabel=null){
			if (string.IsNullOrEmpty(overrideLabel)){
				overrideLabel = ObjectNames.NicifyVariableName(sp.name);
			}
			
			string tooltip = sp.tooltip;
			if (string.IsNullOrEmpty(tooltip)){
				var fi = sp.serializedObject.targetObject.GetType().GetField(sp.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				if (fi != null){
					var ats = fi.GetCustomAttributes(typeof(TooltipAttribute), false);
					if (ats != null && ats.Length > 0){
						tooltip = (ats[0] as TooltipAttribute).tooltip;
					}
				}
			}
			return new GUIContent(overrideLabel, tooltip);
		}
	}
}