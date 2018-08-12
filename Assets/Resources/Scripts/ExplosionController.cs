using System.Collections;
using UnityEngine;

public class ExplosionController : MonoBehaviour {

	public string identifierName;
	public float radius = 1f;
	public float power = 500f;
	public float sleepDelay = 3f;

	private Rigidbody[] rigidbodies;
	private Collider[] colliders;
	private Vector3[] positions;
	private Quaternion[] rotations;
	private Coroutine _sleepCoroutine;

	private void Awake() {
		rigidbodies = GetComponentsInChildren<Rigidbody>();
		colliders = new Collider[rigidbodies.Length];
		positions = new Vector3[rigidbodies.Length];
		rotations = new Quaternion[rigidbodies.Length];
		for (int i = 0, l = rigidbodies.Length; i < l; i++) {
			colliders[i] = rigidbodies[i].GetComponent<Collider>();
			positions[i] = rigidbodies[i].transform.localPosition;
			rotations[i] = rigidbodies[i].transform.localRotation;
		}
	}

	public void Explode() {
		foreach (var rigidbody in rigidbodies) rigidbody.AddExplosionForce(power, transform.position, radius);
		_sleepCoroutine = StartCoroutine(ExeSleepCoroutine());
	}

	public void Reset() {
		if (_sleepCoroutine != null) {
			StopCoroutine(_sleepCoroutine);
			_sleepCoroutine = null;
		}
		
		for (int i = 0, l = rigidbodies.Length; i < l; i++) {
			colliders[i].enabled = true;
			rigidbodies[i].isKinematic = false;
			rigidbodies[i].velocity = PlayerController.Zero;
			rigidbodies[i].detectCollisions = true;
			rigidbodies[i].transform.localPosition = positions[i];
			rigidbodies[i].transform.localRotation = rotations[i];
		}
	}

	private IEnumerator ExeSleepCoroutine() {
		yield return new WaitForSeconds(sleepDelay);
		for (int i = 0, l = rigidbodies.Length; i < l; i++) {
			colliders[i].enabled = false;
			rigidbodies[i].isKinematic = true;
			rigidbodies[i].detectCollisions = false;
		}

		_sleepCoroutine = null;
	}
}