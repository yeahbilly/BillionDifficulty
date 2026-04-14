using BillionDifficulty.EnemyPatches;
using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.GenaralPatches;

// cybergrind wave end cleanup patch
[HarmonyPatch(typeof(EndlessGrid))]
public class EndlessGridPatch {
	[HarmonyPrefix]
	[HarmonyPatch(typeof(EndlessGrid), nameof(EndlessGrid.NextWave))]
	public static void NextWavePrefix() {
		if (!Util.IsDifficulty(19))
			return;

		DestroyAllOfType<SandificationZone>(); // removes sand zones
		DestroyAllOfType<SlowDownOverTime>(); // removes mindflayer green orbs
		DestroyAllOfType<SlowDownOverTimeEase>(); // removes schism projectiles
	}

	public static void DestroyAllOfType<T>() where T : Component {
		T[] objs = UnityObject.FindObjectsByType<T>(FindObjectsSortMode.None);
		foreach (T obj in objs) {
			UnityObject.Destroy(obj.transform.root.gameObject);
		}
	}
}