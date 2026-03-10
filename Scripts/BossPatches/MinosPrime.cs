using HarmonyLib;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: SISYPHUSPRIME !!!
[HarmonyPatch(typeof(MinosPrime))]
public class MinosPrimePatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MinosPrime), nameof(MinosPrime.SetSpeed))]
	public static void SetSpeedPostfix(MinosPrime __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		__instance.anim.speed = 1.35f * __instance.eid.totalSpeedModifier; // Brutal: 1.125f * ...
	}
}