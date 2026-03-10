using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using ULTRAKILL.Cheats;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: STALKER !!!
// STALKER PATCHES (makes the sand zone heal the stalker)
[HarmonyPatch(typeof(Stalker))]
public class StalkerPatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Stalker), nameof(Stalker.SetSpeed))]
	public static void SetSpeedPostfix(Stalker __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		__instance.anim.speed *= 1.25f; // Brutal: 1
		__instance.explodeSpeed = 1.25f; // Brutal: 1
		__instance.anim.SetFloat("ExplodeSpeed", __instance.explodeSpeed); // Brutal: 1
		__instance.nma.speed *= 1.25f; // Brutal: * 1
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Stalker), nameof(Stalker.SandExplode))]
	public static bool SandExplodePrefix(int onDeath, Stalker __instance) {
		if (__instance.exploded) {
			return false;
		}
		GameObject sandZone = UnityObject.Instantiate<GameObject>(__instance.explosion.ToAsset(), __instance.transform.position + Vector3.up * 2.5f, Quaternion.identity);

		if (__instance.difficulty == 19) {
			// heals itself
			StalkerHealBack healBack = __instance.gameObject.AddComponent<StalkerHealBack>();
			healBack.healed = true;

			StalkerHealBackMessenger messenger = sandZone.transform.Find("GameObject").gameObject.AddComponent<StalkerHealBackMessenger>();
			messenger.source = __instance.mach;
		}

		if (onDeath != 1) {
			sandZone.transform.localScale *= 1.5f;
		}
		if (__instance.eid.stuckMagnets.Count > 0) {
			float num = 0.75f;
			if (__instance.eid.stuckMagnets.Count > 1) {
				num -= 0.125f * (float)(__instance.eid.stuckMagnets.Count - 1);
			}
			sandZone.transform.localScale *= num;
		}
		if (__instance.eid.target != null && __instance.eid.target.enemyIdentifier && __instance.eid.target.enemyIdentifier.sandified) {
			if (StalkerController.Instance.CheckIfTargetTaken(__instance.eid.target.targetTransform)) {
				StalkerController.Instance.targets.Remove(__instance.eid.target.targetTransform);
			}
			EnemyIdentifier enemyIdentifier;
			if (__instance.eid.target.targetTransform.TryGetComponent<EnemyIdentifier>(out enemyIdentifier) && enemyIdentifier.buffTargeter == __instance.eid) {
				enemyIdentifier.buffTargeter = null;
			}
		}
		if ((__instance.difficulty > 3 || __instance.eid.blessed || InvincibleEnemies.Enabled) && onDeath != 1) {
			__instance.exploding = false;
			__instance.countDownAmount = 0f;
			__instance.explosionCharge = 0f;
			__instance.currentColor = __instance.lightColors[0];
			__instance.lightAud.clip = __instance.lightSounds[0];
			__instance.blinking = false;
			return false;
		}
		__instance.exploded = true;
		if (!__instance.mach.limp && onDeath != 1) {
			__instance.mach.GoLimp();
			__instance.eid.Death();
		}
		if (__instance.eid.drillers.Count != 0) {
			for (int i = __instance.eid.drillers.Count - 1; i >= 0; i--) {
				UnityObject.Destroy(__instance.eid.drillers[i].gameObject);
			}
		}
		__instance.gameObject.SetActive(false);
		UnityObject.Destroy(__instance.gameObject);
		return false;
	}
}

[HarmonyPatch(typeof(SandificationZone))]
public class SandificationZonePatch
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(SandificationZone), nameof(SandificationZone.Enter))]
	public static void EnterPostfix(Collider other, SandificationZone __instance) {
		EnemyIdentifierIdentifier eidid = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
		if (eidid == null) {
			return;
		}

		EnemyIdentifier eid = eidid.eid;
		if (eid == null) {
			return;
		}
		if (eid.dead) {
			return;
		}
		
		StalkerHealBackMessenger messenger = __instance.gameObject.GetComponent<StalkerHealBackMessenger>();
		if (__instance.difficulty == 19 && messenger != null) {
			StalkerHealBack healBack = eid.gameObject.GetComponent<StalkerHealBack>();
			if (healBack == null) {
				healBack = eid.gameObject.AddComponent<StalkerHealBack>();
			}
			healBack.source = messenger.source;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(SandificationZone), nameof(SandificationZone.Start))]
	public static void StartPostfix(SandificationZone __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		foreach (RemoveOnTime rem in __instance.transform.parent.GetComponentsInChildren<RemoveOnTime>()) {
			rem.CancelInvoke("Remove");
			rem.Invoke("Remove", 3.5f);
		}
		foreach (ScaleNFade snf in __instance.transform.parent.GetComponentsInChildren<ScaleNFade>()) {
			snf.fadeSpeed /= 5f;
			snf.scaleSpeed /= 5f;
		}
	}
}