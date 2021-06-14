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
	[SerializeField] Animator animationController;
	[SerializeField] ExtendoScript extendo;

	[SerializeField] Transform extendoConnectPoint;
	[SerializeField] Transform extendoDisplayTransform;
	#endregion

	#region Settings
	[SerializeField] float movementSpeed;
	[SerializeField] float aimSpeedModifier;
	[SerializeField] float dodgeForce;
	[SerializeField] float dodgeDecaySpeed;
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
	private Vector3 dodgeForceVector = Vector3.zero;
	private float kickTimeRemaining = 0;
	private float lightAttackTimeRemaining = 0;
	private float heavyAttackTimeRemaining = 0;
	private bool isLightAttackButtonDown = false;
	private bool isHeavyAttackButtonDown = false;
	private bool isDodgeButtonDown = false;
	private bool isKickButtonDown = false;
	private bool isFireButtonDown = false;

	public Vector3 LastLookVector { get; private set; } = Vector2.zero;

	void Start() {
		#if UNITY_EDITOR
		if (physicsBody == null) Debug.LogError("Player Rigidbody Not Found");
		if (rotationTransform == null) Debug.LogError("Player Rotation Point Not Found");
		if (damageSource == null) Debug.LogError("Damage Source Transform Not Found");
		if (animationController == null) Debug.LogError("Animation Controller Not Found");
#endif

		Health = maxHealth;
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

		UpdateFire();

		extendoDisplayTransform.position = extendoConnectPoint.position;
		extendoDisplayTransform.rotation = extendoConnectPoint.rotation;
	}

	private void UpdateMovement() {
		if (CanMoveInState(state)) {
			Vector3 movement = ReadMovementInputs();

			animationController.SetBool("is_moving", movement != Vector3.zero);

			Move(movement);

			if (CanLookInState(state)) {
				Vector3 lookVector = ReadLookInputs();

				if (lookVector == Vector3.zero) lookVector = movement;

				if (lookVector != Vector3.zero) {

					TurnPlayerToDirection(lookVector);
					LastLookVector = lookVector;
				}
			}
		} else {

			animationController.SetBool("is_moving", false);

			if (CanLookInState(state)) {
				Vector3 lookVector = ReadLookInputs();

				if (lookVector != Vector3.zero) {

					TurnPlayerToDirection(lookVector);
					LastLookVector = lookVector;
				}
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
			physicsBody.AddForce(dodgeForceVector, ForceMode.Impulse);
			dodgeForceVector = Vector3.Lerp(dodgeForceVector, Vector3.zero, Time.deltaTime * dodgeDecaySpeed);
			if (timeRemainingInDodge <= 0) {
				state = PlayerState.DEFAULT;
				Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);
			}
		}
		isDodgeButtonDown = Input.GetAxis("Dodge") > 0;
	}
	
	private void UpdateBlock() {
		if (CanBlockInState(state)) {
			if (Input.GetAxis("Block") > 0) {
				state = PlayerState.BLOCKING;
			}
		}

		if (state == PlayerState.BLOCKING) {
			if (Input.GetAxis("Block") <= 0) {
				state = PlayerState.DEFAULT;
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
			}
		}

		if (state == PlayerState.LIGHT_SWING) {
			lightAttackTimeRemaining -= Time.deltaTime;
			if (lightAttackTimeRemaining <= 0) {
				state = PlayerState.LIGHT_COMPLETE;
				lightAttackTimeRemaining = lightAttackCompletionTime;
			}
		}

		if (state == PlayerState.LIGHT_WINDUP) {
			lightAttackTimeRemaining -= Time.deltaTime;
			if (lightAttackTimeRemaining <= 0) {
				state = PlayerState.LIGHT_SWING;
				lightAttackTimeRemaining = lightAttackDuration;
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
			}
		}

		if (state == PlayerState.HEAVY_WINDUP) {
			heavyAttackTimeRemaining -= Time.deltaTime;
			if (heavyAttackTimeRemaining <= 0) {
				state = PlayerState.HEAVY_SWING;
				heavyAttackTimeRemaining = heavyAttackDuration;
			}
		}

		if (CanHeavyAttackInState(state)) {
			if (!isHeavyAttackButtonDown && Input.GetAxis("Heavy Attack") > 0) {
				HeavyAttack();
			}
		}

		isLightAttackButtonDown = Input.GetAxis("Light Attack") > 0;
	}

	private void UpdateFire() {
		if (CanFireInState(state)) {
			if (!isFireButtonDown && Input.GetAxis("Fire") > 0) {
				Fire();
			}
		}

		isFireButtonDown = Input.GetAxis("Fire") > 0;
	}

	#endregion

	#region Damage
	public override void OnDamageEntity(Damage damage) {

		switch (damage.Type) {
			case Damage.DamageType.KNOCKDOWN:
				if (CanBeDamagedInState(state)) {
					Hurt(1);
				} else if (state == PlayerState.BLOCKING) {
					state = PlayerState.STUNNED;
					stunTimeRemaining = stunDuration;
					physicsBody.AddForce(damage.Direction * damage.Amount * 100);
				}
				break;
			default:
				if (CanBeDamagedInState(state)) {
					Hurt(damage.Amount);
				}
				break;
		}
	}

	private void Hurt(int amount) {
		animationController.SetTrigger("hurt");

		state = PlayerState.STUNNED;
		stunTimeRemaining = 1.1533f;

		Health -= amount;

		if (Health <= 0) {
			// Player Death
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

		dodgeForceVector = joyInput * dodgeForce;

		animationController.SetTrigger("dodge");

		Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
	}
	#endregion

	#region Kick

	private void Kick() {
		kickTimeRemaining = kickWindupTime;
		state = PlayerState.KICK_WINDUP;

		animationController.SetTrigger("kick");
	}

	private void KickApplyDamage() {
		state = PlayerState.DEFAULT;

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
		} else {
			state = PlayerState.LIGHT_WINDUP;
			lightAttackTimeRemaining = lightAttackWindupTime;
		}

		animationController.SetTrigger("light_attack");
		animationController.SetBool("light_attack_mirror", !animationController.GetBool("light_attack_mirror"));
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

		animationController.SetTrigger("heavy_attack");
	}

	public void ApplyHeavyAttackDamage() {
		if (Physics.Raycast(damageSource.position, LastLookVector, out RaycastHit hit, heavyAttackRange)) {
			DamageableEntity entity = hit.collider.GetComponentInChildren<DamageableEntity>();
			if (entity) entity.OnDamageEntity(Damage.HeavyDamage(heavyAttackDamage));
		}

	}

	#endregion

	#region Fire
	public void Fire() {
		bool success = extendo.Fire(LastLookVector);

		if (success) animationController.SetTrigger("fire");
	}
	#endregion

	#region State Permissions
	public bool CanMoveInState(PlayerState state) {
		return state == PlayerState.DEFAULT
			|| state == PlayerState.AIMING;
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
			&& state != PlayerState.STUNNED
			&& state != PlayerState.DODGING;
	}
	#endregion

	public Vector3 GetPosition() {
		return new Vector3(damageSource.position.x, 0, damageSource.position.z);
	}


}
