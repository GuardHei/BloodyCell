using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class CancerManager : MonoBehaviour {

	public static CancerManager Instance { get; private set; }
	
	private static Transform cancerRoot;
	private static readonly Dictionary<string, GameObject> prototypes = new Dictionary<string, GameObject>();
	private static readonly Dictionary<string, Queue<CancerController>> idleCancers = new Dictionary<string, Queue<CancerController>>(3);

	public float spawnInterval;
	public int minSpawnNumber;
	public int maxSpawnNumber;
	public SpawnSet[] spawnSets;
	
	[SerializeField]
	private Vector2Int _size;
	private bool[,] cancerMap;
	private List<Vector2Int> _idleTiles;
	private List<Vector2Int> _busyTiles;
	private float _lastSpawnTime;

	private void Awake() {
		Instance = this;
		
		cancerRoot = transform;
		cancerMap = new bool[_size.x, _size.y];
		_idleTiles = new List<Vector2Int>(cancerMap.Length);
		_busyTiles = new List<Vector2Int>(cancerMap.Length);
		
		prototypes.Add("Cancer 0", Resources.Load<GameObject>("Prefabs/Cancer 0"));
		prototypes.Add("Cancer 1", Resources.Load<GameObject>("Prefabs/Cancer 1"));
		prototypes.Add("Cancer 2", Resources.Load<GameObject>("Prefabs/Cancer 2"));
	}

	public void Activate() {
		_idleTiles.Clear();
		_busyTiles.Clear();
		for (int i = 0; i < _size.x; i++)
			for (int j = 0; j < _size.y; j++) {
				cancerMap[i, j] = false;
				_idleTiles.Add(new Vector2Int(i, j));
			}
		_lastSpawnTime = Time.time;
		enabled = true;
	}

	public void Deactivate() {
		enabled = false;
	}

	private void Update() {
		float time = Time.time;
		if (time - _lastSpawnTime > spawnInterval) {
			_lastSpawnTime = time;
			Spawn();
		}
	}
	
	public static CancerController Get(string name) {
		Queue<CancerController> cancers;
		if (!idleCancers.ContainsKey(name)) {
			cancers = new Queue<CancerController>(5);
			idleCancers[name] = cancers;
		} else {
			cancers = idleCancers[name];
		}

		CancerController cancer;
		if (cancers.Count > 0) {
			cancer = cancers.Dequeue();
			cancer.gameObject.SetActive(true);
		} else {
			cancer = Instantiate(prototypes[name].GetComponent<CancerController>());
			cancer.transform.parent = cancerRoot;
		}
		
		cancer.Init(prototypes[name].GetComponent<CancerController>());
		return cancer;
	}

	public static void Recycle(CancerController cancer) {
		cancer.gameObject.SetActive(false);
		idleCancers[cancer.identifierName].Enqueue(cancer);
		TerrainController.BakeNavMesh();
	}

	private void Spawn() {
		int num = Random.Range(minSpawnNumber, maxSpawnNumber);
		int totalWeight = 0;
		foreach (var spawnSet in spawnSets) totalWeight += spawnSet.weight;
		
		for (int i = 0; i < num; i++) {
			int r = Random.Range(0, totalWeight - 1);
			foreach (var spawnSet in spawnSets) {
				r -= spawnSet.weight;
				if (r <= 0) {
					Vector3 position = GetSpawnPoint();
					if (position == PlayerController.Zero) break;
					CancerController cancer = Get(spawnSet.cancerName);
					cancer.transform.position = position;
					TerrainController.BakeNavMesh();
					break;
				}
			}
		}
	}

	private Vector3 GetSpawnPoint() {
		if (_idleTiles.Count > 0) {
			int i = 0;
			while (true) {
				i++;
				if (i == 10) return Vector3.zero;
				int r = Random.Range(0, _idleTiles.Count);
				Vector2Int tilePos = _idleTiles[r];
				Vector3 worldPos = new Vector3(-_size.x / 2 + tilePos.x, 1.5f, -_size.y / 2 + tilePos.y);
				bool isOverlapped = Physics.CheckBox(worldPos, new Vector3(.9f, .9f, .9f), Quaternion.identity, ~(1 << LayerManager.TerrainLayer | 1 << LayerManager.CancerLayer | 1 << LayerManager.EnemyLayer | 1 << LayerManager.PlayerLayer));
				if (!isOverlapped) {
					_idleTiles.RemoveAt(r);
					_busyTiles.Add(tilePos);
					return worldPos;
				}
			}
		}

		return Vector3.zero;
	}

	[Serializable]
	public class SpawnSet {
		public string cancerName;
		public int weight;
	}
}