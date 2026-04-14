using BillionDifficulty.EnemyPatches;
using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.GenaralPatches;

[HarmonyPatch(typeof(NewMovement))]
public class NewMovementPatch {
	// hard damage increase
	[HarmonyPrefix]
	[HarmonyPatch(typeof(NewMovement), nameof(NewMovement.GetHurt))]
	public static bool GetHurtPrefix(NewMovement __instance, out float __state) {
		__state = __instance.antiHp;
		return true;
	}
	[HarmonyPostfix]
	[HarmonyPatch(typeof(NewMovement), nameof(NewMovement.GetHurt))]
	public static void GetHurtPostfix(NewMovement __instance, float __state) {
		if (__instance.difficulty != 19)
			return;

		float num = __instance.antiHp - __state;
		if (num > 0f) {
			num *= 50f/35f; // Brutal: 35% hard damage
			__instance.antiHp = __state + num;
		}
	}
	
	// stops streetcleaner afterburn when respawning
	[HarmonyPostfix]
	[HarmonyPatch(typeof(NewMovement), nameof(NewMovement.Respawn))]
	public static void RespawnPostfix(NewMovement __instance) {
		if (__instance.difficulty != 19)
			return;

		StreetcleanerAfterburn[] allAfterburns = UnityObject.FindObjectsByType<StreetcleanerAfterburn>(FindObjectsSortMode.None);
		foreach (StreetcleanerAfterburn afterburn in allAfterburns) {
			afterburn.ticks = afterburn.tickCount;
		}

		RemoveOnRespawn[] removes = UnityObject.FindObjectsByType<RemoveOnRespawn>(FindObjectsSortMode.None);
		foreach (var remove in removes) {
			UnityObject.Destroy(remove.gameObject);
		}
	}
}