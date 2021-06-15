using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

	[SerializeField] private Player player;
	[SerializeField] private bool isGameScene;
	[SerializeField] private string nextLevel;

	private List<Enemy> enemies;
	private bool hasEnemiesRegistered = false;

	private void Start() {
		GameManager.Instance.SetLevelManager(this);

		enemies = new List<Enemy>();
	}

	private void Update() {
		if (CanLevelComplete()) {
			// Complete Level
			CompleteLevel();
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
