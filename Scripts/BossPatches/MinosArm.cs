using HarmonyLib;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: MINOSARM !!!
[HarmonyPatch(typeof(MinosArm))]
public class MinosArmPatch {
	// MINOS ARM PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MinosArm), nameof(MinosArm.SetSpeed))]
	public static void SetSpeedPostfix(MinosArm __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		__instance.maxSlams = 99; // BRUTAL: 99
		__instance.originalAnimSpeed = 1.375f * __instance.eid.totalSpeedModifier; // default: 1f * ...
		__instance.anim.speed = __instance.originalAnimSpeed * (1f + __instance.speedState / 4f);
	}
}