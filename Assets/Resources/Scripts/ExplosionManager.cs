using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour {

	public const int CAPACITY = 20;
	
	private static Transform explosionRoot;
	private static readonly Dictionary<string, GameObject> prototypes = new Dictionary<string, GameObject>();
	private static Dictionary<string, int> indice;
	private static Dictionary<string, ExplosionController[]> explosions;

	private void Awake() {
		explosionRoot = transform;
		
		prototypes.Add("Body Explosion 0", Resources.Load<GameObject>("Prefabs/Body Explosion 0"));
		prototypes.Add("Body Explosion 1", Resources.Load<GameObject>("Prefabs/Body Explosion 1"));
		prototypes.Add("Body Explosion 2", Resources.Load<GameObject>("Prefabs/Body Explosion 2"));
		
		indice = new Dictionary<string, int>();
		explosions = new Dictionary<string, ExplosionController[]>();
		foreach (var pair in prototypes) {
			indice.Add(pair.Key, 0);
			explosions.Add(pair.Key, new ExplosionController[CAPACITY]);
			for (int i = 0; i < CAPACITY; i++) {
				explosions[pair.Key][i] = Instantiate(pair.Value).GetComponent<ExplosionController>();
				explosions[pair.Key][i].transform.parent = explosionRoot;
				explosions[pair.Key][i].gameObject.SetActive(false);
			}
		}
	}

	public static void Reset() {
		ExplosionController[] explosions = explosionRoot.GetComponentsInChildren<ExplosionController>();
		foreach (var explosion in explosions) {
			explosion.Reset();
			explosion.gameObject.SetActive(false);
		}
	}

	public static ExplosionController Get(string name) {
		if (indice[name] >= CAPACITY) indice[name] = 0;
		ExplosionController explosion = explosions[name][indice[name]];
		explosion.gameObject.SetActive(true);
		explosion.Reset();
		indice[name]++;
		return explosion;
	}

	public static T Get<T>(string name) where T : ExplosionController {
		return Get(name) as T;
	}
}