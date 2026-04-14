using HarmonyLib;


namespace BillionDifficulty.GenaralPatches;

[HarmonyPatch(typeof(Punch), nameof(Punch.ParriedProjectileBeam))]
public class PunchPatch {
	public static bool Prefix(ContinuousBeam beam) {
		if (!Util.IsDifficulty(19))
			return true;
		if (beam == null)
			return false;
		
		if (beam.enemy) {
			beam.parryMultiplier = 2.5f;
		}
		beam.enemy = false;
		// beam.canHitEnemy = true;
		beam.canHitPlayer = false;
		return false;
	}
}