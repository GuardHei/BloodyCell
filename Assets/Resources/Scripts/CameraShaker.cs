using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraShaker : MonoBehaviour {

	public static Camera MainCamera => _camera;

	private static Camera _camera;
	private static CinemachineImpulseSource _source;

	private void Awake() {
		LoadUpCamera();
	}

	private void LoadUpCamera() {
		_camera = Camera.main;
		_source = _camera.GetComponent<CinemachineImpulseSource>();
	}

	public static void ShakeAt(Vector3 position, Vector3 velocity) {
		_source.GenerateImpulseAt(position, velocity);
	}
}