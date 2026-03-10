using HarmonyLib;
using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: Mandalore !!!
[HarmonyPatch(typeof(Mandalore))]
public class MandalorePatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Mandalore), nameof(Mandalore.Update))]
	public static void UpdatePostfix(Mandalore __instance) {
		if (!Util.IsDifficulty(19)) {
			return;
		}

        __instance.cooldown -= 1f * Time.deltaTime;
	}
}