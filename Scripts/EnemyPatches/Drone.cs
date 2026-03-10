using Billion = BillionDifficulty.Plugin;
using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using System.Collections.Generic;
using System;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: DRONE !!!
[HarmonyPatch(typeof(Drone))]
public class DronePatch {
	// DRONE PATCH (setup big orb attack)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Drone), nameof(Drone.Start))]
	public static void StartPostfix(Drone __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		CounterInt counter = __instance.gameObject.AddComponent<CounterInt>();
		counter.randomized = true;
		counter.randomMin = 4;
		counter.randomMax = 6; // big orb every 4-6 attacks
		counter.value += 2; // first big orb starts sooner
	}

	// DRONE PATCH (decreases dodge distance)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Drone), nameof(Drone.Dodge), new Type[] {typeof(Vector3)})]
	public static bool DodgePrefix(ref Vector3 direction, Drone __instance) {
		if (__instance.difficulty != 19) {
			return true;
		}

		float forceMult = 20f; // default: 50f
		if (__instance.eid.enemyType == EnemyType.Providence) {
			forceMult = 600f; // default: 750f
		} else if (__instance.eid.enemyType == EnemyType.Virtue) {
			forceMult = 150f;
		}
		forceMult *= __instance.eid.totalSpeedModifier;

		if (PortalPhysicsV2.Raycast(__instance.transform.position, direction.normalized, 7f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.UseGlobal))
		{
			direction *= -1f;
		}
		if (__instance.dodgeSound && !__instance.hasDimensionalTarget) {
			__instance.dodgeSound.PlayClipAtPoint(
				MonoSingleton<AudioMixerController>.Instance.allGroup,
				__instance.transform.position,
				128,
				0f,
				0.15f,
				UnityEngine.Random.Range(0.75f, 1.25f),
				AudioRolloffMode.Linear,
				1f,
				100f
			);
		}
		
		__instance.rb.AddForce(direction.normalized * forceMult, ForceMode.Impulse);
		return false;
	}

	// DRONE PATCH AND VIRTUE PATCH AND PROVIDENCE PATCH (dodge cooldown)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Drone), nameof(Drone.GetCooldownSpeed))]
	public static bool GetCooldownSpeedPrefix(ref float __result, Drone __instance) {
		float mult = (float)__instance.difficulty / 2f;
		if (__instance.difficulty == 19) {
			switch (__instance.eid.enemyType) {
				case EnemyType.Drone:
				case EnemyType.Virtue:
					mult = 2.25f; // Brutal: 2f, Violent: 1.5f (dodge cooldown)
					break;
				case EnemyType.Providence:
					mult = 1.65f; // Brutal: 1.5f
					break;
			}
		} else if (__instance.eid.enemyType == EnemyType.Providence && __instance.difficulty >= 3) {
			mult -= (mult - 1f) / 2f;
		} else if (__instance.eid.enemyType == EnemyType.Virtue && __instance.difficulty >= 4) {
			mult = 1.2f;
		} else if (__instance.difficulty == 1) {
			mult = 0.75f;
		} else if (__instance.difficulty == 0) {
			mult = 0.5f;
		}

		if (mult == 0f) {
			mult = 0.25f;
		}

		__result = mult * __instance.eid.totalSpeedModifier;
		return false;
	}

