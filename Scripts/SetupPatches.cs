using Billion = BillionDifficulty.Plugin;
using Steamworks;
using System;
using System.IO;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using UnityEngine.UI;


namespace BillionDifficulty.SetupPatches;

// adds the difficulty id
[HarmonyPatch(typeof(PrefsManager), nameof(PrefsManager.EnsureValid))]
public class EnsureValidPatch {
	public static bool Prefix(ref object __result, string key, object value) {
		if (key != "difficulty" || (int)value != 19) {
			return true;
		}
		
		__result = 19;
		return false;
	}
}

[HarmonyPatch(typeof(PrefsManager), MethodType.Constructor)]
public class PrefsManagerValidation {
	public static void Postfix(Dictionary<string, Func<object, object>> ___propertyValidators) {
		Dictionary<string, Func<object, object>> dictionary = new Dictionary<string, Func<object, object>>();
		dictionary.Add("difficulty", delegate(object value) {
			if (value is not int) {
				Billion.Logger.LogWarning("Difficulty value is not an int");
				return 2;
			}
			return null;
		});
		___propertyValidators["difficulty"] = dictionary["difficulty"];
	}
}


// changes the title
[HarmonyPatch(typeof(DifficultyTitle), nameof(DifficultyTitle.Check))]
public class DifficultyTitlePatch {
	public static void Postfix(DifficultyTitle __instance) {
		if (!Util.IsDifficulty(19)) {
			return;
		}

		__instance.txt2.text = __instance.lines ? "-- BILLION --" : "BILLION"; // __instance.lines ? "-- <color=#aaccff>BILLION</color> --" : "<color=#aaccff>BILLION</color>"
		// changes the title for "big" levels
		var frFound = UnityObject.FindObjectsByType<FinalRank>(FindObjectsSortMode.None);
		if (frFound == null || frFound.Length == 0) {
			return;
		}
		FinalRank fr = frFound[0];
		Transform title = fr.transform.Find("Title");
		string levelName = title.Find("Text").GetComponent<TextMeshProUGUI>().text;

		bool haveFun = false;
		string color = "ffffff";

		if (levelName.StartsWith("8-4:")) { // TODO: replace with 9-2 after treachery comes out
			haveFun = true;
			color = "ffaaaa";
		} else if (levelName.StartsWith("P-1:")) {
			haveFun = true;
			color = "aaffff";
		} else if (levelName.StartsWith("P-2:")) {
			haveFun = true;
			color = "ffccaa";
		} else if (levelName.StartsWith("0-E:")) {
			haveFun = true;
			color = "aaccff";
		} else if (levelName.StartsWith("1-E:")) {
			haveFun = true;
			color = "aaaaaa";
		} else if (levelName.StartsWith("2-E:")) { // random bullshit go
			haveFun = true;
			color = "ffbb99";
		} else if (levelName.StartsWith("3-E:")) {
			haveFun = true;
			color = "ccffaa";
		} else if (levelName.StartsWith("9-2:") || levelName.StartsWith("P-") || levelName.Contains("-E")) {
			haveFun = true;
		}

		if (haveFun) {
			string baseName = String.Format("<color=#{0}>BILLION</color>S MUST HAVE FUN", color);
			__instance.txt2.text = __instance.lines ? "-- "+baseName+" --" : baseName;
		}
	}
}

// makes the game go over the difficulty's save
[HarmonyPatch(typeof(GameProgressSaver), nameof(GameProgressSaver.GetProgress))]
public class GetProgressPatch {
	public static bool Prefix(int difficulty, ref int __result) {
		int num = 1;
		int[] difficultyArray = {0, 1, 2, 3, 4, 5, 19};
		int index = difficultyArray.IndexOf(difficulty);

		if (index == -1) {
			index = 0;
		}

		while (index < difficultyArray.Length) {
			GameProgressData gameProgress = GameProgressSaver.GetGameProgress(difficultyArray[index]);
			if (gameProgress != null && gameProgress.difficulty == difficultyArray[index] && gameProgress.levelNum > num) {
				num = gameProgress.levelNum;
			}
			index++;
		}
		__result = num;
		return false;
	}
}

