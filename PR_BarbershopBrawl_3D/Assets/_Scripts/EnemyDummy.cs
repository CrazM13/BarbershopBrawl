using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDummy : Enemy {
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

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		UpdateStateMachine();
	}
}
