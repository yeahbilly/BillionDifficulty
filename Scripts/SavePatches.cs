using System;
using System.IO;
using HarmonyLib;
using UnityEngine;


namespace BillionDifficulty.SetupPatches;

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
		if (diff != 19)
			return;

		__result = Path.Combine(
			GameProgressSaver.BaseSavePath,
			string.Format("Slot{0}", GameProgressSaver.currentSlot + 1),
			/* "difficulty19",*/ "difficulty19progress.bepis"
		);
	}
}

[HarmonyPatch(typeof(GameProgressSaver), nameof(GameProgressSaver.LevelProgressPath))]
public class LevelProgressPathPatch {
	public static void Postfix(int lvl, ref string __result) {
		if (!Util.IsDifficulty(19))
			return;

		// string lvl_19 = Path.Combine(GameProgressSaver.SavePath, string.Format("difficulty19lvl{0}progress.bepis", lvl));
		// __result = lvl_19;
		// return false;

		string[] parts = Path.GetFileName(__result).Split(".");
		if (parts.Length == 0)
			return;


		if (!__result.Contains("difficulty19")) {
			parts[0] = "difficulty19" + parts[0];

			__result = Path.Join(
				Path.GetDirectoryName(__result),
				string.Join(".", parts)
			);
		}
	}
}

[HarmonyPatch(typeof(GameProgressSaver), nameof(GameProgressSaver.cyberGrindHighScorePath), MethodType.Getter)]
public class CGPathPatch {
	public static bool Prefix(ref string __result) {
		if (Util.GetDifficulty() != 19)
			return true;

		__result = Path.Combine(GameProgressSaver.SavePath, "cybergrindhighscore19.bepis");
		return false;
	}
}

[HarmonyPatch(typeof(GameProgressSaver), nameof(GameProgressSaver.GetDirectorySlotData))]
public class GetDirectorySlotDataPatch {
	public static bool Prefix(string path, ref object __result) {
		if (!Util.IsDifficulty(19))
			return true;

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

[HarmonyPatch(typeof(GameProgressSaver), nameof(GameProgressSaver.GetRankData),
new Type[] {typeof(string), typeof(int), typeof(bool)},
new ArgumentType[] {ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal})]
public class GetRankDataPatch {
	public static bool Prefix(ref string path, int lvl, bool returnNull, ref RankData __result) {
		if (!Util.IsDifficulty(19))
			return true;

		GameProgressSaver.PrepareFs();
		string currentLevelPath = GameProgressSaver.resolveCurrentLevelPath;
		string levelProgressPath = GameProgressSaver.LevelProgressPath(lvl);
		if (/*currentLevelPath == "" ||*/ levelProgressPath == "")
			return false;

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

[HarmonyPatch(typeof(GameProgressSaver), nameof(GameProgressSaver.GetCyberRankData))]
public class GetCyberRankDataPatch {
	public static bool Prefix(ref CyberRankData __result) {
		if (Util.GetDifficulty() != 19)
			return true;

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
