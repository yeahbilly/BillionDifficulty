using System.Collections;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: ZOMBIE !!!
// Zombie
[HarmonyPatch(typeof(Zombie))]
public class ZombiePatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(ZombieMelee), nameof(ZombieMelee.Start))]
	public static void ZombieMeleeStartPostfix(ZombieMelee __instance) {
		if (__instance.difficulty != 19)
			return;
		if (__instance.eid.dead)
			return;

		float speedMultiplier = 1f;
		float animSpeed = 1f;
		float nmaSpeed = 1f;
		float nmaAngularSpeed = 1f;
		float nmaAcceleration = 1f;
		
		switch (__instance.eid.enemyType) {
			case EnemyType.Filth:
				if (!Util.IsHardMode()) {
					speedMultiplier = 1.75f; // Brutal: 1.5f
					animSpeed = 1.75f; // Brutal: 1.5f
					nmaAngularSpeed = 9000f; // Brutal: 9000f
					nmaAcceleration = 240f; // Brutal: 120f
					nmaSpeed = 20f * speedMultiplier; // default: 10f * ...
					break;
				}
				speedMultiplier = 2f; // Brutal: 1.5f
				animSpeed = 2f; // Brutal: 1.5f
				nmaAngularSpeed = 12000f; // Brutal: 9000f
				nmaAcceleration = 400f; // Brutal: 120f
				nmaSpeed = 20f * speedMultiplier; // default: 10f * ...
				break;
		}

		__instance.anim.speed = animSpeed * __instance.eid.totalSpeedModifier;
		__instance.nma.speed = nmaSpeed * __instance.eid.totalSpeedModifier;
		__instance.nma.angularSpeed = nmaAngularSpeed * __instance.eid.totalSpeedModifier;
		__instance.nma.acceleration = nmaAcceleration * __instance.eid.totalSpeedModifier;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ZombieProjectiles), nameof(ZombieProjectiles.Start))]
	public static void ZombieProjectilesStartPostfix(ZombieProjectiles __instance) {
		if (__instance.difficulty != 19)
			return;
		if (__instance.eid.dead)
			return;

		float speedMultiplier = 1f;
		float animSpeed = 1f;
		float nmaSpeed = 1f;
		float nmaAngularSpeed = 1f;
		float nmaAcceleration = 1f;
		
		switch (__instance.eid.enemyType) {
			case EnemyType.Stray:
				if (!Util.IsHardMode()) {
					speedMultiplier = 1.75f; // Brutal: 1.5f
					animSpeed = 1.75f; // Brutal: 1.5f
					nmaSpeed = 12.5f * speedMultiplier; // default: 10f * ...
					nmaAngularSpeed = 800f; // Brutal: 800f
					nmaAcceleration = 50f; // Brutal: 30f
					break;
				}
				speedMultiplier = 2f; // Brutal: 1.5f
				animSpeed = 2.5f; // Brutal: 1.5f
				nmaSpeed = 17.5f * speedMultiplier; // default: 10f * ...
				nmaAngularSpeed = 1600f; // Brutal: 800f
				nmaAcceleration = 80f; // Brutal: 30f
				break;
			case EnemyType.Schism:
				if (!Util.IsHardMode()) {
					speedMultiplier = 1.75f; // Brutal: 1.5f
					animSpeed = 1.5f; // Brutal: 1.5f
					nmaSpeed = 12.5f * speedMultiplier; // default: 10f * ...
					nmaAngularSpeed = 800f; // Brutal: 800f
					nmaAcceleration = 50f; // Brutal: 30f
					break;
				}
				speedMultiplier = 2f; // Brutal: 1.5f
				animSpeed = 1.5f; // Brutal: 1.5f
				nmaSpeed = 17.5f * speedMultiplier; // default: 10f * ...
				nmaAngularSpeed = 1600f; // Brutal: 800f
				nmaAcceleration = 80f; // Brutal: 30f
				break;
			case EnemyType.Soldier:
				float runSpeed;
				if (!Util.IsHardMode()) {
					speedMultiplier = 1.75f; // Brutal: 1.5f
					animSpeed = 1.2f; // Brutal: 1f
					nmaAngularSpeed = 480f; // Brutal: 480f
					nmaAcceleration = 480f; // Brutal: 480f
					runSpeed = 1.2f * speedMultiplier; // Brutal: 1f * ...
					__instance.anim.SetFloat("RunSpeed", runSpeed);
					nmaSpeed = 20f * speedMultiplier; // Brutal: 17.5f * ...
					break;
				}
				speedMultiplier = 2f; // Brutal: 1.5f
				animSpeed = 1.35f; // Brutal: 1f
				nmaAngularSpeed = 600f; // Brutal: 480f
				nmaAcceleration = 560f; // Brutal: 480f
				runSpeed = 1.35f * speedMultiplier; // Brutal: 1f * ...
				__instance.anim.SetFloat("RunSpeed", runSpeed);
				nmaSpeed = 24f * speedMultiplier; // Brutal: 17.5f * ...
				break;
		}

		__instance.anim.speed = animSpeed * __instance.eid.totalSpeedModifier;
		__instance.nma.speed = nmaSpeed * __instance.eid.totalSpeedModifier;
		__instance.nma.angularSpeed = nmaAngularSpeed * __instance.eid.totalSpeedModifier;
		__instance.nma.acceleration = nmaAcceleration * __instance.eid.totalSpeedModifier;
	}
}


