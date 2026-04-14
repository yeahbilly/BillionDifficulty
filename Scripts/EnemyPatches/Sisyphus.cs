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
		if (__instance.difficulty != 19)
			return;

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
		if (Util.IsHardMode()) {
			tracker.speedBuffTime = 10f;
		}

		BoolValue bv = __instance.gameObject.AddComponent<BoolValue>();
		bv.value = false;
		bv.description = "enraged";
	}

	// SISYPHUS PATCH (black hole)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Sisyphus), nameof(Sisyphus.SetupExplosion))]
	public static void SetupExplosionPrefix(Sisyphus __instance) {
		if (!Util.IsHardMode())
			return;

		// doesn't let it have more than 1 black hole
		if (BoolValue.Get("enraged", __instance.gameObject) == false) {
			foreach (BlackHoleFromSisyphus bhfs in UnityObject.FindObjectsByType<BlackHoleFromSisyphus>(FindObjectsSortMode.None)) {
				if (bhfs.sisy == __instance)
					return;
			}
		}

		GameObject blackHole = UnityObject.Instantiate<GameObject>(Plugin.Prefabs["BlackHoleEnemy"], __instance.boulderCb.transform.position, Quaternion.identity);
		blackHole.transform.localScale *= 3f;
		BlackHoleProjectile blackHoleComp = blackHole.GetComponent<BlackHoleProjectile>();
		blackHoleComp.speed = Random.Range(15, 26) / 2f; //Random.Range(7.5f, 12.5f);
		blackHoleComp.Activate();
		blackHole.AddComponent<BlackHoleFromSisyphus>().sisy = __instance;
	}

	// SISYPHUS PATCH (speed)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Sisyphus), nameof(Sisyphus.GetSpeed))]
	public static bool GetSpeedPrefix(int difficulty, ref EnemyMovementData __result) {
		if (difficulty != 19)
			return true;
		
		float hardModeMult = (!Util.IsHardMode()) ? 1f : 1.25f;
		__result = new EnemyMovementData {
			speed = 15f * hardModeMult, // default: 10f
			angularSpeed = 999f, // default: 999f
			acceleration = 666f // default: 666f
		};
		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Sisyphus), nameof(Sisyphus.Death))]
	public static void DeathPostfix(Sisyphus __instance) {
		if (__instance.difficulty != 19)
			return;
		Transform rage = __instance.mach.chest.transform.Find("RageEffect(Clone)");
		if (rage != null)
			UnityObject.Destroy(rage.gameObject);
	}

	// SISYPHUS PATCH (enrage)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Sisyphus), nameof(Sisyphus.SetSpeed))]
	public static void SetSpeedPostfix(Sisyphus __instance) {
		if (__instance.difficulty != 19)
			return;
		if (__instance.eid.dead)
			return;

		DamageOverTimeTracker tracker = __instance.GetComponent<DamageOverTimeTracker>();
		if (tracker == null)
			return;

		float speedBuffMultiplier = 1f;

		bool? isEnraged = BoolValue.Get("enraged", __instance.gameObject);

		if (tracker.buffingSpeed && isEnraged == false) {
			speedBuffMultiplier = tracker.speedBuff;
			BoolValue.Set("enraged", true, __instance.gameObject);

			// puppet color doesn't change
			if (!__instance.eid.puppet) {
				SkinnedMeshRenderer[] renderers = __instance.GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach (SkinnedMeshRenderer renderer in renderers)
					renderer.material.color = new Color(1f, 0.35f, 0.35f);
			}

			UnityObject.Instantiate<GameObject>(DefaultReferenceManager.Instance.enrageEffect, __instance.mach.chest.transform);
		} else if (!tracker.buffingSpeed && isEnraged == true) {
			BoolValue.Set("enraged", false, __instance.gameObject);
			SkinnedMeshRenderer[] renderers = __instance.GetComponentsInChildren<SkinnedMeshRenderer>();
			if (!__instance.eid.puppet) {
				foreach (SkinnedMeshRenderer renderer in renderers) {
					renderer.material.color = new Color(1f, 1f, 1f);
				}
			}
			Transform rage = __instance.mach.chest.transform.Find("RageEffect(Clone)");
			if (rage != null)
				UnityObject.Destroy(rage.gameObject);
		}

		float hardModeMult = (!Util.IsHardMode()) ? 1f : 1.25f;
		float totalMult = hardModeMult * speedBuffMultiplier * __instance.eid.totalSpeedModifier;

		__instance.anim.speed = 1.05f * totalMult; // default: 1f * ...
		__instance.nma.speed = 15f * totalMult; // default: 10f * ...
		__instance.anim.SetFloat("StompSpeed", 1.1f * totalMult); // Brutal: 1f
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Sisyphus), nameof(Sisyphus.Update))]
	public static void UpdatePostfix(Sisyphus __instance) {
		if (__instance.difficulty != 19)
			return;

		DamageOverTimeTracker tracker = __instance.GetComponent<DamageOverTimeTracker>();
		if (tracker.buffSpeed && !tracker.buffingSpeed)
			__instance.SetSpeed();

		bool reached = tracker.reached && !tracker.onCooldown;
		if (!reached)
			return;

		tracker.onCooldown = true;
		__instance.SetSpeed();
		GameObject explosionObject = UnityObject.Instantiate<GameObject>(Plugin.Prefabs["ExplosionSuper"], __instance.transform.position, Quaternion.identity);
		explosionObject.transform.LookAt(NewMovement.Instance.transform);
		explosionObject.transform.localScale *= 1.5f;
		foreach (Explosion explosion in explosionObject.GetComponentsInChildren<Explosion>()) {
			explosion.maxSize *= 1.5f;
			explosion.damage = Mathf.RoundToInt(40 * __instance.eid.totalDamageModifier); // default: 50
			explosion.enemyDamageMultiplier = (!Util.IsHardMode()) ? 0.875f : 0f; // default: 1f
		}
	}
}