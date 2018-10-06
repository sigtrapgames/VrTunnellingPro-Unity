using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Sigtrap.VrTunnellingPro.Editors {
	public static class VrTunnellingProEditorUtils {
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
	}
}