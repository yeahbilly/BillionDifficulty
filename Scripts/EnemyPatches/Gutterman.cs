using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: GUTTERMAN !!!
[HarmonyPatch(typeof(Gutterman))]
public class GuttermanPatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Gutterman), nameof(Gutterman.Start))]
	public static void StartPostfix(Gutterman __instance) {
		if (__instance.difficulty != 19)
			return;

		BoolValue bv = __instance.gameObject.AddComponent<BoolValue>();
		bv.description = "targetInZone";
		MoveBacker mb = __instance.gameObject.AddComponent<MoveBacker>();

		if (Util.IsHardMode()) {
			TimerFloat timer = __instance.gameObject.AddComponent<TimerFloat>();
			timer.cooldownMax = 1.6f;
		}
	}
	
	// GUTTERMAN PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Gutterman), nameof(Gutterman.SetSpeed))]
	public static void SetSpeedPostfix(Gutterman __instance) {
		if (__instance.difficulty != 19)
			return;
		
		float hardModeMult = (!Util.IsHardMode()) ? 1f : 1.15f;
		float totalMult = hardModeMult * __instance.eid.totalSpeedModifier;
		__instance.anim.speed = 1.2f * totalMult; // Standard~Brutal: 1f, Lenient: 0.9f
		__instance.defaultMovementSpeed = 12f * totalMult; // Standard~Brutal: 10f, Lenient: 9f
		__instance.windupSpeed = 1.35f * totalMult; // Brutal: 1f, Lenient: 0.75f
		__instance.nma.speed = __instance.slowMode ? (__instance.defaultMovementSpeed / 1.5f) : __instance.defaultMovementSpeed; // default: divided by 2f
		if (Util.IsHardMode()) {
			__instance.nma.angularSpeed = 200f; // default: 120f
		}
		__instance.trackingSpeedMultiplier = !Util.IsHardMode() ? 1.2f : 1.45f; // Brutal: 1f
		if (__instance.trackingSpeed < __instance.defaultTrackingSpeed) {
			__instance.trackingSpeed = __instance.defaultTrackingSpeed;
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Gutterman), nameof(Gutterman.GetSpeed))]
	public static bool GetSpeedPrefix(int difficulty, ref EnemyMovementData __result) {
		if (difficulty != 19)
			return true;
		
		float hardModeMult = (!Util.IsHardMode()) ? 1f : 1.15f;
		__result = new EnemyMovementData {
			speed = 12f * hardModeMult,
			angularSpeed = !Util.IsHardMode() ? 120f : 200f, // default: 120f
			acceleration = 8f // default: 8f
		};
		return false;
	}
	
	// GUTTERMAN PATCH (dodges backwards)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Gutterman), nameof(Gutterman.FixedUpdate))]
	public static bool UpdatePrefix(Gutterman __instance) {
		if (__instance.difficulty != 19 || __instance.eid.dead || __instance.eid.target == null)
			return true;

		MoveBacker mb = __instance.GetComponent<MoveBacker>();
		if (mb == null) {
			mb = __instance.gameObject.AddComponent<MoveBacker>();
		}
		TimerFloat timer = null;
		if (Util.IsHardMode()) {
			timer = __instance.GetComponent<TimerFloat>();
		}

		float distance = Vector3.Distance(__instance.transform.position, __instance.eid.target.position);
		bool canDodge = BoolValue.Get("targetInZone", __instance.gameObject) == false || timer?.reached == true;
		if (distance <= 10f && canDodge) {
			if (Util.IsHardMode()) {
				GameObject explosion = UnityObject.Instantiate<GameObject>(
					Plugin.Prefabs["ExplosionSisyphusPrimeCharged"],
					__instance.transform.position + 2f * __instance.transform.up,
					Quaternion.identity
				);
				explosion.GetComponentInChildren<ExplosionController>(includeInactive: true).transform.localScale = Vector3.one;
				foreach (Explosion component in explosion.GetComponentsInChildren<Explosion>(includeInactive: true)) {
					component.speed *= 25f;
					component.maxSize *= 0.5f;
					component.enemyDamageMultiplier = 0f;
					component.originEnemy = __instance.eid;
				}
			}
			__instance.StartCoroutine(mb.MoveBack());
			BoolValue.Set("targetInZone", true, __instance.gameObject);
			timer?.ResetAndRun();
		} else if (distance >= 20) {
			BoolValue.Set("targetInZone", false, __instance.gameObject);
		}

		if (mb.moving)
			return false;

		return true;
	}
}