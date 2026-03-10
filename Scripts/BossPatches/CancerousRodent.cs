using HarmonyLib;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: CANCEROUSRODENT !!!
[HarmonyPatch(typeof(CancerousRodent))]
public class CancerousRodentPatch {
	// CANCEROUS RODENT PATCH (projectile amount)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(CancerousRodent), nameof(CancerousRodent.Start))]
	public static void StartPostfix(CancerousRodent __instance) {
		if (!Util.IsDifficulty(19)) {
			return;
		}

		__instance.projectileAmount = 5; // default: 3
	}

	// CANCEROUS RODENT PATCH (attack cooldown)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(CancerousRodent), nameof(CancerousRodent.Update))]
	public static void UpdatePostfix(CancerousRodent __instance) {
		if (!Util.IsDifficulty(19)) {
			return;
		}

		if (__instance.coolDown > 2f) { // it's 3 by default
			__instance.coolDown -= 1f;
		}
	}
}