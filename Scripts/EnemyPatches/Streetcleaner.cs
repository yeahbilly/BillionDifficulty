using HarmonyLib;
using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: STREETCLEANER !!!
[HarmonyPatch(typeof(Streetcleaner))]
public class StreetcleanerPatch {
	// STREETCLEANER PATCH (fire size)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Streetcleaner), nameof(Streetcleaner.Start))]
	public static void StartPostfix(Streetcleaner __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		//__instance.gameObject.AddComponent<StreetcleanerAfterburn>().damage = Mathf.RoundToInt(4 * __instance.eid.totalDamageModifier);
		__instance.anim.speed = 1.25f; // Brutal: 1f
		__instance.nma.speed = 30f; // Brutal: 24f

		// Transform fire = __instance.transform.Find("flameboi2rig2/Armature/flamethrowergrip/Fire");
		Transform fire = __instance.transform.Find("flameboi2rig2").Find("Armature").Find("flamethrowergrip").Find("Fire");
		if (fire == null) {
			return;
		}
		ParticleSystem particles = fire.Find("Particle System").GetComponent<ParticleSystem>();
		var limitVelocity = particles.limitVelocityOverLifetime;
		limitVelocity.dampen /= 1.35f; // particles go 1.35 times further
		// fire.Find("Particle System").Find("Cube").localScale *= 1.35f;
		fire.Find("Particle System/Cube").localScale *= 1.35f;
	}

	// STREETCLEANER PATCH (speed)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Streetcleaner), nameof(Streetcleaner.GetSpeed))]
	public static bool GetSpeedPrefix(int difficulty, ref EnemyMovementData __result) {
		if (difficulty != 19) {
			return true;
		}
		__result = new EnemyMovementData {
			acceleration = 80f, // default: 64f
			angularSpeed = 5000f, // default: 1600f
			speed = 30f
		};
		return false;
	}

	// STREETCLEANER PATCH (reduces needed distance to the target)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.GetReachDistanceMultiplier))]
	public static bool GetReachDistanceMultiplierPostfix(EnemyIdentifier __instance, ref float __result) {
		bool isStreetcleaner = __instance.difficulty == 19 && __instance.enemyType == EnemyType.Streetcleaner;
		if (!isStreetcleaner) {
			return true;
		}
		__result = 0.75f;
		return false;
	}

	// STREETCLEANER PATCH (lets them look upwards)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Streetcleaner), nameof(Streetcleaner.LateUpdate))]
	public static bool LateUpdatePrefix(Streetcleaner __instance) {
		if (__instance.difficulty < 4) {
			return false;
		}
		if (!__instance.attacking) {
			return false;
		}
		if (__instance.target == null) {
			return false;
		}
		Vector3 a = __instance.hasVision ? __instance.targetData.headPosition : __instance.target.headPosition;
		float maxDegreesDelta = (float)((__instance.difficulty == 5) ? 90 : 35);
		if (__instance.difficulty == 19) {
			maxDegreesDelta = 90f;
		}
		Quaternion rotation = __instance.aimBone.rotation;
		Quaternion quaternion = Quaternion.RotateTowards(__instance.aimBone.rotation, Quaternion.LookRotation(a - __instance.aimBone.position, Vector3.up), maxDegreesDelta);
		Quaternion rhs = Quaternion.Inverse(__instance.transform.rotation * __instance.torsoDefaultRotation) * __instance.aimBone.rotation;
		if (Vector3.Dot(Vector3.up, quaternion * Vector3.forward) > 0f) {
			__instance.aimBone.rotation = quaternion * rhs;
		}
		Quaternion rhs2 = Quaternion.Inverse(rotation) * __instance.aimBone.rotation;
		rhs2 = Quaternion.Euler(-rhs2.eulerAngles.y, rhs2.eulerAngles.z, -rhs2.eulerAngles.x);
		__instance.flameThrowerBone.rotation *= rhs2;
		return false;
	}

	// STREETCLEANER PATCHES (afterburn and speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Streetcleaner), nameof(Streetcleaner.Update))]
	public static void UpdatePostfix(Streetcleaner __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		if (!__instance.dead) {
			if (__instance.nma.speed == 24f || __instance.nma.speed == 20f) {
				__instance.nma.speed = 30f;
			}
			return;
		}

		StreetcleanerAfterburn afterburn = __instance.gameObject.GetComponent<StreetcleanerAfterburn>();
		if (afterburn != null) {
			afterburn.destroyOnEnd = true;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Streetcleaner), nameof(Streetcleaner.StartDamaging))]
	public static void StartDamagingPostfix(Streetcleaner __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		StreetcleanerAfterburn afterburn = __instance.gameObject.GetComponent<StreetcleanerAfterburn>();
		if (afterburn != null) {
			afterburn.damagedThePlayer = true;
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Streetcleaner), nameof(Streetcleaner.StopFire))]
	public static void StopFirePrefix(Streetcleaner __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		if (!__instance.attacking || !__instance.damaging) {
			return;
		}

		StreetcleanerAfterburn afterburn = __instance.gameObject.GetComponent<StreetcleanerAfterburn>();
		if (afterburn != null) {
			afterburn.stoppedAttacking = true;
		}
	}
}