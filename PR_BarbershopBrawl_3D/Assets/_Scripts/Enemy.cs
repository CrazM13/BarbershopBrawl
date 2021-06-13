using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : DamageableEntity {

	#region FSM

	protected EnemyState state;
	public enum EnemyState {
		DEFAULT,
		STUNNED,
		LIGHT_WINDUP,
		LIGHT_SWING,
		LIGHT_COMPLETE,
		HEAVY_WINDUP,
		HEAVY_SWING,
		KICK_WINDUP,
		AIMING
	}
	#endregion

	#region Component
	[SerializeField] NavMeshAgent pathfinding;
	[SerializeField] Rigidbody physicsBody;
	[SerializeField] float kickWindupTime;
	[SerializeField] float lightAttackCompletionTime;
	[SerializeField] float lightAttackWindupTime;
	[SerializeField] float lightAttackDuration;
	[SerializeField] float lightAttackRange;
	[SerializeField] int lightAttackDamage;
	[SerializeField] float heavyAttackWindupTime;
	[SerializeField] float heavyAttackDuration;
	[SerializeField] float heavyAttackRange;
	[SerializeField] int heavyAttackDamage;
	#endregion

	#region Settings
	[SerializeField] float stunDuration;
	#endregion

	private float stunTimeRemaining = 0;
	private float kickTimeRemaining = 0;
	private float lightAttackTimeRemaining = 0;
	private float heavyAttackTimeRemaining = 0;

	private Vector3 knockbackRemaining = Vector3.zero;

	protected void UpdateStateMachine() {
		if (state == EnemyState.STUNNED) {
			stunTimeRemaining -= Time.deltaTime;
			if (stunTimeRemaining <= 0) {
				state = EnemyState.DEFAULT;
			}
		}

		if (state == EnemyState.KICK_WINDUP) {
			kickTimeRemaining -= Time.deltaTime;
			if (kickTimeRemaining <= 0) {

				// KICK

				state = EnemyState.DEFAULT;
			}
		}

		if (state == EnemyState.STUNNED) {
			physicsBody.AddForce(knockbackRemaining);
			knockbackRemaining = Vector3.Lerp(knockbackRemaining, Vector3.zero, Time.deltaTime);
		}

		if (state == EnemyState.LIGHT_COMPLETE) {
			lightAttackTimeRemaining -= Time.deltaTime;
			if (lightAttackTimeRemaining <= 0) {
				state = EnemyState.DEFAULT;
			}
		}

		if (state == EnemyState.LIGHT_SWING) {
			lightAttackTimeRemaining -= Time.deltaTime;
			if (lightAttackTimeRemaining <= 0) {

				// Attack

				state = EnemyState.LIGHT_COMPLETE;
				lightAttackTimeRemaining = lightAttackCompletionTime;
			}
		}

		if (state == EnemyState.LIGHT_WINDUP) {
			lightAttackTimeRemaining -= Time.deltaTime;
			if (lightAttackTimeRemaining <= 0) {
				state = EnemyState.LIGHT_SWING;
				lightAttackTimeRemaining = lightAttackDuration;
			}
		}

		if (state == EnemyState.HEAVY_SWING) {
			heavyAttackTimeRemaining -= Time.deltaTime;
			if (heavyAttackTimeRemaining <= 0) {

				// Attack

				state = EnemyState.DEFAULT;
			}
		}

		if (state == EnemyState.HEAVY_WINDUP) {
			heavyAttackTimeRemaining -= Time.deltaTime;
			if (heavyAttackTimeRemaining <= 0) {
				state = EnemyState.HEAVY_SWING;
				heavyAttackTimeRemaining = heavyAttackDuration;
			}
		}
	}

	protected void Knockdown(float force, Vector3 direction) {
		state = EnemyState.STUNNED;
		stunTimeRemaining = stunDuration;
		knockbackRemaining = direction * force;
	}

	protected void MoveToLocation(Vector3 location) {
		pathfinding.SetDestination(location);
	}

	public bool CanSeeLocation(Vector3 position, Vector3 sightVector, float visionAngle = 90) {
		float angle = Vector3.Angle(position, sightVector);

		return angle < visionAngle;
	}

	protected void StartHeavyAttack() {
		state = EnemyState.HEAVY_WINDUP;
		heavyAttackTimeRemaining = heavyAttackWindupTime;
	}

	protected void StartLightAttack() {
		state = EnemyState.LIGHT_WINDUP;
		heavyAttackTimeRemaining = lightAttackWindupTime;
	}

	protected void StartKickAttack() {
		state = EnemyState.KICK_WINDUP;
		heavyAttackTimeRemaining = kickWindupTime;
	}

}
