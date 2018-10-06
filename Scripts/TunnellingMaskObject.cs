using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sigtrap.VrTunnellingPro {
	/// <summary>
	/// Add this script to gameobjects to exclude them from the tunnelling effect.<br />
	/// Enabling/disabling this script will dynamically add/remove it from the mask.<br />
	/// Requires masking to be activated in the effect settings.
	/// Requires a Renderer.
	/// </summary>
	[RequireComponent(typeof(Renderer))]
	public class TunnellingMaskObject : MonoBehaviour {
		/// <summary>
		/// If true, all children with a Renderer will also be masked.
		/// </summary>
		public bool autoAddChildren;

		Renderer _r;
		bool _started;

		void Awake(){
			_r = GetComponent<Renderer>();
		}
		void Start(){
			_started = true;
			OnEnable();
		}
		void OnEnable(){
			if (!_started) return;
			if (TunnellingImageBase.instance) {
				TunnellingImageBase.instance.AddObjectToMask(_r, autoAddChildren);
			} else {
				Debug.LogWarning("No VrTunnellingPro instance found");
			}
		}
		void OnDisable(){
			if (TunnellingImageBase.instance) {
				TunnellingImageBase.instance.RemoveObjectFromMask(_r, autoAddChildren);
			} else {
				Debug.LogWarning("No VrTunnellingPro instance found");
			}
		}
	}
}