// ZombieProjectiles
[HarmonyPatch(typeof(ZombieProjectiles))]
public class ZombieProjectilesPatch {
	// STRAY/SCHISM/SOLDIER PATCH (speed)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(ZombieProjectiles), nameof(ZombieProjectiles.GetSpeed))]
	public static bool GetSpeedPrefix(int difficulty, ZombieProjectiles __instance, ref EnemyMovementData __result) {
		if (difficulty != 19)
			return true;

		float speedResult = 0f;
		float angularSpeedResult = 0f;
		float accelerationResult = 0f;
		
		switch (__instance.eid.enemyType) {
			case EnemyType.Soldier:
				if (!Util.IsHardMode()) {
					speedResult = 17.5f * 1.75f; // Brutal: 30f
					angularSpeedResult = 480f; // Brutal: 480f
					accelerationResult = 440f; // Brutal: 400f
					break;
				}
				speedResult = 40f; // Brutal: 30f
				angularSpeedResult = 600f; // Brutal: 480f
				accelerationResult = 560f; // Brutal: 400f
				break;
			case EnemyType.Stray:
			case EnemyType.Schism:
				if (!Util.IsHardMode()) {
					speedResult = 12.5f * 1.75f; // Brutal: 15f
					angularSpeedResult = 800f; // Brutal: 800f
					accelerationResult = 50f; // Brutal: 30f
					break;
				}
				speedResult = 35f; // Brutal: 15f
				angularSpeedResult = 1600f; // Brutal: 800f
				accelerationResult = 80f; // Brutal: 30f
				break;
		}

		__result = new EnemyMovementData {
			speed = speedResult,
			angularSpeed = angularSpeedResult,
			acceleration = accelerationResult
		};
		return false;
	}

	// STRAY PATCH AND SOLDIER PATCH (setup)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(ZombieProjectiles), nameof(ZombieProjectiles.Start))]
	public static void StartPostfix(ZombieProjectiles __instance) {
		if (__instance.difficulty != 19)
			return;

		if (__instance.eid.enemyType == EnemyType.Stray) {
			StrayAttack strayAttack = __instance.gameObject.AddComponent<StrayAttack>();
			strayAttack.eid = __instance.eid;
			strayAttack.anim = __instance.anim;
			strayAttack.projectilesLeft = 0;
			if (!Util.IsHardMode()) {
				strayAttack.animSpeedSlow = 1.75f;
				strayAttack.animSpeedFast = 4f;
			} else {
				strayAttack.animSpeedSlow = 2.5f;
				strayAttack.animSpeedFast = 6f;
			}
		}
		else if (Util.IsHardMode() && __instance.eid.enemyType == EnemyType.Soldier) {
			// __instance.projectile = Plugin.Prefabs["Projectile"];
			__instance.projectile = Plugin.Prefabs["ProjectileHomingExplosive"];
			CounterInt counter = __instance.gameObject.AddComponent<CounterInt>();
			counter.maxValue = 2;
		}
	}
	
