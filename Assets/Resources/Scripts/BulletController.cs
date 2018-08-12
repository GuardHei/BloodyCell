using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BulletController : MonoBehaviour {

	public string identifierName;
	public float lifespan;
	public float destroyDelay;
	public WeaponBaseSettings baseSettings;

	private bool _isFired;
	private bool _isDelayed;
	private float _fireTime;
	private Vector3 _velocity;
	private Rigidbody _rigidbody;

	private void Awake() {
		_rigidbody = gameObject.AddComponent<Rigidbody>();
		_rigidbody.isKinematic = true;
	}

	private void Update() {
		if (_isFired) {
			if (Time.time - _fireTime > lifespan) Recycle();
			else transform.position += _velocity * Time.deltaTime;
		}
	}

	public void Fire(Vector3 velocity) {
		_velocity = velocity;
		_isFired = true;
		_isDelayed = false;
		_fireTime = Time.time;
	}

	private void OnHit() {
		_fireTime = Time.time;
		lifespan = destroyDelay;
		_isDelayed = true;
	}

	public void Recycle() {
		_isFired = false;
		_isDelayed = false;
		BulletManager.Recycle(this);
	}

	private void OnTriggerEnter(Collider other) {
		if (_isDelayed) return;
		int layer = other.gameObject.layer;
		if (layer == LayerManager.TerrainLayer) {
			OnHit();
			return;
		}
		
		if (other.gameObject.layer == LayerManager.EnemyLayer) {
			OnHit();
			baseSettings.Attack(other.GetComponent<EnemyController>(), GameSceneController.PlayerTransform.position);
		} else if (other.gameObject.layer == LayerManager.CancerLayer) {
			OnHit();
			baseSettings.Attack(other.GetComponent<CancerController>());
		}
	}
}