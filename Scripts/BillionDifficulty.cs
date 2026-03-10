using Billion = BillionDifficulty.Plugin;

using BepInEx;
using BepInEx.Logging;
using System.Linq;
using System.IO;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;


namespace BillionDifficulty;

public class Util {
	public static bool IsDifficulty(int difficulty) {
		return PrefsManager.Instance.GetInt("difficulty") == difficulty;
	}

	public static int GetDifficulty() {
		return PrefsManager.Instance.GetInt("difficulty");
	}

	public static Texture2D LoadEmbeddedTexture(string resourceName) {
		Assembly assembly = Assembly.GetExecutingAssembly();
		using Stream stream = assembly.GetManifestResourceStream(resourceName);
		if (stream == null) {
			Billion.Logger.LogError($"Embedded resource '{resourceName}' not found");
			return null;
		}

		byte[] buffer = new byte[stream.Length];
		stream.Read(buffer, 0, buffer.Length);

		Texture2D tex = new Texture2D(2, 2);
		if (tex.LoadImage(buffer)) {
			return tex;
		} else {
			Billion.Logger.LogError("Failed to load embedded texture");
			return null;
		}
	}
}

//[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInPlugin("billy.billiondifficulty", "Billion Difficulty", "1.6.0")]
[BepInProcess("ULTRAKILL.exe")]
public class Plugin : BaseUnityPlugin {
	private static readonly Harmony Harmony = new Harmony("billy.billiondifficulty");
	internal static new ManualLogSource Logger;
	public static bool loadedAssets = false;

	public void Awake() {
		Logger = base.Logger;
		Harmony.PatchAll();
		Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded! WOAHHHH");
		SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
		GetAssets();
	}

	public void SceneManager_activeSceneChanged(Scene arg0, Scene arg1) {
		// main menu?
		string targetSceneName = "b3e7f2f8052488a45b35549efb98d902";
		string activeSceneName = SceneManager.GetActiveScene().name;
		if (activeSceneName == targetSceneName) {
			AddDifficultyButton();
		}
	}

	public static Texture2D blueFilthTexture = Util.LoadEmbeddedTexture("BillionDifficulty.assets.zombie_MeleeHusk_MouthOpen_Diffuse.png");
	public static Texture2D blueFilthBiteTexture = Util.LoadEmbeddedTexture("BillionDifficulty.assets.zombie_MeleeHusk_MouthClosed_Diffuse.png");

	public T Ass<T>(string path) {
		return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
	}

	// ADDRESSABLES
	public static GameObject ProjectileExplosiveHH;
	public static GameObject ProjectileHoming;
	public static GameObject Projectile;
	public static GameObject GasolineProjectile;
	public static GameObject ExplosionSuper;
	public static GameObject Explosion;
	public static GameObject PumpChargeSound;
	public void GetAssets() {
		if (loadedAssets) {
			return;
		}
		ProjectileExplosiveHH = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Explosive HH.prefab");
		ProjectileHoming = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Homing.prefab");
		Projectile = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile.prefab");
		GasolineProjectile = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/GasolineProjectile.prefab");
		ExplosionSuper = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Super.prefab");
		Explosion = Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion.prefab");
		PumpChargeSound = Ass<GameObject>("Assets/Particles/SoundBubbles/PumpChargeSound.prefab");
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
}