	// STRAY PATCH and SOLDIER PATCH (attack)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(ZombieProjectiles), nameof(ZombieProjectiles.ThrowProjectile))]
	public static void ThrowProjectilePostfix(ZombieProjectiles __instance, bool __runOriginal) {
		if (__instance.difficulty != 19 || !__runOriginal)
			return;

		Projectile proj = __instance.currentProjectile.GetComponent<Projectile>();
		if (__instance.eid.enemyType == EnemyType.Stray) {
			__instance.coolDownReduce = __instance.coolDown;

			proj.speed = 90f; // Brutal: 65f
			if (Util.IsHardMode()) {
				proj.speed = 90f;
				proj.homingType = HomingType.Gradual; // Brutal: none
				proj.turningSpeedMultiplier = 0.7f; // Brutal: none
				// proj.turnSpeed = 75f; // Brutal: none
			}

			StrayAttack strayAttack = __instance.GetComponent<StrayAttack>();
			strayAttack.projectilesLeft -= 1;
			if (strayAttack.projectilesLeft == 0) {
				if (!Util.IsHardMode()) {
					__instance.coolDownReduce = 1.6f;
				} else {
					__instance.coolDownReduce = 1.2f;
				}
			}
		}

		if (__instance.eid.enemyType != EnemyType.Soldier)
			return;
		proj = __instance.currentProjectile.GetComponentInChildren<Projectile>();

		__instance.coolDownReduce = 2f; // Brutal: 1f
		if (Util.IsHardMode())
			__instance.coolDownReduce = 2.5f;
		
		if (!Util.IsHardMode()) {
			__instance.currentProjectile.transform.localScale = new Vector3(3, 3, 3); // default: 1, 1, 1
			ProjectileSpread projectileSpreadComp = __instance.currentProjectile.GetComponent<ProjectileSpread>();
			projectileSpreadComp.projectileAmount = 8; // Brutal: 5
			projectileSpreadComp.spreadAmount = 12f; // Brutal: 10f

			proj.speed = 82.5f; // Brutal: 65f
			return;
		}

		proj.bigExplosion = true;
		proj.speed = 60f;
		proj.ignoreExplosions = true;
		proj.enemyDamageMultiplier = 0f;
		proj.safeEnemyType = EnemyType.Soldier;
		proj.homingType = HomingType.Instant;
		proj.turningSpeedMultiplier = 1f;
		proj.stopTrackingAfterSeconds = 2f;
		proj.GetComponent<RemoveOnTime>().time = 2.5f;

		CounterInt counter = __instance.GetComponent<CounterInt>();
		switch (counter.value) {
			case 1:
				proj.speed -= 15f;
				break;
			case 2:
				proj.speed += 15f;
				break;
		}

		if (counter.value < 2) {
			counter.Add();
			// __instance.ThrowProjectile();
			__instance.StartCoroutine(SoldierAttackDelay(__instance));
			return;
		}
		counter.Add();

		// __instance.currentProjectile.transform.localScale *= 1.5f;
		// AlternatingProjectileSplits splits = __instance.currentProjectile.AddComponent<AlternatingProjectileSplits>();
		// splits.maxLayer = 3;
		// splits.cooldownMax = 0.1f;
		// splits.projectileSpeed = 80f;
		// splits.projectileSpeedIncreasePerLayer = 15f;
		// splits.projectileScaleMultiplier = 1.5f;
		// splits.projectileScaleIncreasePerLayer = 1.25f;
		// splits.rotationOffset = 17.5f;
		// __instance.currentProjectile.GetComponent<Projectile>().speed = 80f;
	}

	public static IEnumerator SoldierAttackDelay(ZombieProjectiles __instance) {
		yield return new WaitForSeconds(0.15f);
		__instance.ThrowProjectile();
		yield break;
	}

	// STRAY PATCH (resets stray attack counter after 3 attacks)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(ZombieProjectiles), nameof(ZombieProjectiles.Swing))]
	public static void SwingPrefix(ZombieProjectiles __instance) {
		if (__instance.difficulty != 19)
			return;

		if (__instance.eid.enemyType == EnemyType.Stray) {
			StrayAttack strayAttack = __instance.GetComponent<StrayAttack>();
			if (strayAttack.projectilesLeft == 0) {
				strayAttack.projectilesLeft = 3;
			}
		}
	}
	
	// STRAY PATCH (resets stray speed when interrupted)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(ZombieProjectiles), nameof(ZombieProjectiles.Update))]
	public static void UpdatePostfix(ZombieProjectiles __instance) {
		if (__instance.difficulty != 19)
			return;

		if (__instance.eid.enemyType == EnemyType.Stray) {
			StrayAttack strayAttack = __instance.GetComponent<StrayAttack>();
			if (__instance.anim.GetBool("Running")) {
				strayAttack.projectilesLeft = 0;
			}
		}
	}
	
	// SCHISM PATCH (attack)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(ZombieProjectiles), nameof(ZombieProjectiles.ShootProjectile))]
	public static void ShootProjectilePostfix(ZombieProjectiles __instance) {
		if (__instance.difficulty != 19)
			return;

		Projectile projectile = __instance.currentProjectile.GetComponent<Projectile>();
		projectile.speed = 60f; // Brutal: 30f
		projectile.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); // default: 1f, 1f, 1f

