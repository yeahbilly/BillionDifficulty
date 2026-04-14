using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: IDOL !!!
[HarmonyPatch(typeof(EnemyIdentifier))]
public class BlessPatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.Bless))]
	public static void BlessPostfix(EnemyIdentifier __instance) {
		if (__instance.difficulty != 19)
			return;
		if (__instance.GetComponent<FerrymanFake>() != null)
			return;

		IdolHealingSetup setup = __instance.gameObject.AddComponent<IdolHealingSetup>();
		setup.cooldownMax = 2f;

		if (Util.IsHardMode())
			setup.cooldownMax = 1.5f;
	}
	[HarmonyPostfix]
	[HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.Unbless))]
	public static void UnblessPostfix(EnemyIdentifier __instance) {
		if (__instance.difficulty != 19)
			return;
		UnityObject.Destroy(__instance.GetComponent<IdolHealingSetup>());
	}
}

[HarmonyPatch(typeof(Explosion), nameof(Explosion.Start))]
public class ExplosionIdolPatch {
	public static void Postfix(Explosion __instance) {
		if (!Util.IsDifficulty(19) || __instance.transform.parent.name != "Idol Healing Explosion")
			return;

		EnemyIdentifierSaver saver = __instance.GetComponent<EnemyIdentifierSaver>();
		IdolHealingSetup setup;

		// __instance.GetComponent<SphereCollider>().enabled = true;
		IdolHealingExplosion idolHeal = __instance.gameObject.AddComponent<IdolHealingExplosion>();
		idolHeal.healing = 0.8f;
		if (Util.IsHardMode()) {
			idolHeal.healing = 1.2f;
			idolHeal.healingReducedHardMode = 0.9f;
		}

		// finds the idol that blesses the enemy
		EnemyIdentifier[] eids = UnityObject.FindObjectsByType<EnemyIdentifier>(FindObjectsSortMode.None);
		foreach (EnemyIdentifier eid in eids) {
			if (eid.idol?.target == saver?.eid && saver?.eid != null) {
				//idolHeal.healing *= eid.totalHealthModifier;
				setup = saver.eid.GetComponent<IdolHealingSetup>();
				setup.cooldownMax = setup.cooldownMaxOriginal / eid.totalHealthModifier;
				break;
			}
		}

	}
}