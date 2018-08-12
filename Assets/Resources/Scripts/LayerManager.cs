using UnityEngine;

public static class LayerManager {

	public static readonly int TerrainLayer = LayerMask.NameToLayer("Terrain");
	public static readonly int PlayerLayer = LayerMask.NameToLayer("Player");
	public static readonly int EnemyLayer = LayerMask.NameToLayer("Enemy");
	public static readonly int WeaponLayer = LayerMask.NameToLayer("Weapon");
	public static readonly int BulletLayer = LayerMask.NameToLayer("Bullet");
	public static readonly int CancerLayer = LayerMask.NameToLayer("Cancer");
	public static readonly int ParticleLayer = LayerMask.NameToLayer("Particle");

	public static void SetLayers(GameObject obj, int layer) {
		SetLayers(obj.transform, layer);
	}
	
	public static void SetLayers(Transform transform, int layer) {
		transform.gameObject.layer = layer;
		for (int i = 0, l = transform.childCount; i < l; i++) {
			SetLayers(transform.GetChild(i), layer);
		}
	}
}