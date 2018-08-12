using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerController : MonoBehaviour {

	public static readonly Vector3 Zero = Vector3.zero;

	public int maxHealth = 100;
	public int health = 100;
	public AudioClip bleedSound;
	public string bleedEffect;
	public AudioClip explodeSound;
	public string explodeEffect;
	public float speed = 10f;
	[Range(0, 100)]
	public int condition;
	public List<IWeapon> weapons = new List<IWeapon>();
	public int chemoTreatDamage = 1000;
	public int chemoTreatSelfDamage = 30;
	public float chemoTreatRange = 15f;
	public GameObject chemoTreatUI;
	public float chemoTreatTransitionTime = .25f;
	public float chemoTreatDisplayTime = .5f;
	public HealthBarController healthBarController;
	public event Action<string> onDeath;

	private Collider _collider;
	private Rigidbody _rigidbody;
	private int _currentWeaponIndex = -1;
	private IWeapon _currentWeapon;
	private float horizontal;
	private float vertical;
	private bool hasMovedHorizontally;
	private bool hasMovedVertically;
	private bool _isAttacking;
	private Coroutine _displayChemoTreatmentCoroutine;

	public int WeaponCount => weapons.Count;

	private void Awake() {
		_collider = GetComponent<Collider>();
		
		_rigidbody = gameObject.AddComponent<Rigidbody>();
		_rigidbody.drag = 0;
		_rigidbody.angularDrag = 0;
		_rigidbody.useGravity = false;
		_rigidbody.isKinematic = false;
		_rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		_rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		_rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
		
		healthBarController.UpdateDisplay(health, maxHealth);
	}

	private void Update() {
		if (_currentWeapon != null) {
			if (Input.GetMouseButtonDown(0)) _currentWeapon.ReceiveAttackCommand();
			else if (Input.GetMouseButtonUp(0)) _currentWeapon.FinishAttackCommand();
			/*
			float attack = Input.GetAxis("Attack");
			if (attack > .5f && !_isAttacking) {
				_currentWeapon.ReceiveAttackCommand();
				_isAttacking = true;
			} else if (attack < .5f && _isAttacking) {
				_currentWeapon.FinishAttackCommand();
				_isAttacking = false;
			}
			*/
		}
		
		if (Input.GetMouseButtonDown(1) && _displayChemoTreatmentCoroutine == null) ChemoTreat();
	}
	
	private void FixedUpdate() {
		Move();
		Turn();
	}

	private void Move() {
		horizontal = 0f;
		vertical = 0f;
		hasMovedHorizontally = true;
		hasMovedVertically = true;

		if (Input.GetKey(KeyCode.W)) vertical = 1f;
		else if (Input.GetKeyUp(KeyCode.W)) vertical = Input.GetKey(KeyCode.S) ? -1f : 0f;
		else if (Input.GetKey(KeyCode.S)) vertical = -1f;
		else if (Input.GetKeyUp(KeyCode.S)) vertical = Input.GetKey(KeyCode.W) ? 1f : 0f;
		else hasMovedHorizontally = false;

		if (Input.GetKey(KeyCode.D)) horizontal = 1f;
		else if (Input.GetKeyUp(KeyCode.D)) horizontal = Input.GetKey(KeyCode.A) ? -1f : 0f;
		else if (Input.GetKey(KeyCode.A)) horizontal = -1f;
		else if (Input.GetKeyUp(KeyCode.A)) horizontal = Input.GetKey(KeyCode.D) ? 1f : 0f;
		else hasMovedVertically = false;

		/*
		horizontal = Input.GetAxis("Horizontal");
		vertical = Input.GetAxis("Vertical");

		if (Mathf.Abs(horizontal) < .3f) horizontal = 0f;
		if (Mathf.Abs(vertical) < .3f) vertical = 0f;
		*/

		if (hasMovedHorizontally || hasMovedVertically) _rigidbody.velocity = new Vector3(horizontal * speed, 0, vertical * speed);
	}

	private void Turn() {
		RaycastHit hit;
		Ray ray = CameraShaker.MainCamera.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out hit, 100, 1 << 9)) {
			Vector3 target = hit.point;
			Vector3 pos = transform.position;

			float deltaX = target.x - pos.x;
			float deltaZ = target.z - pos.z;

			Rotate(deltaX, deltaZ);
		}
		
		// Rotate(Input.GetAxis("Sight X"), Input.GetAxis("Sight Y"));
	}

	private void Rotate(float deltaX, float deltaZ) {
		if (Mathf.Abs(deltaX) < .05f) deltaX = 0f;
		if (Mathf.Abs(deltaZ) < .05f) deltaZ = 0f;
		if (deltaX == 0 && deltaZ == 0) return;
		if (deltaX > 0) {
			if (deltaZ > 0) _rigidbody.MoveRotation(Quaternion.Euler(0, -Mathf.Atan(deltaZ / deltaX) * Mathf.Rad2Deg, 0));
			if (deltaZ < 0) _rigidbody.MoveRotation(Quaternion.Euler(0, 360f - Mathf.Atan(deltaZ / deltaX) * Mathf.Rad2Deg, 0));
		} else if (deltaZ > 0) _rigidbody.MoveRotation(Quaternion.Euler(0, 180f - Mathf.Atan(deltaZ / deltaX) * Mathf.Rad2Deg, 0));
		else _rigidbody.MoveRotation(Quaternion.Euler(0, 180f - Mathf.Atan(deltaZ / deltaX) * Mathf.Rad2Deg, 0));
	}

	private void ChemoTreat() {
		EnemyController[] enemies = EnemyManager.Enemies;
		foreach (var enemy in enemies) enemy.GetDamaged(chemoTreatDamage);

		Collider[] colliders = Physics.OverlapSphere(transform.position, chemoTreatRange, 1 << LayerManager.CancerLayer);
		foreach (var collider in colliders) collider.GetComponent<CancerController>().GetDamaged(chemoTreatDamage);

		_displayChemoTreatmentCoroutine = StartCoroutine(ExeDisplayChemoTreatmentCoroutine());
		
		GetDamaged("chemo", chemoTreatSelfDamage);
	}

	public void GetDamaged(string reason, int damage) {
		Bleed();
		health -= damage;
		healthBarController.UpdateDisplay(health, maxHealth);
		if (health <= 0) {
			health = 0;
			healthBarController.UpdateDisplay(health, maxHealth);
			Die(reason);
		}
	}
	
	private void Bleed() {
		if (bleedSound) AudioManager.PlayAtPoint(bleedSound, transform.position);
		if (!string.IsNullOrEmpty(bleedEffect)) {
			BurstParticleController blood = ParticleManager.Get<BurstParticleController>(bleedEffect);
			blood.transform.position = transform.position;
			blood.Burst();
		}
	}

	private void Die(string reason) {
		gameObject.SetActive(false);
		ExplosionController explosion = ExplosionManager.Get(explodeEffect);
		explosion.transform.position = transform.position;
		explosion.Explode();
		onDeath?.Invoke(reason);
	}

	public void SwitchWeapon(int index) {
		if (index < 0 || index > WeaponCount - 1 || index == _currentWeaponIndex) return;
		if (_currentWeaponIndex > -1) _currentWeapon.OnSwitchOff();
		_currentWeaponIndex = index;
		_currentWeapon = weapons[_currentWeaponIndex];
		_currentWeapon.OnSwitchOn();
	}

	private void SwitchPrevWeapon() {
		int weaponCount = WeaponCount;
		if (weaponCount == 0 || weaponCount == 1) return;
		if (_currentWeaponIndex == -1) return;
		_currentWeapon.OnSwitchOff();
		if (_currentWeaponIndex == 0) _currentWeaponIndex = weaponCount - 1;
		_currentWeapon = weapons[_currentWeaponIndex];
		_currentWeapon.OnSwitchOn();
	}

	private void SwitchNextWeapon() {
		int weaponCount = WeaponCount;
		if (weaponCount == 0 || weaponCount == 1) return;
		if (_currentWeaponIndex == -1) return;
		_currentWeapon.OnSwitchOff();
		if (_currentWeaponIndex == weaponCount - 1) _currentWeaponIndex = 0;
		_currentWeapon = weapons[_currentWeaponIndex];
		_currentWeapon.OnSwitchOn();
	}

	private IEnumerator ExeDisplayChemoTreatmentCoroutine() {
		chemoTreatUI.SetActive(true);
		CanvasGroup group = chemoTreatUI.GetComponent<CanvasGroup>();
		float startTime = Time.time;
		float timePast;
		do {
			yield return null;
			timePast = Time.time - startTime;
			group.alpha = timePast / chemoTreatTransitionTime;
		} while (timePast < chemoTreatTransitionTime);

		group.alpha = 1f;
		yield return new WaitForSeconds(chemoTreatDisplayTime);

		startTime = Time.time;
		do {
			yield return null;
			timePast = Time.time - startTime;
			group.alpha = 1f - timePast / chemoTreatTransitionTime;
		} while (timePast < chemoTreatTransitionTime);

		group.alpha = 0f;
		chemoTreatUI.SetActive(false);

		_displayChemoTreatmentCoroutine = null;
	}
}
