using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityObject = UnityEngine.Object;

namespace BillionDifficulty.EnemyPatches;

/*
if variables are separated by an empty line then the top ones need to be specified for it all to work
*/
class BillionExplosionController : MonoBehaviour {
	public float maxSizeMultiplier = 1f;
	public float speedMultiplier = 1f;
	public float enemyDamageMultiplier = 1f;
	public int damage = -1;
	public float damageMultiplier = 1f;
	public void Start() {
		maxSizeMultiplier = 1f;
		speedMultiplier = 1f;
		enemyDamageMultiplier = 1f;
		damage = -1;
		damageMultiplier = 1f;
	}
}

public class StrayAttack : MonoBehaviour {
	public int projectilesLeft = 0;
	private Animator anim;
	private EnemyIdentifier eid;
	public void Start() {
		anim = this.GetComponent<Animator>();
		eid = this.GetComponent<EnemyIdentifier>();
	}
	public void Update() {
		if (!eid || eid.dead || !anim) {
			UnityObject.Destroy(this);
			return;
		}
		if (projectilesLeft > 0 && eid.zombie && eid.zombie.grounded) {
			anim.speed = 4f; // Brutal: 1.5f
		} else if (eid.zombie && !eid.zombie.grounded) {
			projectilesLeft = 0;
		}
		if (projectilesLeft == 0) {
			anim.speed = 2f; // Brutal: 1.5f
		}
	}
}

class StreetcleanerAfterburn : MonoBehaviour {
	public int damage = 0;
	public int tickCount = 4;
	
	public float cooldownMax = 0.5f;
	public float cooldown = 0f;
	public bool damagedThePlayer = false;
	public bool stoppedAttacking = false;
	public bool destroyOnEnd = false;
	public int ticks = 0;
	public void Update() {
		if (!damagedThePlayer || !stoppedAttacking) {
			return;
		}

		cooldown += Time.deltaTime;
		if (cooldown < cooldownMax) {
			return;
		}
		cooldown = 0f;

		if (ticks < tickCount) {
			NewMovement.Instance.GetHurt(damage: damage, invincible: false, scoreLossMultiplier: 0f);
			ticks += 1;
			return;
		}

		if (destroyOnEnd) {
			UnityObject.Destroy(this);
			return;
		}
		ticks = 0;
		damagedThePlayer = false;
		stoppedAttacking = false;
	}
}

/// <summary>
/// Added to the sand zone
/// </summary>
class StalkerHealBackMessenger : MonoBehaviour {
	public Enemy source;
	public void Awake() {
		source = null; // to avoid the warning message
	}
	public void HealSelf() {
		source.health += 1.5f;
		source.eid.health += 1.5f;
	}
}
/// <summary>
/// Added to enemies that touch the sand zone
/// </summary>
class StalkerHealBack : MonoBehaviour {
	public Enemy source;
	public bool healed = false;
	public void Update() {
		if (source != null && !healed) {
			source.health += 1.5f;
			source.eid.health += 1.5f;
			healed = true;
		}
	}
}

class SentryMortar : MonoBehaviour {
	public bool canShootOrb = true;
	public Rigidbody rb;
	public void FixedUpdate() {
		if (rb != null) {
			rb.AddForce(0, -5f * Mathf.Abs(rb.velocity.y), 0);
		}
	}
}

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
		mach = this.GetComponent<Enemy>();
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
			if (cooldown < cooldownMax) {
				return;
			}
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

class FerrymanStuff : MonoBehaviour {
	public float initialSpeed = 0f;
	public float maxSpeed = 0f;
	public float speedChangeMultiplier = 1f;
	public bool reset = false;
	public bool changeColor = true;
	public Color targetColor;
	public Color targetFerrymanCloakColor;

	public bool reached = false;
	public bool addedStyle = false;
	public float currentValue;
	public float time = 0f;
	public GameObject obj;
	public EnemyIdentifier eid;
	public Material colorMaterial;
	public Material ferrymanCloakColorMaterial;
	public Color initialColor;
	public Color initialFerrymanCloakColor;
	public void Start() {
		currentValue = initialSpeed;
		obj = this.gameObject;
		
		if (targetColor != null) {
			colorMaterial = obj.GetComponentInChildren<SkinnedMeshRenderer>().materials[0];
			initialColor = colorMaterial.color;
		}
		if (targetFerrymanCloakColor != null) {
			ferrymanCloakColorMaterial = obj.GetComponentInChildren<SkinnedMeshRenderer>().materials[1];
			initialFerrymanCloakColor = ferrymanCloakColorMaterial.color;
		}
	}
	public void Update() {
		if (currentValue != maxSpeed) {
			time += Time.deltaTime;
		}
		currentValue = Mathf.MoveTowards(currentValue, maxSpeed, speedChangeMultiplier * Time.deltaTime * maxSpeed);

		if (changeColor && targetColor != null && eid != null && !eid.dead) {
			colorMaterial.color = Color.Lerp(initialColor, targetColor, (currentValue - initialSpeed) / (maxSpeed - initialSpeed));
		}
		if (changeColor && targetFerrymanCloakColor != null && eid != null && !eid.dead) {
			ferrymanCloakColorMaterial.color = Color.Lerp(initialFerrymanCloakColor, targetFerrymanCloakColor, (currentValue - initialSpeed) / (maxSpeed - initialSpeed));
		}

		if (currentValue < maxSpeed) {
			return;
		}

		if (!addedStyle) {
			reached = true;
		}
		if (reset) {
			currentValue = 0f;
		}
	}
}

