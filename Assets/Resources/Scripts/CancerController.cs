using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CancerController : MonoBehaviour {

	public string identifierName;
	public int health;
	public AudioClip bleedSound;
	public string bleedEffect;
	public AudioClip explodeSound;
	public string explodeEffect;
	public float spawnRadius;
	public float spawnInterval;
	public int minSpawnNumber;
	public int maxSpawnNumber;
	public List<SpawnSet> spawnSets;

	private bool _isAlive;
	private float _lastSpawnTime;

	public void Init(CancerController cancer) {
		health = cancer.health;
		_isAlive = true;
	}

	private void Update() {
		float time = Time.time;
		if (time - _lastSpawnTime > spawnInterval) {
			_lastSpawnTime = time;
			Spawn();
		}
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
		_isAlive = false;
		ExplosionController explosion = ExplosionManager.Get(explodeEffect);
		explosion.transform.position = transform.position;
		explosion.Explode();
		if (explodeSound) AudioManager.PlayAtPoint(explodeSound, transform.position);
		CancerManager.Recycle(this);
		GameSceneController.OnCancerDeath(identifierName);
	}

	public void Spawn() {
		int num = Random.Range(minSpawnNumber, maxSpawnNumber);
		int totalWeight = 0;
		foreach (var spawnSet in spawnSets) totalWeight += spawnSet.weight;
		
		for (int i = 0; i < num; i++) {
			int r = Random.Range(0, totalWeight - 1);
			foreach (var spawnSet in spawnSets) {
				r -= spawnSet.weight;
				if (r <= 0) {
					EnemyController enemy = EnemyManager.Get(spawnSet.enemyName);
					float rad = Random.Range(0f, 6.28f);
					Vector3 spawnPoint = new Vector3(Mathf.Cos(rad), 1f, Mathf.Sin(rad)) + transform.position;
					enemy.transform.position = spawnPoint;
					break;
				}
			}
		}
	}

	[Serializable]
	public class SpawnSet {
		public string enemyName;
		public int weight;
	}
}