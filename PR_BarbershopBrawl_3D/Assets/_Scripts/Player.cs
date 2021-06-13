using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour {

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
	#endregion

	#region Settings
	[SerializeField] float movementSpeed;
	[SerializeField] float aimSpeedModifier;
	#endregion

	public Vector3 LastLookVector { get; private set; } = Vector2.zero;

	void Start() {
		#if UNITY_EDITOR
		if (physicsBody == null) Debug.LogError("Player Rigidbody Not Found");
		if (rotationTransform == null) Debug.LogError("Player Rotation Point Not Found");
		#endif
	}

	void Update() {

		if (CanMoveInState(state)) {
			Vector3 movement = ReadMovementInputs();

			Move(movement);

			if (CanLookInState(state)) {
				Vector3 lookVector = ReadLookInputs();

				if (lookVector == Vector3.zero) lookVector = movement;

				TurnPlayerToDirection(lookVector);

				LastLookVector = lookVector;
			}
		} else if (CanLookInState(state)) {
			Vector3 lookVector = ReadLookInputs();

			if (lookVector != Vector3.zero) {

				TurnPlayerToDirection(lookVector);
				LastLookVector = lookVector;
			}
		}
	}

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
	#endregion

}
