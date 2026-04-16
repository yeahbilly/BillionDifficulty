using BepInEx;
using BepInEx.Logging;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;
using Configgy;
using System;
using BillionDifficulty.EnemyPatches;


namespace BillionDifficulty;

public class Util {
	public static bool IsDifficulty(int difficulty) {
		return PrefsManager.Instance.GetInt("difficulty") == difficulty;
	}

	public static int GetDifficulty() {
		return PrefsManager.Instance.GetInt("difficulty");
	}

	public static bool IsHardMode() {
		return Plugin.IsBrilliantBillion.Value && IsDifficulty(19);
	}

	public static T[] AddToArray<T>(ref T[] array, T element, int index = -1) {
		if (index == -1) {
			index = array.Length;
		}
		
		List<T> list = array.ToList();
		list.Insert(index, element);
		array = list.ToArray();
		return array;
	}

	public static T[] AddToArray<T>(ref T[] array, T[] elements, int index = -1) {
		if (index == -1) {
			index = array.Length;
		}
		int elementIndex = 0;
		int insertIndex = index;
		List<T> list = array.ToList();
		while (elementIndex < elements.Length) {
			list.Insert(insertIndex, elements[elementIndex]);
			elementIndex++;
			insertIndex++;
		}
		array = list.ToArray();
		return array;
	}

	public static Texture2D LoadEmbeddedTexture(string resourceName) {
		Assembly assembly = Assembly.GetExecutingAssembly();
		using Stream stream = assembly.GetManifestResourceStream(resourceName);
		if (stream == null) {
			Plugin.Logger.LogError($"Embedded resource '{resourceName}' not found");
			return null;
		}

		byte[] buffer = new byte[stream.Length];
		stream.Read(buffer, 0, buffer.Length);
		Texture2D tex = new Texture2D(2, 2);
		if (tex.LoadImage(buffer)) {
			return tex;
		} else {
			Plugin.Logger.LogError("Failed to load embedded texture");
			return null;
		}
	}
}


//[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInPlugin("billy.billiondifficulty", "Billion Difficulty", MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("Hydraxous.ULTRAKILL.Configgy")]
[BepInProcess("ULTRAKILL.exe")]
public class Plugin : BaseUnityPlugin {
	private ConfigBuilder config;

	[
		Configgable(
			path: "",
			displayName: "Brilliant Billion mode",
			orderInList: 0,
			description: "A very difficult mode intended to make the main campaign a lot harder (doesn't properly update enemies that are already spawned)"
		)
	]
	public static ConfigToggle IsBrilliantBillion = new ConfigToggle(false);
	public static bool StayedOnHardMode = false;
	public const int BlueFilthRarity = 250;

	private static readonly Harmony Harmony = new Harmony("billy.billiondifficulty");
	internal static new ManualLogSource Logger;
	private static bool loadedAssets = false;

	public void Awake() {
		Logger = base.Logger;
		Harmony.PatchAll();
		Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded! WOAHHHH");
		SceneManager.activeSceneChanged += SceneManagerActiveSceneChanged;
		GetAssets();

		config = new ConfigBuilder("billy.billiondifficulty", "Billion Difficulty");
		IsBrilliantBillion.OnValueChanged += HardModeValueChanged;
		config.BuildAll();
	}

	public void SceneManagerActiveSceneChanged(Scene arg0, Scene arg1) {
		// main menu?
		// string targetSceneName = "b3e7f2f8052488a45b35549efb98d902";
		// string activeSceneName = SceneManager.GetActiveScene().name;
		if (SceneHelper.CurrentScene == "Main Menu") {
			AddDifficultyButton();
		}

		Plugin.StayedOnHardMode = Util.IsHardMode();
	}

	public static void HardModeValueChanged(bool v) {
		string newName = v ? "BRILLIANT BILLION" : "BILLION";
		Plugin.StayedOnHardMode = false;
		if (SceneHelper.CurrentScene != "Main Menu") {
			newName = "BILLION";
		}
		
		foreach (DifficultyTitle title in UnityObject.FindObjectsByType<DifficultyTitle>(FindObjectsSortMode.None))
			title.Check();

		PresenceController presence = UnityObject.FindAnyObjectByType<PresenceController>();
		if (presence != null && presence.diffNames.Length >= 20) {
			if (newName == presence.diffNames[19])
				return;

			presence.diffNames[19] = newName;
			DiscordController discord = UnityObject.FindAnyObjectByType<DiscordController>();
			discord?.FetchSceneActivity(SceneHelper.CurrentScene);
			SteamController steam = UnityObject.FindAnyObjectByType<SteamController>();
			steam?.FetchSceneActivity(SceneHelper.CurrentScene);
		}
	}

