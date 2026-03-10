using HarmonyLib;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: MINOTAUR !!!
[HarmonyPatch(typeof(Minotaur))]
public class MinotaurPatch {
	// MINOTAUR PATCH (speed)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Minotaur), nameof(Minotaur.GetSpeed))]
	public static bool GetSpeedPrefix(int difficulty, Minotaur __instance, ref EnemyMovementData __result) {
		if (difficulty != 19) {
			return true;
		}

		float num = 1.4f; // Brutal: 1.2f

		__result = new EnemyMovementData {
			speed = 50f * num,
			angularSpeed = 12000f,
			acceleration = 100f
		};
		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Minotaur), nameof(Minotaur.SetSpeed))]
	public static void SetSpeedPostfix(Minotaur __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		float num = 1.4f; // Brutal: 1.2f
		__instance.anim.speed = num * __instance.eid.totalSpeedModifier;
		__instance.nma.speed = 50f * __instance.anim.speed;
	}
}

// !!! PATCHGROUP: MINOTAURCHASE !!!
[HarmonyPatch(typeof(MinotaurChase))]
public class MinotaurChasePatch {
	// MINOTAUR PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MinotaurChase), nameof(MinotaurChase.SetSpeed))]
	public static void SetSpeedPostfix(MinotaurChase __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		__instance.movementSpeed = 40f * __instance.eid.totalSpeedModifier; // Brutal: 35f * ...
		__instance.anim.speed = 1.4f * __instance.eid.totalSpeedModifier; // Brutal: 1.2f * ...
	}
}