public class MoveBacker : MonoBehaviour {
	Rigidbody rb;
	NavMeshAgent nma;
	EnemyIdentifier eid;
	float strength = 150f;
	public bool moving = false;
	public void Start() {
		rb = this.GetComponent<Rigidbody>();
		nma = this.GetComponent<NavMeshAgent>();
		eid = this.GetComponent<EnemyIdentifier>();
	}
	public void Update() {
		if (eid.dead) {
			UnityObject.Destroy(this);
		}
	}
	public IEnumerator MoveBack() {
		moving = true;

		if (!eid || eid.dead) {
			yield break;
		}

		Vector3 target = (eid.target == null) ? NewMovement.Instance.transform.position : eid.target.position;
		Vector3 forward = target - this.transform.position;
		forward.y = 0f;
		forward.Normalize();

		Vector3 targetPosition = this.transform.position - 15f * forward;

		NavMeshHit hit;
		LayerMask groundLayer = LayerMask.GetMask("Outdoors", "OutdoorsBaked", "Environment", "EnvironmentBaked");
		float navMeshSampleRadius = 1f;
		float groundCheckDistance = 2f;

		if (!NavMesh.SamplePosition(targetPosition, out hit, navMeshSampleRadius, NavMesh.AllAreas)) {
			moving = false;
			yield break;
		}

		// checks if floor exists below the navmesh position
		Vector3 rayOrigin = hit.position + this.transform.up * 0.1f; // slightly above to avoid self-collision
		if (!Physics.Raycast(rayOrigin, -1f * this.transform.up, groundCheckDistance, groundLayer)) {
			moving = false;
			yield break;
		}
		
		if (!nma) {
			moving = false;
			yield break;
		}

		nma.enabled = false;
		rb.isKinematic = false;
		rb.useGravity = true;
		rb.drag = 12f;
		rb.AddForce(-forward * strength, ForceMode.VelocityChange);
		yield return new WaitForSeconds(0.5f);
		//yield return new WaitUntil(() => rb.velocity.magnitude < 0.05f);
		rb.isKinematic = true;
		rb.useGravity = false;

		if (!nma) {
			moving = false;
			yield break;
		}

		nma.enabled = true;
		rb.drag = 0f;
		nma.Warp(rb.position);
		moving = false;
	}
}

class CounterInt : MonoBehaviour {
	public int maxValue;
	public bool randomized = false;
	public int randomMin;
	public int randomMax;

	public int value = 1;
	public void Start() {
		TryRandomize();
	}
	public void Add() {
		if (value < maxValue) {
			value += 1;
			return;
		}
		value = 1;
		TryRandomize();
	}
	public void TryRandomize() {
		if (randomized) {
			maxValue = Random.Range(randomMin, randomMax + 1);
		}
	}
}

class TimerFloat : MonoBehaviour {
	public float target = 1f;
	public bool deleteOnReached = false;

	public bool running = false;
	public bool keepRunning = false;
	public bool reached = false;
	public float cooldownMax = 1f;
	public float cooldown = 0f;
	public void Start() {
		cooldownMax = target;
	}
	public void Update() {
		if (reached && deleteOnReached) {
			UnityObject.Destroy(this);
			return;
		}
		if (!running) {
			return;
		}

		cooldown += Time.deltaTime;
		if (cooldown < cooldownMax) {
			return;
		}

		reached = true;
		if (!keepRunning) {
			running = false;
		}
	}
	public void Run() {
		running = true;
	}
	public void Reset() {
		cooldown = 0f;
		reached = false;
	}
	public void ResetAndRun() {
		Reset();
		Run();
	}
	public void Stop() {
		running = false;
	}
	public void ResetAndStop() {
		Reset();
		Stop();
	}
}

public class BoolValue : MonoBehaviour {
	public bool value = false;
	public string description;

	public static bool? Get(string target, GameObject obj) {
		foreach (BoolValue bv in obj.GetComponents<BoolValue>()) {
			if (bv.description == target) {
				return bv.value;
			}
		}
		return null;
	}
	public static void Set(string target, bool newValue, GameObject obj) {
		foreach (BoolValue bv in obj.GetComponents<BoolValue>()) {
			if (bv.description == target) {
				bv.value = newValue;
			}
		}
	}
}