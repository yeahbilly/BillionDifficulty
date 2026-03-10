using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: MANNEQUIN !!!
[HarmonyPatch(typeof(Mannequin))]
public class MannequinPatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Mannequin), nameof(Mannequin.Start))]
	public static void StartPostfix(Mannequin __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		
		CounterInt counter = __instance.gameObject.AddComponent<CounterInt>();
		counter.maxValue = 2;
	}

	// MANNEQUIN PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Mannequin), nameof(Mannequin.SetSpeed))]
	public static void SetSpeedPostfix(Mannequin __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		__instance.anim.speed = 1.5f * __instance.eid.totalSpeedModifier; // Brutal: 1.25f
		__instance.walkSpeed = 25f * __instance.eid.totalSpeedModifier; // Brutal: 20f
		__instance.skitterSpeed = 90f * __instance.eid.totalSpeedModifier; // Brutal: 64f
	}

	// MANNEQUIN PATCH (fixes the attack cooldown)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Mannequin), nameof(Mannequin.ProjectileAttack))]
	public static void ProjectileAttackPostfix(Mannequin __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		__instance.projectileCooldown = UnityEngine.Random.Range(1f, 3f) / __instance.eid.totalSpeedModifier; // Brutal: Range(2f, 4f)
	}

	// MANNEQUIN PATCH (attack)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Mannequin), nameof(Mannequin.ShootProjectile))]
	public static bool ShootProjectilePrefix(Mannequin  __instance) {
		if (__instance.currentChargeProjectile) {
			UnityObject.Destroy(__instance.currentChargeProjectile);
		}
		if (__instance.projectile == null || __instance.projectile.Equals(null)) {
			__instance.trackTarget = false;
			__instance.chargingProjectile = false;
			return true;
		}

		CounterInt counter = __instance.gameObject.GetComponent<CounterInt>();
		Vector3 addedLookRotation = new Vector3(0, 0, 0);
		//Vector3 targetFuturePos = new Vector3(0, 0, 0);
		if (__instance.difficulty == 19) {
			if (counter != null && counter.value == 2) {
				addedLookRotation = new Vector3(0, 2f, 0);
			}
		}

		Quaternion lookRotation =
			(__instance.eid.target != null)
			? Quaternion.LookRotation(__instance.shootTarget.position - __instance.shootPoint.position + addedLookRotation)
			: __instance.shootPoint.rotation;

		Projectile projectile = UnityObject.Instantiate<Projectile>(__instance.projectile, __instance.shootPoint.position, lookRotation);
		#pragma warning disable CS0618 // Type or member is obsolete
		projectile.target = __instance.eid.target;
		#pragma warning restore CS0618 // Type or member is obsolete
		projectile.safeEnemyType = EnemyType.Mannequin;

		if (__instance.difficulty <= 2) {
			projectile.turningSpeedMultiplier = 0.75f;
		}
		__instance.trackTarget = false;
		__instance.chargingProjectile = false;

		if (__instance.difficulty != 19) {
			return false;
		}

		if (counter != null && counter.value == 2) {
			projectile.homingType = HomingType.Instant;
			projectile.turningSpeedMultiplier = 0.2f;
			projectile.speed = 5f;
			projectile.ignoreEnvironment = true;

			SlowDownOverTime slowDown = projectile.gameObject.AddComponent<SlowDownOverTime>();
			slowDown.slowRate = -2.5f;

			ChangeScaleOverTime changeScale = projectile.gameObject.AddComponent<ChangeScaleOverTime>();
			changeScale.targetScaleMultiplier = 30f;
			changeScale.time = 3f;

			projectile.gameObject.GetComponent<RemoveOnTime>().time = 3f;
		}
		counter.Add();
		
		return false;
	}
}