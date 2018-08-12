using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour {

	public static EnemyController[] Enemies => busyRoot.GetComponentsInChildren<EnemyController>();

	private static Transform idleRoot;
	private static Transform busyRoot;
	
	private static readonly Dictionary<string, GameObject> prototypes = new Dictionary<string, GameObject>();
	private static readonly Dictionary<string, Queue<EnemyController>> idleEnemies = new Dictionary<string, Queue<EnemyController>>();

	private void Awake() {
		idleRoot = new GameObject("Idle Root").transform;
		idleRoot.parent = transform;
		busyRoot = new GameObject("Busy Root").transform;
		busyRoot.parent = transform;
		prototypes.Add("Enemy 0", Resources.Load<GameObject>("Prefabs/Enemy 0"));
		prototypes.Add("Enemy 1", Resources.Load<GameObject>("Prefabs/Enemy 1"));
		prototypes.Add("Enemy 2", Resources.Load<GameObject>("Prefabs/Enemy 2"));
	}

	public static void Reset() {
		foreach (var enemy in Enemies) enemy.Recycle();
	}

	public static EnemyController Get(string name) {
		Queue<EnemyController> enemys;
		if (!idleEnemies.ContainsKey(name)) {
			enemys = new Queue<EnemyController>(5);
			idleEnemies[name] = enemys;
		} else enemys = idleEnemies[name];
		EnemyController enemy;
		if (enemys.Count > 0) {
			enemy = enemys.Dequeue();
			enemy.gameObject.SetActive(true);
		} else enemy = Instantiate(prototypes[name].GetComponent<EnemyController>());
		enemy.transform.parent = busyRoot;
		enemy.Init(prototypes[name].GetComponent<EnemyController>());
		return enemy;
	}

	public static T Get<T>(string name) where T : EnemyController {
		return Get(name) as T;
	}

	public static void Recycle(EnemyController enemy) {
		enemy.gameObject.SetActive(false);
		enemy.transform.parent = idleRoot;
		idleEnemies[enemy.identifierName].Enqueue(enemy);
	}
}