// prime progress
[HarmonyPatch(typeof(GameProgressSaver), nameof(GameProgressSaver.GetPrime))]
public class GetPrimePatch {
	public static bool Prefix(ref int difficulty, ref int level, ref int __result) {
		if (SceneHelper.IsPlayingCustom) {
			__result = 0;
			return false;
		}

		// Billion.Logger.LogWarning("checking diff " + difficulty.ToString());
		// Billion.Logger.LogWarning("checking level: " + level.ToString());

		level--;
		int num = 0;
		int[] difficultyArray = {0, 1, 2, 3, 4, 5, 19};
		int index = difficultyArray.IndexOf(difficulty);

		if (index == -1) {
			index = 0;
		}

		while (index < difficultyArray.Length) {
			GameProgressData gameProgress = GameProgressSaver.GetGameProgress(difficultyArray[index]);
			if (gameProgress != null && gameProgress.difficulty == difficultyArray[index] && gameProgress.primeLevels != null && gameProgress.primeLevels.Length > level && gameProgress.primeLevels[level] > num) {
				Debug.Log("Highest: . Data: " + gameProgress.primeLevels[level].ToString());
				if (gameProgress.primeLevels[level] >= 2) {
					__result = 2;
					return false;
				}
				num = gameProgress.primeLevels[level];
			}
			index++;
		}
		__result = num;
		return false;
	}
}

// encore progress
[HarmonyPatch(typeof(GameProgressSaver), nameof(GameProgressSaver.GetEncoreProgress))]
public class GetEncoreProgressPatch {
	public static bool Prefix(int difficulty, ref int __result) {
		int num = 0;
		int[] difficultyArray = {0, 1, 2, 3, 4, 5, 19};
		int index = difficultyArray.IndexOf(difficulty);

		if (index == -1) {
			index = 0;
		}

		while (index < difficultyArray.Length) {
			GameProgressData gameProgress = GameProgressSaver.GetGameProgress(difficultyArray[index]);
			if (gameProgress != null && gameProgress.difficulty == difficultyArray[index] && gameProgress.encores > num) {
				num = gameProgress.encores;
			}
			index++;
		}
		__result = num;
		return false;
	}
}


[HarmonyPatch(typeof(GameProgressSaver), nameof(GameProgressSaver.DifficultySavePath))]
public class DifficultySavePathPatch {
	public static void Postfix(int diff, ref object __result) {
		if (diff != 19) {
			return;
		}
		__result = Path.Combine(
			GameProgressSaver.BaseSavePath,
			string.Format("Slot{0}", GameProgressSaver.currentSlot + 1),
			/* "difficulty19",*/ "difficulty19progress.bepis"
		);
	}
}
[HarmonyPatch(typeof(GameProgressSaver), nameof(GameProgressSaver.GetDirectorySlotData))]
public class GetDirectorySlotDataPatch {
	public static bool Prefix(string path, ref object __result) {
		if (!Util.IsDifficulty(19)) {
			return true;
		}

		Debug.Log("Generating SlotData for " + path);
		int num = 0;
		int num2 = 0;
		GameProgressData gameProgressData = GameProgressSaver.ReadFile(Path.Combine(path, "difficulty19progress.bepis")) as GameProgressData;
		if (gameProgressData != null && (gameProgressData.levelNum > num || (gameProgressData.levelNum == num && gameProgressData.difficulty > num2))) {
			num = gameProgressData.levelNum;
			num2 = gameProgressData.difficulty;
		}
		__result = new SaveSlotMenu.SlotData {
			exists = true,
			highestDifficulty = num2,
			highestLvlNumber = num
		};
		return false;
	}
}


