using System;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.SetupPatches;

// adds the difficulty id
[HarmonyPatch(typeof(PrefsManager), nameof(PrefsManager.EnsureValid))]
public class EnsureValidPatch {
	public static bool Prefix(ref object __result, string key, object value) {
		if (key != "difficulty" || (int)value != 19)
			return true;
		__result = 19;
		return false;
	}
}

// changes the title
[HarmonyPatch(typeof(DifficultyTitle), nameof(DifficultyTitle.Check))]
public class DifficultyTitlePatch {
	public static void Postfix(DifficultyTitle __instance) {
		if (!Util.IsDifficulty(19))
			return;

		bool trueHardMode = Util.IsHardMode() && (Plugin.StayedOnHardMode || SceneHelper.CurrentScene == "Main Menu");
		bool semiHardMode = Util.IsHardMode() && !Plugin.StayedOnHardMode;

		string baseName = "BILLION";
		if (trueHardMode) {
			baseName = "BRILLIANT BILLION";
		} else if (semiHardMode) {
			baseName = "BILLION*";
		}
		__instance.txt2.text =
			__instance.lines
			? "-- " + baseName + " --"
			: baseName;
		// __instance.lines ? "-- <color=#aaccff>BILLION</color> --" : "<color=#aaccff>BILLION</color>"

		// changes the title for "big" levels
		var frFound = UnityObject.FindObjectsByType<FinalRank>(FindObjectsSortMode.None);
		if (frFound == null || frFound.Length == 0)
			return;

		FinalRank fr = frFound[0];
		Transform title = fr.transform.Find("Title");
		string levelName = title.Find("Text").GetComponent<TextMeshProUGUI>().text;

		string color;
		if (levelName.StartsWith("8-4:")) // TODO: replace with 9-2 after treachery comes out
			color = "ffaaaa";
		else if (levelName.StartsWith("P-1:"))
			color = "aaffff";
		else if (levelName.StartsWith("P-2:"))
			color = "ffccaa";
		else if (levelName.StartsWith("0-E:"))
			color = "aaccff";
		else if (levelName.StartsWith("1-E:"))
			color = "aaaaaa";
		else if (levelName.StartsWith("2-E:")) // random bullshit go
			color = "ffbb99";
		else if (levelName.StartsWith("3-E:"))
			color = "ccffaa";
		else if (levelName.StartsWith("9-2:") || levelName.StartsWith("P-") || levelName.Contains("-E"))
			color = "ffffff";
		else
			return;


		string baseNameFun = String.Format("<color=#{0}>BILLION</color>S MUST HAVE FUN", color);
		if (trueHardMode) {
			baseNameFun = String.Format("<color=#{0}>BRILLIANT</color> BILLION", color);
		} else if (semiHardMode) {
			baseNameFun = String.Format("<color=#{0}>BILLION</color>S MUST HAVE FUN*", color);
		}
		__instance.txt2.text =
			__instance.lines
			? "-- " + baseNameFun + " --"
			: baseNameFun;
	}
}

[HarmonyPatch(typeof(LeaderboardController), nameof(LeaderboardController.CanSubmitScores), MethodType.Getter)]
public class CanSubmitScoresPatch {
	public static bool Prefix(ref bool __result) {
		if (!Util.IsDifficulty(19))
			return true;
		__result = false;
		return false;
	}
}

[HarmonyPatch(typeof(StatsManager), nameof(StatsManager.SendInfo))]
public class SendInfoPatch {
	public static void Postfix() {
		Plugin.Logger.LogWarning("Score submitted: " + LeaderboardController.CanSubmitScores.ToString());
	}
}
[HarmonyPatch(typeof(FinalCyberRank), nameof(FinalCyberRank.GameOver))]
public class CyberGameOverPatch {
	public static void Postfix() {
		if (!Util.IsDifficulty(19))
			return;
		Plugin.Logger.LogWarning("Score submitted: " + LeaderboardController.CanSubmitScores.ToString());
	}
}

[HarmonyPatch(typeof(PresenceController), nameof(PresenceController.Start))]
public class PresenceControllerPatch {
	public static void Prefix(PresenceController __instance) {
		if (__instance.diffNames.Length < 20) {
			Array.Resize(ref __instance.diffNames, 20);
		}
		__instance.diffNames[19] = Util.IsHardMode() ? "BRILLIANT BILLION" : "BILLION";
	}
}