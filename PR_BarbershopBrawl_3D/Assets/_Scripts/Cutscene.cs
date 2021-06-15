using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cutscene : MonoBehaviour {

	[SerializeField] private CutsceneScene[] scenes;

	[SerializeField] private RawImage imageBox;
	[SerializeField] private Text textBox;

	[SerializeField] private float typeSpeed;
	
	private int sceneIndex = 0;
	private int textIndex = 0;

	private float textTimer = 0;

	private bool isADown = false;

	void Start() {
		imageBox.texture = scenes[sceneIndex].image;
	}

	void Update() {
		textBox.text = scenes[sceneIndex].text.Substring(0, textIndex);

		textTimer -= Time.deltaTime * typeSpeed;
		if (textTimer <= 0) {
			textTimer += 1;
			textIndex = Math.Min(textIndex + 1, scenes[sceneIndex].text.Length);
		}

		if (!isADown && Input.GetAxis("Kick") > 0) {
			if (textIndex < scenes[sceneIndex].text.Length) textIndex = scenes[sceneIndex].text.Length;
			else {
				if (sceneIndex < scenes.Length - 1) {
					sceneIndex++;
					textIndex = 0;
					imageBox.texture = scenes[sceneIndex].image;
				} else {
					GameManager.Instance?.LevelManager?.CompleteLevel();
				}
			}
		}
		isADown = Input.GetAxis("Kick") > 0;

	}
}

[System.Serializable]
public class CutsceneScene {
	public Texture2D image;
	public string text;
}
