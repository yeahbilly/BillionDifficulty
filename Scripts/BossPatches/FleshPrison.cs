using HarmonyLib;
using UnityEngine;

namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: FLESHPRISON !!!
[HarmonyPatch(typeof(FleshPrison))]
public class FleshPrisonPatch {
	// FLESH PRISON PATCH (insignia amount fix)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(FleshPrison), nameof(FleshPrison.SpawnInsignia))]
	public static bool SpawnInsigniaPrefix(FleshPrison __instance) {
		if (__instance.difficulty != 19)
			return true;
		if (__instance.eid.target == null)
			return false;

		__instance.inAction = false;
		GameObject gameObject = Object.Instantiate<GameObject>(__instance.insignia, __instance.eid.target.position, Quaternion.identity);
		if (__instance.altVersion) {
			Vector3 velocity = __instance.eid.target.GetVelocity();
			velocity.y = 0f;
			if (velocity.magnitude > 0f) {
				gameObject.transform.LookAt(__instance.eid.target.position + velocity);
			} else {
				gameObject.transform.Rotate(Vector3.up * Random.Range(0f, 360f), Space.Self);
			}
			gameObject.transform.Rotate(Vector3.right * 90f, Space.Self);
		}
		VirtueInsignia virtueInsignia;
		if (gameObject.TryGetComponent<VirtueInsignia>(out virtueInsignia)) {
			virtueInsignia.predictive = true;
			virtueInsignia.noTracking = true;
			virtueInsignia.otherParent = __instance.transform;
			virtueInsignia.charges = (__instance.stat.health > __instance.maxHealth / 2f) ? 2 : 3;
			// if (__instance.difficulty >= 3) {
			// 	virtueInsignia.charges += __instance.difficulty - 2;
			// }
			virtueInsignia.charges += 3; // Brutal: 2
			virtueInsignia.windUpSpeedMultiplier = 0.5f;
			virtueInsignia.windUpSpeedMultiplier *= __instance.eid.totalSpeedModifier;
			virtueInsignia.damage = Mathf.RoundToInt((float)virtueInsignia.damage * __instance.eid.totalDamageModifier);
			virtueInsignia.target = __instance.eid.target;
			virtueInsignia.predictiveVersion = null;
			Light light = gameObject.AddComponent<Light>();
			light.range = 30f;
			light.intensity = 50f;
		}
		float num = 8f;
		switch (__instance.difficulty) {
			case 0:
				num = 5f;
				break;
			case 1:
				num = 7f;
				break;
			case 2:
			case 3:
			case 4:
			case 5:
			case 19:
				num = 8f;
				break;
		}

		if (Util.IsHardMode())
			num = 12f;

		gameObject.transform.localScale = new Vector3(num, 2f, num);
		gameObject.transform.SetParent(GoreZone.ResolveGoreZone(__instance.transform).transform, true);
		if (__instance.fleshDroneCooldown < 1f) {
			__instance.fleshDroneCooldown = 1f;
		}
		return false;
	}

	// FLESH PRISON PATCH (projectile homing fix)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(FleshPrison), nameof(FleshPrison.ProjectileBurstUpdate))]
	public static bool ProjectileBurstUpdatePrefix(FleshPrison __instance) {
		if (__instance.difficulty != 19)
			return true;

		__instance.homingProjectileCooldown = Mathf.MoveTowards(__instance.homingProjectileCooldown, 0f, Time.deltaTime * (Mathf.Abs(__instance.rotationSpeed) / 10f) * __instance.eid.totalSpeedModifier);
		if (__instance.homingProjectileCooldown <= 0f) {
			GameObject gameObject = Object.Instantiate<GameObject>(__instance.homingProjectile, __instance.rotationBone.position + __instance.rotationBone.up * 8f, __instance.rotationBone.rotation);
			Projectile component = gameObject.GetComponent<Projectile>();
			#pragma warning disable CS0618 // Type or member is obsolete
			component.target = __instance.eid.target;
			#pragma warning restore CS0618 // Type or member is obsolete
			component.safeEnemyType = __instance.altVersion ? EnemyType.FleshPanopticon : EnemyType.FleshPrison;
			switch (__instance.difficulty) {
				case 0:
					component.turningSpeedMultiplier = 0.4f;
					break;
				case 1:
					component.turningSpeedMultiplier = 0.45f;
					break;
				case 2:
				case 3:
					component.turningSpeedMultiplier = 0.5f;
					break;
				case 4:
				case 5:
				case 19:
					component.turningSpeedMultiplier = 0.66f;
					break;
			}
			if (__instance.altVersion) {
				component.turnSpeed *= 4f;
				component.turningSpeedMultiplier *= 4f;
				component.predictiveHomingMultiplier = 1.25f;
				Rigidbody rigidbody;
				if (gameObject.TryGetComponent<Rigidbody>(out rigidbody)) {
					rigidbody.AddForce(Vector3.up * 50f, ForceMode.VelocityChange);
				}
			}
			component.damage *= __instance.eid.totalDamageModifier;
			__instance.homingProjectileCooldown = 1f;
			__instance.currentProjectile++;
			gameObject.transform.SetParent(__instance.transform, true);
		}
		if (__instance.currentProjectile >= __instance.projectileAmount) {
			__instance.inAction = false;
			Animator animator = __instance.anim;
			if (animator != null) {
				animator.SetBool("Shooting", false);
			}
			__instance.rotationSpeedTarget = (float)((__instance.rotationSpeed >= 0f) ? 45 : -45);
			if (__instance.fleshDroneCooldown < 1f) {
				__instance.fleshDroneCooldown = 1f;
			}
		}
		return false;
	}