[HarmonyPatch(typeof(StatsManager), nameof(StatsManager.SendInfo))]
public class SendInfoPatch {
	public static void Postfix() {
		if (!Util.IsDifficulty(19)) {
			return;
		}
		Billion.Logger.LogWarning("Score submitted: " + LeaderboardController.CanSubmitScores.ToString());
	}
}
[HarmonyPatch(typeof(FinalCyberRank), nameof(FinalCyberRank.GameOver))]
public class CyberGameOverPatch {
	public static void Postfix() {
		if (!Util.IsDifficulty(19)) {
			return;
		}
		Billion.Logger.LogWarning("Score submitted: " + LeaderboardController.CanSubmitScores.ToString());
	}
}

[HarmonyPatch(typeof(LeaderboardController), "CanSubmitScores", MethodType.Getter)]
public class CanSubmitScoresPatch {
	public static bool Prefix(ref bool __result) {
		if (!Util.IsDifficulty(19)) {
			return true;
		}
		__result = false;
		return false;
	}
}

[HarmonyPatch(typeof(GameProgressSaver), nameof(GameProgressSaver.LevelProgressPath))]
public class LevelProgressPathPatch {
	public static bool Prefix(int lvl, ref string __result) {
		if (!Util.IsDifficulty(19)) {
			return true;
		}
		string lvl_19 = Path.Combine(GameProgressSaver.SavePath, string.Format("difficulty19lvl{0}progress.bepis", lvl));
		__result = lvl_19;
		return false;
	}
}

[HarmonyPatch(typeof(RankData), MethodType.Constructor, new Type[] { typeof(StatsManager) })]
public class RankDataConstructorPatch {
	public static bool Prefix(StatsManager sman, RankData __instance) {
		int @int = Util.GetDifficulty();
		__instance.levelNumber = sman.levelNumber;
		RankData rank = GameProgressSaver.GetRank(true, -1);
		if (rank != null) {
			__instance.ranks = rank.ranks;
			if (rank.majorAssists != null) {
				__instance.majorAssists = rank.majorAssists;
			} else {
				__instance.majorAssists = new bool[20];
			}
			if (rank.stats != null) {
				__instance.stats = rank.stats;
			} else {
				__instance.stats = new RankScoreData[20];
			}
			if ((sman.rankScore >= rank.ranks[@int] && (rank.majorAssists == null || (!sman.majorUsed && rank.majorAssists[@int]))) || sman.rankScore > rank.ranks[@int] || rank.levelNumber != __instance.levelNumber) {
				__instance.majorAssists[@int] = sman.majorUsed;
				__instance.ranks[@int] = sman.rankScore;
				if (__instance.stats[@int] == null) {
					__instance.stats[@int] = new RankScoreData();
				}
				__instance.stats[@int].kills = sman.kills;
				__instance.stats[@int].style = sman.stylePoints;
				__instance.stats[@int].time = sman.seconds;
			}
			__instance.secretsAmount = sman.secretObjects.Length;
			__instance.secretsFound = new bool[__instance.secretsAmount];
			int num = 0;
			while (num < __instance.secretsAmount && num < rank.secretsFound.Length) {
				if (sman.secretObjects[num] == null || rank.secretsFound[num]) {
					__instance.secretsFound[num] = true;
				}
				num++;
			}
			__instance.challenge = rank.challenge;
			return false;
		}
		__instance.ranks = new int[20];
		__instance.stats = new RankScoreData[20];
		if (__instance.stats[@int] == null) {
			__instance.stats[@int] = new RankScoreData();
		}
		__instance.majorAssists = new bool[20];
		for (int i = 0; i < __instance.ranks.Length; i++) {
			__instance.ranks[i] = -1;
		}
		__instance.ranks[@int] = sman.rankScore;
		__instance.majorAssists[@int] = sman.majorUsed;
		__instance.stats[@int].kills = sman.kills;
		__instance.stats[@int].style = sman.stylePoints;
		__instance.stats[@int].time = sman.seconds;
		__instance.secretsAmount = sman.secretObjects.Length;
		__instance.secretsFound = new bool[__instance.secretsAmount];
		for (int j = 0; j < __instance.secretsAmount; j++) {
			if (sman.secretObjects[j] == null) {
				__instance.secretsFound[j] = true;
			}
		}
		return false;
	}
}

