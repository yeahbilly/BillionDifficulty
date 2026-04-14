using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using System.Collections.Generic;
using System;
using System.Collections;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: DRONE !!!
[HarmonyPatch(typeof(Drone))]
public class DronePatch {
	// DRONE PATCH (setup big orb attack)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Drone), nameof(Drone.Start))]
	public static void StartPostfix(Drone __instance) {
		if (__instance.difficulty != 19)
			return;

		bool isFleshDrone = __instance.GetComponent<DroneFlesh>() != null;
		EnemyIdentifier eid = __instance.eid;

		switch (eid.enemyType) {
			case EnemyType.Drone:
				if (isFleshDrone)
					break;
				CounterInt counterDrone = __instance.gameObject.AddComponent<CounterInt>();
				counterDrone.randomized = true;
				counterDrone.randomMin = 4;
				counterDrone.randomMax = 6; // big orb every 4-6 attacks
				counterDrone.value += 2; // first big orb starts sooner
				break;
			case EnemyType.Virtue:
				Plugin.SetEnemyWeakness("railcannon", 0.5f, eid); // default: 1.5f
				break;
			case EnemyType.Providence:
				Plugin.SetEnemyWeakness("cannonball", 2f, eid); // default: 10f
				Plugin.SetEnemyWeakness("railcannon", 0.75f, eid); // default: 1.5f
				break;
		}

		if (!Util.IsHardMode())
			return;
			
		switch (eid.enemyType) {
			case EnemyType.Drone:
				if (isFleshDrone)
					return;
				OriginalHealth ohDrone =__instance.gameObject.AddComponent<OriginalHealth>();
				ohDrone.health = __instance.enemy.health;
				Util.AddToArray(ref eid.weaknesses, "revolver");
				Util.AddToArray(ref eid.weaknessMultipliers, 0.4f);
				// prevents getting hit twice by multiple sub-explosions in one
				TimerFloat timer = __instance.gameObject.AddComponent<TimerFloat>();
				timer.cooldownMax = 0.2f;
				timer.cooldown = 0.2f;
				timer.Run();
				break;
			case EnemyType.Virtue:
				CounterInt counterVirtue = __instance.gameObject.AddComponent<CounterInt>();
				counterVirtue.maxValue = 2;
				break;
			case EnemyType.Providence:
				OriginalHealth ohProvidence =__instance.gameObject.AddComponent<OriginalHealth>();
				ohProvidence.health = __instance.enemy.health;
				BoolValue bv = __instance.gameObject.AddComponent<BoolValue>();
				bv.description = "enraged";
				bv.value = false;

				CounterInt counterProvidence = __instance.gameObject.AddComponent<CounterInt>();
				counterProvidence.maxValue = 3;
				counterProvidence.Add();
				Plugin.SetEnemyWeakness("cannonball", 1f, eid); // default: 10f
				break;
		}
	}

	// DRONE PATCH (explosion damage)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Drone), nameof(Drone.GetHurt))]
	public static void GetHurtPrefix(Vector3 force, ref float multiplier, GameObject sourceWeapon, bool fromExplosion, Drone __instance) {
		if (!Util.IsHardMode())
			return;

		bool isFleshDrone = __instance.GetComponent<DroneFlesh>() != null;
		if (__instance.eid.enemyType == EnemyType.Drone && !isFleshDrone) {
			if (__instance.eid.hitter != "explosion" && __instance.eid.hitter != "ffexplosion")
				return;
			
			TimerFloat timer = __instance.GetComponent<TimerFloat>();
			if (!timer.reached) {
				multiplier = 0f;
				return;
			}
			timer.ResetAndRun();

			OriginalHealth oh = __instance.GetComponent<OriginalHealth>();
			multiplier = Mathf.Min(multiplier, oh.health / 2f);
		} else if (__instance.eid.enemyType == EnemyType.Providence) {
			OriginalHealth oh = __instance.GetComponent<OriginalHealth>();
			multiplier = Mathf.Min(multiplier, oh.health / 2f);
		}
	}

