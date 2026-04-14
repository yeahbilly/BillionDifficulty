using System;
using HarmonyLib;


namespace BillionDifficulty.GenaralPatches;

[HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.Start))]
public class RadiancePatch {
	public static void Postfix(EnemyIdentifier __instance) {
		if (!Util.IsDifficulty(19))
			return;

		switch (__instance.enemyType) {
			case EnemyType.Filth:
				if (Util.IsHardMode())
					__instance.healthBuffModifier = 2.5f / 1.5f;
				break;
			case EnemyType.Turret:
			case EnemyType.Virtue:
				__instance.speedBuffModifier = 1.1f;
				break;
			case EnemyType.Ferryman:
			case EnemyType.Sisyphus:
			case EnemyType.Providence:
			case EnemyType.Power:
				__instance.speedBuffModifier = 1.15f;
				break;
			default:
				__instance.speedBuffModifier = 1.25f;
				break;
		}
	}
}

[HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.SpeedBuff), new Type[] {typeof(float)})]
public class RadianceSpeedPatch {
	public static bool Prefix(float modifier, EnemyIdentifier __instance) {
		if (__instance.difficulty != 19)
			return true;

		__instance.speedBuffRequests++;
		switch (__instance.enemyType) {
			case EnemyType.Turret:
			case EnemyType.Virtue:
				__instance.speedBuffModifier = 1.1f;
				break;
			case EnemyType.Ferryman:
			case EnemyType.Sisyphus:
			case EnemyType.Providence:
			case EnemyType.Power:
				__instance.speedBuffModifier = 1.15f;
				break;
			default:
				__instance.speedBuffModifier = 1.25f;
				break;
		}
		__instance.UpdateBuffs(false, true);
		return false;
	}
}

[HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.HealthBuff), new Type[] {typeof(float)})]
public class RadianceHealthPatch {
	public static bool Prefix(float modifier, EnemyIdentifier __instance) {
		if (!Util.IsHardMode())
			return true;

		__instance.healthBuffRequests++;
		if (__instance.enemyType == EnemyType.Filth) {
			__instance.healthBuffModifier = 2.5f / 1.5f;
		}
		__instance.UpdateBuffs(false, true);
		return false;
	}
}