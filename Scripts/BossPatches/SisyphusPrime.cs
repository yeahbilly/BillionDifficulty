using HarmonyLib;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: SISYPHUSPRIME !!!
[HarmonyPatch(typeof(SisyphusPrime))]
public class SisyphusPrimePatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(SisyphusPrime), nameof(SisyphusPrime.SetSpeed))]
	public static void SetSpeedPostfix(SisyphusPrime __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		__instance.anim.speed = 1.35f * __instance.eid.totalSpeedModifier; // Brutal: 1.125f * ...
	}
}