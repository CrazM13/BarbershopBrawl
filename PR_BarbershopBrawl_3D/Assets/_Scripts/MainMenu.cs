using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

	[SerializeField] private string newGameScene;

	[SerializeField] private Button continueButton;
	[SerializeField] private Transform optionsMenu;

	private string continueScene;

	private bool isOptionsMenuOpen = false;

	public void Start() {
		continueScene = GameManager.Instance.GetContinueLevel();

		continueButton.interactable = continueScene != "";
	}

	public void Update() {
		optionsMenu.localScale = Vector3.one * (isOptionsMenuOpen ? 1 : 0);
	}

	public void NewGame() {
		SceneManager.LoadScene(newGameScene);
	}

	public void ContinueGame() {
		SceneManager.LoadScene(continueScene);
	}

	public void OpenOptionsMenu() {
		isOptionsMenuOpen = true;
	}

	public void CloseOptionsMenu() {
		isOptionsMenuOpen = false;
	}

	public void QuitGame() {
		Application.Quit();
	}


}
