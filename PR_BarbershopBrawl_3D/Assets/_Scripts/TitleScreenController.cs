using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenController : MonoBehaviour {

	[SerializeField] Transform titleTransform;
	[SerializeField] Transform cameraTransform;

	[SerializeField] AudioSource music;
	[SerializeField] float musicCD;

	[SerializeField] AnimationCurve titleRotationCurve;
	[SerializeField] float cameraRotationSpeed;

	private Vector3 cameraEulars;

	private float time = 0;
	private float musicTimer = 0;

	// Start is called before the first frame update
	void Start() {
		cameraEulars = cameraTransform.localRotation.eulerAngles;

		musicTimer = -musicCD;
	}

	// Update is called once per frame
	void Update() {

		titleTransform.localRotation = Quaternion.AngleAxis(titleRotationCurve.Evaluate(time % titleRotationCurve.keys[titleRotationCurve.length - 1].time), Vector3.forward);

		cameraTransform.localRotation = Quaternion.Euler(cameraEulars + new Vector3(0, time * cameraRotationSpeed, 0));

		time += Time.deltaTime;

		musicTimer += Time.deltaTime;
		if (musicTimer >= music.clip.length) {
			musicTimer = -musicCD;
		} else if (musicTimer > 0 && !music.isPlaying) {
			music.Play();
		}

	}
}