[HarmonyPatch(typeof(GameProgressSaver), "GetRankData",
new Type[] {typeof(string), typeof(int), typeof(bool)},
new ArgumentType[] {ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal})]
public class GetRankDataPatch {
	public static bool Prefix(ref string path, int lvl, bool returnNull, ref RankData __result) {
		if (!Util.IsDifficulty(19)) {
			return true;
		}

		GameProgressSaver.PrepareFs();
		string currentLevelPath = GameProgressSaver.resolveCurrentLevelPath;
		string levelProgressPath = GameProgressSaver.LevelProgressPath(lvl);
		if (/*currentLevelPath == "" ||*/ levelProgressPath == "") {
			return false;
		}
		path = (lvl < 0) ? currentLevelPath : levelProgressPath;
		RankData rankData = GameProgressSaver.ReadFile(path) as RankData;
		if (rankData == null) {
			rankData = returnNull ? null : new RankData(StatsManager.Instance);
			if (rankData == null) {
				__result = null;
				return false;
			}
		}
		if (rankData.ranks.Length < 20) {
			Array.Resize(ref rankData.ranks, 20);
			rankData.ranks[19] = -1;
		}
		if (rankData.majorAssists.Length < 20) {
			Array.Resize(ref rankData.majorAssists, 20);
		}
		if (rankData.secretsFound.Length < 20) {
			Array.Resize(ref rankData.secretsFound, 20);
		}
		if (rankData.stats.Length < 20) {
			Array.Resize(ref rankData.stats, 20);
		}
		__result = rankData;
		return false;
	}
}

[HarmonyPatch(typeof(GameProgressSaver), "cyberGrindHighScorePath", MethodType.Getter)]
public class CGPathPatch {
	public static bool Prefix(ref string __result) {
		if (Util.GetDifficulty() != 19) {
			return true;
		}

		__result = Path.Combine(GameProgressSaver.SavePath, "cybergrindhighscore19.bepis");
		return false;
	}
}

[HarmonyPatch(typeof(GameProgressSaver), nameof(GameProgressSaver.GetCyberRankData))]
public class GetCyberRankDataPatch {
	public static bool Prefix(ref CyberRankData __result) {
		if (Util.GetDifficulty() != 19) {
			return true;
		}

		string cyberScorePath = GameProgressSaver.cyberGrindHighScorePath;
		//string cyberScorePath = Path.Combine(GameProgressSaver.SavePath, "cybergrindhighscore19.bepis");
		CyberRankData cyberRankData = GameProgressSaver.ReadFile(cyberScorePath) as CyberRankData;
		if (cyberRankData == null) {
			cyberRankData = new CyberRankData();
		}
		if (cyberRankData.preciseWavesByDifficulty == null) {
			cyberRankData.preciseWavesByDifficulty = new float[20];
		} else if (cyberRankData.preciseWavesByDifficulty.Length < 20) {
			Array.Resize(ref cyberRankData.preciseWavesByDifficulty, 20);
		}
		if (cyberRankData.style == null) {
			cyberRankData.style = new int[20];
		} else if (cyberRankData.style.Length < 20) {
			Array.Resize(ref cyberRankData.style, 20);
		}
		if (cyberRankData.kills == null) {
			cyberRankData.kills = new int[20];
		} else if (cyberRankData.kills.Length < 20) {
			Array.Resize(ref cyberRankData.kills, 20);
		}
		if (cyberRankData.time == null) {
			cyberRankData.time = new float[20];
		} else if (cyberRankData.time.Length < 20) {
			Array.Resize(ref cyberRankData.time, 20);
		}
		__result = cyberRankData;
		return false;
	}
}

