using HarmonyLib;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: MIRRORREAPER !!!
[HarmonyPatch(typeof(MirrorReaper))]
public class MirrorReaperPatch {
	// MIRROR REAPER PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MirrorReaper), nameof(MirrorReaper.UpdateDifficulty))]
	public static void UpdateDifficultyPostfix(MirrorReaper __instance) {
        if (__instance.difficulty != 19) {
            return;
        }
        __instance.anim.speed = 1f * __instance.eid.totalSpeedModifier; // Brutal: 1f * ...
        __instance.maxGroundWaves = 3; // Brutal: 3
	}
}