using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: ZOMBIE !!!
// Zombie
[HarmonyPatch(typeof(Zombie))]
public class ZombiePatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Zombie), nameof(Zombie.SetSpeed))]
	public static void SetSpeedPostfix(Zombie __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		if (__instance.eid.dead) {
			return;
		}
		
		switch (__instance.eid.enemyType) {
			case EnemyType.Stray:
				__instance.speedMultiplier = 1.75f; // Brutal: 1.5f
				__instance.anim.speed = 1.75f; // Brutal: 1.5f
				__instance.nma.speed = 12.5f * __instance.speedMultiplier; // default: 10f * ...
				break;
			case EnemyType.Soldier:
				__instance.speedMultiplier = 1.75f; // Brutal: 1.5f
				__instance.anim.speed = 1.25f; // Brutal: 1f
				__instance.nma.angularSpeed = 480f; // Brutal: 480f
				__instance.nma.acceleration = 480f; // Brutal: 480f
				float runSpeed = 1.5f * __instance.speedMultiplier; // Brutal: 1f * ...
				__instance.anim.SetFloat("RunSpeed", runSpeed);
				__instance.nma.speed = 17.5f * __instance.speedMultiplier; // Brutal: 17.5f * ...
				break;
			case EnemyType.Schism:
				__instance.speedMultiplier = 1.75f; // Brutal: 1.5f
				__instance.anim.speed = 1.75f; // Brutal: 1.5f
				__instance.nma.speed = 12.5f * __instance.speedMultiplier; // default: 10f * ...
				break;
			case EnemyType.Filth:
				__instance.speedMultiplier = 1.75f; // Brutal: 1.5f
				__instance.anim.speed = 2.25f; // Brutal: 1.5f
				__instance.nma.acceleration = 240f; // Brutal: 120f
				__instance.nma.angularSpeed = 9000f; // Brutal: 9000f
				__instance.nma.speed = 20f * __instance.speedMultiplier; // default: 10f * ...
				break;
		}
	}
}
// ZombieProjectiles
[HarmonyPatch(typeof(ZombieProjectiles))]
public class ZombieProjectilesPatch {
	// STRAY/SCHISM/SOLDIER PATCH (speed)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(ZombieProjectiles), nameof(ZombieProjectiles.GetSpeed))]
	public static bool GetSpeedPrefix(int difficulty, ZombieProjectiles __instance, ref EnemyMovementData __result) {
		if (difficulty != 19) {
			return true;
		}
		
		switch (__instance.eid.enemyType) {
			case EnemyType.Soldier:
				__result = new EnemyMovementData {
					speed = 32.5f, // Brutal: 30f
					angularSpeed = 480f, // Brutal: 480f
					acceleration = 440f // Brutal: 400f
				};
				break;
			case EnemyType.Stray:
			case EnemyType.Schism:
				__result = new EnemyMovementData {
					speed = 12.5f * 1.75f, // Brutal: 15f
					angularSpeed = 800f, // Brutal: 800f
					acceleration = 50f // Brutal: 30f
				};
				break;
		}
		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ZombieProjectiles), nameof(ZombieProjectiles.Start))]
	public static void StartPostfix(ZombieProjectiles __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		if (__instance.eid.enemyType == EnemyType.Stray) {
			StrayAttack strayAttack = __instance.gameObject.AddComponent<StrayAttack>();
			strayAttack.projectilesLeft = 0;
		}
		// BB
		/*
		else if (__instance.eid.enemyType == EnemyType.Soldier) {
			__instance.projectile = Billion.Projectile;
		}*/
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ZombieProjectiles), nameof(ZombieProjectiles.ThrowProjectile))]
	public static void ThrowPostfix(ZombieProjectiles __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		// STRAY PATCH (attack)
		if (__instance.eid.enemyType == EnemyType.Stray) {
			__instance.coolDownReduce = __instance.coolDown; // Brutal: 1f

			Projectile projectileComp = __instance.currentProjectile.GetComponent<Projectile>();
			projectileComp.speed = 90f; // Brutal: 65f
			/*projectileComp.homingType = HomingType.Gradual; // Brutal: none
			projectileComp.turningSpeedMultiplier = 0.4f; // Brutal: none
			projectileComp.turnSpeed = 50f; // Brutal: none*/

			StrayAttack strayAttack = __instance.gameObject.GetComponent<StrayAttack>();
			strayAttack.projectilesLeft -= 1;
			if (strayAttack.projectilesLeft == 0) {
				__instance.coolDownReduce = 1.6f;
			}
		}
		// SOLDIER PATCH (attack)
		else if (__instance.eid.enemyType == EnemyType.Soldier) {
			__instance.coolDownReduce = 2f; // Brutal: 1f

			__instance.currentProjectile.transform.localScale = new Vector3(3, 3, 3); // default: 1, 1, 1
			ProjectileSpread projectileSpreadComp = __instance.currentProjectile.GetComponent<ProjectileSpread>();
			projectileSpreadComp.projectileAmount = 8; // Brutal: 5
			projectileSpreadComp.spreadAmount = 12f; // Brutal: 10f

			Projectile projectileComp = __instance.currentProjectile.GetComponentInChildren<Projectile>();
			projectileComp.speed = 82.5f; // Brutal: 65f

			// BB
			/*
			AlternatingProjectileSplits splits = __instance.currentProjectile.AddComponent<AlternatingProjectileSplits>();
			splits.maxLayer = 3;
			__instance.currentProjectile.GetComponent<Projectile>().speed = 75f;
			splits.projectileSpeed = 75f;
			splits.cooldownMax = 0.15f;*/
		}
	}

	// STRAY PATCH (resets stray attack counter after 3 attacks)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(ZombieProjectiles), nameof(ZombieProjectiles.Swing))]
	public static void SwingPrefix(ZombieProjectiles __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		if (__instance.eid.enemyType == EnemyType.Stray) {
			StrayAttack strayAttack = __instance.gameObject.GetComponent<StrayAttack>();
			if (strayAttack.projectilesLeft == 0) {
				strayAttack.projectilesLeft = 3;
			}
		}
	}
	
	// STRAY PATCH (resets stray speed when interrupted)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(ZombieProjectiles), nameof(ZombieProjectiles.Update))]
	public static void UpdatePostfix(ZombieProjectiles __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		if (__instance.eid.enemyType == EnemyType.Stray) {
			StrayAttack strayAttack = __instance.gameObject.GetComponent<StrayAttack>();
			if (__instance.anim.GetBool("Running")) {
				strayAttack.projectilesLeft = 0;
			}
		}
	}
	
	// SCHISM PATCH (attack)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(ZombieProjectiles), nameof(ZombieProjectiles.ShootProjectile))]
	public static void ShootPostfix(ZombieProjectiles __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

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
		
		ChangeScaleOverTime changeScale = __instance.currentProjectile.AddComponent<ChangeScaleOverTime>();
		changeScale.delay = 4f;
		changeScale.time = 1f;
		changeScale.targetScaleMultiplier = 0f;

		LineRenderer line = __instance.currentProjectile.GetComponentInChildren<LineRenderer>();
		if (line != null) {
			line.startWidth = 1.5f; // default: 1f
			line.endWidth = 1.5f; // default: 1f
			changeScale.scaleBeam = true;
		}
	}

}

