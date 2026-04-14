using HarmonyLib;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: MINOSARM !!!
[HarmonyPatch(typeof(MinosArm))]
public class MinosArmPatch {
	// MINOS ARM PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MinosArm), nameof(MinosArm.SetSpeed))]
	public static void SetSpeedPostfix(MinosArm __instance) {
		if (__instance.difficulty != 19)
			return;

		float hardModeMult = (!Util.IsHardMode()) ? 1.375f : 1.6f;

		__instance.maxSlams = 99; // BRUTAL: 99
		__instance.originalAnimSpeed = hardModeMult * __instance.eid.totalSpeedModifier; // default: 1f * ...
		__instance.anim.speed = __instance.originalAnimSpeed * (1f + __instance.speedState / 4f);
	}
}