		SlowDownOverTimeEase slowDown = __instance.currentProjectile.AddComponent<SlowDownOverTimeEase>();
		//slowDown.slowRate = 0.65f; // no Ease
		slowDown.slowRate = 0.15f; // with Ease
		slowDown.rateEase = 0.05f;
		slowDown.makeUnparryableOnStop = true;
		slowDown.changeColorOnStop = true;
		slowDown.hasBeam = true;
		slowDown.newProjectileColor = new Color(0.6f, 0.65f, 0f);
		slowDown.volumeMultiplier = 0.8f;
		if (Util.IsHardMode()) {
			slowDown.reverseTimes = 5;
			projectile.ignoreEnvironment = true;
		}
		
		ChangeScaleOverTime changeScale = __instance.currentProjectile.AddComponent<ChangeScaleOverTime>();
		changeScale.delay = 5.25f;
		changeScale.time = 1f;
		changeScale.targetScaleMultiplier = 0f;
		RemoveOnTime remover = __instance.currentProjectile.GetComponent<RemoveOnTime>();
		remover.time = 6.25f;
		if (Util.IsHardMode()) {
			changeScale.delay = 6.75f;
			remover.time = 7.75f;
		}

		LineRenderer line = __instance.currentProjectile.GetComponentInChildren<LineRenderer>();
		if (line != null) {
			line.startWidth = 1.5f; // default: 1f
			line.endWidth = 1.5f; // default: 1f
			changeScale.scaleBeam = true;
		}
	}
}

// SCHISM PATCH (makes beams not get destroyed)
[HarmonyPatch(typeof(ContinuousBeam), nameof(ContinuousBeam.Start))]
public class ContinuousBeamPatch {
	public static void Postfix(ContinuousBeam __instance) {
		if (!Util.IsHardMode())
			return;

		SlowDownOverTimeEase slowDown = __instance.transform.parent.GetComponent<SlowDownOverTimeEase>();
		if (slowDown != null) {
			__instance.cancelIfEndPointBlocked = false;
			__instance.canHitEnemy = false;
		}
	}
}

// SCHISM PATCH (makes projectiles not get destroyed after colliding with a wall)
// [HarmonyPatch(typeof(Projectile), nameof(Projectile.Collided))]
// public class ZombieProjectileCollidedPatch {
// 	public static bool Prefix(Collider other, Projectile __instance) {
// 		if (!Util.IsHardMode() || __instance.GetComponent<SlowDownOverTimeEase>() == null) {
// 			return true;
// 		}

// 		int[] ignoredLayers = new int[] {0, 6, 7, 8, 24, 25};

// 		if (ignoredLayers.Contains(other.gameObject.layer)) {
// 			return false;
// 		}

// 		// if (!other.gameObject.CompareTag("Player")) {
// 		// 	return false;
// 		// }

// 		return true;
// 	}
// }


