using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour {

	private static Transform bulletRoot;
	
	private static readonly Dictionary<string, GameObject> prototypes = new Dictionary<string, GameObject>();
	private static readonly Dictionary<string, Queue<BulletController>> idleBullets = new Dictionary<string, Queue<BulletController>>();

	private void Awake() {
		bulletRoot = transform;

		prototypes.Add("Bullet 0", Resources.Load<GameObject>("Prefabs/Bullet 0"));
	}

	public static BulletController Get(string name) {
		Queue<BulletController> bullets;
		if (!idleBullets.ContainsKey(name)) {
			bullets = new Queue<BulletController>(5);
			idleBullets[name] = bullets;
		} else {
			bullets = idleBullets[name];
		}

		BulletController bullet;
		if (bullets.Count > 0) {
			bullet = bullets.Dequeue();
			bullet.gameObject.SetActive(true);
		} else {
			bullet = Instantiate(prototypes[name].GetComponent<BulletController>());
			bullet.transform.parent = bulletRoot;
		}
		
		return bullet;
	}

	public static T Get<T>(string name) where T : BulletController {
		return Get(name) as T;
	}

	public static void Recycle(BulletController bullet) {
		bullet.gameObject.SetActive(false);
		idleBullets[bullet.identifierName].Enqueue(bullet);
	}
}