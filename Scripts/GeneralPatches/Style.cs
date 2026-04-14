using HarmonyLib;


namespace BillionDifficulty.GenaralPatches;

[HarmonyPatch(typeof(StyleHUD))]
public class StyleHUDPatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(StyleHUD), nameof(StyleHUD.Start))]
	public static void StartPostfix(StyleHUD __instance) {
		if (!__instance.idNameDict.ContainsKey("billion.blue")) {
			__instance.idNameDict.Add("billion.blue", "<color=#00ffffff>I'M BLUE</color>");
		}
	}
}