using HarmonyLib;
using UnityEngine;


// NOTHING TO PATCH FOR GABRIELSECOND

namespace BillionDifficulty.EnemyPatches;

[HarmonyPatch(typeof(GabrielBase))]
public class GabrielBasePatch {
	// GABRIEL BASE PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(GabrielBase), nameof(GabrielBase.UpdateSpeed))]
	public static void UpdateSpeedPostfix(GabrielBase __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		__instance.anim.speed = 1.15f * __instance.eid.totalSpeedModifier; // default: 1f * ...
		__instance.defaultAnimSpeed = __instance.anim.speed;
	}
}

// !!! PATCHGROUP: GABRIEL !!!
[HarmonyPatch(typeof(Gabriel))]
public class GabrielPatch {
	// GABRIEL PATCH (spear combo fix)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Gabriel), nameof(Gabriel.SpearCombo))]
	public static bool SpearComboPrefix(Gabriel __instance) {
		if (__instance.difficulty != 19) {
			return true;
		}
		switch (__instance.difficulty) {
			case 0:
				__instance.gabe.forwardSpeed = 60f;
				break;
			case 1:
				__instance.gabe.forwardSpeed = 75f;
				break;
			case 2:
			case 3:
			case 4:
			case 5:
			case 19:
				__instance.gabe.forwardSpeed = 150f;
				break;
		}
		__instance.gabe.forwardSpeed *= __instance.eid.totalSpeedModifier;
		__instance.spearAttacks = 1;
		if (__instance.gabe.enraged) {
			__instance.spearAttacks++;
		}
		if (__instance.gabe.secondPhase) {
			__instance.spearAttacks++;
		}
		__instance.SpawnRightHandWeapon(GabrielWeaponType.Spear);
		__instance.gabe.inAction = true;
		__instance.anim.Play("SpearReady");

		return false;
	}

	// GABRIEL PATCH (spear attack fix)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Gabriel), nameof(Gabriel.SpearAttack))]
	public static bool SpearAttackPrefix(Gabriel __instance) {
		if (__instance.difficulty != 19) {
			return true;
		}

		if (__instance.gabe.juggled) {
			return false;
		}
		if (__instance.target == null) {
			__instance.spearAttacks = 0;
		}
		if (__instance.spearAttacks == 0) {
			__instance.SpearThrow();
			return false;
		}
		__instance.gabe.spearing = true;
		__instance.gabe.goForward = false;
		__instance.spearAttacks--;
		float num = 1.5f;
		switch (__instance.difficulty) {
			case 0:
			case 1:
				num = 2f;
				break;
			case 2:
				num = 1.5f;
				break;
			case 3:
			case 4:
			case 5:
				num = 0.75f;
				break;
			case 19:
				num = 0.6f;
				break;
		}
		__instance.Invoke("SpearAttack", num / __instance.eid.totalSpeedModifier);
		num = 0.75f;
		switch (__instance.difficulty) {
			case 0:
			case 1:
				num = 1f;
				break;
			case 2:
				num = 0.75f;
				break;
			case 3:
			case 4:
			case 5:
				num = 0.5f;
				break;
			case 19:
				num = 0.4f;
				break;
		}
		Vector3 position = __instance.target.headPosition;
		bool flag = false;
		RaycastHit raycastHit;
		if (!Physics.Raycast(__instance.target.headPosition, Vector3.up, out raycastHit, 17f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore)) {
			position = __instance.target.headPosition + Vector3.up * 15f;
			flag = true;
		} else if (!Physics.Raycast(__instance.target.headPosition, Vector3.down, out raycastHit, 17f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore)) {
			position = __instance.transform.position + Vector3.down * 15f;
			flag = true;
		}
		if (!flag || (__instance.difficulty >= 4 && __instance.gabe.enraged && Random.Range(0f, 1f) > 0.5f)) {
			__instance.anim.Play("SpearStinger");
			__instance.gabe.Teleport(false, true, true, true, false);
			__instance.gabe.FollowTarget();
			__instance.Invoke("SpearFlash", num / 2f / __instance.eid.totalSpeedModifier);
			__instance.Invoke("SpearGoHorizontal", num / __instance.eid.totalSpeedModifier);
			return false;
		}
		__instance.gabe.TeleportTo(position);
		__instance.gabe.LookAtTarget(0);
		Animator anim = __instance.anim;
		if (anim != null) {
			anim.Play("SpearDown");
		}
		__instance.Invoke("SpearFlash", num / 2f / __instance.eid.totalSpeedModifier);
		__instance.Invoke("SpearGo", num / __instance.eid.totalSpeedModifier);
		return false;
	}
}