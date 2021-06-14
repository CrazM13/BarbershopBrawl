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

		if (GameManager.Instance?.LevelManager && state == EnemyState.DEFAULT) {

			Vector3 playerPos = GameManager.Instance.LevelManager.GetPlayerPosition();

			Vector3 lookDirection = playerPos - transform.position;
			transform.rotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0, lookDirection.z));

			if (Vector3.Distance(transform.position, targetPosition) < 3) {

				float distanceToPlayer = Vector3.Distance(transform.position, playerPos);

				if (distanceToPlayer > maxDistanceToPlayer || distanceToPlayer < minDistanceToPlayer) {
					targetPosition = playerPos + (Vector3.Cross(playerPos - transform.position, Vector3.up).normalized * ((maxDistanceToPlayer + minDistanceToPlayer) / 2));
				}
			}

			MoveToLocation(targetPosition);

			Fire();
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

	private void OnDrawGizmos() {
		if (GameManager.Instance?.LevelManager == null) return;
		Vector3 playerPos = GameManager.Instance.LevelManager.GetPlayerPosition();

		Handles.DrawWireDisc(playerPos, Vector3.up, minDistanceToPlayer);
		Handles.DrawWireDisc(playerPos, Vector3.up, maxDistanceToPlayer);

		
		Handles.DrawSolidDisc(targetPosition, Vector3.up, 1);
	}
}
