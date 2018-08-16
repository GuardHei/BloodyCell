using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneController : MonoBehaviour {

	public static GameSceneController Instance { get; private set; }
	public static Transform PlayerTransform;

	private static bool isGameOver;
	private static int score;
	private static int levelScore;
	private static int playerLevel;
	private static int cancerKilled;
	private static int enemyKilled;
	private static string deathReason;

	public PlayerController playerController;
	public LevelSettings[] levelSettingses;
	public Text scoreText;
	public Text levelText;
	public Text enemyKilledText;
	public Text cancerKillerText;
	public GameObject levelUpUI;
	public float levelUpTransitionTime;
	public float levelUpDisplayTime;
	public GameObject gameOverUI;
	
	private Coroutine _displayLevelUpCoroutine;

	private void Awake() {
		Instance = this;
		
		PlayerTransform = playerController.transform;
		playerController.weapons.Add(playerController.GetComponentInChildren<IWeapon>());
		playerController.SwitchWeapon(0);
		playerController.onDeath += OnPlayerDeath;

		Play();
	}

	public void Play() {
		gameOverUI.SetActive(false);
		isGameOver = false;
		score = 0;
		levelScore = 0;
		enemyKilled = 0;
		cancerKilled = 0;
		CancerManager.Instance.Activate();
		playerController.gameObject.SetActive(true);
		playerController.transform.position = new Vector3(0f, 1.1f, 0f);
		playerController.health = playerController.maxHealth;
		playerController.healthBarController.UpdateDisplay(playerController.health, playerController.maxHealth);
		RefreshUI();
		LevelUp(Instance.levelSettingses[0]);
	}

	public void Replay() {
		EnemyManager.Reset();
		ParticleDecalManager.Reset();
		ExplosionManager.Reset();
		BulletController[] bullets = FindObjectsOfType<BulletController>();
		foreach (var bullet in bullets) bullet.Recycle();
		ParticleController[] particles = FindObjectsOfType<ParticleController>();
		foreach (var particle in particles) ParticleManager.Recycle(particle);
		CancerController[] cancers = FindObjectsOfType<CancerController>();
		foreach (var cancer in cancers) CancerManager.Recycle(cancer);
		CancerManager.Instance.Deactivate();
		
		Play();
	}

	private void OnPlayerDeath(string reason) {
		deathReason = reason;
		isGameOver = true;
		GameOver();
	}

	private void GameOver() {
		gameOverUI.SetActive(true);
	}

	public void Quit() {
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	public static void OnEnemyDeath(string name) {
		if (isGameOver) return;
		score += 1;
		levelScore += 1;
		enemyKilled++;
		RefreshUI();
		CheckLevelUp();
	}

	public static void OnCancerDeath(string name) {
		if (isGameOver) return;
		score += 5;
		levelScore += 5;
		cancerKilled++;
		RefreshUI();
		CheckLevelUp();
	}

	private static void RefreshUI() {
		Instance.scoreText.text = "Score = " + score;
		Instance.levelText.text = "Level = " + playerLevel;
		Instance.cancerKillerText.text = "Cancer      Killed = " + cancerKilled;
		Instance.enemyKilledText.text = "Cancer Cell Killed = " + enemyKilled;
	}

	private static void CheckLevelUp() {
		if (playerLevel == Instance.levelSettingses.Length - 1) return;
		LevelSettings settings = Instance.levelSettingses[playerLevel + 1];
		if (levelScore >= settings.scoreRequired) {
			playerLevel++;
			levelScore -= settings.scoreRequired;
			LevelUp(Instance.levelSettingses[playerLevel]);
		}
	}

	private static void LevelUp(LevelSettings settings) {
		ShooterController shooter = Instance.playerController.weapons[0] as ShooterController;
		shooter.firePower = settings.firePower;
		shooter.recoilTime = settings.recoilTime;
		shooter.recoilShake = settings.recoilShake;
		shooter.baseSettings = settings.weaponBaseSettings;
		shooter.fireOrigins = settings.fireOrigins;

		if (Instance._displayLevelUpCoroutine != null) Instance.StopCoroutine(Instance._displayLevelUpCoroutine);
		Instance.StartCoroutine(Instance.ExeDisplayChemoTreatmentCoroutine());
	}
	
	private IEnumerator ExeDisplayChemoTreatmentCoroutine() {
		levelUpUI.SetActive(true);
		CanvasGroup group = levelUpUI.GetComponent<CanvasGroup>();
		float startTime = Time.time;
		float timePast;
		do {
			yield return null;
			timePast = Time.time - startTime;
			group.alpha = timePast / levelUpTransitionTime;
		} while (timePast < levelUpTransitionTime);

		group.alpha = 1f;
		yield return new WaitForSeconds(levelUpDisplayTime);

		startTime = Time.time;
		do {
			yield return null;
			timePast = Time.time - startTime;
			group.alpha = 1f - timePast / levelUpTransitionTime;
		} while (timePast < levelUpTransitionTime);

		group.alpha = 0f;
		levelUpUI.SetActive(false);

		_displayLevelUpCoroutine = null;
	}

	[Serializable]
	public class LevelSettings {
		public int scoreRequired;
		public float firePower;
		public float recoilTime;
		public Vector3 recoilShake;
		public WeaponBaseSettings weaponBaseSettings;
		public FireOrigin[] fireOrigins;
	}
}