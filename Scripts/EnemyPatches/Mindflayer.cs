using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: MINDFLAYER !!!
[HarmonyPatch(typeof(Mindflayer))]
public class MindflayerPatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Mindflayer), nameof(Mindflayer.SetSpeed))]
	public static void SetSpeedPostfix(Mindflayer __instance) {
		__instance.difficulty = Util.GetDifficulty();
		if (__instance.difficulty != 19)
			return;

		float hardModeMult = (!Util.IsHardMode()) ? 1f : 1.35f;
		__instance.cooldownMultiplier = 3.25f * hardModeMult * __instance.eid.totalSpeedModifier; // Brutal: 2.5f
		__instance.anim.speed = 1.75f * hardModeMult * __instance.eid.totalSpeedModifier; // Brutal: 1.5f
		__instance.defaultAnimSpeed = __instance.anim.speed;
	}

	// MINDFLAYER PATCH (virtue beam)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Mindflayer), nameof(Mindflayer.ShootProjectiles))]
	public static void ShootProjectilesPostfix(Mindflayer __instance) {
		if (!Util.IsHardMode())
			return;
		if (!__instance.hadVision)
			return;

		Plugin.CreateVirtueInsignia(
			scaleMult: 0.5f,
			windUpSpeedMult: 0.7f,
			explosionLength: 5f,
			targetPosition: __instance.eid.target.position,
			lookAtPosition: __instance.transform.position,
			enemyTarget: __instance.eid.target,
			enemy: __instance.enemy,
			totalDamageMult: __instance.eid.totalDamageModifier,
			lightIntensityMultiplier: 0.5f
		);
	}

	// MINDFLAYER PATCH (small green orbs)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Mindflayer), nameof(Mindflayer.StartBeam))]
	public static void StartBeamPostfix(Mindflayer __instance) {
		if (__instance.difficulty != 19)
			return;

		int projectileAmount = !__instance.isEnraged ? 10 : 15;

		for (int i = 0; i < projectileAmount; i++) {
			Vector3 randomDir = Random.onUnitSphere;
			// converts to spherical coordinates (yaw, pitch)
			// yaw - angle around Y axis (0 to 360)
			float yaw = Mathf.Atan2(randomDir.z, randomDir.x) * Mathf.Rad2Deg;
			if (yaw < 0) {
				yaw += 360f;
			}
			// pitch - angle from horizontal plane (-90 to 90)
			float pitch = Mathf.Asin(randomDir.y) * Mathf.Rad2Deg;
			// snaps yaw and pitch to nearest multiple of 2 degrees
			yaw = Mathf.Round(yaw / 2f) * 2f;
			pitch = Mathf.Round(pitch / 2f) * 2f;

			float yawRad = yaw * Mathf.Deg2Rad;
			float pitchRad = pitch * Mathf.Deg2Rad;

			float x = Mathf.Cos(pitchRad) * Mathf.Cos(yawRad);
			float y = Mathf.Sin(pitchRad);
			float z = Mathf.Cos(pitchRad) * Mathf.Sin(yawRad);

			Vector3 randomRotation = new Vector3(x, y, z).normalized;

			Projectile currentProjectile = UnityObject.Instantiate<GameObject>(Plugin.Prefabs["ProjectileHoming"], __instance.transform.position + 2f * randomRotation, Quaternion.LookRotation(randomRotation)).GetComponent<Projectile>();
			currentProjectile.homingType = HomingType.None;
			currentProjectile.speed = 60f;
			currentProjectile.unparryable = true;
			currentProjectile.damage = 15; // default: 30
			currentProjectile.enemyDamageMultiplier = 1f;

			if (Util.IsHardMode()) {
				currentProjectile.transform.localScale *= 2f;
				if (__instance.isEnraged) {
					currentProjectile.transform.localScale *= 1.25f;
				}
			}

			SlowDownOverTime slowDown = currentProjectile.gameObject.AddComponent<SlowDownOverTime>();
			slowDown.slowRate = !__instance.isEnraged ? UnityEngine.Random.Range(1.8f, 3.75f) : UnityEngine.Random.Range(2f, 4.5f);

			if (Util.IsHardMode())
				slowDown.slowRate /= 1.4f; // 1.75f

			RemoveOnTime removeOnTime = currentProjectile.GetComponent<RemoveOnTime>();
			removeOnTime.time = 5f;

			ChangeScaleOverTime changeScale = currentProjectile.gameObject.AddComponent<ChangeScaleOverTime>();
			changeScale.delay = 4f;
			changeScale.time = 1f;
			changeScale.targetScaleMultiplier = 0f;
			
			AudioSource audio = currentProjectile.GetComponent<AudioSource>();
			audio.volume *= 0.125f; // default: 0.5f
			//audio.pitch *= 1.5f; // default: 1.8f-1.9f
			AudioSource audio2 = currentProjectile.transform.Find("ChargeEffect").GetComponent<AudioSource>();
			audio2.volume *= 0.125f; // default: 0.5f
			//audio2.pitch *= 1.5f; // default: 3f

			Color newProjectileColor = new Color(0.5f, 0.85f, 0.35f);

			// projectile and charge effect
			MeshRenderer[] renderers = currentProjectile.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer renderer in renderers) {
				renderer.material.color = newProjectileColor;
				//renderer.material.renderQueue = 3000; // default: 2000
			}

			// trail
			TrailRenderer trail = currentProjectile.GetComponentInChildren<TrailRenderer>();
			if (trail != null) {
				trail.startColor = newProjectileColor;
				trail.endColor = newProjectileColor;
			}

			// projectile light
			Light light = currentProjectile.GetComponent<Light>();
			light.color = newProjectileColor;

			// particle system
			ParticleSystem particleSystem = currentProjectile.GetComponentInChildren<ParticleSystem>();
			particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
			var main = particleSystem.main;
			main.loop = false;
			main.duration = 4f; // default: 5f
			main.startColor = newProjectileColor;
			particleSystem.Play();
		}
	}
}