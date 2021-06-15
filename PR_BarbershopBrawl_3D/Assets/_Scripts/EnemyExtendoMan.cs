using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemyExtendoMan : Enemy {
	public float minDistanceToPlayer;
	public float maxDistanceToPlayer;

	public Transform rotation;

	private Vector3 targetPosition;

	// Start is called before the first frame update
	void Start() {
		Health = maxHealth;

		targetPosition = transform.position;
		MoveToLocation(targetPosition);
	}

	// Update is called once per frame
	void Update() {
		UpdateStateMachine();

		if (lastPlayerPosition.HasValue && state == EnemyState.DEFAULT) {

			Vector3 lookDirection = lastPlayerPosition.Value - transform.position;
			rotation.rotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0, lookDirection.z));

			if (Vector3.Distance(transform.position, targetPosition) < 3) {

				float distanceToPlayer = Vector3.Distance(transform.position, lastPlayerPosition.Value);

				if (distanceToPlayer > maxDistanceToPlayer || distanceToPlayer < minDistanceToPlayer) {
					targetPosition = lastPlayerPosition.Value + (Vector3.Cross(lastPlayerPosition.Value - transform.position, Vector3.up).normalized * ((maxDistanceToPlayer + minDistanceToPlayer) / 2));
				}
			}

			MoveToLocation(targetPosition);

			Fire();
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
