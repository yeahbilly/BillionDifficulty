using HarmonyLib;
using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: MINOSBOSS !!!
[HarmonyPatch(typeof(MinosBoss))]
public class MinosPatch {
	// MINOS CORPSE PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MinosBoss), nameof(MinosBoss.SetSpeed))]
	public static void SetSpeedPostfix(MinosBoss __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		__instance.anim.speed = 1.6f * __instance.eid.totalSpeedModifier; // BRUTAL: 1.25f
	}

	// MINOS CORPSE PATCH (black hole speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MinosBoss), nameof(MinosBoss.SpawnBlackHole))]
	public static void SpawnBlackHolePostfix(MinosBoss __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		__instance.currentBlackHole.speed *= 1.8f; // Brutal: 1.5f
	}

	// MINOS CORPSE PATCH (attack fix)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(MinosBoss), nameof(MinosBoss.Update))]
	public static bool UpdatePrefix(MinosBoss __instance) {
		if (__instance.difficulty != 19) {
			return true;
		}

		if (__instance.dead && !__instance.anim.GetCurrentAnimatorStateInfo(0).IsName("Death")) {
			__instance.anim.Play("Death");
		}
		if (__instance.currentBlackHole == null && __instance.blackHoleCooldown > 0f && (__instance.phase < 2 || __instance.difficulty > 2)) {
			__instance.blackHoleCooldown = Mathf.MoveTowards(__instance.blackHoleCooldown, 0f, Time.deltaTime * __instance.eid.totalSpeedModifier);
		}
		if (__instance.stat && __instance.stat.health < __instance.originalHealth / 2f && __instance.phase < 2 && !__instance.anim.IsInTransition(0)) {
			__instance.inPhaseChange = true;
			__instance.PhaseChange(2);
		}
		if (__instance.eid.target == null) {
			return false;
		}

		if (__instance.inAction || __instance.inPhaseChange) {
			return false;
		}

		if (__instance.currentBlackHole == null && __instance.blackHoleCooldown == 0f && __instance.difficulty >= 2 && (__instance.phase < 2 || __instance.difficulty > 2)) {
			__instance.BlackHole();
			return false;
		}
		if (__instance.cooldown > 0f) {
			__instance.cooldown = Mathf.MoveTowards(__instance.cooldown, 0f, Time.deltaTime * __instance.anim.speed);
			return false;
		}
		if (__instance.anim.IsInTransition(0)) {
			return false;
		}

		if (__instance.phase == 1 && __instance.difficulty < 4) {
			__instance.cooldown = (__instance.difficulty >= 4) ? 1f : 2f;
		} else if (__instance.phase == 2 || __instance.difficulty >= 4) {
			if ((__instance.difficulty == 4 && __instance.punchesSinceBreak < 2) || __instance.difficulty == 5 || __instance.difficulty == 19) {
				__instance.punchesSinceBreak++;
				__instance.cooldown = 0f;
			} else {
				__instance.punchesSinceBreak = 0;
				__instance.cooldown = 3f;
			}
		} else {
			__instance.cooldown = 0f;
		}
		if (__instance.onRight) {
			if (__instance.onMiddle && Random.Range(0f, 1f) > 0.5f) {
				__instance.SlamMiddle();
				return false;
			}
			__instance.SlamRight();
			return false;
		} else if (__instance.onLeft) {
			if (__instance.onMiddle && Random.Range(0f, 1f) > 0.5f) {
				__instance.SlamMiddle();
				return false;
			}
			__instance.SlamLeft();
			return false;
		} else {
			__instance.SlamMiddle();
		}

		return false;
	}
}