using UnityEngine;
using System.Collections;

namespace Sigtrap.VrTunnellingPro.Examples {
	public class VRTP_ExampleMovement : MonoBehaviour {
		[SerializeField]
		bool _disableModes = false;
		[SerializeField]
		float _rotSensitivity = 180;
		[SerializeField]
		float _movSensitivity = 10;
		[SerializeField]
		Cubemap[] _skyboxes;
		[SerializeField]
		GameObject[] _cages;
		[SerializeField]
		Preset[] _modePresets;

		[System.Serializable]
		struct Preset {
			public KeyCode key;
			public GameObject[] worldObjects;
			public TunnellingPreset postPreset;
		}

		int _currentMode = 0;
		bool _hasWarnedInput = false;
		Tunnelling _tunnelling;
		CharacterController _char;
		int _currentSkybox = 0;
		int _currentCage = 0;

		Vector3 _speedCurrent, _speedSlew;
		[SerializeField]
		float _speedSmoothing = 0;
		float _rotCurrent, _rotSlew;
		[SerializeField]
		float _rotSmoothing = 0;

		void Awake(){
			_tunnelling = GetComponentInChildren<Tunnelling>();
			_char = GetComponent<CharacterController>();
		}

		void Update () {
			float rot = 0;
			Vector3 mov = Vector3.zero;

			try {
				rot = Input.GetAxis("Mouse X");
			} catch {
				if (!_hasWarnedInput){
					Debug.LogError("Mouse input unavailable for ExampleMovement - input manager axis Mouse X probably not present. Just use Q/E instead!");
					_hasWarnedInput = true;
				}
			}

			if (Input.GetKey(KeyCode.Q)){
				rot -= 1;
			}
			if (Input.GetKey(KeyCode.E)){
				rot += 1;
			}

			if (Input.GetKey(KeyCode.W)){
				mov.z += 1;
			}
			if (Input.GetKey(KeyCode.S)){
				mov.z -= 1;
			}
			if (Input.GetKey(KeyCode.A)){
				mov.x -= 1;
			}
			if (Input.GetKey(KeyCode.D)){
				mov.x += 1;
			}

			if (Input.GetKeyDown(KeyCode.Space)){
				++_currentSkybox;
				if (_currentSkybox >= _skyboxes.Length){
					_currentSkybox = 0;
				}
				_tunnelling.effectSkybox = _skyboxes[_currentSkybox];
			}
			if (Input.GetKeyDown(KeyCode.LeftControl)){
				++_currentCage;
				if (_currentCage >= _cages.Length){
					_currentCage = 0;
				}
				for (int i=0; i<_cages.Length; ++i){
					_cages[i].SetActive(i == _currentCage);
				}
			}

			_rotCurrent = Mathf.SmoothDamp(_rotCurrent, rot, ref _rotSlew, _rotSmoothing);
			transform.Rotate(0, _rotCurrent * _rotSensitivity * Time.deltaTime, 0);

			_speedCurrent = Vector3.SmoothDamp(_speedCurrent, mov, ref _speedSlew, _speedSmoothing);
			_char.SimpleMove(transform.rotation * _speedCurrent * _movSensitivity);

			if (_disableModes) return;
			
			for (int i=0; i<_modePresets.Length; ++i) {
				if (Input.GetKeyDown(_modePresets[i].key)) {
					SetPreset(i);
					break;
				}
			}

			if (Input.GetMouseButtonDown(0)){
				++_currentMode;
				if (_currentMode >= _modePresets.Length){
					_currentMode = 0;
				}
				SetPreset(_currentMode);
			}
		}

		void SetPreset(int i){
			Preset p = _modePresets[i];
			for (int j=0; j<_modePresets.Length; ++j){
				if (j == i) continue;
				foreach (var w in _modePresets[j].worldObjects){
					w.SetActive(false);
				}
			}
			foreach (var w in p.worldObjects){
				w.SetActive(true);
			}
			_tunnelling.ApplyPreset(p.postPreset);
			_currentMode = i;
		}
	}
}