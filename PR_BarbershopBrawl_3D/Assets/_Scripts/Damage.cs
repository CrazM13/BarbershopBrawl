using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage {

	public static Damage KnockbackDamage(int force, Vector3 direction) {
		return new Damage(force, DamageType.KNOCKDOWN, direction);
	}

	public static Damage RangedDamage(int amount) {
		return new Damage(amount, DamageType.RANGED);
	}

	public static Damage LightDamage(int amount) {
		return new Damage(amount, DamageType.LIGHT);
	}

	public static Damage HeavyDamage(int amount) {
		return new Damage(amount, DamageType.HEAVY);
	}

	public int Amount { private set; get; }
	public DamageType Type { private set; get; }
	public Vector3 Direction { private set; get; }

	public enum DamageType {
		LIGHT,
		HEAVY,
		KNOCKDOWN,
		RANGED
	}

	public Damage(int amount, DamageType type) : this(amount, type, Vector3.zero) {}

	public Damage(int amount, DamageType type, Vector3 direction) {
		this.Amount = amount;
		this.Type = type;
		this.Direction = direction;
	}

}
