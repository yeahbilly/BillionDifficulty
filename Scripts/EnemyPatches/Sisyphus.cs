using Billion = BillionDifficulty.Plugin;
using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: SISYPHUS !!!
[HarmonyPatch(typeof(Sisyphus))]
public class SisyphusPatch {
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Sisyphus), nameof(Sisyphus.Start))]
	public static void StartPrefix(Sisyphus __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		
		if (!__instance.eid) {
			__instance.eid = __instance.GetComponent<EnemyIdentifier>();
		}
		DamageOverTimeTracker tracker = __instance.gameObject.AddComponent<DamageOverTimeTracker>();
		tracker.damageThreshold = 30f * __instance.eid.totalHealthModifier;
		tracker.timeWindow = 5f;
		tracker.cooldownMax = 1f;
		tracker.buffSpeed = true;
		tracker.speedBuff = 1.1f;
		tracker.speedBuffTime = 7f;

		BoolValue bv = __instance.gameObject.AddComponent<BoolValue>();
		bv.value = false;
		bv.description = "enraged";
	}

	// SISYPHUS PATCH (speed)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Sisyphus), nameof(Sisyphus.GetSpeed))]
	public static bool GetSpeedPrefix(int difficulty, ref EnemyMovementData __result) {
		if (difficulty != 19) {
			return true;
		}
		__result = new EnemyMovementData {
			speed = 15f, // default: 10f
			angularSpeed = 999f, // default: 999f
			acceleration = 666f // default: 666f
		};
		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Sisyphus), nameof(Sisyphus.Death))]
	public static void DeathPostfix(Sisyphus __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		Transform rage = __instance.mach.chest.transform.Find("RageEffect(Clone)");
		if (rage != null) {
			UnityObject.Destroy(rage.gameObject);
		}
	}

	// SISYPHUS PATCH (enrage)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Sisyphus), nameof(Sisyphus.SetSpeed))]
	public static void SetSpeedPostfix(Sisyphus __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		if (__instance.eid.dead) {
			return;
		}

		DamageOverTimeTracker tracker = __instance.gameObject.GetComponent<DamageOverTimeTracker>();
		if (tracker == null) {
			return;
		}

		float speedBuffMultiplier = 1f;

		bool? isEnraged = BoolValue.Get("enraged", __instance.gameObject);

		if (tracker.buffingSpeed && isEnraged == false) {
			speedBuffMultiplier = tracker.speedBuff;
			BoolValue.Set("enraged", true, __instance.gameObject);
		
			SkinnedMeshRenderer[] renderers = __instance.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer renderer in renderers) {
				renderer.material.color = new Color(1f, 0.35f, 0.35f);
			}

			UnityObject.Instantiate<GameObject>(DefaultReferenceManager.Instance.enrageEffect, __instance.mach.chest.transform);
		} else if (isEnraged == true) {
			BoolValue.Set("enraged", false, __instance.gameObject);
			SkinnedMeshRenderer[] renderers = __instance.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer renderer in renderers) {
				renderer.material.color = new Color(1f, 1f, 1f);
			}
			Transform rage = __instance.mach.chest.transform.Find("RageEffect(Clone)");
			if (rage != null) {
				UnityObject.Destroy(rage.gameObject);
			}
		}

		__instance.anim.speed = 1.05f * speedBuffMultiplier * __instance.eid.totalSpeedModifier; // default: 1f * ...
		__instance.nma.speed = 15f * speedBuffMultiplier * __instance.eid.totalSpeedModifier; // default: 10f * ...
		__instance.anim.SetFloat("StompSpeed", 1.1f * speedBuffMultiplier * __instance.eid.totalSpeedModifier); // Brutal: 1f
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Sisyphus), nameof(Sisyphus.Update))]
	public static void UpdatePostfix(Sisyphus __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		DamageOverTimeTracker tracker = __instance.gameObject.GetComponent<DamageOverTimeTracker>();
		if (tracker.buffSpeed && !tracker.buffingSpeed) {
			__instance.SetSpeed();
		}

		bool reached = tracker.reached && !tracker.onCooldown;
		if (!reached) {
			return;
		}

		tracker.onCooldown = true;
		__instance.SetSpeed();
		GameObject explosionObject = UnityObject.Instantiate<GameObject>(Billion.ExplosionSuper, __instance.transform.position, Quaternion.identity);
		explosionObject.transform.LookAt(NewMovement.Instance.transform);
		explosionObject.transform.localScale *= 1.5f;
		foreach (Explosion explosion in explosionObject.GetComponentsInChildren<Explosion>()) {
			explosion.maxSize *= 1.5f;
			explosion.damage = Mathf.RoundToInt(40 * __instance.eid.totalDamageModifier); // default: 50
			explosion.enemyDamageMultiplier = 0.875f; // default: 1f
		}
	}
}