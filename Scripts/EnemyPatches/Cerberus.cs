using System;
using HarmonyLib;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

// CERBERUS PATCH (enraged knockback)
[HarmonyPatch(typeof(SwingCheck2), nameof(SwingCheck2.Update))]
public class CerberusDashPatch {
	public static void Postfix(SwingCheck2 __instance) {
		if (!Util.IsDifficulty(19))
			return;

		if (__instance.eid.enemyType == EnemyType.Cerberus && __instance.eid.GetComponent<StatueBoss>().isEnraged)
			__instance.knockBackForce = 23f; // 22.5f; default: 100f
	}
}


// !!! PATCHGROUP: STATUE !!!
[HarmonyPatch(typeof(StatueBoss))]
public class StatueBossPatch {
	// CERBERUS PATCH (damage from cannonballs)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(StatueBoss), nameof(StatueBoss.Start))]
	public static void StartPostfix(StatueBoss __instance) {
		if (__instance.difficulty != 19)
			return;
		Plugin.SetEnemyWeakness("cannonball", 0.8f, __instance.eid); // default: 2f
	}

	// CERBERUS PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(StatueBoss), nameof(StatueBoss.SetSpeed))]
	public static void SetSpeedPostfix(StatueBoss __instance) {
		if (__instance.difficulty != 19)
			return;

		if (__instance.eid.totalSpeedModifier > 1.2f)
			__instance.realSpeedModifier -= 0.2f;
		if (Util.IsHardMode())
			__instance.realSpeedModifier *= 1.3f;

		__instance.anim.speed = 1.45f * __instance.realSpeedModifier; // Brutal: 1.35f * ...
		if (__instance.isEnraged) 
			__instance.anim.speed = 1.65f * __instance.realSpeedModifier; // Brutal: 1.5f * ...
		if (__instance.nma)
			__instance.nma.speed *= 1.2f;
	}

	// CERBERUS PATCH (stomp more often)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(StatueBoss), nameof(StatueBoss.AttackCheck))]
	public static bool AttackCheckPrefix(StatueBoss __instance) {
		if (!Util.IsHardMode())
			return true;

		if (__instance.attackCheckCooldown > 0f)
			return true;
		if (__instance.inAction || !__instance.gc.onGround)
			return false;
		if (!__instance.CheckAndSetAttackVision())
			return false;

		bool shouldStomp = !__instance.isEnraged && UnityEngine.Random.Range(0, 60) > __instance.tackleChance && UnityEngine.Random.Range(0, 4) == 0;
		bool shouldStompEnraged = __instance.isEnraged && UnityEngine.Random.Range(0, 8) == 0;

		if (shouldStomp || shouldStompEnraged) {
			if (__instance.tackleChance < 50) {
				__instance.tackleChance = 50;
			}
			__instance.tackleChance += 20;
			__instance.inAction = true;
			__instance.Stomp();
			return false;
		}
		return true;
	}


	// CERBERUS PATCH (always flash)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(StatueBoss), nameof(StatueBoss.Tackle))]
	public static void TacklePrefix(StatueBoss __instance, bool __runOriginal) {
		if (__instance.difficulty != 19)
			return;
		if (!__runOriginal)
			return;
		
		GameObject gameObject = UnityObject.Instantiate<GameObject>(
			DefaultReferenceManager.Instance.unparryableFlash,
			__instance.eid.weakPoint.transform.position + __instance.transform.forward * 1.5f, //+ Vector3.up * 6f + __instance.transform.forward * 3f,
			__instance.transform.rotation
		);
		gameObject.transform.localScale *= 5f;
		gameObject.transform.SetParent(__instance.eid.weakPoint.transform, true);
	}

	// CERBERUS PATCH (extra dash)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(StatueBoss), nameof(StatueBoss.Tackle))]
	public static void TacklePostfix(StatueBoss __instance, bool __runOriginal) {
		if (__instance.difficulty != 19)
			return;
		if (!__runOriginal)
			return;
		__instance.extraTackles = 1; // Brutal: 1
	}
	
	// CERBERUS PATCH (quick dash)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(StatueBoss), nameof(StatueBoss.StopDash))]
	public static bool StopDashPrefix(StatueBoss __instance) {
		__instance.dashPower = 0f;
		if (__instance.gc.onGround) {
			__instance.rb.isKinematic = true;
		} else {
			__instance.rb.velocity = Vector3.zero;
		}
		__instance.damaging = false;
		__instance.partAud.Stop();
		__instance.StopDamage();

		if (__instance.extraTackles > 0) {
			__instance.dontFall = true;
			__instance.extraTackles--;
			__instance.tracking = true;
			__instance.anim.speed = 0.1f;
			GameObject gameObject = UnityObject.Instantiate<GameObject>(
				DefaultReferenceManager.Instance.unparryableFlash,
				__instance.eid.weakPoint.transform.position + __instance.transform.forward * 1.5f,
				__instance.transform.rotation
			);
			gameObject.transform.localScale *= 5f;
			gameObject.transform.SetParent(__instance.eid.weakPoint.transform, true);
			__instance.anim.Play("Tackle", -1, 0.4f);
			float tackleSpeed = 0.5f;
			if (Util.IsHardMode()) {
				tackleSpeed = 0.2f;
			} else if (__instance.difficulty == 19) {
				tackleSpeed = 0.35f;
			}
			__instance.Invoke("DelayedTackle", tackleSpeed / __instance.realSpeedModifier);
			return false;
		}

		__instance.dontFall = false;
		return false;
	}