	// DRONE PATCH (attack speed)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Drone), nameof(Drone.Update))]
	public static bool UpdatePrefix(Drone __instance) {
		if (__instance.crashing) {
			return false;
		}

		__instance.UpdateRigidbodySettings();
		bool flag = __instance.hasAnyTarget || (__instance.eid.enemyType == EnemyType.Virtue && __instance.eid.target != null);
		if ((__instance.targetSpotted || __instance.eid.enemyType == EnemyType.Virtue) && flag) {
			if (__instance.hasDroneVision) {
				TargetData targetData = __instance.vision.CalculateData(__instance.droneTargetHandle);
				__instance.viewTarget = targetData.headPosition;
			} else if (__instance.hasDimensionalTarget) {
				__instance.viewTarget = __instance.lastDimensionalTarget;
			} else if (__instance.eid.enemyType == EnemyType.Virtue && __instance.eid.target != null) {
				__instance.viewTarget = __instance.eid.target.position;
			}

			float num = __instance.GetCooldownSpeed();
			if (__instance.eid.enemyType == EnemyType.Providence && __instance.difficulty <= 3) {
				num /= 2f;
			}
			if (__instance.dodgeCooldown > 0f) {
				__instance.dodgeCooldown = Mathf.MoveTowards(__instance.dodgeCooldown, 0f, Time.deltaTime * num);
			} else if (!__instance.stationary && !__instance.lockPosition) {
				__instance.dodgeCooldown = UnityEngine.Random.Range(1f, 3f);
				__instance.RandomDodge(false);
			}
		}

		if (__instance.ShouldProcessAttack()) {
			float cooldownSpeed = __instance.GetCooldownSpeed();
			if (__instance.difficulty == 19 && __instance.eid.enemyType == EnemyType.Virtue) {
				cooldownSpeed = 1.2f; // Brutal: 1.2f
				cooldownSpeed *= __instance.eid.totalSpeedModifier;
			}
			if (__instance.attackCooldown > 0f) {
				__instance.attackCooldown = Mathf.MoveTowards(__instance.attackCooldown, 0f, Time.deltaTime * cooldownSpeed);
			} else if (__instance.projectile != null && (!__instance.vc || __instance.vc.virtueCooldown == 0f)) {
				__instance.ProcessAttack();
			}
		}
		if (__instance.eid && __instance.eid.hooked && !__instance.hooked) {
			__instance.Hooked();
		} else if (__instance.eid && !__instance.eid.hooked && __instance.hooked) {
			__instance.Unhooked();
		}

		if (__instance.enemy.parryable) {
			__instance.sinceParryable = 0f;
		}

		if (__instance.eid.enemyType == EnemyType.Providence && __instance.hooked && !__instance.CanBeHooked()) {
			MonoSingleton<HookArm>.Instance.StopThrow(1f, true);
			__instance.Unhooked();
			__instance.RandomDodge(true);
		}

		if (__instance.droneWingRotationSpeed != 0f) {
			for (int i = 0; i < __instance.rotatorWings.Length; i++) {
				__instance.rotatorWings[i].Rotate(
					Vector3.right,
					360f * Time.deltaTime * __instance.droneWingRotationSpeed * (float)((i % 2 == 0) ? -1 : 1)
				);
			}
		}

		return false;
	}

	// DRONE PATCH AND PROVIDENCE PATCH (big middle projectile)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Drone), nameof(Drone.Shoot))]
	public static bool ShootPrefix(Drone __instance) {
		__instance.enemy.parryable = false;

		bool isEverythingGood = !__instance.crashing && __instance.projectile.RuntimeKeyIsValid();
		if (!isEverythingGood) {
			return false;
		}

		if (__instance.eid.enemyType == EnemyType.Drone && !__instance.eid.puppet) {
			EnemySimplifier[] componentsInChildren = __instance.modelTransform.GetComponentsInChildren<EnemySimplifier>();
			for (int i = 0; i < componentsInChildren.Length; i++) {
				componentsInChildren[i].ChangeMaterialNew(EnemySimplifier.MaterialState.normal, __instance.origMaterial);
			}
		}
		if (!__instance.gameObject.activeInHierarchy) {
			return false;
		}

		Vector3 position = __instance.transform.position;
		Vector3 forward = __instance.transform.forward;
		Vector3 position2 = position + forward;
		Quaternion quaternion = __instance.transform.rotation;
		PhysicsCastResult physicsCastResult;
		Vector3 vector;
		PortalTraversalV2[] array;
		PortalPhysicsV2.ProjectThroughPortals(position, forward, default(LayerMask), out physicsCastResult, out vector, out array);
		bool flag = false;
		if (array.Length != 0) {
			PortalTraversalV2 portalTraversalV = array[0];
			PortalHandle portalHandle = portalTraversalV.portalHandle;
			Portal portalObject = portalTraversalV.portalObject;
			if (portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile)) {
				Matrix4x4 travelMatrix = PortalUtils.GetTravelMatrix(array);
				position2 = vector;
				quaternion = travelMatrix.rotation * quaternion;
			} else {
				position2 = portalObject.GetTransform(portalHandle.side).GetPositionInFront(array[0].entrancePoint, 0.05f);
				flag = !portalObject.passThroughNonTraversals;
			}
		}

		List<Projectile> list = new List<Projectile>();

