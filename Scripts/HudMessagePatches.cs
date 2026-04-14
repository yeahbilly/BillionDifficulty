using HarmonyLib;
using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

// !!! HUD MESSAGES !!!
[HarmonyPatch(typeof(EnemyIdentifier))]
public class EnemyIdentifierHudMessagePatch {
	// message setup
	[HarmonyPostfix]
	[HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.Start))]
	public static void StartPostfix(EnemyIdentifier __instance) {
		if (__instance.difficulty != 19)
			return;

		
		GameObject customMessages = GameObject.Find("BillionCustomHudMessages");
		if (customMessages == null) {
			customMessages = new GameObject("BillionCustomHudMessages");
		}

		switch (__instance.enemyType) {
			case EnemyType.HideousMass:
				Transform mass = customMessages.transform.Find("Mass");
				if (mass == null) {
					mass = new GameObject("Mass").transform;
				}
				mass.SetParent(customMessages.transform);
				if (mass.GetComponent<BoolValue>() != null)
					break;
				BoolValue bvMass = mass.gameObject.AddComponent<BoolValue>();
				bvMass.description = "shownMessage";
				break;
			case EnemyType.Stalker:
				Transform stalker = customMessages.transform.Find("Stalker");
				if (stalker == null) {
					stalker = new GameObject("Stalker").transform;
				}
				stalker.SetParent(customMessages.transform);
				if (stalker.GetComponent<BoolValue>() != null)
					break;
				BoolValue bvStalker = stalker.gameObject.AddComponent<BoolValue>();
				bvStalker.description = "shownMessage";
				break;
			case EnemyType.Idol:
				Transform idol = customMessages.transform.Find("Idol");
				if (idol == null) {
					idol = new GameObject("Idol").transform;
				}
				idol.SetParent(customMessages.transform);
				if (idol.GetComponent<BoolValue>() != null)
					break;
				BoolValue bvIdol = idol.gameObject.AddComponent<BoolValue>();
				bvIdol.description = "shownMessage";
				break;
			case EnemyType.Ferryman:
				Transform ferryman = customMessages.transform.Find("Ferryman");
				if (ferryman == null) {
					ferryman = new GameObject("Ferryman").transform;
				}
				ferryman.SetParent(customMessages.transform);
				if (ferryman.GetComponent<BoolValue>() != null)
					break;
				BoolValue bvFerryman = ferryman.gameObject.AddComponent<BoolValue>();
				bvFerryman.description = "shownMessage";
				break;
			// case EnemyType.Sisyphus:
			// 	Transform sisyphus = customMessages.transform.Find("Sisyphus");
			// 	if (sisyphus == null) {
			// 		sisyphus = new GameObject("Sisyphus").transform;
			// 	}
			// 	sisyphus.SetParent(customMessages.transform);
			// 	if (sisyphus.GetComponent<BoolValue>() != null)
			// 		break;
			// 	BoolValue bvSisyphus = sisyphus.gameObject.AddComponent<BoolValue>();
			// 	bvSisyphus.description = "shownMessage";
			// 	break;
		}
	}

	// shows message
	[HarmonyPrefix]
	[HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.Update))]
	public static void UpdatePrefix(EnemyIdentifier __instance) {
		if (__instance.difficulty != 19 || __instance.dead)
			return;

		Transform customMessages = GameObject.Find("BillionCustomHudMessages").transform;
		if (__instance.enemyType == EnemyType.HideousMass && SceneHelper.CurrentScene == "Level 1-3" && __instance.transform.root.name == "Boss Arena") {
			GameObject mass = customMessages.Find("Mass").gameObject;
			if (BoolValue.Get("shownMessage", mass) != false)
				return;

			HudMessage message = mass.AddComponent<HudMessage>();
			message.message = "<color=#aaccff>(Billion)</color> HM's hook can be destroyed with 1 Knuckleblaster punch";
			message.timed = true;
			message.timerTime = 5f;
			message.PlayMessage();
			BoolValue.Set("shownMessage", true, mass);
		} else if (__instance.enemyType == EnemyType.Stalker && SceneHelper.CurrentScene == "Level 4-2" && __instance.transform.root.name == "5 - Temple Entrance") {
			GameObject stalker = customMessages.Find("Stalker").gameObject;
			if (BoolValue.Get("shownMessage", stalker) != false)
				return;

			HudMessage message = stalker.AddComponent<HudMessage>();
			message.message = "<color=#aaccff>(Billion)</color> Stalkers get overhealed when sanding enemies";
			message.timed = true;
			message.timerTime = 5f;
			message.PlayMessage();
			BoolValue.Set("shownMessage", true, stalker);
		} else if (__instance.enemyType == EnemyType.Idol && SceneHelper.CurrentScene == "Level 5-2" && __instance.transform.root.name == "3 - Ferryman's Cabin") {
			GameObject idol = customMessages.Find("Idol").gameObject;
			if (BoolValue.Get("shownMessage", idol) != false)
				return;

			HudMessage message = idol.AddComponent<HudMessage>();
			message.message = "<color=#aaccff>(Billion)</color> Idoled enemies overheal surrounding enemies";
			message.timed = true;
			message.timerTime = 7f;
			message.PlayMessage();
			BoolValue.Set("shownMessage", true, idol);
		} else if (__instance.enemyType == EnemyType.Ferryman && SceneHelper.CurrentScene == "Level 5-2" && __instance.transform.root.name == "8 - Ship" && __instance.GetComponent<FerrymanFake>() == null) {
			GameObject ferryman = customMessages.Find("Ferryman").gameObject;
			if (BoolValue.Get("shownMessage", ferryman) != false)
				return;

			HudMessage message = ferryman.AddComponent<HudMessage>();
			message.message = "<color=#aaccff>(Billion)</color> Ferrymen speed up over time";
			message.timed = true;
			message.timerTime = 5f;
			message.PlayMessage();
			BoolValue.Set("shownMessage", true, ferryman);
		}
	}
}

// insurrectionist message
[HarmonyPatch(typeof(ObjectActivationCheck))]
public class ObjectActivationCheckPatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(ObjectActivationCheck), nameof(ObjectActivationCheck.StateChange))]
	public static void StateChangePostfix(ObjectActivationCheck __instance) {
		if (!Util.IsDifficulty(19))
			return;

		if (__instance.readyToActivate && SceneHelper.CurrentScene == "Level 4-2" && __instance.transform.root.name == "7 - Boss Arena") {
			HudMessage message = __instance.gameObject.AddComponent<HudMessage>();
			message.message = "<color=#aaccff>(Billion)</color> Dealing 30 damage to an Insurrectionist in 5 seconds makes them explode and temporarily enrage";
			message.timed = true;
			message.timerTime = 7f;
			message.PlayMessage();
		}
	}
}