	public static Texture2D blueFilthTexture = Util.LoadEmbeddedTexture("BillionDifficulty.assets.zombie_MeleeHusk_MouthOpen_Diffuse.png");
	public static Texture2D blueFilthBiteTexture = Util.LoadEmbeddedTexture("BillionDifficulty.assets.zombie_MeleeHusk_MouthClosed_Diffuse.png");

	public static T Ass<T>(string path) {
		return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
	}

	public static Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject> {
		["ProjectileExplosiveHH"] = null,
		["ProjectileHoming"] = null,
		["Projectile"] = null,
		["GasolineProjectile"] = null,
		["ExplosionSuper"] = null,
		["Explosion"] = null,
		["PumpChargeSound"] = null,
		
		// BB
		["MirrorReaperGroundWave"] = null,
		["ProjectileHomingAcid"] = null,
		["ProjectileHomingExplosive"] = null,
		["ProjectileMinosPrimeSnake"] = null,
		["ExplosionSisyphusPrimeCharged"] = null,
		["BlackHoleEnemy"] = null,
		["VirtueInsignia"] = null,
		["GeryonForwardArrowBeam"] = null,
		["PhysicalShockwave"] = null,
		["GoopLarge"] = null,
		["GoopLargeLong"] = null,
	};

	public static AudioClip PumpCharge = null;

	public static void GetAssets() {
		if (loadedAssets)
			return;
		
		Prefabs["ProjectileExplosiveHH"] = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Explosive HH.prefab");
		Prefabs["ProjectileHoming"] = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Homing.prefab");
		Prefabs["Projectile"] = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile.prefab");
		Prefabs["GasolineProjectile"] = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/GasolineProjectile.prefab");
		Prefabs["ExplosionSuper"] = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Super.prefab");
		Prefabs["Explosion"] = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion.prefab");
		Prefabs["PumpChargeSound"] = Ass<GameObject>("Assets/Particles/SoundBubbles/PumpChargeSound.prefab");
		PumpCharge = Ass<AudioClip>("Assets/Sounds/Weapons/pumpCharge.wav");

		// BB
		Prefabs["MirrorReaperGroundWave"] = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/MirrorReaperGroundWave.prefab");
		Prefabs["ProjectileHomingAcid"] = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Homing Acid.prefab");
		Prefabs["ProjectileHomingAcid"].transform.Find("GoopCloud").gameObject.AddComponent<RemoveOnRespawn>();
		Prefabs["ProjectileHomingAcid"].transform.Find("GoopCloud").GetComponent<RemoveOnTime>().time = 10f;
		Prefabs["ProjectileHomingExplosive"] = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Homing Explosive.prefab");
		Prefabs["ProjectileMinosPrimeSnake"] = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Minos Prime Snake.prefab");
		Prefabs["ExplosionSisyphusPrimeCharged"] = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Sisyphus Prime Charged.prefab");
		Prefabs["BlackHoleEnemy"] = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Black Hole Enemy.prefab");
		Prefabs["VirtueInsignia"] = Ass<GameObject>("Virtue Insignia");
		Prefabs["GeryonForwardArrowBeam"] = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/GeryonForwardArrowBeam.prefab");
		Prefabs["PhysicalShockwave"] = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/PhysicalShockwave.prefab");
		Prefabs["GoopLarge"] = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/GoopLarge.prefab");
		Prefabs["GoopLargeLong"] = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/GoopLargeLong.prefab");

		loadedAssets = true;
	}

	public Transform FindCanvas() {
		Scene activeScene = SceneManager.GetActiveScene();
		Transform canvas = (
			from obj in activeScene.GetRootGameObjects()
			where obj.name == "Canvas"
			select obj
		).First().transform;
		return canvas;
	}

