using Billion = BillionDifficulty.Plugin;
using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: TURRET !!!
[HarmonyPatch(typeof(Turret))]
public class TurretPatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Turret), nameof(Turret.Start))]
	public static void StartPostfix(Turret __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		__instance.maxAimTime = 3.75f; // (to counterbalance the speed buff) Brutal: 3f
		__instance.anim.speed *= 1.25f;
		__instance.nma.speed *= 1.25f;
		SentryMortar sentryMortar = __instance.gameObject.AddComponent<SentryMortar>();
		sentryMortar.rb = __instance.gameObject.GetComponent<Rigidbody>();
	}

	// SENTRY PATCH (shoot 3 times)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Turret), nameof(Turret.Shoot))]
	public static bool ShootPrefix(Turret __instance) {
		if (__instance.difficulty != 19) {
			return false;
		}
		SentryMortar sentryMortar = __instance.gameObject.GetComponent<SentryMortar>();
		sentryMortar.canShootOrb = false;


		if (!__instance.isBarrelPortalBlocked) {
			Vector3 shootPoint = __instance.isBarrelPortalCrossed ? __instance.barrelPos : new Vector3(__instance.transform.position.x, __instance.barrelTip.transform.position.y, __instance.transform.position.z);
			RevolverBeam revolverBeam = UnityObject.Instantiate<RevolverBeam>(
				__instance.beam,
				shootPoint,
				__instance.shootRotation
			);
			revolverBeam.alternateStartPoint = __instance.isBarrelPortalCrossed ? Vector3.zero : __instance.barrelPos;
			RevolverBeam revolverBeam2;
			if (revolverBeam.TryGetComponent<RevolverBeam>(out revolverBeam2)) {
				revolverBeam2.target = __instance.eid.target;
				revolverBeam2.damage *= __instance.eid.totalDamageModifier;
				if (__instance.difficulty == 19) {
					revolverBeam2.damage *= 0.8f; // 50 (x2) -> 40 (x3)
				}
			}
		}
		__instance.anim.Play("Shoot");
		__instance.CancelAim(false);
		__instance.BodyFreeze();
		__instance.cooldown = Random.Range(2.5f, 3.5f);
		__instance.shotsInARow++;

		// if ((__instance.difficulty == 4 && __instance.shotsInARow < 2) || __instance.difficulty == 5) {
		// 	__instance.Invoke("PreReAim", 0.25f);
		// }
		if (__instance.shotsInARow < 3 /*&& __instance.difficulty == 19*/) {
			__instance.Invoke("PreReAim", 0f);
		}

		return false;
	}

	public static float CalculateMortarAngle(float shotNumber) {
		// 0:0, 1:1, 2:-1, 3:2, 4:-2, 5:3, 6:-3
		if (shotNumber % 2 == 0)
			return -(shotNumber / 2f);
		else
			return (shotNumber + 1f) / 2f;
	}

	public static IEnumerator ShootMortar(Turret __instance, Transform barrelTip, int shotsLeft, int maxShots) {
		if (shotsLeft <= 0) {
			yield break;
		}
		
		yield return new WaitForSeconds(shotsLeft == maxShots ? 0.15f : 0.25f);

		float upOffset = shotsLeft != maxShots ? 4.5f : 5f;
		float forwardOffset = shotsLeft != maxShots ? 3f : 4.5f;
		GameObject projectile = UnityObject.Instantiate<GameObject>(Billion.ProjectileExplosiveHH, __instance.transform.position + Vector3.up * upOffset + __instance.transform.forward * forwardOffset, Quaternion.identity);
		projectile.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); // default: 2, 2, 2

		Projectile projectileComp = projectile.GetComponent<Projectile>();
		//projectileComp.explosionEffect.transform.localScale = new Vector3(6, 6, 6); // default: 12, 12, 12
		foreach (Explosion explosion in projectileComp.explosionEffect.transform.GetComponentsInChildren<Explosion>()) {
			BillionExplosionController excon = explosion.GetComponent<BillionExplosionController>();
			if (excon == null) {
				excon = explosion.gameObject.AddComponent<BillionExplosionController>();
			}
			excon.enemyDamageMultiplier = 0f;
			excon.damageMultiplier = 1.5f;
			//explosion.transform.localScale *= 1.25f;
			if (shotsLeft == maxShots) {
				excon.maxSizeMultiplier = 2.5f;
				excon.speedMultiplier = 0.8f;
			} else {
				excon.maxSizeMultiplier = 1f;
				excon.speedMultiplier = 1.5f;
			}
		}
		projectileComp.damage = 30; // default: 60
		projectileComp.enemyDamageMultiplier = 0f;
		projectileComp.predictiveHomingMultiplier = 0.75f; // default: 0
		projectileComp.bigExplosion = false;

		Vector3 bigProjectileExtraForce = new Vector3(0,0,0);
		// big projectile
		if (shotsLeft == maxShots) {
			Vector3 distance = __instance.eid.target.position - __instance.transform.position;
			// Vector3(0, 24f, 0) + 0.7f *
			bigProjectileExtraForce = new Vector3(0.9f * distance.x, 16f + 0.75f * distance.y, 0.9f * distance.z);
			projectile.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);;
			ProjectileHeightExplosion heightExplosion = projectile.AddComponent<ProjectileHeightExplosion>();
			heightExplosion.maxDistance = 8f;
			heightExplosion.target = __instance.eid.target;

			Color newProjectileColor = new Color(0.75f, 0.6f, 0f);

			// projectile and charge effect
			MeshRenderer[] renderers = projectileComp.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer renderer in renderers) {
				renderer.material.color = newProjectileColor;
				//renderer.material.renderQueue = 3000; // default: 2000
			}
			// trail
			TrailRenderer trail = projectileComp.GetComponent<TrailRenderer>();
			trail.startColor = newProjectileColor;
			trail.endColor = newProjectileColor;
			// projectile light
			Light light = projectileComp.GetComponent<Light>();
			light.color = newProjectileColor;
		}

		Rigidbody projectileRigidbody = projectile.GetComponent<Rigidbody>();
		projectileRigidbody.drag = shotsLeft != maxShots ? -2.2f : 0f;
		
		Rigidbody sentryRigidbody = __instance.gameObject.GetComponent<Rigidbody>();
		if (sentryRigidbody.velocity.y > 2f) {
			projectileRigidbody.drag /= 0.5f * sentryRigidbody.velocity.y;
			if (projectileRigidbody.drag < -2.2f) {
				projectileRigidbody.drag = 0f;
			}
		}
		AddForce forceComp = projectile.AddComponent<AddForce>();
		forceComp.onEnable = true;
		forceComp.force = new Vector3(0, 12, 0)
			+ 10f * (
				barrelTip.forward
				- new Vector3(0, barrelTip.forward.y, 0) // makes it not shoot upwards too much
				+ 0.2f * barrelTip.right * CalculateMortarAngle(maxShots-shotsLeft)
			)
			+ bigProjectileExtraForce;
		projectile.SetActive(false);
		projectile.SetActive(true);

		__instance.StartCoroutine(ShootMortar(__instance, __instance.barrelTip, shotsLeft-1, maxShots));
	}

	// SENTRY PATCH (interruption attack)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Turret), nameof(Turret.CancelAim))]
	public static void CancelAimPrefix(Turret __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		// if (__instance.isBarrelPortalBlocked) {
		// 	return;
		// }
		
		SentryMortar sentryMortar = __instance.gameObject.GetComponent<SentryMortar>();
		if (__instance.aiming && sentryMortar.canShootOrb) {
			__instance.StartCoroutine(ShootMortar(__instance, __instance.barrelTip, 3, 3));
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Turret), nameof(Turret.CancelAim))]
	public static void CancelAimPostfix(Turret __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		SentryMortar sentryMortar = __instance.gameObject.GetComponent<SentryMortar>();
		sentryMortar.canShootOrb = true;
	}
}