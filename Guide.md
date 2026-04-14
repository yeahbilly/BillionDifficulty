# A rough explanation of how this mod works
Someone asked for it and I thought why not

**Note: this is just a structural guide and not a guide on how to write/organize code. The mod may work but it's very fragile and would need to get fixed after any major game updates**

## For reference
- [BepInEx guide](https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial) (modding framework)
- [Harmony guide](https://harmony.pardeike.net/articles/intro.html) (used for patching methods)
- [HarmonyX wiki](https://github.com/BepInEx/HarmonyX/wiki) (the BepInEx version of Harmony I think)


# Patches
Found in `/Scripts/SetupPatches.cs` and `/Scripts/SavePatches.cs`

Used to make the game recognize the custom difficulty. There will be a short explanation for each patch in the order they appear in the files, so you can open that alongside this guide and see what's actually being described. The explanations might sometimes be vague because this stuff is confusing and it's been a while since I touched those patches

About difficulty id's: every difficulty needs its own id which must be a positive integer, preferably not too big so that you won't need to have arrays with hundreds of useless elements when each element is supposed to represent its own difficulty. You should also avoid using the same id as the vanilla ones (`0`, `1`, `2`, `3`, `4` and `5`) or other existing custom difficulties

This mod uses the difficulty id `19` so you can find it everywhere in the source code

## _Basic functionality_ (`SetupPatches.cs`)
### `PrefsManager.EnsureValid`
Makes the game treat `19` as a valid difficulty

### `DifficultyTitle.Check`
Updates the title in the chapter select menu and level end screen

### `LeaderboardController.CanSubmitScores`
Stops the game from submitting any scores achieved on this difficulty

### ~~`StatsManager.SendInfo`~~ / ~~`FinalCyberRank.GameOver`~~
(Not required) Used to check that leaderboard scores aren't being submitted

### `PresenceController.Start`
Defines the difficulty name for Discord and Steam rich presence

## _Saves_ (`SavePatches.cs`)
### `GameProgressSaver.GetProgress` / `.GetPrime` / `.GetEncoreProgress`
Processes the custom difficulty's save data

### `GameProgressSaver.DifficultySavePath`
Defines the path for a separate save file unique to this difficulty

### `GameProgressSaver.LevelProgressPath`
Defines the path for each level's save files on this difficulty

### `GameProgressSaver.cyberGrindHighScorePath`
Defines the path for the cyber grind's save file on this difficulty

### `GameProgressSaver.GetDirectorySlotData`
Fetches the save data from the unique save file

### `RankData constructor`
Creates more saved data to accommodate for the new difficulty or something like that

### `GameProgressSaver.GetRankData`
I think this increases the amount of data for existing saves while the one above is for newly made ones

### `GameProgressSaver.GetCyberRankData`
Same as above but for cg data


# Enemy patches
Found in `/Scripts/EnemyPatches.cs` and `/Scripts/BossPatches.cs`

All enemy patches do either one or both of these 2 things:
- Modifying variables such as speed or projectile data
- Setting up custom behaviours that are defined in any of the files in /Scripts/Classes/ (used for logic that isn't already in the game)

Neither of these things have anything special about them that I can think of off the top of my head, so you should be able to figure out how to do it yourself assuming you can understand the basics of Unity and stuff like that
