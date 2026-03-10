using HarmonyLib;
using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: FERRYMAN !!!
[HarmonyPatch(typeof(Ferryman))]
public class FerrymanPatch {
	// FERRYMAN PATCH (setup blueing)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Ferryman), nameof(Ferryman.Start))]
	public static void StartPrefix(Ferryman __instance) {
		__instance.difficulty = Util.GetDifficulty();
		if (__instance.difficulty != 19) {
			return;
		}

		if (__instance.eid == null) {
			__instance.eid = __instance.GetComponent<EnemyIdentifier>();
		}

		FerrymanStuff fs = __instance.gameObject.AddComponent<FerrymanStuff>();
		fs.initialSpeed = 1.1f;
		fs.maxSpeed = 1.3f;
		fs.changeColor = true;
		fs.targetColor = new Color(0.4f, 0.4f, 1f); // (0.5f, 0.5f, 1f)
		fs.targetFerrymanCloakColor = new Color(0.55f, 0.55f, 1f); // (0.65f, 0.65f, 1f)
		fs.eid = __instance.eid;

		// saves total health
		CounterInt counter = __instance.gameObject.AddComponent<CounterInt>();
		counter.maxValue = Mathf.RoundToInt(__instance.mach.health);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Ferryman), nameof(Ferryman.Update))]
	public static void UpdatePrefix(Ferryman __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		__instance.SetSpeed();
	}

	// FERRYMAN PATCH (speed up)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Ferryman), nameof(Ferryman.SetSpeed))]
	public static void SetSpeedPostfix(Ferryman __instance) {
		if (__instance.eid.dead) {
			return;
		}
		if (__instance.difficulty != 19) {
			return;
		}

		CounterInt counter = __instance.gameObject.GetComponent<CounterInt>();
		FerrymanStuff fs = __instance.gameObject.GetComponent<FerrymanStuff>();
		fs.speedChangeMultiplier = 0.4f / (counter.maxValue * __instance.eid.totalHealthModifier); // more health = slower

		__instance.defaultMovementSpeed = 32f * fs.currentValue;
		__instance.anim.speed = fs.currentValue * __instance.eid.totalSpeedModifier;
		__instance.nma.speed = __instance.defaultMovementSpeed * __instance.eid.totalSpeedModifier;

		if (fs.reached && !fs.addedStyle) {
			MonoSingleton<StyleHUD>.Instance.AddPoints(25, "billion.blue", null, __instance.eid, -1, "", "");
			fs.addedStyle = true;
		}
	}
}