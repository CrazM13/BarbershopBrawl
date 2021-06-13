using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : DamageableEntity {

	#region State Machine
	private PlayerState state = PlayerState.DEFAULT;

	public enum PlayerState {
		DEFAULT,
		STUNNED,
		DODGING,
		BLOCKING,
		LIGHT_WINDUP,
		LIGHT_SWING,
		LIGHT_COMPLETE,
		HEAVY_WINDUP,
		HEAVY_SWING,
		KICK_WINDUP,
		AIMING
	}
	#endregion

	#region Components
	[SerializeField] Rigidbody physicsBody;
	[SerializeField] Transform rotationTransform;
	[SerializeField] Transform damageSource;
	[SerializeField] MeshRenderer debugPlayerModel;
	[SerializeField] GameObject debugKickIndicator;
	#endregion

	#region Settings
	[SerializeField] float movementSpeed;
	[SerializeField] float aimSpeedModifier;
	[SerializeField] float dodgeForce;
	[SerializeField] float dodgeCompletionTime;
	[SerializeField] float kickWindupTime;
	[SerializeField] float kickDistance;
	[SerializeField] int kickForce;
	[SerializeField] int stunDuration;
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

	private float stunTimeRemaining = 0;
	private float timeRemainingInDodge = 0;
	private float kickTimeRemaining = 0;
	private float lightAttackTimeRemaining = 0;
	private float heavyAttackTimeRemaining = 0;
	private bool isLightAttackButtonDown = false;
	private bool isHeavyAttackButtonDown = false;
	private bool isDodgeButtonDown = false;
	private bool isKickButtonDown = false;

	public Vector3 LastLookVector { get; private set; } = Vector2.zero;

	void Start() {
		#if UNITY_EDITOR
		if (physicsBody == null) Debug.LogError("Player Rigidbody Not Found");
		if (rotationTransform == null) Debug.LogError("Player Rotation Point Not Found");
		if (damageSource == null) Debug.LogError("Damage Source Transform Not Found");
		if (debugPlayerModel == null) Debug.LogError("Debug Player Model Not Found. Debug using it will be ignored");
		if (debugKickIndicator == null) Debug.LogError("Debug Kick Indicator Not Found. Debug using it will be ignored");
		#endif
	}

	#region Update
	void Update() {

		UpdateMovement();

		UpdateDodge();

		UpdateBlock();

		UpdateKick();

		UpdateStun();

		UpdateLightAttack();

		UpdateHeavyAttack();
	}

	private void UpdateMovement() {
		if (CanMoveInState(state)) {
			Vector3 movement = ReadMovementInputs();

			Move(movement);

			if (CanLookInState(state)) {
				Vector3 lookVector = ReadLookInputs();

				if (lookVector == Vector3.zero) lookVector = movement;

				if (lookVector != Vector3.zero) {

					TurnPlayerToDirection(lookVector);
					LastLookVector = lookVector;
				}
			}
		} else if (CanLookInState(state)) {
			Vector3 lookVector = ReadLookInputs();

			if (lookVector != Vector3.zero) {

				TurnPlayerToDirection(lookVector);
				LastLookVector = lookVector;
			}
		}
	}

	private void UpdateDodge() {
		if (CanDodgeInState(state)) {
			if (!isDodgeButtonDown && Input.GetAxis("Dodge") > 0) {
				Dodge();
			}
		}
		if (state == PlayerState.DODGING) {
			timeRemainingInDodge -= Time.deltaTime;
			if (timeRemainingInDodge <= 0) state = PlayerState.DEFAULT;
		}
		isDodgeButtonDown = Input.GetAxis("Dodge") > 0;
	}
	
	private void UpdateBlock() {
		if (CanBlockInState(state)) {
			if (Input.GetAxis("Block") > 0) {
				state = PlayerState.BLOCKING;
				DebugTint(Color.yellow);
			}
		}

		if (state == PlayerState.BLOCKING) {
			if (Input.GetAxis("Block") <= 0) {
				state = PlayerState.DEFAULT;
				DebugTint(Color.grey);
			}
		}
	}

	private void UpdateKick() {
		if (state == PlayerState.KICK_WINDUP) {
			kickTimeRemaining -= Time.deltaTime;

			if (kickTimeRemaining <= 0) {
				KickApplyDamage();
			}
		}

		if (CanKickInState(state)) {
			if (!isKickButtonDown && Input.GetAxis("Kick") > 0) {
				Kick();
			}
		}
		isKickButtonDown = Input.GetAxis("Kick") > 0;
	}

	private void UpdateStun() {
		if (state == PlayerState.STUNNED) {
			stunTimeRemaining -= Time.deltaTime;
			if (stunTimeRemaining <= 0) {
				state = PlayerState.DEFAULT;
			}
		}
	}

	private void UpdateLightAttack() {
		if (state == PlayerState.LIGHT_COMPLETE) {
			lightAttackTimeRemaining -= Time.deltaTime;
			if (lightAttackTimeRemaining <= 0) {
				state = PlayerState.DEFAULT;

				DebugTint(Color.grey);
			}
		}

		if (state == PlayerState.LIGHT_SWING) {
			lightAttackTimeRemaining -= Time.deltaTime;
			if (lightAttackTimeRemaining <= 0) {
				state = PlayerState.LIGHT_COMPLETE;
				lightAttackTimeRemaining = lightAttackCompletionTime;

				DebugTint(Color.blue);
			}
		}

		if (state == PlayerState.LIGHT_WINDUP) {
			lightAttackTimeRemaining -= Time.deltaTime;
			if (lightAttackTimeRemaining <= 0) {
				state = PlayerState.LIGHT_SWING;
				lightAttackTimeRemaining = lightAttackDuration;

				DebugTint(Color.red);
			}
		}

		if (CanLightAttackInState(state)) {
			if (!isLightAttackButtonDown && Input.GetAxis("Light Attack") > 0) {
				LightAttack();
			}
		}

		isLightAttackButtonDown = Input.GetAxis("Light Attack") > 0;
	}

	private void UpdateHeavyAttack() {

		if (state == PlayerState.HEAVY_SWING) {
			heavyAttackTimeRemaining -= Time.deltaTime;
			if (heavyAttackTimeRemaining <= 0) {
				state = PlayerState.DEFAULT;

				DebugTint(Color.grey);
			}
		}

		if (state == PlayerState.HEAVY_WINDUP) {
			heavyAttackTimeRemaining -= Time.deltaTime;
			if (heavyAttackTimeRemaining <= 0) {
				state = PlayerState.HEAVY_SWING;
				heavyAttackTimeRemaining = heavyAttackDuration;

				DebugTint(Color.red);
			}
		}

		if (CanHeavyAttackInState(state)) {
			if (!isHeavyAttackButtonDown && Input.GetAxis("Heavy Attack") > 0) {
				HeavyAttack();
			}
		}

		isLightAttackButtonDown = Input.GetAxis("Light Attack") > 0;
	}
	#endregion

	#region Damage
	public override void OnDamageEntity(Damage damage) {
		switch (damage.Type) {
			case Damage.DamageType.KNOCKDOWN:
				state = PlayerState.STUNNED;
				stunTimeRemaining = stunDuration;
				physicsBody.AddForce(damage.Direction * damage.Amount);
				break;
			default:
				if (!CanBeDamagedInState(state)) {
					Health -= damage.Amount;

					if (Health <= 0) {
						// Player Death
					}
				}

				break;
		}
	}
	#endregion

	#region Movement
	private Vector3 ReadMovementInputs() {
		Vector3 movementDirection = Vector3.zero;

		movementDirection.x = Input.GetAxis("Horizontal");
		movementDirection.z = Input.GetAxis("Vertical");

		if (movementDirection.magnitude == 0) return Vector3.zero;

		return movementDirection.normalized;
	}

	private void Move(Vector3 direction) {
		Vector3 movementVector = direction * GetMovementSpeedFromState(state);

		physicsBody.AddForce(movementVector, ForceMode.Impulse);
	}

	private float GetMovementSpeedFromState(PlayerState state) {
		if (state == PlayerState.AIMING) return movementSpeed * aimSpeedModifier;
		return movementSpeed;
	}
	#endregion

	#region Aim
	private Vector3 ReadLookInputs() {
		Vector3 lookDirection = Vector3.zero;

		lookDirection.x = Input.GetAxis("Look Horizontal");
		lookDirection.z = Input.GetAxis("Look Vertical");

		if (lookDirection.magnitude == 0) return Vector3.zero;

		return lookDirection.normalized;
	}

	private void TurnPlayerToDirection(Vector3 direction) {
		Quaternion targetRotation = Quaternion.LookRotation(direction);

		rotationTransform.localRotation = targetRotation;
	}
	#endregion

	#region Dodge
	private void Dodge() {
		state = PlayerState.DODGING;
		timeRemainingInDodge = dodgeCompletionTime;

		Vector3 joyInput = ReadMovementInputs();

		if (joyInput == Vector3.zero) joyInput = Vector3.back;

		physicsBody.AddForce(joyInput * dodgeForce, ForceMode.Impulse); 
	}
	#endregion

	#region Kick

	private void Kick() {
		kickTimeRemaining = kickWindupTime;
		state = PlayerState.KICK_WINDUP;

		DebugKickIndicator(true);
	}

	private void KickApplyDamage() {
		state = PlayerState.DEFAULT;
		DebugKickIndicator(false);

		if (Physics.Raycast(damageSource.position, LastLookVector, out RaycastHit hit, kickDistance)) {
			DamageableEntity entity = hit.collider.GetComponentInChildren<DamageableEntity>();
			if (entity) entity.OnDamageEntity(Damage.KnockbackDamage(kickForce, LastLookVector));
		}
	}

	#endregion

	#region Light Attack

	public void LightAttack() {
		if (state == PlayerState.LIGHT_COMPLETE) {
			state = PlayerState.LIGHT_SWING;
			lightAttackTimeRemaining = lightAttackDuration;

			DebugTint(Color.red);
		} else {
			state = PlayerState.LIGHT_WINDUP;
			lightAttackTimeRemaining = lightAttackWindupTime;

			DebugTint(Color.green);
		}
	}

	public void ApplyLightAttackDamage() {
		RaycastHit[] hits = Physics.BoxCastAll(damageSource.position + (LastLookVector * lightAttackRange), new Vector3(lightAttackRange, lightAttackRange, lightAttackRange), LastLookVector);

		foreach (RaycastHit hit in hits) {
			DamageableEntity entity = hit.collider.GetComponentInChildren<DamageableEntity>();
			if (entity) entity.OnDamageEntity(Damage.LightDamage(lightAttackDamage));
		}

	}

	#endregion

	#region Heavy Attack

	public void HeavyAttack() {
		state = PlayerState.HEAVY_WINDUP;
		heavyAttackTimeRemaining = heavyAttackWindupTime;

		DebugTint(Color.green);
	}

	public void ApplyHeavyAttackDamage() {
		if (Physics.Raycast(damageSource.position, LastLookVector, out RaycastHit hit, heavyAttackRange)) {
			DamageableEntity entity = hit.collider.GetComponentInChildren<DamageableEntity>();
			if (entity) entity.OnDamageEntity(Damage.HeavyDamage(heavyAttackDamage));
		}

	}

	#endregion

	#region State Permissions
	public bool CanMoveInState(PlayerState state) {
		return state == PlayerState.DEFAULT
			|| state == PlayerState.AIMING
			|| state == PlayerState.LIGHT_COMPLETE;
	}

	public bool CanLookInState(PlayerState state) {
		return state != PlayerState.STUNNED;
	}

	public bool CanBlockInState(PlayerState state) {
		return state != PlayerState.LIGHT_SWING
			&& state != PlayerState.HEAVY_SWING
			&& state != PlayerState.STUNNED
			&& state != PlayerState.DODGING;
	}

	public bool CanDodgeInState(PlayerState state) {
		return state != PlayerState.LIGHT_SWING
			&& state != PlayerState.HEAVY_SWING
			&& state != PlayerState.STUNNED
			&& state != PlayerState.DODGING;
	}

	public bool CanLightAttackInState(PlayerState state) {
		return state == PlayerState.DEFAULT
			|| state == PlayerState.AIMING
			|| state == PlayerState.LIGHT_COMPLETE;
	}

	public bool CanHeavyAttackInState(PlayerState state) {
		return state == PlayerState.DEFAULT
			|| state == PlayerState.AIMING;
	}

	public bool CanFireInState(PlayerState state) {
		return state == PlayerState.DEFAULT
			|| state == PlayerState.AIMING;
	}

	public bool CanKickInState(PlayerState state) {
		return state == PlayerState.DEFAULT
			|| state == PlayerState.AIMING;
	}

	public bool CanBeDamagedInState(PlayerState state) {
		return state != PlayerState.BLOCKING
			&& state != PlayerState.DODGING;
	}
	#endregion

	public Vector3 GetPosition() {
		return new Vector3(damageSource.position.x, 0, damageSource.position.z);
	}

	#region Debug
	private void DebugTint(Color c) {
		if (!debugPlayerModel) return;

		debugPlayerModel.material.color = c;
	}

	private void DebugKickIndicator(bool show) {
		if (!debugKickIndicator) return;

		debugKickIndicator.SetActive(show);
	}
	#endregion

}
