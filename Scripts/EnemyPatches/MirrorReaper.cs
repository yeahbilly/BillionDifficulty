using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

[HarmonyPatch(typeof(GroundWave), nameof(GroundWave.FixedUpdate))]
public class GroundWavePatch {
	public static bool Prefix(GroundWave __instance, bool __runOriginal) {
		if (__instance.difficulty != 19)
			return true;
		if (!__instance.isTraversingLink || !__runOriginal)
			return false;

		Vector3 vector = __instance.traversalVelocity * Time.fixedDeltaTime;
		__instance.transform.position += vector;
		if (__instance.traversalVelocity.sqrMagnitude > 0.001f) {
			__instance.transform.rotation = Quaternion.LookRotation(__instance.traversalVelocity.normalized, Vector3.up);
		}
		if ((bool)__instance.rb) {
			__instance.rb.position = __instance.transform.position;
			__instance.rb.rotation = __instance.transform.rotation;
		}
		if (__instance.hasCrossed) {
			__instance.postTeleportDistance += vector.magnitude;
			if (__instance.postTeleportDistance >= 2f) {
				__instance.isTraversingLink = false;
				__instance.nma.enabled = true;
				__instance.nma.Warp(__instance.transform.position);
				__instance.nma.velocity = __instance.traversalVelocity;
				if (__instance.nma != null && __instance.nma.enabled)
					__instance.nma.SetDestination(__instance.target.GetNavPoint());
			}
		}
		return false;
	}
}


// !!! PATCHGROUP: MIRRORREAPER !!!
[HarmonyPatch(typeof(MirrorReaper))]
public class MirrorReaperPatch {
	// MIRROR REAPER PATCH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MirrorReaper), nameof(MirrorReaper.UpdateDifficulty))]
	public static void UpdateDifficultyPostfix(MirrorReaper __instance) {
		if (__instance.difficulty != 19)
			return;
		float speedMult = (!Util.IsHardMode()) ? 1.4f : 1.75f;
		__instance.anim.speed = speedMult * __instance.eid.totalSpeedModifier; // Brutal: 1f * ...
		__instance.maxGroundWaves = (!Util.IsHardMode()) ? 3 : 4; // Brutal: 3
	}

	// MIRROR REAPER PATCH (cooldown)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MirrorReaper), nameof(MirrorReaper.AttackCheck))]
	public static void AttackCheckPostfix(MirrorReaper __instance) {
		if (__instance.difficulty != 19)
			return;
		float cooldownReduce = (!Util.IsHardMode()) ? 0.2f : 0.4f;
		__instance.cooldown -= cooldownReduce * Time.deltaTime;
	}

	// MIRROR REAPER PATCH (mortar attack)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MirrorReaper), nameof(MirrorReaper.SpawnGroundWave))]
	public static void SpawnGroundWavePostfix(MirrorReaper __instance) {
		if (__instance.difficulty != 19)
			return;
		// MirrorReaperMortar mortar = __instance.GetComponent<MirrorReaperMortar>();
		// if (mortar != null && !mortar.canShootMortar)
		// 	return;
		// mortar.canShootMortar = false;
		
		Vector3 distance;
		if (__instance.eid.target != null)
			distance = __instance.eid.target.position - __instance.transform.position;
		else
			distance = NewMovement.Instance.transform.position - __instance.transform.position;
		Vector3 direction = distance.normalized;
		
		float upOffset = 5f;
		float forwardOffset = 8f;
		GameObject projectile = UnityObject.Instantiate<GameObject>(
			Plugin.Prefabs["ProjectileExplosiveHH"],
			__instance.transform.position + __instance.transform.up * upOffset + direction * forwardOffset,
			Quaternion.identity
		);
		projectile.transform.localScale = 1.5f * Vector3.one; // default: 2, 2, 2

		Projectile projectileComp = projectile.GetComponent<Projectile>();
		//projectileComp.explosionEffect.transform.localScale = new Vector3(6, 6, 6); // default: 12, 12, 12
		foreach (Explosion explosion in projectileComp.explosionEffect.transform.GetComponentsInChildren<Explosion>()) {
			BillionExplosionController excon = explosion.GetComponent<BillionExplosionController>();
			if (excon == null) {
				excon = explosion.gameObject.AddComponent<BillionExplosionController>();
			}
			excon.enemyDamageMultiplier = 0f;
			excon.damageMultiplier = 1.5f;
			// excon.maxSizeMultiplier = 2.5f;
			// excon.speedMultiplier = 0.8f;
		}
		projectileComp.damage = 30; // default: 60
		projectileComp.enemyDamageMultiplier = 0f;
		projectileComp.turningSpeedMultiplier = 3f; // default: 1f
		projectileComp.predictiveHomingMultiplier = 0.75f; // default: 0
		projectileComp.turnSpeed = 300f;
		projectileComp.bigExplosion = false;
		projectileComp.safeEnemyType = EnemyType.MirrorReaper;

		// big projectile
		// Vector3(0, 24f, 0) + 0.7f *
		// Vector3 bigProjectileExtraForce = new Vector3(1.5f * distance.x, 16f + 0.75f * distance.y, 1.5f * distance.z);
		projectile.transform.localScale = 2.5f * Vector3.one;
		ProjectileHeightExplosion heightExplosion = projectile.AddComponent<ProjectileHeightExplosion>();
		heightExplosion.maxDistance = 4f;
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

		Rigidbody projectileRigidbody = projectile.GetComponent<Rigidbody>();
		projectileRigidbody.drag = 0f;

		AddForce forceComp = projectile.AddComponent<AddForce>();
		forceComp.onEnable = true;
		forceComp.force = 2.3f * distance + 0.03f * distance * __instance.nma.velocity.magnitude;
		// forceComp.force =
		// 	3f * __instance.transform.up //new Vector3(0, 12, 0)
		// 	+ 6f * (
		// 		__instance.transform.forward
		// 		- new Vector3(0, __instance.transform.forward.y, 0) // makes it not shoot upwards too much
		// 	)
		// 	+ bigProjectileExtraForce
		// 	+ 6f * distance.normalized;
		projectile.SetActive(false);
		projectile.SetActive(true);
	}
}