	// DRONE PATCH and PROVIDENCE PATCH (decreases dodge distance)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Drone), nameof(Drone.Dodge), new Type[] {typeof(Vector3)})]
	public static bool DodgePrefix(ref Vector3 direction, Drone __instance, bool __runOriginal) {
		if (!__runOriginal)
			return false;
		if (__instance.difficulty != 19)
			return true;

		float forceMult = 20f; // default: 50f
		if (Util.IsHardMode()) {
			forceMult = 35f;
		}
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
		if (__instance.difficulty != 19)
			return true;

		float mult = (float)__instance.difficulty / 2f;
		if (Util.IsHardMode()) {
			switch (__instance.eid.enemyType) {
				case EnemyType.Drone:
				case EnemyType.Virtue:
					//mult = 2.75f;
					mult = 2.8f; // Brutal: 2f, Violent: 1.5f (dodge cooldown)
					break;
				case EnemyType.Providence:
					//mult = 1.925f;
					mult = 2f; // Brutal: 1.5f
					break;
			}
		}
		else if (__instance.difficulty == 19) {
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

	public static GameObject SpawnBigOrb(Vector3 position, Quaternion rotation, EnemyTarget target, bool startsSlow = false) {
		GameObject proj = UnityObject.Instantiate<GameObject>(Plugin.Prefabs["ProjectileExplosiveHH"], position, rotation);
		Projectile projectileComp = proj.GetComponent<Projectile>();
		proj.transform.localScale *= 0.75f;

		projectileComp.damage = 35; // default: 25 (drone projectile)
		projectileComp.speed = 55f; // Brutal: 45f
		projectileComp.bigExplosion = false;
		#pragma warning disable CS0618 // Type or member is obsolete
		projectileComp.target = target;
		#pragma warning restore CS0618 // Type or member is obsolete

		foreach (Explosion explosion in projectileComp.explosionEffect.transform.GetComponentsInChildren<Explosion>()) {
			BillionExplosionController excon = explosion.GetComponent<BillionExplosionController>();
			if (excon == null) {
				excon = explosion.gameObject.AddComponent<BillionExplosionController>();
			}
			excon.enemyDamageMultiplier = 0f;
		}

		projectileComp.homingType = HomingType.None;
		projectileComp.turningSpeedMultiplier = 0f;
		// projectileComp.turnSpeed = 9999f;

		ProjectileSpeedStutter stutter = projectileComp.gameObject.AddComponent<ProjectileSpeedStutter>();
		stutter.homing = true;
		stutter.slowRate = 2.25f;
		stutter.explodeWhenClose = true;
		stutter.changeColor = true;
		stutter.explodeDistance = 8f;
		stutter.explodeStutterDelay = 2;
		stutter.originalSpeed = projectileComp.speed;

		if (startsSlow) {
			projectileComp.speed = 1f;
			stutter.cooldown = -0.4f;
		}

		stutter.targetSpeedMultiplier = 0f;

		return proj;
	}

	// DRONE PATCH (summon big orb when dying)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Drone), nameof(Drone.Explode))]
	public unsafe static void ExplodePrefix(Drone __instance) {
		if (!Util.IsHardMode() || __instance.eid.enemyType != EnemyType.Drone)
			return;
		if (__instance.GetComponent<DroneFlesh>() != null)
			return;

		// prevents doubled orbs
		if (BoolValue.Get("shotOrbOnDeath", __instance.gameObject) == true)
			return;
		if (UnityEngine.Random.Range(0, 2) == 0)
			return;

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
				#pragma warning disable CS0618 // Type or member is obsolete
				position2 = portalObject.GetTransform(portalHandle.side).GetPositionInFront(array[0].entrancePoint, 0.05f);
				#pragma warning restore CS0618 // Type or member is obsolete
				flag = !portalObject.passThroughNonTraversals;
			}
		}

		EnemyTarget orbTarget = __instance?.eid?.target;
		if (__instance?.eid?.target == null) {
			orbTarget = new EnemyTarget(NewMovement.Instance.transform);
		}

		GameObject orb = SpawnBigOrb(position2, quaternion, orbTarget, true);
		if (__instance?.eid?.target?.position != null) {
			orb.transform.LookAt(__instance.eid.target.position);
		}
		orb.transform.position += 5f * orb.transform.forward;

		orb.GetComponent<ProjectileSpeedStutter>().explodeStutterCounter = -1;

		BoolValue bv = __instance.gameObject.AddComponent<BoolValue>();
		bv.value = true;
		bv.description = "shotOrbOnDeath";
	}