		CounterInt counter = __instance.GetComponent<CounterInt>();
		bool isBigDroneProjectile = __instance.difficulty == 19 && __instance.eid.enemyType == EnemyType.Drone && counter != null && counter.value == counter.maxValue;

		GameObject proj;
		if (!isBigDroneProjectile) {
			proj = UnityObject.Instantiate<GameObject>(__instance.projectile.ToAsset(), position2, quaternion);
		} else {
			proj = UnityObject.Instantiate<GameObject>(Billion.ProjectileExplosiveHH, position2, quaternion);
			Projectile projectileComp = proj.GetComponent<Projectile>();
			proj.transform.localScale *= 0.75f;

			projectileComp.damage = 35; // default: 25 (drone projectile)
			projectileComp.speed = 55f; // Brutal: 45f
			projectileComp.bigExplosion = false;

			foreach (Explosion explosion in projectileComp.explosionEffect.transform.GetComponentsInChildren<Explosion>()) {
				BillionExplosionController excon = explosion.gameObject.GetComponent<BillionExplosionController>();
				if (excon == null) {
					excon = explosion.gameObject.AddComponent<BillionExplosionController>();
				}
				excon.enemyDamageMultiplier = 0f;
			}

			projectileComp.homingType = HomingType.None;
			projectileComp.turningSpeedMultiplier = 0f;
			projectileComp.turnSpeed = 9999f;

			ProjectileSpeedStutter stutter = projectileComp.gameObject.AddComponent<ProjectileSpeedStutter>();
			stutter.homing = true;
			stutter.slowRate = 2.25f;
			stutter.explodeWhenClose = true;
			stutter.changeColor = true;
			stutter.explodeDistance = 8f;
			stutter.explodeStutterDelay = 2;
		}

		if (__instance.eid.enemyType == EnemyType.Drone) {
			if (__instance.difficulty == 19) {
				counter.Add();
			}

			Transform transform = proj.transform;
			Vector3 position3 = transform.position;
			Vector3 up = transform.up;
			Quaternion rotation = transform.rotation;
			Vector3 eulerAngles = rotation.eulerAngles;

			if (!isBigDroneProjectile) {
				proj.transform.localScale *= 0.5f;
			}

			proj.transform.rotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, (float)UnityEngine.Random.Range(0, 360));
			list.Add(proj.GetComponent<Projectile>());
			List<Projectile> list2 = list;
			__instance.SetProjectileSettings(list2[list2.Count - 1]);
			
			GameObject gameObject2 = UnityObject.Instantiate<GameObject>(__instance.projectile.ToAsset(), position3 + up, rotation);
			if (__instance.difficulty > 2) {
				gameObject2.transform.rotation = Quaternion.Euler(eulerAngles.x + 10f, eulerAngles.y, eulerAngles.z);
			}
			gameObject2.transform.localScale *= 0.5f;
			list.Add(gameObject2.GetComponent<Projectile>());
			List<Projectile> list3 = list;
			__instance.SetProjectileSettings(list3[list3.Count - 1]);

