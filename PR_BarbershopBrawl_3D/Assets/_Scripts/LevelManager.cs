using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

	[SerializeField] private Player player;

	private void Start() {
		GameManager.Instance.SetLevelManager(this);
	}

	public Vector3 GetPlayerPosition() {
		return player.GetPosition();
	}

}
