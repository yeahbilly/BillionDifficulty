using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

[HarmonyPatch(typeof(MassSpear))]
public class MassSpearPatch {
	// MASS HOOK PATCH (makes it take the same amount of damage as on violent and below)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(MassSpear), nameof(MassSpear.GetHurt))]
	public static bool GetHurtPrefix(float damage, MassSpear __instance) {
		UnityObject.Instantiate<GameObject>(__instance.breakMetalSmall, __instance.transform.position, Quaternion.identity);
		__instance.spearHealth -=
			(__instance.difficulty >= 4 && __instance.difficulty != 19)
			? (damage / 1.5f)
			: damage;
		return false;
	}

	// MASS HOOK PATCH (makes it fast)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(MassSpear), nameof(MassSpear.Start))]
	public static bool StartPrefix(MassSpear __instance) {
		if (__instance.difficulty != 19) {
			return true;
		}
		
		__instance.lr = __instance.GetComponentInChildren<LineRenderer>();
		__instance.rb = __instance.GetComponent<Rigidbody>();
		__instance.aud = __instance.GetComponent<AudioSource>();
		__instance.mass = __instance.originPoint.GetComponentInParent<Mass>();
		__instance.Invoke("CheckForDistance", 3f / __instance.speedMultiplier);
		float num = 75f;
		switch (__instance.difficulty) {
			case 1:
				num = 75f;
				break;
			case 2:
				num = 200f;
				break;
			case 3:
			case 4:
			case 5:
			case 19:
				num = 250f;
				break;
		}
		__instance.rb.AddForce(num * __instance.speedMultiplier * __instance.transform.forward, ForceMode.VelocityChange);
		__instance.lastPosition = __instance.lr.transform.position;
		__instance.distanceTravelled = 0f;
		return false;
	}
}

// !!! PATCHGROUP: MASS !!!
[HarmonyPatch(typeof(Mass))]
public class MassPatch {
	// HIDEOUS MASS PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Mass), nameof(Mass.SetSpeed))]
	public static void SetSpeedPostfix(Mass __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		__instance.anim.speed = 1.5f * __instance.eid.totalSpeedModifier; // Brutal: 1.25f;
	}

	// HIDEOUS MASS PATCH (mortar launch force)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Mass), nameof(Mass.Start))]
	public static void StartPostfix(Mass __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		__instance.explosiveProjectileLaunchVelocity = 25f; // default: 50f
	}

	// HIDEOUS MASS PATCH (mortar projectile homing)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Mass), nameof(Mass.ShootProjectile))]
	public static bool ShootProjectilePrefix(int arm, GameObject projectile, ref float velocity, Mass __instance) {
		if (__instance.dead || __instance.eid.target == null) {
			return false;
		}

		Transform transform = __instance.shootPoints[arm];

		GameObject gameObject = UnityObject.Instantiate<GameObject>(projectile, transform.position, transform.rotation);

		Rigidbody rigidbody;
		if (gameObject.TryGetComponent<Rigidbody>(out rigidbody)) {
			Vector3 forwardForce =
				__instance.difficulty != 19
				? new Vector3(0,0,0)
				: 1.5f * (__instance.targetPos - __instance.transform.position).normalized;
			rigidbody.AddForce(transform.up * velocity + forwardForce, ForceMode.VelocityChange);
		}

		Projectile projectileComp;
		if (gameObject.TryGetComponent<Projectile>(out projectileComp)) {
			#pragma warning disable CS0618 // Type or member is obsolete
			projectileComp.target = __instance.eid.target;
			#pragma warning restore CS0618 // Type or member is obsolete
			projectileComp.safeEnemyType = EnemyType.HideousMass;
			projectileComp.transform.SetParent(__instance.stat.GetGoreZone().transform, true);
			projectileComp.damage *= __instance.eid.totalDamageModifier;

			if (__instance.difficulty == 19) {
				projectileComp.predictiveHomingMultiplier = 0.5f; // default: 0

				Rigidbody projectileRigidbody = projectileComp.gameObject.GetComponent<Rigidbody>();
				projectileRigidbody.drag = -1f; // -2.65f; default: 0
			}
		}
		return false;
	}

	// HIDEOUS MASS PATCH (faster slam shockwave)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Mass), nameof(Mass.SlamImpact))]
	public static bool SlamImpactPrefix(Mass __instance) {
		if (__instance.dead) {
			return false;
		}
		int shockwaveCount = 1;
		if (__instance.difficulty == 19) {
			shockwaveCount = 2;
		}
		for (int currentShockwaveCount = 0; currentShockwaveCount < shockwaveCount; currentShockwaveCount++) {
			GameObject gameObject = UnityObject.Instantiate<GameObject>(
				__instance.slamExplosion,
				new Vector3(
					__instance.shootPoints[2].position.x,
					__instance.transform.position.y,
					__instance.shootPoints[2].position.z
				),
				Quaternion.identity
			);
			PhysicalShockwave component = gameObject.GetComponent<PhysicalShockwave>();
			float heightMult = 1.5f;
			switch (__instance.difficulty) {
				case 0:
					component.speed = 15f;
					heightMult = 1.5f;
					break;
				case 1:
					component.speed = 20f;
					heightMult = 2f;
					break;
				case 2:
				case 3:
					component.speed = 25f;
					heightMult = 2.5f;
					break;
				case 4:
				case 5:
					component.speed = 35f;
					heightMult = 2.5f;
					break;
				case 19:
					component.speed = 50f;
					heightMult = 2.5f;
					break;
			}
			gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y * heightMult, gameObject.transform.localScale.z);

			component.damage = Mathf.RoundToInt(30f * __instance.eid.totalDamageModifier);
			component.maxSize = 100f;
			component.enemy = true;
			component.enemyType = EnemyType.HideousMass;
			gameObject.transform.SetParent(__instance.stat.GetGoreZone().transform, true);

			if (currentShockwaveCount == 0) {
				continue;
			}

			component.speed *= currentShockwaveCount * 0.6f; // Cerberus shockwave: component.speed /= 1 + currentShockwaveCount * 2
			AudioSource audioSource;
			if (component.TryGetComponent<AudioSource>(out audioSource)) {
				audioSource.enabled = false;
			}
		}

		return false;
	}
}