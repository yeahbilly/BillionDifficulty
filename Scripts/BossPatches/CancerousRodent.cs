using HarmonyLib;
using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: CANCEROUSRODENT !!!
[HarmonyPatch(typeof(CancerousRodent))]
public class CancerousRodentPatch {
	// CANCEROUS RODENT PATCH (projectile amount)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(CancerousRodent), nameof(CancerousRodent.Start))]
	public static void StartPostfix(CancerousRodent __instance) {
		if (!Util.IsDifficulty(19))
			return;
		__instance.projectileAmount = 5; // default: 3
		if (Util.IsHardMode())
			__instance.projectileAmount = 7;
	}

	// CANCEROUS RODENT PATCH (attack cooldown)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(CancerousRodent), nameof(CancerousRodent.Update))]
	public static void UpdatePostfix(CancerousRodent __instance) {
		if (!Util.IsDifficulty(19))
			return;

		// the cooldown is 3 by default
		if (__instance.coolDown != 0f) {
			if (!Util.IsHardMode())
				__instance.coolDown -= 1f * Time.deltaTime;
			else
				__instance.coolDown -= 2f * Time.deltaTime;
		}
		if (__instance.coolDown < 0f)
			__instance.coolDown = 0f;
	}
}