	// DRONE PATCH (attack speed)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Drone), nameof(Drone.Update))]
	public static bool UpdatePrefix(Drone __instance) {
		if (__instance.difficulty != 19)
			return true;
		if (__instance.crashing)
			return false;

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
				float hardModeMult = (!Util.IsHardMode()) ? 1f : 2.25f;
				cooldownSpeed *= hardModeMult * __instance.eid.totalSpeedModifier;
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

		if (__instance.enemy.parryable)
			__instance.sinceParryable = 0f;

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

		if (!Util.IsHardMode() || __instance.eid.enemyType != EnemyType.Providence)
			return false;
		
		OriginalHealth oh = __instance.GetComponent<OriginalHealth>();
		if (__instance.eid.health > 0.5f * oh.health || BoolValue.Get("enraged", __instance.gameObject) == true)
			return false;
		
		BoolValue.Set("enraged", true, __instance.gameObject);
		GameObject rage = UnityObject.Instantiate<GameObject>(
			DefaultReferenceManager.Instance.enrageEffect,
			__instance.transform.Find("Providence")
		);
		rage.transform.localScale = 0.25f * Vector3.one;
		rage.transform.localPosition = new Vector3(0f, 0f, -0.2f);

		foreach (var renderer in __instance.GetComponentsInChildren<SkinnedMeshRenderer>()) {
			renderer.material.color = new Color(1f, 0.4f, 0.4f, 1f);
		}

		return false;
	}

