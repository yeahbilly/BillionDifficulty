using HarmonyLib;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: GERYON !!!
[HarmonyPatch(typeof(Geryon))]
public class GeryonPatch {
	// GERYON PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Geryon), nameof(Geryon.UpdateDifficulty))]
	public static void UpdateDifficultyPostfix(Geryon __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		__instance.anim.speed = 1.3f; // Brutal: 1.2f
		__instance.maximumHeat = 9f; // Brutal: 9f

		if (__instance.secondPhase) {
			__instance.anim.speed *= 1.25f;
		}
	}

	// GERYON PATCH (stun)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Geryon), nameof(Geryon.Stun))]
	public static void StunPrefix(Geryon __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		__instance.stunTime = 5f; // Brutal: 5f
	}
}