// ZombieMelee
[HarmonyPatch(typeof(ZombieMelee))]
public class ZombieMeleePatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(ZombieMelee), nameof(ZombieMelee.Start))]
	public static void StartPostfix(ZombieMelee __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		__instance.defaultCoolDown = 0.2f; // Brutal: 0.25f
		TimerFloat timer = __instance.gameObject.AddComponent<TimerFloat>();
		timer.target = 0.2f; // double jump cooldown
	}

	// FILTH PATCH (speed)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(ZombieMelee), nameof(ZombieMelee.GetSpeed))]
	public static bool GetSpeedPrefix(int difficulty, ref EnemyMovementData __result) {
		if (difficulty != 19) {
			return true;
		}
		__result = new EnemyMovementData {
			speed = 15f, // default: 20f
			acceleration = 240f,
			angularSpeed = 9000f
		};
		return false;
	}

	// FILTH PATCH (double jump and fast attack)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(ZombieMelee), nameof(ZombieMelee.Update))]
	public static bool UpdatePrefix(ZombieMelee __instance) {
		if (__instance.difficulty != 19) {
			return true;
		}

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

		if (__instance.target == null) {
			return false;
		}
		__instance.UpdateTargetVision();

		if (__instance.track && __instance.hasVision) {
			if (__instance.difficulty > 1) {
				__instance.transform.LookAt(__instance.ToPlanePos(__instance.targetData.position));
			} else {
				float num = (__instance.difficulty == 0) ? 360f : 720f;
				__instance.transform.rotation = Quaternion.RotateTowards(__instance.transform.rotation, Quaternion.LookRotation(__instance.ToPlanePos(__instance.targetData.position) - __instance.transform.position), Time.deltaTime * num * __instance.eid.totalSpeedModifier);
			}
		}
		if (__instance.hasDimensionalTarget) {
			return false;
		}

		TimerFloat timer = null;
		if (__instance.difficulty == 19) {
			timer = __instance.gameObject.GetComponent<TimerFloat>();
		}

		if (__instance.coolDown <= 0f) {
			if (timer != null) {
				timer.ResetAndStop();
			}

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
		}
		
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ZombieMelee), nameof(ZombieMelee.DiveCheck))]
	public static bool DiveCheckPrefix(ZombieMelee __instance) {
		if (__instance.difficulty != 19) {
			return true;
		}

		TimerFloat timer = null;
		if (__instance.difficulty == 19) {
			timer = __instance.gameObject.GetComponent<TimerFloat>();
		}

		float jumpDistance;
		if (__instance.difficulty == 19) {
			jumpDistance = 17.5f;
		} else {
			jumpDistance = 20f;
		}
		
		if (!__instance.hasVision || __instance.targetData.DistanceTo(__instance.transform.position, false) > jumpDistance) {
			return false;
		}
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
				if (timer != null) {
					timer.ResetAndRun();
				}
			}
		}
		return false;
	}

	// FILTH PATCH (JUMP FORCE)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(ZombieMelee), nameof(ZombieMelee.JumpStart))]
	public static bool JumpPrefix(ZombieMelee __instance) {
		if (__instance.difficulty != 19) {
			return true;
		}

		__instance.transform.LookAt(__instance.ToPlanePos(__instance.diveTargetPos));
		// default: 2f instead of 1.5f, 25f instead of 8f
		__instance.mach.Jump(Vector3.up * 25f + Vector3.ClampMagnitude(
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