// ZombieMelee
[HarmonyPatch(typeof(ZombieMelee))]
public class ZombieMeleePatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(ZombieMelee), nameof(ZombieMelee.Start))]
	public static void StartPostfix(ZombieMelee __instance) {
		if (__instance.difficulty != 19)
			return;

		__instance.defaultCoolDown = 0.2f; // Brutal: 0.25f
		if (Util.IsHardMode()) {
			__instance.mach.health = 1.5f;
			__instance.eid.health = 1.5f;
		}
	}

	// FILTH PATCH (speed)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(ZombieMelee), nameof(ZombieMelee.GetSpeed))]
	public static bool GetSpeedPrefix(int difficulty, ref EnemyMovementData __result) {
		if (difficulty != 19)
			return true;
		
		if (!Util.IsHardMode()) {
			__result = new EnemyMovementData {
				speed = 35f, // default: 20f
				acceleration = 240f,
				angularSpeed = 9000f
			};
			return false;
		}
		__result = new EnemyMovementData {
			speed = 40f, // default: 20f
			acceleration = 400f,
			angularSpeed = 12000f
		};
		return false;
	}

	// FILTH PATCH (double jump and fast attack)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(ZombieMelee), nameof(ZombieMelee.Update))]
	public static bool UpdatePrefix(ZombieMelee __instance) {
		if (__instance.difficulty != 19)
			return true;

		if (__instance.diving) {
			__instance.transform.localRotation = Quaternion.identity;
			__instance.modelTransform.LookAt(__instance.transform.position + __instance.transform.forward + __instance.rb.velocity.normalized * 5f);
			__instance.modelTransform.Rotate(Vector3.right * 90f, Space.Self);
		} else {
			__instance.modelTransform.localRotation = Quaternion.identity;
			if (__instance.damaging) {
				__instance.rb.isKinematic = false;
				float d = 1f;
				if (__instance.difficulty >= 4) {
					d = 1.25f;
				}
				__instance.rb.velocity = __instance.transform.forward * 40f * d * __instance.anim.speed;
			}
		}

		if (__instance.coolDown >= 0f) {
			__instance.coolDown = Mathf.MoveTowards(__instance.coolDown, 0f, 0.4f * Time.deltaTime * __instance.eid.totalSpeedModifier);
		}

		if (__instance.target == null)
			return false;

		__instance.UpdateTargetVision();

		if (__instance.track && __instance.hasVision) {
			if (__instance.difficulty > 1) {
				__instance.transform.LookAt(__instance.ToPlanePos(__instance.targetData.position));
			} else {
				float num = (__instance.difficulty == 0) ? 360f : 720f;
				__instance.transform.rotation = Quaternion.RotateTowards(__instance.transform.rotation, Quaternion.LookRotation(__instance.ToPlanePos(__instance.targetData.position) - __instance.transform.position), Time.deltaTime * num * __instance.eid.totalSpeedModifier);
			}
		}
		if (__instance.hasDimensionalTarget)
			return false;


		if (__instance.coolDown > 0f)
			return false;


		bool canAttack;
		if (__instance.difficulty == 19) {
			canAttack = true;
		} else {
			canAttack = __instance.mach.grounded && !__instance.nma.isOnOffMeshLink && !__instance.mach.isTraversingPortalLink && !__instance.aboutToDive && !__instance.inAction && !__instance.damaging;
		}

		if (canAttack) {
			if (__instance.difficulty == 19) {
				__instance.coolDown = 0.1f;
			}

			if (Vector3.Distance(__instance.hasVision ? __instance.targetData.position : __instance.target.position, __instance.transform.position) < __instance.swingDistance) {
				__instance.Swing();
				return false;
			}
			if (__instance.difficulty >= 4) {
				__instance.DiveCheck();
			}
		}
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ZombieMelee), nameof(ZombieMelee.DiveCheck))]
	public static bool DiveCheckPrefix(ZombieMelee __instance) {
		if (__instance.difficulty != 19)
			return true;

		float jumpDistance;
		if (__instance.difficulty == 19) {
			jumpDistance = 17.5f;
		} else {
			jumpDistance = 20f;
		}
		
		if (!__instance.hasVision || __instance.targetData.DistanceTo(__instance.transform.position, false) > jumpDistance)
			return false;

		__instance.diveTargetPos = __instance.targetData.position;
		if (__instance.diveTargetPos.y > __instance.transform.position.y + 5f) {
			__instance.aboutToDive = true;
			if (__instance.difficulty == 19) {
				__instance.coolDown = 0.2f; // previous jump cooldown: 0.05f
			}
			__instance.Invoke("JumpAttack", Random.Range(0f, 0.5f));
			return false;
		}
		if (__instance.targetData.DistanceTo(__instance.transform.position, false) > 10f && __instance.randomJumpChanceCooldown > 1f) {
			__instance.randomJumpChanceCooldown = 0f;
			if (Random.Range(0f, 1f) > 0.8f) {
				if (__instance.difficulty == 19) {
					__instance.coolDown = 0.2f; // previous jump cooldown: 0.05f
				}
				__instance.JumpAttack();
			}
		}
		return false;
	}

	// FILTH PATCH (JUMP FORCE)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(ZombieMelee), nameof(ZombieMelee.JumpStart))]
	public static bool JumpPrefix(ZombieMelee __instance) {
		if (__instance.difficulty != 19)
			return true;

		__instance.transform.LookAt(__instance.ToPlanePos(__instance.diveTargetPos));
		// default: 2f instead of 1.5f, 25f instead of 8f
		__instance.mach.Jump(__instance.transform.up * 25f + Vector3.ClampMagnitude(
			new Vector3(
				(__instance.diveTargetPos.x - __instance.transform.position.x) * 1.5f,
				0f,
				(__instance.diveTargetPos.z - __instance.transform.position.z) * 1.5f
				),
			8f
		));
		UnityObject.Instantiate<GameObject>(__instance.swingSound, __instance.transform);
		__instance.diving = true;
		__instance.DamageStart();
		__instance.mach.ParryableCheck();
		__instance.Invoke("CheckThatJumpStarted", 1f);
		return false;
	}
}