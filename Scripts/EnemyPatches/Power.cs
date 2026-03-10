using HarmonyLib;
using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: POWER !!!
[HarmonyPatch(typeof(Power))]
public class PowerPatch {
	// POWER PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Power), nameof(Power.UpdateSpeed))]
	public static void UpdateSpeedPostfix(Power __instance) {
		__instance.anim.speed = 1.15f * __instance.eid.totalSpeedModifier; // Brutal: 0.95f * ...
	}

	// POWER PATCH (attack cooldown)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Power), nameof(Power.PickAttack))]
	public static void PickAttackPostfix(Power __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		__instance.attackCooldown = 1.5f; // Brutal: 2f
	}

	// POWER PATCH (spear attack velocity)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Power), nameof(Power.Spear))]
	public static void SpearPrefix(Power __instance) {
		if (__instance.difficulty != 19) {
            return;
        }
        __instance.forwardSpeed = 150f; // Standard~Brutal: 150f
	}

	// POWER PATCH (spear attack time? idk)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Power), nameof(Power.SpearAttack))]
	public static bool SpearAttackPrefix(Power __instance) {
		if (__instance.difficulty != 19) {
			return true;
		}

		if (!__instance.active || __instance.juggled) {
			return false;
		}
		if (__instance.target == null) {
			__instance.spearAttacks = 0;
		}
		if (__instance.spearAttacks == 0) {
			__instance.ToSpearThrow();
			return false;
		}
		__instance.spearing = Power.SpearAttackState.Vertical;
		__instance.goForward = false;
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
			case 19:
				num = 0.75f;
				break;
		}
		__instance.Invoke("SpearAttack", num / __instance.eid.totalSpeedModifier);
		bool flag = false;
		Vector3 vector = __instance.lastTargetData.realHeadPosition;
		float num2 = Random.Range(0f, 1f);
		PhysicsCastResult physicsCastResult;
		Vector3 vector2;
		if (!PortalPhysicsV2.Raycast(__instance.lastTargetData.realHeadPosition, Vector3.up, out physicsCastResult, out vector2, 17f, __instance.environmentMask, QueryTriggerInteraction.Ignore)) {
			PortalPhysicsV2.Raycast(__instance.lastTargetData.realHeadPosition, Vector3.up, out physicsCastResult, out vector2, 15f, __instance.environmentMask, QueryTriggerInteraction.Ignore);
			vector = vector2;
			flag = true;
		} else if (!PortalPhysicsV2.Raycast(__instance.lastTargetData.realHeadPosition, Vector3.down, out physicsCastResult, out vector2, 17f, __instance.environmentMask, QueryTriggerInteraction.Ignore)) {
			PortalPhysicsV2.Raycast(__instance.transform.position, Vector3.down, out physicsCastResult, out vector2, 15f, __instance.environmentMask, QueryTriggerInteraction.Ignore);
			vector = vector2;
			flag = true;
		}
		if (!flag || ((__instance.difficulty >= 4 || __instance.enraged) && num2 > 0.5f)) {
			__instance.spearing = Power.SpearAttackState.Horizontal;
			__instance.anim.Play("SpearStinger");
			__instance.Teleport(false, true, true, true, false, false);
			__instance.FollowTarget();
			__instance.Invoke("SpearFlash", 0.25f / __instance.eid.totalSpeedModifier);
			__instance.Invoke("SpearGoHorizontal", 0.5f / __instance.eid.totalSpeedModifier);
			return false;
		}
		if (__instance.anim != null) {
			__instance.anim.Play("SpearDrop");
		}
		int num3 = Mathf.RoundToInt(Vector3.Distance(__instance.transform.position, vector) / 2.5f);
		for (int i = 0; i < num3; i++) {
			__instance.CreateDecoy(Vector3.Lerp(__instance.transform.position, vector, (float)i / (float)num3), (float)i / (float)num3 + 0.1f, null);
		}
		__instance.transform.position = vector;
		__instance.teleportAttempts = 0;
		Object.Instantiate<GameObject>(__instance.teleportSound, __instance.transform.position, Quaternion.identity);
		if (__instance.eid.hooked) {
			MonoSingleton<HookArm>.Instance.StopThrow(1f, true);
		}
		__instance.LookAtTarget(0);
		float num4 = 0.75f;
		switch (__instance.difficulty) {
			case 0:
			case 1:
				num4 = 1f;
				break;
			case 2:
				num4 = 0.75f;
				break;
			case 3:
			case 4:
			case 5:
			case 19:
				num4 = 0.5f;
				break;
		}
		__instance.Invoke("SpearFlash", num4 / 2f / __instance.eid.totalSpeedModifier);
		__instance.Invoke("SpearGo", num4 / __instance.eid.totalSpeedModifier);
		return false;
	}
}