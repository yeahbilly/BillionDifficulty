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
		if (__instance.difficulty != 19)
			return true;
		
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
		if (__instance.difficulty != 19)
			return true;
		
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
	// HIDEOUS MASS PATCH (mortar launch force)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Mass), nameof(Mass.Start))]
	public static void StartPostfix(Mass __instance) {
		if (__instance.difficulty != 19)
			return;
		__instance.explosiveProjectileLaunchVelocity = 25f; // default: 50f

		if (!Util.IsHardMode())
			return;
		CounterInt counter = __instance.gameObject.AddComponent<CounterInt>();
		counter.maxValue = 3;

	}

	// HIDEOUS MASS PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Mass), nameof(Mass.SetSpeed))]
	public static void SetSpeedPostfix(Mass __instance) {
		if (__instance.difficulty != 19)
			return;
		float mult = !Util.IsHardMode() ? 1.5f : 2f;
		__instance.anim.speed = mult * __instance.eid.totalSpeedModifier; // Brutal: 1.25f;
	}

	// HIDEOUS MASS PATCH (hard mode virtue insignia)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Mass), nameof(Mass.SlamImpact))]
	public static void SlamImpactPostfix(Mass __instance) {
		if (!Util.IsHardMode())
			return;

		Vector3 spawnPosition = 14f * __instance.transform.forward + 8f * __instance.transform.up;

		Plugin.CreateVirtueInsignia(
			scaleMult: 0.5f,
			windUpSpeedMult: 0.75f,
			explosionLength: 4.5f,
			targetPosition: __instance.transform.position + spawnPosition,
			lookAtPosition: __instance.transform.position + spawnPosition + __instance.transform.forward,
			enemyTarget: __instance.eid.target,
			enemy: __instance.stat,
			totalDamageMult: __instance.eid.totalDamageModifier,
			lightIntensityMultiplier: 0.5f
		);
	}

	// HIDEOUS MASS PATCH (hard mode virtue insignia)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Mass), nameof(Mass.SwingEnd))]
	public static void SwingEndPostfix(Mass __instance) {
		if (!Util.IsHardMode())
			return;

		Vector3 spawnPosition = 14f * __instance.transform.forward + 8f * __instance.transform.up;

		Plugin.CreateVirtueInsignia(
			scaleMult: 0.5f,
			windUpSpeedMult: 0.75f,
			explosionLength: 4.5f,
			targetPosition: __instance.transform.position + spawnPosition,
			lookAtPosition: __instance.transform.position + spawnPosition + __instance.transform.right,
			enemyTarget: __instance.eid.target,
			enemy: __instance.stat,
			totalDamageMult: __instance.eid.totalDamageModifier,
			lightIntensityMultiplier: 0.5f
		);
	}

	// HIDEOUS MASS PATCH (mortar projectile homing)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Mass), nameof(Mass.ShootProjectile))]
	public static bool ShootProjectilePrefix(int arm, GameObject projectile, ref float velocity, Mass __instance) {
		if (__instance.difficulty != 19)
			return true;
		if (__instance.dead || __instance.eid.target == null)
			return false;

		Transform transform = __instance.shootPoints[arm];
		GameObject projectileObject = UnityObject.Instantiate<GameObject>(projectile, transform.position, transform.rotation);

		Rigidbody rigidbody;
		if (projectileObject.TryGetComponent<Rigidbody>(out rigidbody)) {
			Vector3 forwardForce =
				__instance.difficulty != 19
				? Vector3.zero
				: 1.5f * (__instance.targetPos - __instance.transform.position).normalized;
			rigidbody.AddForce(transform.up * velocity + forwardForce, ForceMode.VelocityChange);
		}

		Projectile projectileComp;
		if (projectileObject.TryGetComponent<Projectile>(out projectileComp)) {
			#pragma warning disable CS0618 // Type or member is obsolete
			projectileComp.target = __instance.eid.target;
			#pragma warning restore CS0618 // Type or member is obsolete
			projectileComp.safeEnemyType = EnemyType.HideousMass;
			projectileComp.transform.SetParent(__instance.stat.GetGoreZone().transform, true);
			projectileComp.damage *= __instance.eid.totalDamageModifier;

			if (__instance.difficulty == 19) {
				projectileComp.predictiveHomingMultiplier = 0.5f; // default: 0
				projectileComp.turningSpeedMultiplier = 2.64f;
				projectileComp.turnSpeed = 100f;

				Rigidbody projectileRigidbody = projectileComp.GetComponent<Rigidbody>();
				projectileRigidbody.drag = -1f; // -2.65f; default: 0
			}

			if (Util.IsHardMode()) {
				CounterInt counter = __instance.GetComponent<CounterInt>();
				if (__instance.crazyMode && counter.value != 1) {
					counter.Add();
					return false;
				}
				if (__instance.crazyMode) {
					counter.Add();
				}
				ShockwaveOnExplode shockwave = projectileObject.AddComponent<ShockwaveOnExplode>();
				shockwave.enemyType = EnemyType.HideousMass;
				shockwave.totalDamageModifier = __instance.eid.totalDamageModifier;
				shockwave.speed = 70f;
			}
		}
		return false;
	}

	// HIDEOUS MASS PATCH (faster slam shockwave)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Mass), nameof(Mass.SlamImpact))]
	public static bool SlamImpactPrefix(Mass __instance) {
		if (__instance.dead)
			return false;
		if (__instance.difficulty != 19)
			return true;

		int shockwaveCount = 2; // default: 1
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

			if (currentShockwaveCount == 0)
				continue;

			component.speed *= currentShockwaveCount * 0.6f; // Cerberus shockwave: component.speed /= 1 + currentShockwaveCount * 2
			AudioSource audioSource;
			if (component.TryGetComponent<AudioSource>(out audioSource)) {
				audioSource.enabled = false;
			}
		}
		return false;
	}
}