using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class Cutscene : MonoBehaviour {

	[SerializeField] private CutsceneScene[] scenes;

	

	[SerializeField] private RawImage imageBox;
	[SerializeField] private Text textBox;
	[SerializeField] private Text nameBoxLeft;
	[SerializeField] private Text nameBoxRight;

	[SerializeField] private float typeSpeed;
	
	private int sceneIndex = 0;
	private int textIndex = 0;

	private float textTimer = 0;

	private bool isADown = false;

	void Start() {
		imageBox.texture = scenes[sceneIndex].image;
		nameBoxLeft.text = scenes[sceneIndex].name;
		nameBoxRight.text = scenes[sceneIndex].name;

		nameBoxLeft.transform.parent.gameObject.SetActive(scenes[sceneIndex].leftNameplate);
		nameBoxRight.transform.parent.gameObject.SetActive(scenes[sceneIndex].rightNameplate);
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
					nameBoxLeft.text = scenes[sceneIndex].name;
					nameBoxRight.text = scenes[sceneIndex].name;

					nameBoxLeft.transform.parent.gameObject.SetActive(scenes[sceneIndex].leftNameplate);
					nameBoxRight.transform.parent.gameObject.SetActive(scenes[sceneIndex].rightNameplate);
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
	public string name;
	public bool leftNameplate = false;
	public bool rightNameplate = false;
}