// doesn't work
[HarmonyPatch(typeof(PresenceController), nameof(PresenceController.Start))]
public class PresenceControllerPatch {
	public static void Prefix(PresenceController __instance) {
		if (__instance.diffNames.Length < 20) {
			Array.Resize(ref __instance.diffNames, 20);
			__instance.diffNames[19] = "BILLION";
		}
	}
}


[HarmonyPatch(typeof(LevelSelectPanel), nameof(LevelSelectPanel.CheckScore))]
public class CheckScorePatch {
	public static bool Prefix(LevelSelectPanel __instance) {
		if (!Util.IsDifficulty(19)) {
			return true;
		}

		__instance.Setup();
		__instance.rectTransform = __instance.GetComponent<RectTransform>();
		if (__instance.levelNumber == 666) {
			__instance.tempInt = GameProgressSaver.GetPrime(Util.GetDifficulty(), __instance.levelNumberInLayer);
		} else if (__instance.levelNumber == 100) {
			__instance.tempInt = GameProgressSaver.GetEncoreProgress(Util.GetDifficulty());
		} else {
			__instance.tempInt = GameProgressSaver.GetProgress(Util.GetDifficulty());
		}
		int num = __instance.levelNumber;
		if (__instance.levelNumber == 666 || __instance.levelNumber == 100) {
			num += __instance.levelNumberInLayer - 1;
		}
		__instance.origName = GetMissionName.GetMission(num);
		if ((__instance.levelNumber == 666 && __instance.tempInt == 0) || (__instance.levelNumber == 100 && __instance.tempInt < __instance.levelNumberInLayer - 1) || (__instance.levelNumber != 666 && __instance.levelNumber != 100 && __instance.tempInt < __instance.levelNumber) || __instance.forceOff) {
			string str = __instance.ls.layerNumber.ToString();
			if (__instance.ls.layerNumber == 666) {
				str = "P";
			}
			if (__instance.ls.layerNumber == 100) {
				__instance.transform.Find("Name").GetComponent<TMP_Text>().text = (__instance.levelNumberInLayer - 1).ToString() + "-E: ???";
			} else {
				__instance.transform.Find("Name").GetComponent<TMP_Text>().text = str + "-" + __instance.levelNumberInLayer.ToString() + ": ???";
			}
			__instance.transform.Find("Image").GetComponent<Image>().sprite = __instance.lockedSprite;
			__instance.GetComponent<Button>().enabled = false;
			__instance.rectTransform.sizeDelta = new Vector2(__instance.rectTransform.sizeDelta.x, __instance.collapsedHeight);
			__instance.leaderboardPanel.SetActive(false);
		} else {
			bool flag;
			if (__instance.tempInt == __instance.levelNumber || (__instance.levelNumber == 100 && __instance.tempInt == __instance.levelNumberInLayer - 1) || (__instance.levelNumber == 666 && __instance.tempInt == 1)) {
				flag = false;
				__instance.transform.Find("Image").GetComponent<Image>().sprite = __instance.unlockedSprite;
				__instance.transform.Find("Name").GetComponent<TMP_Text>().text = (__instance.levelNumberInLayer - 1).ToString() + "-E: ???";
			} else {
				flag = true;
				__instance.transform.Find("Image").GetComponent<Image>().sprite = __instance.origSprite;
			}
			if (__instance.levelNumber != 100 || __instance.tempInt != __instance.levelNumberInLayer - 1) {
				__instance.transform.Find("Name").GetComponent<TMP_Text>().text = __instance.origName;
			}
			__instance.GetComponent<Button>().enabled = true;
			if (__instance.challengeIcon != null) {
				if (__instance.challengeChecker == null) {
					__instance.challengeChecker = __instance.challengeIcon.transform.Find("EventTrigger").gameObject;
				}
				if (__instance.tempInt > __instance.levelNumber) {
					__instance.challengeChecker.SetActive(true);
				}
			}
			if (LeaderboardController.ShowLevelLeaderboards && flag) {
				__instance.rectTransform.sizeDelta = new Vector2(__instance.rectTransform.sizeDelta.x, __instance.expandedHeight);
				__instance.leaderboardPanel.SetActive(true);
			} else {
				__instance.rectTransform.sizeDelta = new Vector2(__instance.rectTransform.sizeDelta.x, __instance.collapsedHeight);
				__instance.leaderboardPanel.SetActive(false);
			}
		}
		
		RankData rank = GameProgressSaver.GetRank(num, false);
		if (rank == null) {
			Debug.Log("Didn't Find Level " + __instance.levelNumber.ToString() + " Data");
			Image component = __instance.transform.Find("Stats").Find("Rank").GetComponent<Image>();
			component.color = Color.white;
			component.sprite = __instance.unfilledPanel;
			component.GetComponentInChildren<TMP_Text>().text = "";
			__instance.allSecrets = false;
			foreach (Image image in __instance.secretIcons) {
				image.enabled = true;
				image.sprite = __instance.unfilledPanel;
			}
			return false;
		}
		int @int = Util.GetDifficulty();
		if (rank.levelNumber == __instance.levelNumber || ((__instance.levelNumber == 666 || __instance.levelNumber == 100) && rank.levelNumber == __instance.levelNumber + __instance.levelNumberInLayer - 1)) {
			TMP_Text componentInChildren = __instance.transform.Find("Stats").Find("Rank").GetComponentInChildren<TMP_Text>();
			if (rank.ranks[@int] == 12 && (rank.majorAssists == null || !rank.majorAssists[@int])) {
				componentInChildren.text = "<color=#FFFFFF>P</color>";
				Image component2 = componentInChildren.transform.parent.GetComponent<Image>();
				component2.color = new Color(1f, 0.686f, 0f, 1f);
				component2.sprite = __instance.filledPanel;
				__instance.ls.AddScore(4, true);
			} else if (rank.majorAssists != null && rank.majorAssists[@int]) {
				if (rank.ranks[@int] < 0) {
					componentInChildren.text = "";
				} else {
					switch (rank.ranks[@int]) {
						case 1:
							componentInChildren.text = "C";
							__instance.ls.AddScore(1, false);
							break;
						case 2:
							componentInChildren.text = "B";
							__instance.ls.AddScore(2, false);
							break;
						case 3:
							componentInChildren.text = "A";
							__instance.ls.AddScore(3, false);
							break;
						case 4:
						case 5:
						case 6:
							__instance.ls.AddScore(4, false);
							componentInChildren.text = "S";
							break;
						default:
							__instance.ls.AddScore(0, false);
							componentInChildren.text = "D";
							break;
					}
					Image component3 = componentInChildren.transform.parent.GetComponent<Image>();
					component3.color = new Color(0.3f, 0.6f, 0.9f, 1f);
					component3.sprite = __instance.filledPanel;
				}
			} else if (rank.ranks[@int] < 0) {
				componentInChildren.text = "";
				Image component4 = componentInChildren.transform.parent.GetComponent<Image>();
				component4.color = Color.white;
				component4.sprite = __instance.unfilledPanel;
			} else {
				switch (rank.ranks[@int]) {
					case 1:
						componentInChildren.text = "<color=#4CFF00>C</color>";
						__instance.ls.AddScore(1, false);
						break;
					case 2:
						componentInChildren.text = "<color=#FFD800>B</color>";
						__instance.ls.AddScore(2, false);
						break;
					case 3:
						componentInChildren.text = "<color=#FF6A00>A</color>";
						__instance.ls.AddScore(3, false);
						break;
					case 4:
					case 5:
					case 6:
						__instance.ls.AddScore(4, false);
						componentInChildren.text = "<color=#FF0000>S</color>";
						break;
					default:
						__instance.ls.AddScore(0, false);
						componentInChildren.text = "<color=#0094FF>D</color>";
						break;
				}
				Image component5 = componentInChildren.transform.parent.GetComponent<Image>();
				component5.color = Color.white;
				component5.sprite = __instance.unfilledPanel;
			}
			if (rank.secretsAmount > 0) {
				__instance.allSecrets = true;
				for (int j = 0; j < 5; j++) {
					if (j < rank.secretsAmount && rank.secretsFound[j]) {
						__instance.secretIcons[j].sprite = __instance.filledPanel;
					} else {
						__instance.allSecrets = false;
						__instance.secretIcons[j].sprite = __instance.unfilledPanel;
					}
				}
			} else {
				Image[] array = __instance.secretIcons;
				for (int i = 0; i < array.Length; i++) {
					array[i].enabled = false;
				}
			}
			if (__instance.challengeIcon) {
				if (rank.challenge) {
					__instance.challengeIcon.sprite = __instance.filledPanel;
					TMP_Text componentInChildren2 = __instance.challengeIcon.GetComponentInChildren<TMP_Text>();
					componentInChildren2.text = "C O M P L E T E";
					if (rank.ranks[@int] == 12 && (__instance.allSecrets || rank.secretsAmount == 0)) {
						componentInChildren2.color = new Color(0.6f, 0.4f, 0f, 1f);
					} else {
						componentInChildren2.color = Color.black;
					}
				} else {
					__instance.challengeIcon.sprite = __instance.unfilledPanel;
					TMP_Text componentInChildren3 = __instance.challengeIcon.GetComponentInChildren<TMP_Text>();
					componentInChildren3.text = "C H A L L E N G E";
					componentInChildren3.color = Color.white;
				}
			}
		} else {
			Debug.Log("Error in finding " + __instance.levelNumber.ToString() + " Data");
			Image component6 = __instance.transform.Find("Stats").Find("Rank").GetComponent<Image>();
			component6.color = Color.white;
			component6.sprite = __instance.unfilledPanel;
			component6.GetComponentInChildren<TMP_Text>().text = "";
			__instance.allSecrets = false;
			foreach (Image image2 in __instance.secretIcons)
			{
				image2.enabled = true;
				image2.sprite = __instance.unfilledPanel;
			}
		}
		if ((rank.challenge || !__instance.challengeIcon) && rank.ranks[@int] == 12 && (__instance.allSecrets || rank.secretsAmount == 0)) {
			__instance.ls.Gold();
			__instance.GetComponent<Image>().color = new Color(1f, 0.686f, 0f, 0.75f);
			return false;
		}
		__instance.GetComponent<Image>().color = __instance.defaultColor;
		return false;
	}
}

