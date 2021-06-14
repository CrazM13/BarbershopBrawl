using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendoScript : MonoBehaviour {

	[SerializeField] float maxExtendTime;
	[SerializeField] bool returnOnCollide;
	[SerializeField] float detectRadius;
	[SerializeField] int damage;
	[SerializeField] float speed;
	[SerializeField] LayerMask ignoreLayers;
	[SerializeField] Transform returnTransform;

	private ProjectileState state;
	private float extendTimer;

	private Vector3 direction;

	private enum ProjectileState {
		IDLE,
		EXTENDING,
		RETURNING
	}

	void Start() {
		
	}

	// Update is called once per frame
	void Update() {
		if (state == ProjectileState.EXTENDING) {
			extendTimer -= Time.deltaTime;

			transform.position += direction * speed * Time.deltaTime;
			RaycastHit[] hits = Physics.BoxCastAll(transform.position, new Vector3(detectRadius, detectRadius, detectRadius), direction, Quaternion.identity, detectRadius, ~ignoreLayers);

			if (hits.Length > 0) {
				state = ProjectileState.RETURNING;

				foreach (RaycastHit hit in hits) {
					DamageableEntity entity = hit.collider.GetComponentInChildren<DamageableEntity>();
					if (entity) entity.OnDamageEntity(Damage.RangedDamage(damage));
				}
			} else if (extendTimer <= 0) {
				state = ProjectileState.RETURNING;
			}
			
		} else if (state == ProjectileState.RETURNING) {
			direction = (returnTransform.position - transform.position).normalized;

			transform.position += direction * speed * Time.deltaTime;

			if (Vector3.Distance(returnTransform.position, transform.position) < 1) {
				state = ProjectileState.IDLE;
				transform.position = returnTransform.position;
			}
		} else if (state == ProjectileState.IDLE) {
			transform.position = returnTransform.position;
		}
	}

	public bool Fire(Vector3 direction) {
		if (state != ProjectileState.IDLE) return false;

		this.direction = direction;

		extendTimer = maxExtendTime;

		transform.position = returnTransform.position;
		state = ProjectileState.EXTENDING;

		return true;
	}

}