	// FLESH PRISON PATCH (restore skull drones)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(FleshPrison), nameof(FleshPrison.SpawnFleshDrones))]
	public static bool SpawnFleshDronesPrefix(FleshPrison __instance) {
		if (__instance.difficulty != 19)
			return true;
		if (__instance.eid.target == null)
			return false;

		float d = 360f / (float)__instance.droneAmount;
		if (__instance.currentDrone == 0) {
			__instance.targeter = new GameObject("Targeter");
			__instance.targeter.transform.position = __instance.rotationBone.position;
			Vector3 vector = __instance.altVersion ? Vector3.up : (new Vector3(__instance.eid.target.position.x, __instance.targeter.transform.position.y, __instance.eid.target.position.z) - __instance.targeter.transform.position);
			Quaternion rotation = __instance.altVersion ? Quaternion.LookRotation(vector.normalized) : Quaternion.LookRotation(vector.normalized, Vector3.up);
			__instance.targeter.transform.rotation = rotation;
			__instance.targeter.transform.Rotate(Vector3.forward * d / 2f);
		}
		if (__instance.currentDrone < __instance.droneAmount) {
			__instance.secondaryBarValue = (float)__instance.currentDrone / (float)__instance.droneAmount;

			GameObject droneToSpawn;
			if (!Util.IsHardMode() || (Util.IsHardMode() && __instance.altVersion))
				droneToSpawn = (__instance.currentDrone % 2 == 0) ? __instance.skullDrone : __instance.fleshDrone;
			else
				droneToSpawn = __instance.skullDrone;

			GameObject gameObject = Object.Instantiate<GameObject>(
				droneToSpawn,
				__instance.targeter.transform.position + __instance.targeter.transform.up * (float)(__instance.altVersion ? 50 : 20),
				__instance.targeter.transform.rotation
				);
			
			gameObject.transform.SetParent(__instance.transform, true);
			EnemyIdentifier enemyIdentifier;
			if (gameObject.TryGetComponent<EnemyIdentifier>(out enemyIdentifier)) {
				enemyIdentifier.dontCountAsKills = true;
				enemyIdentifier.damageBuff = __instance.eid.damageBuff;
				enemyIdentifier.healthBuff = __instance.eid.healthBuff;
				enemyIdentifier.speedBuff = __instance.eid.speedBuff;
			}
			DroneFlesh item;
			if (gameObject.TryGetComponent<DroneFlesh>(out item)) {
				__instance.currentDrones.Add(item);
			}
			__instance.targeter.transform.Rotate(Vector3.forward * d);
			__instance.currentDrone++;
			__instance.Invoke("SpawnFleshDrones", 0.1f / __instance.eid.totalSpeedModifier);
			return false;
		}
		__instance.inAction = false;
		__instance.rotationSpeedTarget = (float)((Random.Range(0, 2) == 0) ? 45 : -45);
		__instance.aud.Stop();
		__instance.shakingCamera = false;
		__instance.currentDrone = 0;
		Object.Destroy(__instance.targeter);
		__instance.fleshDroneCooldown = (float)(__instance.altVersion ? 30 : 25);
		__instance.healing = false;

		return false;
	}
}