	// DRONE PATCH and PROVIDENCE PATCH (attacks)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Drone), nameof(Drone.Shoot))]
	public unsafe static bool ShootPrefix(Drone __instance) {
		if (__instance.difficulty != 19)
			return true;
		
		__instance.enemy.parryable = false;
		bool isEverythingGood = !__instance.crashing && __instance.projectile.RuntimeKeyIsValid();
		if (!isEverythingGood)
			return false;

		if (__instance.eid.enemyType == EnemyType.Drone && !__instance.eid.puppet) {
			EnemySimplifier[] componentsInChildren = __instance.modelTransform.GetComponentsInChildren<EnemySimplifier>();
			for (int i = 0; i < componentsInChildren.Length; i++) {
				componentsInChildren[i].ChangeMaterialNew(EnemySimplifier.MaterialState.normal, __instance.origMaterial);
			}
		}
		if (!__instance.gameObject.activeInHierarchy)
			return false;

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
				#pragma warning disable CS0618 // Type or member is obsolete
				position2 = portalObject.GetTransform(portalHandle.side).GetPositionInFront(array[0].entrancePoint, 0.05f);
				#pragma warning restore CS0618 // Type or member is obsolete
				flag = !portalObject.passThroughNonTraversals;
			}
		}

		CounterInt counter = __instance.GetComponent<CounterInt>();
		bool isBigDroneProjectile = __instance.difficulty == 19 && __instance.eid.enemyType == EnemyType.Drone && counter != null && counter.value == counter.maxValue;
		bool isProvidenceFart = Util.IsHardMode() && __instance.eid.enemyType == EnemyType.Providence && counter != null && counter.value == counter.maxValue;

		List<Projectile> list = new List<Projectile>();
		GameObject proj;
		if (isBigDroneProjectile) {
			EnemyTarget orbTarget = __instance?.eid?.target;
			if (__instance?.eid?.target == null) {
				orbTarget = new EnemyTarget(NewMovement.Instance.transform);
			}
			proj = SpawnBigOrb(position2, quaternion, orbTarget);
		} else if (isProvidenceFart) {
			ShootFart(__instance, 55f);
			ShootFart(__instance, -55f);
			proj = null;
		} else {
			proj = UnityObject.Instantiate<GameObject>(__instance.projectile.ToAsset(), position2, quaternion);
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
			if (Util.IsHardMode()) {
				counter.Add();
			}
			__instance.rb.AddForce(__instance.transform.forward * -1500f, ForceMode.Impulse);
			if (!isProvidenceFart) {
				__instance.SetProjectileSettings(proj.GetComponent<Projectile>());
			}
			if (__instance.difficulty == 19 && !isProvidenceFart) {
				float size = !Util.IsHardMode() ? 2.5f : 3.35f;
				float sizeReduced = !Util.IsHardMode() ? 2.2f : 3f;

				proj.transform.localScale = new Vector3(size, size, size);
				foreach (LineRenderer line in proj.GetComponentsInChildren<LineRenderer>()) {
					line.startWidth = size;
					line.endWidth = size;
				}
				foreach (ContinuousBeam beam in proj.GetComponentsInChildren<ContinuousBeam>()) {
					beam.beamWidth = sizeReduced;
					beam.canHitEnemy = false;
				}
				proj.GetComponent<Projectile>().enemyDamageMultiplier = 0f;

				if (Util.IsHardMode()) {
					bool enraged = BoolValue.Get("enraged", __instance.gameObject) == true;
					if (!enraged) {
						Spin spin = proj.AddComponent<Spin>();
						spin.notRelative = false;
						spin.spinDirection = new Vector3(0f, 0f, 1f);
						spin.speed = 135f;
					} else {
						TimedRotator rotator = proj.AddComponent<TimedRotator>();
						rotator.angle = 22.5f;
						rotator.cooldownMax = 0.25f;
						rotator.spinDirection = new Vector3(0f, 0f, 1f);
					}
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
		if (__instance.difficulty != 19)
			return;

		if (__instance.eid.enemyType != EnemyType.Drone)
			return;

		ProjectileSpeedStutter stutter = proj.GetComponent<ProjectileSpeedStutter>();
		if (stutter == null) {
			stutter = proj.gameObject.AddComponent<ProjectileSpeedStutter>();
		}
		// big orb (speed 55f) doesn't get affected thanks to the Prefix
		if (!__state) {
			proj.speed = 90f; // Brutal: 45f
			if (Util.IsHardMode()) {
				proj.speed = 110f;
			}
			stutter.originalSpeed = proj.speed;
			stutter.slowRate = 3.75f;
			if (Util.IsHardMode()) {
				stutter.slowRate = 4.25f;
			}
		}
	}

	public static GameObject ShootFart(Drone __instance, float rotationOffset = 0f) {
		if (__instance.eid == null || __instance.eid.target == null)
			return null;
		Vector3 forward = (__instance.eid.target.position - __instance.transform.position).normalized;
		Quaternion rot = Quaternion.LookRotation(forward);
		Vector3 rotAngles = rot.eulerAngles;
		rotAngles.y += rotationOffset;
		rot = Quaternion.Euler(rotAngles);

		GameObject fart = UnityObject.Instantiate<GameObject>(
			Plugin.Prefabs["ProjectileHomingAcid"],
			__instance.transform.position + rot * Vector3.forward * 5f,
			rot
		);
		Projectile proj = fart.GetComponent<Projectile>();
		proj.safeEnemyType = EnemyType.Providence;
		proj.speed = 25f;
		#pragma warning disable CS0618 // Type or member is obsolete
		proj.target = __instance.eid.target;
		#pragma warning restore CS0618 // Type or member is obsolete

		HurtZone hurtZone = fart.GetComponentInChildren<HurtZone>(includeInactive: true);
		hurtZone.affected = AffectedSubjects.PlayerOnly; // FUCK YOU

		// fart.transform.eulerAngles = new Vector3(0f, fart.transform.eulerAngles.y + rotationOffset, 0f);
		// fart.transform.position += 3f * fart.transform.forward;

		// fart.transform.Find("GoopCloud").gameObject.AddComponent<RemoveOnRespawn>();

		return fart;
	}

	public static GameObject ShootGeryonBeam(Drone __instance, float rotationOffset = 0f) {
		GameObject beam = UnityObject.Instantiate<GameObject>(Plugin.Prefabs["GeryonForwardArrowBeam"], __instance.transform.position, Quaternion.identity);
		beam.transform.LookAt(__instance.eid.target.position);
		beam.transform.eulerAngles = new Vector3(0f, beam.transform.eulerAngles.y + rotationOffset, 0f);
		beam.transform.position += 5f * beam.transform.forward;
		beam.transform.localScale *= 0.75f;

		HurtZone hurtZone = beam.GetComponentInChildren<HurtZone>();
		hurtZone.affected = AffectedSubjects.PlayerOnly;
		hurtZone.hardDamagePercentage = 0.5f;

		ConstantForce constantForce = beam.GetComponent<ConstantForce>();
		constantForce.relativeForce *= 1.25f;

		RemoveOnTime remove = beam.AddComponent<RemoveOnTime>();
		remove.time = 7f;

		ScaleTransform[] scaleTransforms = beam.GetComponentsInChildren<ScaleTransform>();
		foreach (ScaleTransform scaleTransform in scaleTransforms) {
			scaleTransform.speed = 2.5f;
		}

		beam.AddComponent<RemoveOnRespawn>();

		return beam;
	}

	public static IEnumerator ShootGeryonBeamCoroutine(Drone __instance) {
		yield return new WaitForSeconds(0.7f);
		if (__instance.eid.dead) yield break;
		ShootGeryonBeam(__instance);
		// yield return new WaitForSeconds(0.1f);
		yield break;
	}

	public static IEnumerator ShootGeryonBeamsEnragedCoroutine(Drone __instance, float rotationOffset = 0f) {
		yield return new WaitForSeconds(0.7f);
		if (__instance.eid.dead) yield break;
		ShootGeryonBeam(__instance);
		yield return new WaitForSeconds(0.75f);
		if (__instance.eid.dead) yield break;
		ShootGeryonBeam(__instance, rotationOffset);
		ShootGeryonBeam(__instance, -rotationOffset);
		// yield return new WaitForSeconds(0.1f);
		yield break;
	}

	// PROVIDENCE PATCH (pincer)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Drone), nameof(Drone.ShootSecondary))]
	public static bool ShootSecondaryPrefix(Drone __instance) {
		if (__instance.difficulty != 19)
			return true;

		if (__instance.crashing || !__instance.secondaryProjectile.RuntimeKeyIsValid())
			return false;
		__instance.anim.Play("BeamPrep");
		GameObject gameObject = UnityObject.Instantiate<GameObject>(__instance.secondaryProjectile.ToAsset(), __instance.transform.position + __instance.transform.forward, __instance.transform.rotation);
		gameObject.transform.SetParent(__instance.transform, true);
		Pincer pincer;
		if (gameObject.TryGetComponent<Pincer>(out pincer)) {
			pincer.difficulty = __instance.difficulty;
			__instance.droneCurrentRotation = 0f;
			__instance.droneWingRotationSpeed = (float)((UnityEngine.Random.Range(0f, 1f) > 0.5f) ? 1 : -1);
			pincer.direction *= __instance.droneWingRotationSpeed;
			pincer.firedMessageReceiver = __instance.gameObject;
		}
		
		foreach (ContinuousBeam beam in gameObject.GetComponentsInChildren<ContinuousBeam>()) {
			beam.canHitEnemy = false;
		}

		if (!Util.IsHardMode())
			return false;
		
		bool enraged = BoolValue.Get("enraged", __instance.gameObject) == true;
		if (!enraged)
			return false;
		
		GameObject pincerObject = __instance.GetComponentInChildren<Pincer>().gameObject;
		Pincer pincerNew = UnityObject.Instantiate<GameObject>(pincerObject, __instance.transform)
			.GetComponent<Pincer>();
		pincerNew.rotationSpeed /= 2f;
		return false;
	}

	// VIRTUE PATCH (attack)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Drone), nameof(Drone.SpawnDroneInsignia))]
	public static bool SpawnDroneInsigniaPrefix(Drone __instance) {
		if (__instance.difficulty != 19)
			return true;

		if (__instance.eid.target == null)
			return false;
		if (__instance.crashing)
			return false;

		if (Util.IsHardMode()) {
			CounterInt counter = __instance.GetComponent<CounterInt>();
			if (counter.value == 2) {
				counter.Add();
				if (!__instance.isEnraged) {
					__instance.StartCoroutine(ShootGeryonBeamCoroutine(__instance));
				} else {
					__instance.StartCoroutine(ShootGeryonBeamsEnragedCoroutine(__instance, 25f));
				}
				return false;
			}
			counter.Add();
		}

		__instance.enemy.parryable = false;
		GameObject insignia = UnityObject.Instantiate<GameObject>(__instance.projectile.ToAsset(), __instance.eid.target.position, Quaternion.identity);
		insignia.SetActive(false);
		VirtueInsignia insigniaComp = insignia.GetComponent<VirtueInsignia>();
		insigniaComp.target = __instance.eid.target;
		insigniaComp.parentEnemy = __instance.enemy;
		insigniaComp.hadParent = true;

		if (Util.IsHardMode()) {
			insignia.transform.Find("Capsule").GetComponent<CapsuleCollider>().radius = 0.425f; // default: 0.5f?
			Follow follow = insignia.AddComponent<Follow>();
			follow.speed = 4f;
			if (__instance.isEnraged) {
				follow.speed = 2f;
			}
			follow.followY = false;
			follow.mimicRotation = false;
			follow.target = __instance.eid.target.targetTransform;
		}

		if (__instance.isEnraged) {
			insigniaComp.explosionLength = 1.5f; // Brutal: 3.5f
			insigniaComp.predictive = false; // = Util.IsHardMode();
			insignia.transform.localScale = new Vector3(3.85f, 1, 3.85f); // 3.85f; default: 2, 1, 2
			foreach (SpriteRenderer spriteRenderer in insignia.GetComponentsInChildren<SpriteRenderer>(includeInactive: true)) {
				if (spriteRenderer.name != "Shockwave") {
					spriteRenderer.sprite = insigniaComp.predictiveVersion;
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
		if (((Util.IsHardMode() && __instance.usedAttacks > 1) || (__instance.difficulty > 2 && __instance.usedAttacks > 2) || (__instance.difficulty == 2 && __instance.usedAttacks > 4 && !__instance.eid.blessed)) && !__instance.isEnraged && !__instance.eid.puppet && __instance.vc.currentVirtues.Count < 3) {
			__instance.Invoke("Enrage", 3f / __instance.eid.totalSpeedModifier);
		}
		return false;
	}
}