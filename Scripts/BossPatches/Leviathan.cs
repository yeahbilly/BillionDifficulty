using HarmonyLib;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: LEVIATHAN !!!
[HarmonyPatch(typeof(LeviathanHead))]
public class LeviathanHeadPatch {
	// LEVIATHAN HEAD PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(LeviathanHead), nameof(LeviathanHead.SetSpeed))]
	public static void SetSpeedPostfix(LeviathanHead __instance) {
		if (__instance.lcon.difficulty != 19) {
			return;
		}

		__instance.anim.speed = 1.6f * __instance.lcon.eid.totalSpeedModifier; // Brutal: 1.25f * ...
	}
}


[HarmonyPatch(typeof(LeviathanTail))]
public class LeviathanTailPatch {
	// LEVIATHAN TAIL PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(LeviathanTail), nameof(LeviathanTail.GetAnimSpeed))]
	public static void GetAnimSpeedPostfix(LeviathanTail __instance, ref float __result) {
		if (__instance.lcon.difficulty != 19) {
			return;
		}

		__result = 2f; // Brutal: 1.5f
	}
}