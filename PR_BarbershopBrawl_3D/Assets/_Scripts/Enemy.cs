using System.Collections;
using System.Collections.Generic;
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
		AIMING,
		DEAD
	}
	#endregion

	#region Component
	[SerializeField] protected NavMeshAgent pathfinding;
	[SerializeField] Rigidbody physicsBody;
	[SerializeField] ExtendoScript rangedItem;
	[SerializeField] protected Animator animationController;
	[SerializeField] Transform rootTransform;
	#endregion

	#region Settings
	[SerializeField] float speed;
	[SerializeField] float stunDuration;
	[SerializeField] float kickWindupTime;
	[SerializeField] float kickDistance;
	[SerializeField] int kickForce;
	[SerializeField] float lightAttackCompletionTime;
	[SerializeField] float lightAttackWindupTime;
	[SerializeField] float lightAttackDuration;
	[SerializeField] float lightAttackRange;
	[SerializeField] int lightAttackDamage;
	[SerializeField] float heavyAttackWindupTime;
	[SerializeField] float heavyAttackDuration;
	[SerializeField] float heavyAttackRange;
	[SerializeField] int heavyAttackDamage;
	[SerializeField] float sightDistance = 5;
	[SerializeField] protected float maxChaseTime;
	#endregion

	private float stunTimeRemaining = 0;
	private float kickTimeRemaining = 0;
	private float lightAttackTimeRemaining = 0;
	private float heavyAttackTimeRemaining = 0;

	private float deathTimeRemaining = 10f;

	private int prevHealth;

	private Vector3 knockbackRemaining = Vector3.zero;

	private bool isRegistered = false;

	protected Vector3? lastPlayerPosition;
	protected float seePlayerTime = 0;

	protected void RegisterEnemy() {
		GameManager.Instance?.LevelManager?.RegisterEnemy(this);

		isRegistered = true;
	}

	protected void UpdateStateMachine() {
		if (!isRegistered) {
			RegisterEnemy();
		}

		if (prevHealth != Health) {
			animationController.SetTrigger("hurt");
			prevHealth = Health;
		}

		if (seePlayerTime > 0) seePlayerTime -= Time.deltaTime;

		if (GameManager.Instance?.LevelManager != null) {
			Vector3 playerPos = GameManager.Instance.LevelManager.GetPlayerPosition();
			if (seePlayerTime > 0) {
				lastPlayerPosition = playerPos;
			} else if (CanSeeLocation(playerPos, transform.forward) && Vector3.Distance(playerPos, transform.position) < sightDistance) {
				lastPlayerPosition = playerPos;
				seePlayerTime = maxChaseTime;
			} else {
				lastPlayerPosition = null;
			}
		}

		if (state == EnemyState.DEAD) {
			if (GameManager.Instance?.LevelManager != null) {
				Vector3 playerPos = GameManager.Instance.LevelManager.GetPlayerPosition();

				Vector3 lookDirection = playerPos - transform.position;

				pathfinding.speed = 20f;
				pathfinding.angularSpeed = 120f;
				MoveToLocation(transform.position + (-lookDirection * 1));
			}

			deathTimeRemaining -= Time.deltaTime;
			if (deathTimeRemaining <= 0) {
				rootTransform.gameObject.SetActive(false);
				GameManager.Instance?.LevelManager?.UnegisterEnemy(this);
			}
		}

		animationController.SetBool("is_moving", pathfinding.hasPath);

		if (state == EnemyState.STUNNED) {
			stunTimeRemaining -= Time.deltaTime;
			if (stunTimeRemaining <= 0) {
				state = EnemyState.DEFAULT;
			}
		}

		if (state == EnemyState.KICK_WINDUP) {
			kickTimeRemaining -= Time.deltaTime;
			if (kickTimeRemaining <= 0) {

				if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, kickDistance)) {
					Player entity = hit.collider.GetComponentInChildren<Player>();
					if (entity) entity.OnDamageEntity(Damage.KnockbackDamage(kickForce, transform.forward));
				}

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

				RaycastHit[] hits = Physics.BoxCastAll(transform.position + (transform.forward * lightAttackRange), new Vector3(lightAttackRange, lightAttackRange, lightAttackRange), transform.forward);

				foreach (RaycastHit hit in hits) {
					Player entity = hit.collider.GetComponentInChildren<Player>();
					if (entity) entity.OnDamageEntity(Damage.LightDamage(lightAttackDamage));
				}

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

				if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, heavyAttackRange)) {
					Player entity = hit.collider.GetComponentInChildren<Player>();
					if (entity) entity.OnDamageEntity(Damage.HeavyDamage(heavyAttackDamage));
				}

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

		animationController.SetTrigger("dodge");
	}

	protected void MoveToLocation(Vector3 location) {
		pathfinding.SetDestination(location);
	}

	public bool CanSeeLocation(Vector3 position, Vector3 sightVector, float visionAngle = 90) {
		float angle = Vector3.Angle(-sightVector, (transform.position - position).normalized);

		return angle < visionAngle;
	}

	protected void StartHeavyAttack() {
		state = EnemyState.HEAVY_WINDUP;
		heavyAttackTimeRemaining = heavyAttackWindupTime;

		animationController.SetTrigger("heavy_attack");
	}

	protected void StartLightAttack() {
		state = EnemyState.LIGHT_WINDUP;
		heavyAttackTimeRemaining = lightAttackWindupTime;

		animationController.SetTrigger("light_attack");
		animationController.SetBool("light_attack_mirror", !animationController.GetBool("light_attack_mirror"));
	}

	protected void StartKickAttack() {
		state = EnemyState.KICK_WINDUP;
		kickTimeRemaining = kickWindupTime;

		animationController.SetTrigger("kick");
	}

	protected void DamageStun() {
		stunTimeRemaining = 1.1533f;
		state = EnemyState.STUNNED;
	}

	protected void Fire() {
		bool success = rangedItem?.Fire(transform.forward) ?? false;
	}

}