			gameObject2 = UnityObject.Instantiate<GameObject>(__instance.projectile.ToAsset(), position3 - up, rotation);
			if (__instance.difficulty > 2) {
				gameObject2.transform.rotation = Quaternion.Euler(eulerAngles.x - 10f, eulerAngles.y, eulerAngles.z);
			}
			gameObject2.transform.localScale *= 0.5f;
			list.Add(gameObject2.GetComponent<Projectile>());
			List<Projectile> list4 = list;
			__instance.SetProjectileSettings(list4[list4.Count - 1]);
		} else if (__instance.eid.enemyType == EnemyType.Providence) {
			__instance.SetProjectileSettings(proj.GetComponent<Projectile>());
			__instance.rb.AddForce(__instance.transform.forward * -1500f, ForceMode.Impulse);

			if (__instance.difficulty == 19) {
				proj.transform.localScale = new Vector3(2f, 2f, 2f);
				foreach (LineRenderer line in proj.GetComponentsInChildren<LineRenderer>()) {
					line.startWidth = 2f;
					line.endWidth = 2f;
				}
				foreach (ContinuousBeam beam in proj.GetComponentsInChildren<ContinuousBeam>()) {
					beam.beamWidth = 1.85f;
				}
			}
		} else {
			__instance.SetProjectileSettings(proj.GetComponent<Projectile>());
		}

		if (flag) {
			for (int j = 1; j < list.Count; j++) {
				list[j].Explode();
			}
		}
		return false;
	}

	// DRONE PATCH (projectile speed)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Drone), nameof(Drone.SetProjectileSettings))]
	public static void SetProjectileSettingsPrefix(Projectile proj, Drone __instance, out bool __state) {
		if (__instance.eid.enemyType != EnemyType.Drone) {
			__state = false;
			return;
		}
		if (__instance.difficulty == 19 && proj.speed == 55f) {
			__state = true;
		} else {
			__state = false;
		}
	}
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Drone), nameof(Drone.SetProjectileSettings))]
	public static void SetProjectileSettingsPostfix(Projectile proj, Drone __instance, bool __state) {
		if (__instance.difficulty != 19) {
			return;
		}

		if (__instance.eid.enemyType != EnemyType.Drone) {
			return;
		}

		ProjectileSpeedStutter stutter = proj.gameObject.GetComponent<ProjectileSpeedStutter>();
		if (stutter == null) {
			stutter = proj.gameObject.AddComponent<ProjectileSpeedStutter>();
		}
		// big orb (speed 55f) doesn't get affected thanks to the Prefix
		if (!__state) {
			proj.speed = 90f; // Brutal: 45f
			stutter.slowRate = 3.75f;
		}
		stutter.targetSpeedMultiplier = 0f;
	}

	// VIRTUE PATCH (attack)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Drone), nameof(Drone.SpawnDroneInsignia))]
	public static bool SpawnDroneInsigniaPrefix(Drone __instance) {
		if (__instance.eid.target == null) {
			return false;
		}
		if (__instance.crashing) {
			return false;
		}

		__instance.enemy.parryable = false;
		GameObject insignia = UnityObject.Instantiate<GameObject>(__instance.projectile.ToAsset(), __instance.eid.target.position, Quaternion.identity);
		insignia.SetActive(false);
		VirtueInsignia insigniaComp = insignia.GetComponent<VirtueInsignia>();
		insigniaComp.target = __instance.eid.target;
		insigniaComp.parentEnemy = __instance.enemy;
		insigniaComp.hadParent = true;

		if (__instance.isEnraged) {
			if (__instance.difficulty != 19) {
				insigniaComp.predictive = true;
			} else {
				insigniaComp.explosionLength = 1.5f; // Brutal: 3.5f
				insigniaComp.predictive = false;
				insignia.transform.localScale = new Vector3(3.85f, 1, 3.85f); // 3.85f; default: 2, 1, 2
				foreach (SpriteRenderer spriteRenderer in insignia.GetComponentsInChildren<SpriteRenderer>(includeInactive: true)) {
					if (spriteRenderer.gameObject.name != "Shockwave") {
						spriteRenderer.sprite = insigniaComp.predictiveVersion;
					}
				}
			}
		}
		if (__instance.difficulty == 1) {
			insigniaComp.windUpSpeedMultiplier = 0.875f;
		} else if (__instance.difficulty == 0) {
			insigniaComp.windUpSpeedMultiplier = 0.75f;
		}

		if (__instance.difficulty == 19) {
			if (!__instance.isEnraged) {
				insigniaComp.explosionLength = 5f; // Brutal: 3.5f
			}
		} else if (__instance.difficulty >= 4) {
			insigniaComp.explosionLength = (__instance.difficulty == 5) ? 5f : 3.5f;
		}

		if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer) {
			insignia.transform.localScale *= 0.75f;
			insigniaComp.windUpSpeedMultiplier *= 0.875f;
		}
		insigniaComp.windUpSpeedMultiplier *= __instance.eid.totalSpeedModifier;
		insigniaComp.damage = Mathf.RoundToInt((float)insigniaComp.damage * __instance.eid.totalDamageModifier);

		insignia.SetActive(true);
		__instance.chargeParticle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
		__instance.usedAttacks++;
		if (((__instance.difficulty > 2 && __instance.usedAttacks > 2) || (__instance.difficulty == 2 && __instance.usedAttacks > 4 && !__instance.eid.blessed)) && !__instance.isEnraged && !__instance.eid.puppet && __instance.vc.currentVirtues.Count < 3) {
			__instance.Invoke("Enrage", 3f / __instance.eid.totalSpeedModifier);
		}
		return false;
	}
}