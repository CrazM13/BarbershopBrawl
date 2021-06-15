using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager {

	#region Singleton
	private static GameManager instance;

	public static GameManager Instance {
		get {
			if (instance != null) return instance;
			else {
				instance = new GameManager();
				return instance;
			}
		}
	}
	#endregion

	

	public LevelManager LevelManager { get; private set; }

	public void SetLevelManager(LevelManager levelManager) {
		this.LevelManager = levelManager;
	}

	public string GetContinueLevel() {
		if (PlayerPrefs.HasKey("SavedLevel")) {
			return PlayerPrefs.GetString("SavedLevel");
		}

		return "";
	}

	public void SetContinueLevel(string levelToSave) {
		PlayerPrefs.SetString("SavedLevel", levelToSave);
	}

}