	// CERBERUS PATCH (faster shockwave)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(StatueBoss), nameof(StatueBoss.StompHit))]
	public static bool StompHitPrefix(StatueBoss __instance) {
		if (__instance.difficulty != 19)
			return true;

		if (Util.IsHardMode()) {
			GroundWave groundWave = UnityObject.Instantiate<GameObject>(Plugin.Prefabs["MirrorReaperGroundWave"], __instance.transform.position, __instance.transform.rotation)
				.GetComponent<GroundWave>();
			groundWave.target = __instance.eid.target;
			groundWave.transform.SetParent(__instance.transform.parent ? __instance.transform.parent : GoreZone.ResolveGoreZone(__instance.transform).transform);
			groundWave.lifetime = 15f;
			Breakable componentInChildren = groundWave.GetComponentInChildren<Breakable>();
			if (componentInChildren) {
				componentInChildren.durability = 5f;
			}
			groundWave.eid = __instance.eid;
			groundWave.difficulty = __instance.difficulty;
		}

		__instance.cc.CameraShake(1f);
		if (__instance.currentStompWave != null) {			
			UnityObject.Destroy(__instance.currentStompWave);
		}
		
		int shockwaveCount = 1;
		switch (__instance.difficulty) {
			case 4:
				shockwaveCount = 2;
				break;
			case 5:
				shockwaveCount = 3;
				break;
			case 19:
				shockwaveCount = 2; // Brutal: 2
				break;
		}

		for (int currentShockwaveCount = 0; currentShockwaveCount < shockwaveCount; currentShockwaveCount++) {
			__instance.currentStompWave = UnityObject.Instantiate<GameObject>(__instance.stompWave.ToAsset(), new Vector3(__instance.stompPos.position.x, __instance.transform.position.y, __instance.stompPos.position.z), Quaternion.identity);
			PhysicalShockwave component = __instance.currentStompWave.GetComponent<PhysicalShockwave>();
			switch (__instance.difficulty) {
				case 19:
					component.speed = 100f; // Brutal: 75f
					break;
				case 4:
				case 5:
					component.speed = 75f;
					break;
				case 3:
					component.speed = 50f;
					break;
				case 2:
					component.speed = 35f;
					break;
				case 1:
					component.speed = 25f;
					break;
				case 0:
					component.speed = 15f;
					break;
			}

			component.damage = Mathf.RoundToInt(25f * __instance.eid.totalDamageModifier);
			component.maxSize = 100f;
			component.enemy = true;
			component.enemyType = EnemyType.Cerberus;

			if (currentShockwaveCount == 0)
				continue;
			
			component.speed /= 1 + currentShockwaveCount * 2;
			AudioSource audioSource;
			if (component.TryGetComponent<AudioSource>(out audioSource)) {
				audioSource.enabled = false;
			}
		}
		return false;
	}

	// CERBERUS PATCH (homing orb throw)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(StatueBoss), nameof(StatueBoss.OrbSpawn))]
	public unsafe static bool OrbSpawnPrefix(StatueBoss __instance) {
		if (__instance.difficulty != 19)
			return true;

		Vector3 vector = __instance.transform.position + Vector3.up * 3.5f;
		Vector3 direction = __instance.ToPlanePos(__instance.orbLight.transform.position) + Vector3.up * 3.5f - vector;
		PhysicsCastResult physicsCastResult;
		Vector3 vector2;
		PortalTraversalV2[] array;
		PortalPhysicsV2.ProjectThroughPortals(vector, direction, default(LayerMask), out physicsCastResult, out vector2, out array);
		Vector3 position = vector2;
		bool flag = false;
		Vector3 vector3 = __instance.predictedTargetPosition;
		if (array.Length != 0) {
			PortalTraversalV2 portalTraversalV = array[0];
			PortalHandle portalHandle = portalTraversalV.portalHandle;
			Portal portalObject = portalTraversalV.portalObject;
			if (portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile)) {
				vector3 = PortalUtils.GetTravelMatrix(array).MultiplyPoint3x4(vector3);
			} else if (!portalObject.passThroughNonTraversals) {
				#pragma warning disable CS0618 // Type or member is obsolete
				position = portalObject.GetTransform(portalHandle.side).GetPositionInFront(array[0].entrancePoint, 0.01f);
				#pragma warning restore CS0618 // Type or member is obsolete
				flag = true;
			}
		}

		GameObject gameObject = UnityObject.Instantiate<GameObject>(__instance.orbProjectile.ToAsset(), position, Quaternion.identity);
		gameObject.transform.LookAt(vector3);

		Rigidbody rigidbody;
		if (gameObject.TryGetComponent<Rigidbody>(out rigidbody)) {
			float d = 10000f;
			if (__instance.difficulty > 2) {
				d = 20000f;
			} else if (__instance.difficulty == 2) {
				d = 15000f;
			}
			rigidbody.AddForce(gameObject.transform.forward * d);
		}

		Projectile projectile;
		if (gameObject.TryGetComponent<Projectile>(out projectile)) {
			#pragma warning disable CS0618 // Type or member is obsolete
			projectile.target = __instance.eid.target;
			#pragma warning restore CS0618 // Type or member is obsolete

			projectile.damage *= __instance.eid.totalDamageModifier;

			if (__instance.difficulty <= 2) {
				projectile.bigExplosion = false;
			} else if (__instance.difficulty == 19) {
				projectile.homingType = HomingType.Gradual; // Brutal: none
				projectile.turningSpeedMultiplier = 0.7f; // Brutal: none
				projectile.speed = 90f; // Brutal: 0f
				rigidbody.useGravity = false; // Brutal: true
				// projectile.turnSpeed = 80f; // Brutal: none
			}

			if (flag) {
				projectile.Explode();
			}
		}
		__instance.orbGrowing = false;
		__instance.orbLight.range = 0f;
		__instance.part.Play();
		return false;
	}
}