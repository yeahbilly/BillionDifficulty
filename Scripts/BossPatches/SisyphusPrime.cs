using HarmonyLib;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: SISYPHUSPRIME !!!
[HarmonyPatch(typeof(SisyphusPrime))]
public class SisyphusPrimePatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(SisyphusPrime), nameof(SisyphusPrime.SetSpeed))]
	public static void SetSpeedPostfix(SisyphusPrime __instance) {
		if (__instance.difficulty != 19)
			return;
		float hardModeMult = (!Util.IsHardMode()) ? 1.35f : 1.5f;
		__instance.anim.speed = hardModeMult * __instance.eid.totalSpeedModifier; // Brutal: 1.125f * ...
	}
}