using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

	[SerializeField] private Player player;
	[SerializeField] private int levelID;

	private List<Enemy> enemies;

	private void Start() {
		GameManager.Instance.SetLevelManager(this);

		enemies = new List<Enemy>();
	}

	private void Update() {
		if (CanLevelComplete()) {
			// Complete Level
			Debug.Log("Level Complete");
		}
	}

	public void RegisterEnemy(Enemy enemy) {
		enemies.Add(enemy);
	}

	public void UnegisterEnemy(Enemy enemy) {
		enemies.Remove(enemy);
	}

	public Vector3 GetPlayerPosition() {
		return player.GetPosition();
	}

	public bool CanLevelComplete() {
		return enemies.Count <= 0;
	}

}
