using HarmonyLib;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

// CERBERUS PATCH (enraged knockback)
[HarmonyPatch(typeof(SwingCheck2), nameof(SwingCheck2.Update))]
public class CerberusDashPatch {
	public static void Postfix(SwingCheck2 __instance) {
		if (!Util.IsDifficulty(19)) {
			return;
		}

		if (__instance.eid.enemyType == EnemyType.Cerberus && __instance.eid.gameObject.GetComponent<StatueBoss>().isEnraged) {
			__instance.knockBackForce = 23f; // 22.5f; default: 100f
		}
	}
}

// !!! PATCHGROUP: STATUE !!!
[HarmonyPatch(typeof(StatueBoss))]
public class StatueBossPatch {
	// CERBERUS PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(StatueBoss), nameof(StatueBoss.SetSpeed))]
	public static void SetSpeedPostfix(StatueBoss __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		if (__instance.eid.totalSpeedModifier > 1.2f) {
			__instance.realSpeedModifier -= 0.2f;
		}
		__instance.anim.speed = 1.45f * __instance.realSpeedModifier; // Brutal: 1.35f * ...
		if (__instance.isEnraged) {
			__instance.anim.speed = 1.65f * __instance.realSpeedModifier; // Brutal: 1.5f * ...
		}
		if (__instance.nma) {
			__instance.nma.speed *= 1.2f;
		}
	}

	// CERBERUS PATCH (always flash)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(StatueBoss), nameof(StatueBoss.Tackle))]
	public static void TacklePrefix(ref StatueBoss __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		
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
	public static void TacklePostfix(StatueBoss __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
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
			if (__instance.difficulty == 19) {
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

		for (int currentShockwaveCount = 0; currentShockwaveCount < shockwaveCount; currentShockwaveCount++)
		{
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

			if (currentShockwaveCount == 0) {
				continue;
			}
			
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
	public static bool OrbSpawnPrefix(StatueBoss __instance) {
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
				position = portalObject.GetTransform(portalHandle.side).GetPositionInFront(array[0].entrancePoint, 0.01f);
				flag = true;
			}
		}

		GameObject gameObject = Object.Instantiate<GameObject>(__instance.orbProjectile.ToAsset(), position, Quaternion.identity);
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
				projectile.turnSpeed = 80f; // Brutal: none
				projectile.turningSpeedMultiplier = 0.7f; // Brutal: none
				projectile.speed = 90f; // Brutal: 0f
				rigidbody.useGravity = false; // Brutal: true
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