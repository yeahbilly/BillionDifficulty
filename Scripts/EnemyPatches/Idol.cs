using HarmonyLib;
using UnityObject = UnityEngine.Object;

namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: IDOL !!!
[HarmonyPatch(typeof(EnemyIdentifier))]
public class BlessPatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.Bless))]
	public static void BlessPostfix(EnemyIdentifier __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		if (__instance.gameObject.GetComponent<FerrymanFake>() == null) {
			__instance.gameObject.AddComponent<IdolHealingSetup>();
		}
	}
	[HarmonyPostfix]
	[HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.Unbless))]
	public static void UnblessPostfix(EnemyIdentifier __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		UnityObject.Destroy(__instance.gameObject.GetComponent<IdolHealingSetup>());
	}
}