using UnityEngine;
using UnityEngine.AI;

public class TerrainController : MonoBehaviour {

	private static NavMeshSurface _surface;

	private void Awake() {
		_surface = GetComponent<NavMeshSurface>();
		
		GetComponent<CancerManager>().Activate();
	}

	public static void BakeNavMesh() {
		_surface.BuildNavMesh();
	}
}