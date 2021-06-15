using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beard : MonoBehaviour {

	[SerializeField] DamageableEntity entity;

	[SerializeField] GameObject[] beardPieces;

	private int previousHealth = 0;

	// Start is called before the first frame update
	void Start() {
		for (int i = 0; i < beardPieces.Length; i++) {
			beardPieces[i].SetActive(i < entity.Health);
		}

		previousHealth = entity.Health;
	}

	// Update is called once per frame
	void Update() {
		if (previousHealth != entity.Health) {

			for (int i = 0; i < beardPieces.Length; i++) {
				beardPieces[i].SetActive(i < entity.Health);
			}

			previousHealth = entity.Health;
		}
	}
}
