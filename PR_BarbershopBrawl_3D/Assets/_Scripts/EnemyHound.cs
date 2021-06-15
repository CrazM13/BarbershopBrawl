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

		if (lastPlayerPosition.HasValue && state == EnemyState.DEFAULT) {

			if (Vector3.Distance(transform.position, lastPlayerPosition.Value) > meleeDistanceToPlayer) {
				MoveToLocation(lastPlayerPosition.Value);
			} else {
				StartHeavyAttack();
			}
		}
	}

	public override void OnDamageEntity(Damage damage) {
		seePlayerTime = maxChaseTime;

		if (damage.Type == Damage.DamageType.KNOCKDOWN) {
			Knockdown(damage.Amount, damage.Direction);
		} else {
			Health -= damage.Amount;

			DamageStun();

			if (Health <= 0) {
				state = EnemyState.DEAD;
			}
		}
	}
}