	public void AddDifficultyButton() {
		// finds the stuff
		Transform canvas = FindCanvas();
		Transform interactables = canvas.Find("Difficulty Select (1)").Find("Interactables");

		// makes the button
		Transform difficultyButton = UnityObject.Instantiate<GameObject>(interactables.Find("Brutal").gameObject, interactables).transform;
		DifficultySelectButton difficultyButtonComp = difficultyButton.GetComponent<DifficultySelectButton>();
		difficultyButtonComp.difficulty = 19; // 1*10^9 is a Billion
		difficultyButton.position += new Vector3(700f, 82.5f);
		TextMeshProUGUI buttonTextComp = difficultyButton.Find("Name").GetComponent<TextMeshProUGUI>();
		((TMP_Text)buttonTextComp).text = "BILLION";
		((Graphic)buttonTextComp).color = Color.white;
		// enables the button
		Button difficultyButtonButtonComp = difficultyButtonComp.GetComponent<Button>();
		difficultyButtonButtonComp.interactable = true;

		// adds info
		Transform difficultyInfoOg = interactables.Find("Brutal Info");
		Transform difficultyInfo = UnityObject.Instantiate<GameObject>(difficultyInfoOg.gameObject, difficultyInfoOg.parent).transform;
		Transform difficultyInfoText = difficultyInfo.Find("Text");
		difficultyInfo.Find("Text").GetComponent<TMP_Text>().text = "<color=white>A step up from the brutal difficulty. Faster enemies, changed attacks. Designed to make the game harder while not being annoying.</color>\n\n<color=purple>Requires quick reflexes, knowledge of your arsenal and the enemies.</color>";

		TMP_Text difficultyInfoTitle = difficultyInfo.Find("Title (1)").GetComponent<TextMeshProUGUI>();
		difficultyInfoTitle.fontSize = 46f;
		difficultyInfoTitle.text = "--BILLION--";
		
		// clears existing Brutal triggers
		EventTrigger infoTextDisplayTrigger = difficultyButtonComp.GetComponent<EventTrigger>();
		infoTextDisplayTrigger.triggers.Clear();
		// adds enter trigger
		EventTrigger.Entry appearOnEnter = new EventTrigger.Entry();
		appearOnEnter.eventID = EventTriggerType.PointerEnter;
		appearOnEnter.callback.AddListener((BaseEventData eventData) => { difficultyInfo.gameObject.SetActive(true); });
		infoTextDisplayTrigger.triggers.Add(appearOnEnter);
		// adds exit trigger
		EventTrigger.Entry hideOnExit = new EventTrigger.Entry();
		hideOnExit.eventID = EventTriggerType.PointerExit;
		hideOnExit.callback.AddListener((BaseEventData eventData) => { difficultyInfo.gameObject.SetActive(false); });
		infoTextDisplayTrigger.triggers.Add(hideOnExit);
		// adds click trigger
		EventTrigger.Entry hideOnClick = new EventTrigger.Entry();
		hideOnClick.eventID = EventTriggerType.PointerClick;
		hideOnClick.callback.AddListener((BaseEventData eventData) => {
			difficultyInfo.gameObject.SetActive(false);
		});
		infoTextDisplayTrigger.triggers.Add(hideOnClick);
	}

	public static GameObject CreateVirtueInsignia(float scaleMult, float windUpSpeedMult, float explosionLength, Vector3 targetPosition, Vector3 lookAtPosition, EnemyTarget enemyTarget, Enemy enemy, float totalDamageMult, float lightIntensityMultiplier = 1f) {
		GameObject insignia = UnityObject.Instantiate<GameObject>(Plugin.Prefabs["VirtueInsignia"], targetPosition, Quaternion.identity);
		insignia.SetActive(false);
		insignia.transform.localScale *= scaleMult;
		VirtueInsignia insigniaComp = insignia.GetComponent<VirtueInsignia>();
		insigniaComp.noTracking = true;
		insigniaComp.target = enemyTarget;
		insigniaComp.parentEnemy = enemy;
		insigniaComp.hadParent = true;
		insigniaComp.windUpSpeedMultiplier = windUpSpeedMult;
		insigniaComp.explosionLength = explosionLength;
		insigniaComp.damage = Mathf.RoundToInt(insigniaComp.damage * totalDamageMult);

		foreach (Light light in insignia.GetComponentsInChildren<Light>(includeInactive: true)) {
			light.intensity *= lightIntensityMultiplier;
		}

		// Vector3 direction = insignia.transform.position - targetPosition;
		// insignia.transform.rotation = Quaternion.LookRotation(direction, insignia.transform.right);
		insignia.transform.LookAt(lookAtPosition);
		insignia.transform.Rotate(-90f, 0f, 180f);
		insignia.SetActive(true);

		return insignia;
	}

	public static void SetEnemyWeakness(string name, float value, EnemyIdentifier eid) {
		int index = Array.IndexOf(eid.weaknesses, name);
		if (index != -1)
			eid.weaknessMultipliers[index] = value;
	}
}
