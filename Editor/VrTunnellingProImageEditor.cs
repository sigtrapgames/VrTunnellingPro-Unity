using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Sigtrap.VrTunnellingPro.Editors {
	[CustomEditor(typeof(TunnellingImageBase), true)]
	public class VrTunnellingProImageEditor : VrTunnellingProEditorBase {
		[RuntimeInitializeOnLoadMethod]
		static void _RIOL_InitialiseFog(){
			SetFogParams(0.01f, 2f, 1f, Color.grey);
		}

		public const string LOGO_NAME = "VrTunnellingProLogo";
		override protected string HEADER_LOGO_NAME {get {return LOGO_NAME;}}

		#region SPs
		SerializedProperty _pBkgMode;
		SerializedProperty _pOverlay;

		SerializedProperty _pCageParent;
		SerializedProperty _pCageDownsample;
		SerializedProperty _pCageAa;
		SerializedProperty _pCageUpdate;
		SerializedProperty _pCageFogDensity;
		SerializedProperty _pCageFogPower;
		SerializedProperty _pCageFogBlend;

		SerializedProperty _pMaskMode;
		//SerializedProperty _pMaskAa;

		SerializedProperty _pBlurDownsample;
		SerializedProperty _pBlurDistance;
		SerializedProperty _pBlurPasses;
		SerializedProperty _pBlurKernel;

		SerializedProperty _pZRejectIris;

		AutoProperty _pCounterVelMode = new AutoProperty("counterVelocityMode", "Velocity Mode");
		AutoProperty _pCounterVelResetDistance = new AutoProperty("counterVelocityResetDistance", "Reset Distance");
		AutoProperty _pCounterVelResetTime = new AutoProperty("counterVelocityResetTime", "Reset Time");
		AutoProperty _pCounterVelStr = new AutoProperty("counterVelocityStrength", "Velocity Strength");
		AutoProperty _pCounterVelAxs = new AutoProperty("counterVelocityPerAxis", "Velocity Per-Axis");
		#endregion

		#region Reflection
		PropertyInfo _piCanDrawIris;
		#endregion

		#region Labels
		static readonly GUIContent _gcCageSettings = new GUIContent("Cage Settings", "Render 3D sub-scene 'cage' in vignette to provide strong static reference for player.");
		static readonly GUIContent _gcFogSettings = new GUIContent("Fog Settings", "Render fog in cage sub-scene (for materials using Cage Fogged shaders).\nUses Effect Color as fog color.");

		static readonly GUIContent _gcCageDownsample = new GUIContent("Downsampling", "Render cage at half or quarter resolution.");
		static readonly GUIContent _gcCageAa = new GUIContent("MSAA", "Manually set cage MSAA, or AUTO to follow quality settings.");
		static readonly GUIContent _gcCageUpdate = new GUIContent("Update Every Frame", "FALSE: Cache cage objects on Awake.\n> CAN move/modify objects\n> CANNOT add/destroy objects\nTRUE: Refresh objects each frame.\n> There will be some GC alloc each Update.\n> Consider calling UpdateCage() manually.");
		static readonly GUIContent _gcCageFogDensity = new GUIContent("Density", "Density of fog for materials using Cage Fogged shaders.");
		static readonly GUIContent _gcCageFogPower = new GUIContent("Power", "Fog falloff for materials using Cage Fogged shaders.");
		static readonly GUIContent _gcCageFogBlend = new GUIContent("Strength", "Fog blending for materials using Cage Fogged shaders.\n0: No fog. 1: 100% fog.");

		const string APPLY_COLOR_LABEL = "Apply Color";
		static readonly GUIContent _gcApplyColorCage = new GUIContent(APPLY_COLOR_LABEL, "Apply Effect Color to entire cage render.\n> Alpha controls opacity even when off.");
		static readonly GUIContent _gcApplyColorBlur = new GUIContent(APPLY_COLOR_LABEL, "Apply Effect Color to blur effect.\n> Alpha controls opacity even when off.");
		static readonly GUIContent _gcApplyColorSkybox = new GUIContent(APPLY_COLOR_LABEL, "Apply Effect Color to skybox.\n> Alpha controls opacity even when off.");

		static readonly GUIContent _gcBlurDownsample = new GUIContent("Downsampling", "Downsample before blurring.\nHigher is faster and blurrier.");
		static readonly GUIContent _gcBlurDistance = new GUIContent("Distance", "Blur radius.");
		static readonly GUIContent _gcBlurPasses = new GUIContent("Passes", "Blur passes.\nHigher is slower but blurrier.");
		static readonly GUIContent _gcBlurKernel = new GUIContent("Samples", "Blur samples per pixel per pass.\nHigher is slower but smoother.");
		#endregion

		TunnellingImageBase _tib;

		static bool _showBkgSettings = true;
		static bool _showCageSettings = true;
		static bool _showFogSettings = true;

		protected override void CacheProperties(){
			_pBkgMode = serializedObject.FindProperty("backgroundMode");
			_pOverlay = serializedObject.FindProperty("effectOverlay");

			_pCageParent = serializedObject.FindProperty("_cageParent");
			_pCageDownsample = serializedObject.FindProperty("cageDownsample");
			_pCageAa = serializedObject.FindProperty("cageAntiAliasing");
			_pCageUpdate = serializedObject.FindProperty("cageUpdateEveryFrame");
			_pCageFogDensity = serializedObject.FindProperty("cageFogDensity");
			_pCageFogPower = serializedObject.FindProperty("cageFogPower");
			_pCageFogBlend = serializedObject.FindProperty("cageFogBlend");

			_pMaskMode = serializedObject.FindProperty("maskMode");
			//_pMaskAa = serializedObject.FindProperty("maskAntiAliasing");

			_pBlurDownsample = serializedObject.FindProperty("blurDownsample");
			_pBlurDistance = serializedObject.FindProperty("blurDistance");
			_pBlurPasses = serializedObject.FindProperty("blurPasses");
			_pBlurKernel = serializedObject.FindProperty("blurSamples");

			_pZRejectIris = serializedObject.FindProperty("irisZRejection");

			_piCanDrawIris = typeof(TunnellingImageBase).GetProperty("_canDrawIris", BindingFlags.Instance | BindingFlags.NonPublic);

			InitAps(_pCounterVelStr, _pCounterVelAxs, _pCounterVelMode, _pCounterVelResetDistance, _pCounterVelResetTime);

			_tib = (TunnellingImageBase)target;
		}

		protected override void DrawSettings(){
			VrtpStyles.BeginSectionBox(); {
				++EditorGUI.indentLevel;
				_showBkgSettings = EditorGUILayout.Foldout(_showBkgSettings, "Background Settings", VrtpStyles.sectionFoldout);
				--EditorGUI.indentLevel;
				
				if (_showBkgSettings){
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(_pBkgMode);
					if (_tib.backgroundMode != TunnellingBase.BackgroundMode.COLOR) {
						switch (_tib.backgroundMode) {
							case TunnellingBase.BackgroundMode.SKYBOX:
								EditorGUILayout.LabelField("Skybox Settings");
								++EditorGUI.indentLevel;
								EditorGUILayout.PropertyField(_pApplyColorToBkg, _gcApplyColorSkybox);
								EditorGUILayout.PropertyField(_pFxSkybox);
								EditorGUILayout.PropertyField(_pOverlay);
								--EditorGUI.indentLevel;
								break;
							case TunnellingBase.BackgroundMode.BLUR:
								EditorGUILayout.HelpBox("BLUR mode is performance-intensive", MessageType.Warning);
								EditorGUILayout.LabelField("Blur Settings");
								++EditorGUI.indentLevel;
								EditorGUILayout.PropertyField(_pApplyColorToBkg, _gcApplyColorBlur);
								EditorGUILayout.PropertyField(_pBlurDownsample, _gcBlurDownsample);
								EditorGUILayout.PropertyField(_pBlurDistance, _gcBlurDistance);
								EditorGUILayout.PropertyField(_pBlurPasses, _gcBlurPasses);
								EditorGUILayout.PropertyField(_pBlurKernel, _gcBlurKernel);
								--EditorGUI.indentLevel;
								break;
							case TunnellingBase.BackgroundMode.CAGE_COLOR:
							case TunnellingBase.BackgroundMode.CAGE_SKYBOX:
							case TunnellingBase.BackgroundMode.CAGE_ONLY:
								EditorGUILayout.PropertyField(_pCageParent);
								++EditorGUI.indentLevel;
								_showCageSettings = EditorGUILayout.Foldout(_showCageSettings, _gcCageSettings, VrtpStyles.childFoldout);
								if (_showCageSettings) {
									EditorGUILayout.PropertyField(_pApplyColorToBkg, _gcApplyColorCage);
									EditorGUILayout.PropertyField(_pOverlay);
									if (_tib.backgroundMode == TunnellingBase.BackgroundMode.CAGE_SKYBOX) {
										EditorGUILayout.PropertyField(_pFxSkybox);
									}
									EditorGUILayout.PropertyField(_pCageDownsample, _gcCageDownsample);
									EditorGUILayout.PropertyField(_pCageAa, _gcCageAa);
									EditorGUILayout.PropertyField(_pCageUpdate, _gcCageUpdate);
								}
								_showFogSettings = EditorGUILayout.Foldout(_showFogSettings, _gcFogSettings, VrtpStyles.childFoldout);
								if (_showFogSettings){
									EditorGUILayout.PropertyField(_pCageFogDensity, _gcCageFogDensity);
									EditorGUILayout.PropertyField(_pCageFogPower, _gcCageFogPower);
									EditorGUILayout.PropertyField(_pCageFogBlend, _gcCageFogBlend);
								}
								--EditorGUI.indentLevel;
								break;
						}
					}
					if (_tib.backgroundMode != TunnellingBase.BackgroundMode.BLUR){
						bool canDrawIris = (bool)_piCanDrawIris.GetValue(_tib,null);
						if (!canDrawIris) {
							EditorGUILayout.BeginHorizontal();
						}
						EditorGUILayout.PropertyField(_pZRejectIris);
						if (!canDrawIris) {
							string whyDisabled = "Disabled: ";
							if (_tib.usingMask){
								whyDisabled += "Masking enabled";
							} else if (_tib.backgroundMode == TunnellingBase.BackgroundMode.BLUR){
								whyDisabled += "Blur enabled";
							} else if (_tib.backgroundMode == TunnellingBase.BackgroundMode.CAGE_ONLY){
								whyDisabled += "CAGE_ONLY mode";
							} else {
								whyDisabled += "Effect Color alpha < 1";
							}
							GUI.enabled = false;
							EditorGUILayout.LabelField(whyDisabled);
							GUI.enabled = true;
							EditorGUILayout.EndHorizontal();
						}
					}
				}
			} VrtpStyles.EndSectionBox();

			VrtpStyles.BeginSectionBox();
			EditorGUILayout.PropertyField(_pMaskMode);
			VrtpStyles.EndSectionBox();

			DrawMotionSettings();

			if (!Application.isPlaying) {
				SetFogParams(_tib.cageFogDensity, _tib.cageFogPower, _tib.cageFogBlend, _tib.effectColor);
			}
		}

		protected override void DrawCounterMotionSettings(){
			base.DrawCounterMotionSettings();
			EditorGUILayout.Space();
			VrtpEditorUtils.PropertyField(_pCounterVelMode);
			var mode = (TunnellingImageBase.CounterVelocityMode)_pCounterVelMode.p.intValue;
			if (mode == TunnellingImageBase.CounterVelocityMode.REAL){
				VrtpEditorUtils.PropertyField(_pCounterVelResetDistance);
				VrtpEditorUtils.PropertyField(_pCounterVelResetTime);
			}
			if (mode != TunnellingImageBase.CounterVelocityMode.OFF){
				VrtpEditorUtils.PropertyField(_pCounterVelStr);
				VrtpEditorUtils.PropertyField(_pCounterVelAxs);
			}
		}

		static void SetFogParams(float density, float power, float blend, Color color){
			Shader.SetGlobalFloat(TunnellingBase.GLOBAL_PROP_FOGDENSITY, density);
			Shader.SetGlobalFloat(TunnellingBase.GLOBAL_PROP_FOGPOWER, power);
			Shader.SetGlobalFloat(TunnellingBase.GLOBAL_PROP_FOGBLEND, blend);
			Shader.SetGlobalColor(TunnellingBase.GLOBAL_PROP_FOGCOLOR, color);
		}
	}
}