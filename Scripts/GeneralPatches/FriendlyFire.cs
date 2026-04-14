using HarmonyLib;
using UnityEngine;


namespace BillionDifficulty.GenaralPatches;

[HarmonyPatch(typeof(StyleCalculator), nameof(StyleCalculator.AddPoints))]
public class FriendlyFirePatch {
	public static void Prefix(ref int points, string pointName, EnemyIdentifier eid, GameObject sourceWeapon) {
		if (pointName == "ultrakill.friendlyfire" && Util.IsDifficulty(19)) {
			points = Mathf.RoundToInt(points * 0.2f);
		}
	}
}