using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDummy : Enemy {
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

	// Start is called before the first frame update
	void Start() {
		Health = maxHealth;
	}

	// Update is called once per frame
	void Update() {
		UpdateStateMachine();
	}
}
