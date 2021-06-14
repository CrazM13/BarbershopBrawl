using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EnemyHound : Enemy {

	public float meleeDistanceToPlayer;

	// Start is called before the first frame update
	void Start() {
		Health = maxHealth;
	}

	// Update is called once per frame
	void Update() {
		UpdateStateMachine();

		if (GameManager.Instance?.LevelManager && state == EnemyState.DEFAULT) {

			Vector3 playerPos = GameManager.Instance.LevelManager.GetPlayerPosition();

			if (Vector3.Distance(transform.position, playerPos) > meleeDistanceToPlayer) {
				MoveToLocation(playerPos);
			} else {
				StartHeavyAttack();
			}
		}
	}

	public override void OnDamageEntity(Damage damage) {
		if (damage.Type == Damage.DamageType.KNOCKDOWN) {
			Knockdown(damage.Amount, damage.Direction);
		} else {
			Health -= damage.Amount;

			if (Health <= 0) {
				Health = maxHealth;
				Debug.Log("DEAD");
			}
		}
	}
}
