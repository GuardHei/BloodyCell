using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneController : MonoBehaviour {

	public void LoadNextScene() {
		SceneManager.LoadScene(1);
	}
}