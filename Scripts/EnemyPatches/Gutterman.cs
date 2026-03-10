using HarmonyLib;
using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: GUTTERMAN !!!
[HarmonyPatch(typeof(Gutterman))]
public class GuttermanPatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Gutterman), nameof(Gutterman.Start))]
	public static void StartPostfix(Gutterman __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		BoolValue bv = __instance.gameObject.AddComponent<BoolValue>();
		bv.description = "targetInZone";
		MoveBacker mb = __instance.gameObject.AddComponent<MoveBacker>();
	}
	
	// GUTTERMAN PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Gutterman), nameof(Gutterman.SetSpeed))]
	public static void SetSpeedPostfix(Gutterman __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		
		__instance.anim.speed = 1.2f * __instance.eid.totalSpeedModifier; // Standard~Brutal: 1f, Lenient: 0.9f
		__instance.defaultMovementSpeed = 12f * __instance.eid.totalSpeedModifier; // Standard~Brutal: 10f, Lenient: 9f
		__instance.windupSpeed = 1.35f * __instance.eid.totalSpeedModifier; // Brutal: 1f, Lenient: 0.75f
		__instance.nma.speed = __instance.slowMode ? (__instance.defaultMovementSpeed / 1.5f) : __instance.defaultMovementSpeed; // default: divided by 2f
		__instance.trackingSpeedMultiplier = 1.2f; // Brutal: 1f
		if (__instance.trackingSpeed < __instance.defaultTrackingSpeed) {
			__instance.trackingSpeed = __instance.defaultTrackingSpeed;
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Gutterman), nameof(Gutterman.GetSpeed))]
	public static bool GetSpeedPrefix(int difficulty, ref EnemyMovementData __result) {
		if (difficulty != 19) {
			return true;
		}
		__result = new EnemyMovementData {
			speed = 12f,
			angularSpeed = 120f, // default: 120f
			acceleration = 8f // default: 8f
		};
		return false;
	}
	
	// GUTTERMAN PATCH (dodges backwards)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Gutterman), nameof(Gutterman.FixedUpdate))]
	public static bool UpdatePrefix(Gutterman __instance) {
		if (__instance.difficulty != 19 || __instance.eid.dead || __instance.eid.target == null) {
			return true;
		}

		MoveBacker mb = __instance.gameObject.GetComponent<MoveBacker>();
		if (mb == null) {
			mb = __instance.gameObject.AddComponent<MoveBacker>();
		}

		float distance = Vector3.Distance(__instance.transform.position, __instance.eid.target.position);
		if (distance <= 10f) {
			if (BoolValue.Get("targetInZone", __instance.gameObject) == false) {
				BoolValue.Set("targetInZone", true, __instance.gameObject);
				__instance.StartCoroutine(mb.MoveBack());
			}
		} else if (distance >= 20) {
			BoolValue.Set("targetInZone", false, __instance.gameObject);
		}

		if (mb.moving) {
			return false;
		}

		return true;
	}
}