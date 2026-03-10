using HarmonyLib;
using UnityEngine.Events;


namespace BillionDifficulty.EnemyPatches;

// EARTHMOVER SPINNING BEAM PATCH
[HarmonyPatch(typeof(Spin))]
public class SpinPatch {
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Spin), nameof(Spin.Start))]
	public static void StartPrefix(Spin __instance) {
		if (__instance.eid && __instance.eid.difficultyOverride >= 0) {
			__instance.difficulty = __instance.eid.difficultyOverride;
		} else {
			__instance.difficulty = Util.GetDifficulty();
		}

		if (__instance.difficulty != 19) {
			return;
		}

		if (__instance.transform.name == "LaserRing" && __instance.difficultyVariance) {
			__instance.difficultySpeedMultiplier *= 1.3f;
		}
	}
}

// EARTHMOVER COUNTDOWN PATCH
[HarmonyPatch(typeof(Countdown), nameof(Countdown.GetCountdownLength))]
public class CountdownPatch {
	public static bool Prefix(Countdown __instance, ref float __result) {
		if (!__instance.changePerDifficulty) {
			__result = __instance.countdownLength;
			return false;
		}

		if (__instance.difficulty == 19) {
			__result = __instance.countdownLengthPerDifficulty[4]; // 4 is brutal
			return false;
		}

		__result = __instance.countdownLengthPerDifficulty[__instance.difficulty];
		return false;
	}
}

// EARTHMOVER DEFENSE SYSTEM PATCH (and probably some other stuff)
[HarmonyPatch(typeof(DifficultyDependantObject), nameof(DifficultyDependantObject.Awake))]
public class DifficultyDependantObjectPatch {
	public static bool Prefix(ref DifficultyDependantObject __instance) {
		if (!Util.IsDifficulty(19)) {
			return true;
		}

		__instance.veryHard = true; // veryHard: 4 (brutal)

		UnityEvent unityEvent2 = __instance.onRightDifficulty;
		if (unityEvent2 == null) {
			return false;
		}
		unityEvent2.Invoke();
		return false;
	}
}