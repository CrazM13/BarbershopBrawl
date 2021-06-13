using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DamageableEntity : MonoBehaviour {
	
	[SerializeField] protected int maxHealth;
	public int Health { protected set; get; }

	public abstract void OnDamageEntity(Damage damage);

	public void OnDamageEntity(int amount, Damage.DamageType type, Vector3 direction) {
		OnDamageEntity(new Damage(amount, type, direction));
	}

	public void OnDamageEntity(int amount, Damage.DamageType type) {
		OnDamageEntity(new Damage(amount, type));
	}

}
