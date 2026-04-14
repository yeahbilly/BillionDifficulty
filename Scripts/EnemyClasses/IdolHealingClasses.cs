using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using ULTRAKILL.Cheats;


namespace BillionDifficulty.EnemyPatches;

class EnemyIdentifierSaver : MonoBehaviour {
	public EnemyIdentifier eid = null;
}

class IdolHealingSetup : MonoBehaviour {
	public EnemyIdentifier eid;
	public float cooldownMax = 2f;
	public float cooldown = 0f;
	public bool stop = false;

	public float cooldownMaxOriginal;

	public void Start() {
		eid = this.GetComponent<EnemyIdentifier>();
		cooldown = cooldownMax;
		cooldownMaxOriginal = cooldownMax;
	}

	public void Update() {
		if (stop)
			return;


		if (BlindEnemies.Blind)
			return;


		cooldown += Time.deltaTime;
		if (cooldown < cooldownMax)
			return;

		cooldown = 0f;

		GameObject explosion = UnityObject.Instantiate(Plugin.Prefabs["Explosion"], eid.transform.position, eid.transform.rotation);
		explosion.name = "Idol Healing Explosion";
		explosion.transform.localScale *= 2.5f;
		UnityObject.Destroy(explosion.transform.Find("Sphere_8 (1)").gameObject);

		Explosion explosionComp = explosion.GetComponentInChildren<Explosion>();
		explosionComp.harmless = true;
		explosionComp.damage = 0;
		explosionComp.speed = 1.75f;
		explosionComp.maxSize = 2.5f;
		explosionComp.ignite = false;
		explosion.GetComponent<ExplosionController>().tryIgniteGasoline = false;

		//explosionComp.materialColor = new Color(0, 1, 0);
		MeshRenderer[] renderers = explosion.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer renderer in renderers) {
			Color color = renderer.material.color;
			renderer.material.color = new Color(0, 1, 0);
		}

		explosionComp.gameObject.AddComponent<EnemyIdentifierSaver>().eid = this.eid;

		explosion.GetComponentInChildren<AudioSource>().enabled = false;

		foreach (RemoveOnTime remove in explosion.GetComponentsInChildren<RemoveOnTime>()) {
			remove.time /= 3f;
		}

		explosionComp.GetComponent<Rigidbody>().isKinematic = false;
		explosionComp.GetComponent<SphereCollider>().isTrigger = true;
	}
}

class IdolHealingExplosion : MonoBehaviour {
	public float healing;
	public float healingReducedHardMode;
	public List<EnemyIdentifier> healedList = new List<EnemyIdentifier>();

	// idk
	public void Update() {
		GetComponent<SphereCollider>().enabled = true;
	}

	public void OnTriggerEnter(Collider other) {
		GameObject enemy = other.gameObject;
		if (enemy.layer != 10 && enemy.layer != 11)
			return;


		EnemyIdentifierIdentifier eidid = other.GetComponent<EnemyIdentifierIdentifier>();
		bool notDead = eidid && eidid.eid && !eidid.eid.dead;
		if (!notDead)
			return;

		EnemyIdentifier eid = eidid.eid;

		// heals the enemies
		if (healedList.Contains(eid))
			return;

		if (eid.enemyType == EnemyType.Drone || eid.enemyType == EnemyType.Virtue || eid.enemyType == EnemyType.Providence) {
			if (!eid.drone) {
				eid.drone = eid.GetComponent<Drone>();
			}
			if (eid.drone && eid.drone.Enemy) {
				float usedHealing = healing;
				if (Util.IsHardMode() && eid.drone.Enemy.health < 8f) {
					usedHealing = healingReducedHardMode;
				}
				float newHealth = 0.1f * (float)Mathf.RoundToInt(10f * (eid.drone.Enemy.health + usedHealing));
				eid.drone.Enemy.health = newHealth;
				eid.health = newHealth;
				healedList.Add(eid);
			}
			return;
		} else if (eid.enemyType == EnemyType.MaliciousFace) {
			if (!eid.spider) {
				eid.spider = eid.GetComponent<MaliciousFace>();
			}
			if (eid.spider) {
				float usedHealing = healing;
				if (Util.IsHardMode() && eid.spider.spider.health < 8f) {
					usedHealing = healingReducedHardMode;
				}
				float newHealth = 0.1f * (float)Mathf.RoundToInt(10f * (eid.spider.spider.health + usedHealing));
				eid.spider.spider.health = newHealth;
				eid.health = newHealth;
				healedList.Add(eid);
			}
			return;
		}

		switch (eid.enemyClass) {
			case EnemyClass.Husk:
				if (!eid.zombie) {
					eid.zombie = eid.GetComponent<Enemy>();
				}
				if (eid.zombie) {
					float usedHealing = healing;
					if (Util.IsHardMode() && eid.zombie.health < 8f) {
						usedHealing = healingReducedHardMode;
					}
					Plugin.Logger.LogWarning("healing: " + usedHealing.ToString());
					float newHealth = 0.1f * (float)Mathf.RoundToInt(10f * (eid.zombie.health + usedHealing));
					eid.zombie.health = newHealth;
					eid.health = newHealth;
					healedList.Add(eid);
				}
				break;
			case EnemyClass.Machine:
				if (!eid.machine) {
					eid.machine = eid.GetComponent<Enemy>();
				}
				if (eid.machine) {
					float usedHealing = healing;
					if (Util.IsHardMode() && eid.machine.health < 8f) {
						usedHealing = healingReducedHardMode;
					}
					float newHealth = 0.1f * (float)Mathf.RoundToInt(10f * (eid.machine.health + usedHealing));
					eid.machine.health = newHealth;
					eid.health = newHealth;
					healedList.Add(eid);
				}
				break;
			case EnemyClass.Demon:
				if (!eid.statue) {
					eid.statue = eid.GetComponent<Enemy>();
				}
				if (eid.statue) {
					float usedHealing = healing;
					if (Util.IsHardMode() && eid.statue.health < 8f) {
						usedHealing = healingReducedHardMode;
					}
					float newHealth =  0.1f * (float)Mathf.RoundToInt(10f * (eid.statue.health + usedHealing));
					eid.statue.health = newHealth;
					eid.health = newHealth;
					healedList.Add(eid);
				}
				break;
			default:
				return;
		}
	}
}