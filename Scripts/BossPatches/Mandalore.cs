using HarmonyLib;
using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: Mandalore !!!
[HarmonyPatch(typeof(Mandalore))]
public class MandalorePatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Mandalore), nameof(Mandalore.Update))]
	public static void UpdatePostfix(Mandalore __instance) {
		if (!Util.IsDifficulty(19))
			return;
		if (!Util.IsHardMode())
			__instance.cooldown -= 1f * Time.deltaTime;
		else
			__instance.cooldown -= 1.5f * Time.deltaTime;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Mandalore), nameof(Mandalore.Start))]
	public static void StartPostfix(Mandalore __instance) {
		__instance.fullAutoProjectile.GetComponentInChildren<Projectile>().enemyDamageMultiplier = 0f;
		__instance.fullerAutoProjectile.GetComponentInChildren<Projectile>().enemyDamageMultiplier = 0f;
	}
}