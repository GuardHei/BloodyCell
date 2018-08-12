using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour {

	public string identifierName;
	public int health;
	public float aggressiveDistance;
	public AttackType attackType;
	public float attackInterval;
	public int damage;
	public float attackSpeed;
	public float attackDuration;
	public Vector3 attackShake;
	public float attackRange;
	public Material attackMaterial;
	public Vector3 attackSize;
	public float attackRotationSpeed;
	public float expandTime;
	public float shrinkTime;
	public AudioClip bleedSound;
	public string bleedEffect;
	public AudioClip explodeSound;
	public string explodeEffect;

	private bool _isAlive;
	private bool _isAttacking;
	private bool _isStunned;
	private float _originalSpeed;
	private Vector3 _originalSize;
	private Material _originalMaterial;
	private Renderer _renderer;
	private NavMeshAgent _agent;
	private Collider _collider;
	private readonly Collider[] _victims = new Collider[5];

	private Coroutine _attackCoroutine;
	private Coroutine _stunCoroutine;
	
	private void Awake() {
		_renderer = GetComponentInChildren<Renderer>();
		_agent = GetComponent<NavMeshAgent>();
		_collider = GetComponent<Collider>();
	}

	private void Update() {
		if (_isStunned) return;
		Vector3 targetPos = GameSceneController.PlayerTransform.position;
		if (!_isAttacking && (targetPos - transform.position).sqrMagnitude <= aggressiveDistance * aggressiveDistance) Attack();
		_agent.SetDestination(targetPos);
	}

	public void Init(EnemyController sample) {
		health = sample.health;
		_isAlive = true;
		_isStunned = false;
	}

	public void GetHit(Vector3 hitVelocity) {
		_agent.velocity = hitVelocity;
	}

	public void GetStunned(float duration) {
		if (_isAttacking) CancelAttack();
		if (_isStunned) StopCoroutine(_stunCoroutine);
		_stunCoroutine = StartCoroutine(ExeStunCoroutine(duration));
	}

	public void GetDamaged(int damage) {
		Bleed();
		health -= damage;
		if (health <= 0) {
			health = 0;
			Die();
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

	private void Die() {
		Recycle();
		GameSceneController.OnEnemyDeath(identifierName);
	}

	public void Recycle() {
		_isAlive = false;
		ExplosionController explosion = ExplosionManager.Get(explodeEffect);
		explosion.transform.position = transform.position;
		explosion.Explode();
		if (explodeSound) AudioManager.PlayAtPoint(explodeSound, transform.position);
		if (_isStunned) StopCoroutine(_stunCoroutine);
		_isStunned = false;
		_stunCoroutine = null;
		if (_isAttacking) CancelAttack();
		EnemyManager.Recycle(this);
	}

	private void Attack() {
		_isAttacking = true;
		switch (attackType) {
			case AttackType.Melee:
				_attackCoroutine = StartCoroutine(ExeMeleeAttackCoroutine());
				break;
			case AttackType.Explosion:
				_attackCoroutine = StartCoroutine(ExeExplosionAttackCoroutine());
				break;
		}
	}

	private void CancelAttack() {
		_isAttacking = false;
		transform.localScale = _originalSize;
		_agent.speed = _originalSpeed;
		_renderer.material = _originalMaterial;
	}

	private IEnumerator ExeMeleeAttackCoroutine() {
		_originalSize = transform.localScale;
		_originalSpeed = _agent.speed;
		_originalMaterial = _renderer.material;
		_agent.speed = attackSpeed;
		_agent.SetDestination(GameSceneController.PlayerTransform.position);
		_renderer.material = attackMaterial;
		yield return new WaitForSeconds(attackDuration);
		_agent.speed = _originalSpeed;
		float startTime = Time.time;
		float timePast;
		do {
			yield return null;
			timePast = Time.time - startTime;
			transform.localScale = Vector3.Lerp(_originalSize, attackSize, timePast / expandTime);
			transform.Rotate(0f, attackRotationSpeed * Time.deltaTime, 0f);
		} while (timePast < expandTime);
		
		Physics.OverlapSphereNonAlloc(transform.position, attackRange, _victims, 1 << LayerManager.PlayerLayer);
		if (_victims[0] != null) _victims[0].GetComponent<PlayerController>().GetDamaged(identifierName, damage);
		startTime = Time.time;
		do {
			yield return null;
			timePast = Time.time - startTime;
			transform.localScale = Vector3.Lerp(attackShake, _originalSize, timePast / shrinkTime);
			transform.Rotate(0f, attackRotationSpeed * Time.deltaTime, 0f);
		} while (timePast < shrinkTime);
		
		_renderer.material = _originalMaterial;
		CameraShaker.ShakeAt(transform.position, attackShake);
		
		yield return new WaitForSeconds(attackInterval);
		_attackCoroutine = null;
		_isAttacking = false;
	}

	private IEnumerator ExeExplosionAttackCoroutine() {
		_originalSize = transform.localScale;
		_originalSpeed = _agent.speed;
		_originalMaterial = _renderer.material;
		_agent.speed = attackSpeed;
		_agent.SetDestination(GameSceneController.PlayerTransform.position);
		_renderer.material = attackMaterial;
		yield return new WaitForSeconds(attackDuration);
		_agent.speed = _originalSpeed;
		float startTime = Time.time;
		float timePast;
		do {
			yield return null;
			timePast = Time.time - startTime;
			transform.localScale = Vector3.Lerp(_originalSize, attackSize, timePast / expandTime);
		} while (timePast < expandTime);
		
		transform.localScale = _originalSize;
		_renderer.material = _originalMaterial;
		for (int i = 0; i < _victims.Length; i++) _victims[i] = null;
		Physics.OverlapSphereNonAlloc(transform.position, attackRange, _victims, 1 << LayerManager.PlayerLayer);
		if (_victims[0] != null) _victims[0].GetComponent<PlayerController>().GetDamaged(identifierName, damage);
		for (int i = 0; i < _victims.Length; i++) _victims[i] = null;
		Physics.OverlapSphereNonAlloc(transform.position, attackRange, _victims, 1 << LayerManager.EnemyLayer, QueryTriggerInteraction.Collide);
		foreach (var victim in _victims) {
			if (victim == null || victim == _collider) break;
			victim.GetComponent<EnemyController>().GetDamaged(damage);
		}
		
		GetDamaged(health);
		CameraShaker.ShakeAt(transform.position, attackShake);
		_attackCoroutine = null;
		_isAttacking = false;
	}

	private IEnumerator ExeStunCoroutine(float time) {
		_isStunned = true;
		_agent.isStopped = true;
		yield return new WaitForSeconds(time);
		_agent.isStopped = false;
		_isStunned = false;
		_stunCoroutine = null;
	}

	public enum AttackType {
		Melee,
		Explosion
	}
}