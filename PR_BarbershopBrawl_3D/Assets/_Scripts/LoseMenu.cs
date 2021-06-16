using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseMenu : MonoBehaviour {

	private string continueScene;

	public void Start() {
		continueScene = GameManager.Instance.GetContinueLevel();
	}

	public void ContinueGame() {
		SceneManager.LoadScene(continueScene);
	}

	public void MainMenu() {
		SceneManager.LoadScene("MainMenu");
	}

	public void QuitGame() {
		Application.Quit();
	}
}