// STEAM ACTIVITY
[HarmonyPatch(typeof(SteamController), nameof(SteamController.FetchSceneActivity))]
public class SteamControllerPatch {
	public static bool Prefix(ref string scene, ref SteamController __instance) {
		if (!Util.IsDifficulty(19)) {
			return true;
		}

		if (!SteamClient.IsValid) {
			return false;
		}
		if (SceneHelper.IsPlayingCustom) {
			SteamFriends.SetRichPresence("steam_display", "#AtCustomLevel");
			return false;
		}
		StockMapInfo instance = StockMapInfo.Instance;
		if (scene == "Main Menu") {
			SteamFriends.SetRichPresence("steam_display", "#AtMainMenu");
			return false;
		}
		if (scene == "Endless") {
			SteamFriends.SetRichPresence("steam_display", "#AtCyberGrind");
			SteamFriends.SetRichPresence("difficulty", "BILLION");
			SteamFriends.SetRichPresence("wave", "0");
			return false;
		}
		if (instance != null && !string.IsNullOrEmpty(instance.assets.Deserialize().LargeText)) {
			SteamFriends.SetRichPresence("steam_display", "#AtStandardLevel");
			var dn = PresenceController.Instance.diffNames[Util.GetDifficulty()];
			SteamFriends.SetRichPresence("difficulty", dn);
			SteamFriends.SetRichPresence("level", instance.assets.Deserialize().LargeText);
			return false;
		}
		SteamFriends.SetRichPresence("steam_display", "#UnknownLevel");
		return false;
	}
}