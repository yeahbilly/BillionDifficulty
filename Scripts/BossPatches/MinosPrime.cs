using HarmonyLib;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: SISYPHUSPRIME !!!
[HarmonyPatch(typeof(MinosPrime))]
public class MinosPrimePatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MinosPrime), nameof(MinosPrime.SetSpeed))]
	public static void SetSpeedPostfix(MinosPrime __instance) {
		if (__instance.difficulty != 19)
			return;

		float hardModeMult = (!Util.IsHardMode()) ? 1.35f : 1.6f;
		__instance.anim.speed = hardModeMult * __instance.eid.totalSpeedModifier; // Brutal: 1.125f * ...
	}
}