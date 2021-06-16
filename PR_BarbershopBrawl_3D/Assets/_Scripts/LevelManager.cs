using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

	[SerializeField] private Player player;
	[SerializeField] private bool isGameScene;
	[SerializeField] private string levelName;
	[SerializeField] private string nextLevel;

	private List<Enemy> enemies;
	private bool hasEnemiesRegistered = false;

	private void Start() {
		GameManager.Instance.SetLevelManager(this);

		GameManager.Instance.SetContinueLevel(levelName);

		enemies = new List<Enemy>();
	}

	private void Update() {
		if (CanLevelComplete()) {
			// Complete Level
			CompleteLevel();
		}

		if (Input.GetKeyDown(KeyCode.Backspace)) {
			foreach (Enemy e in enemies) {
				e.OnDamageEntity(Damage.LightDamage(10000));
			}
		}
	}

	public void RegisterEnemy(Enemy enemy) {
		enemies.Add(enemy);
		hasEnemiesRegistered = true;
	}

	public void UnegisterEnemy(Enemy enemy) {
		enemies.Remove(enemy);
	}

	public Vector3 GetPlayerPosition() {
		return player.GetPosition();
	}

	public bool CanLevelComplete() {
		return isGameScene && hasEnemiesRegistered && enemies.Count <= 0;
	}

	public void CompleteLevel() {
		Debug.Log("Level Complete");
		if (nextLevel != "") {
			SceneManager.LoadScene(nextLevel);
		}
	}

}
