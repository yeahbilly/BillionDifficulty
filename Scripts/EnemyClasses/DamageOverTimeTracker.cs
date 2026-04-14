using UnityEngine;
using System.Collections.Generic;


namespace BillionDifficulty.EnemyPatches;

public class DamageOverTimeTracker : MonoBehaviour {
	public float damageThreshold = 30f;
	public float timeWindow = 5f;
	public float cooldownMax = 1f;

	public bool buffSpeed = true;
	public bool buffingSpeed = false;
	public float speedBuff = 1.25f;
	public float speedBuffTime = 5f;
	public float speedCooldown = 0f;

	public bool reached = false;
	public bool onCooldown = false;
	public List<DamageEvent> damageEvents = new List<DamageEvent>();
	public float previousHealth;
	public Enemy mach;
	public float cooldown = 0f;

	public void Start() {
		mach = GetComponent<Enemy>();
		previousHealth = mach.health;
	}
	public void Update() {
		if (buffingSpeed) {
			speedCooldown += Time.deltaTime;
			if (speedCooldown >= speedBuffTime) {
				buffingSpeed = false;
			}
		}

		if (onCooldown) {
			cooldown += Time.deltaTime;
			if (cooldown < cooldownMax)
				return;

			reached = false;
			onCooldown = false;
			cooldown = 0f;
			damageEvents.Clear();
		}

		float currentHealth = mach.health;
		float damageTaken = previousHealth - currentHealth;
		if (damageTaken > 0) {
			RegisterDamage(damageTaken);
		}
		previousHealth = currentHealth;
	}

	public void RegisterDamage(float damage) {
		float currentTime = Time.time;
		damageEvents.Add(new DamageEvent { timestamp = currentTime, amount = damage });

		damageEvents.RemoveAll(e => currentTime - e.timestamp > timeWindow);

		float totalDamage = 0f;
		foreach (var e in damageEvents)
			totalDamage += e.amount;

		if (totalDamage >= damageThreshold) {
			OnDamageThresholdReached();
			damageEvents.Clear();
		}
	}
	public void OnDamageThresholdReached() {
		reached = true;
		onCooldown = false;
		if (buffSpeed) {
			buffingSpeed = true;
		}
	}

	public class DamageEvent {
		public float timestamp;
		public float amount;
	}
}