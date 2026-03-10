using ULTRAKILL.Cheats;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: MALICIOUSFACE !!!
[HarmonyPatch(typeof(MaliciousFace))]
public class MaliciousFacePatch {
	// MAURICE PATCH (explosion parry and projectile damage reduction)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(MaliciousFace), nameof(MaliciousFace.OnParry))]
	public static bool OnParryPrefix(ref DamageData data, bool isShotgun, MaliciousFace __instance) {
		if (!__instance.spiderParryable) {
			return false;
		}

		__instance.spiderParryable = false;
		MonoSingleton<FistControl>.Instance.currentPunch.Parry(false, __instance.eid, "");
		__instance.currentExplosion = UnityObject.Instantiate<GameObject>(__instance.beamExplosion.ToAsset(), __instance.transform.position, Quaternion.identity);
		if (!InvincibleEnemies.Enabled && !__instance.eid.blessed) {
			__instance.spider.health -= (float)((__instance.spider.parryFramesLeft > 0) ? 4 : 5) / __instance.eid.totalHealthModifier;
		}
		foreach (Explosion explosion in __instance.currentExplosion.GetComponentsInChildren<Explosion>()) {
			if (__instance.difficulty != 19) {
				explosion.speed *= __instance.eid.totalDamageModifier;
				explosion.maxSize *= 1.75f * __instance.eid.totalDamageModifier;
				explosion.damage = Mathf.RoundToInt(50f * __instance.eid.totalDamageModifier);
				explosion.canHit = AffectedSubjects.EnemiesOnly;
				explosion.friendlyFire = true;
			} else {
				explosion.speed *= 1.25f;
				explosion.maxSize *= 0.8f;
				__instance.currentExplosion.transform.localScale *= 0.8f; // default: 12, 12, 12
				explosion.damage = Mathf.RoundToInt(25f * __instance.eid.totalDamageModifier); // default: Mathf.RoundToInt(50f * __instance.eid.totalDamageModifier)
				explosion.canHit = AffectedSubjects.PlayerOnly; // default: AffectedSubjects.EnemiesOnly
				explosion.friendlyFire = false; // default: true

				__instance.spider.health += 1.5f / __instance.eid.totalHealthModifier; // there are 2 explosions, default: 3f*2, new: 1.5f*2. The division is so that it still damages with radiance on
				__instance.eid.health += 1.5f / __instance.eid.totalHealthModifier;
			}
		}
		if (__instance.currentEnrageEffect == null) {
			__instance.CancelInvoke("BeamFire");
			__instance.Invoke("StopWaiting", 1f);
			UnityObject.Destroy(__instance.currentCE);
		}

		return false;
	}


	// MAURICE PATCH (burst number and cooldown)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MaliciousFace), nameof(MaliciousFace.Start))]
	public static void StartPostfix(MaliciousFace __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		
		__instance.coolDownMultiplier = 1.5f; // Brutal: 1.25f
		__instance.maxBurst = 18; // Brutal: 10
	}

	// MAURICE PATCH (projectile burst)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(MaliciousFace), nameof(MaliciousFace.ShootProj))]
	public static bool ShootProjPrefix(ref TargetData targetData, MaliciousFace __instance) {
		Vector3 vector = targetData.headPosition;
		Vector3 vector2 = __instance.mouth.position;
		Vector3 direction = vector2 - __instance.transform.position;
		PhysicsCastResult physicsCastResult;
		Vector3 vector3;
		PortalTraversalV2[] array;
		PortalPhysicsV2.ProjectThroughPortals(__instance.transform.position, direction, default(LayerMask), out physicsCastResult, out vector3, out array);
		bool flag = false;
		if (array.Length != 0) {
			PortalTraversalV2 portalTraversalV = array[0];
			PortalHandle portalHandle = portalTraversalV.portalHandle;
			Portal portalObject = portalTraversalV.portalObject;
			if (portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile)) {
				vector2 = vector3;
				vector = PortalUtils.GetTravelMatrix(array).MultiplyPoint3x4(vector);
			} else {
				flag = !portalObject.passThroughNonTraversals;
			}
		}

		__instance.currentProj = UnityObject.Instantiate<GameObject>(__instance.proj, vector2, Quaternion.LookRotation(vector - vector2));

		if (__instance.difficulty == 19) {
			Projectile projectileComp = __instance.currentProj.GetComponent<Projectile>();
			projectileComp.enemyDamageMultiplier = 0.175f; // default: 1
			projectileComp.damage = 20f; // default: 25f
		}

		float projOffset = (float)(1 + __instance.currentBurst / 5 * 2);
		if (__instance.difficulty == 19) {
			switch (__instance.currentBurst % 9) {
				case 0:
					__instance.currentProj.transform.LookAt(vector);
					__instance.currentProj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); // default: 1, 1, 1
					__instance.currentProj.GetComponent<Projectile>().damage = 40f;
					break;
				case 1:
					__instance.currentProj.transform.LookAt(vector + __instance.transform.right * projOffset);
					break;
				case 2:
					__instance.currentProj.transform.LookAt(vector + __instance.transform.right * projOffset
																		+ __instance.transform.up * projOffset);
					break;
				case 3:
					__instance.currentProj.transform.LookAt(vector + __instance.transform.up * projOffset);
					break;
				case 4:
					__instance.currentProj.transform.LookAt(vector + __instance.transform.up * projOffset
																		- __instance.transform.right * projOffset);
					break;
				case 5:
					__instance.currentProj.transform.LookAt(vector - __instance.transform.right * projOffset);
					break;
				case 6:
					__instance.currentProj.transform.LookAt(vector - __instance.transform.right * projOffset
																		- __instance.transform.up * projOffset);
					break;
				case 7:
					__instance.currentProj.transform.LookAt(vector - __instance.transform.up * projOffset);
					break;
				case 8:
					__instance.currentProj.transform.LookAt(vector - __instance.transform.up * projOffset
																		+ __instance.transform.right * projOffset);
					break;
			}
		} else if (__instance.difficulty >= 4) {
			switch (__instance.currentBurst % 5) {
				case 1:
					__instance.currentProj.transform.LookAt(vector + __instance.transform.right * projOffset);
					break;
				case 2:
					__instance.currentProj.transform.LookAt(vector + __instance.transform.up * projOffset);
					break;
				case 3:
					__instance.currentProj.transform.LookAt(vector - __instance.transform.right * projOffset);
					break;
				case 4:
					__instance.currentProj.transform.LookAt(vector - __instance.transform.up * projOffset);
					break;
			}
		}
		Projectile component = __instance.currentProj.GetComponent<Projectile>();
		if (!flag) {
			component.safeEnemyType = EnemyType.MaliciousFace;
			component.targetHandle = targetData.handle;
			if (__instance.difficulty == 19) {
				component.speed *= 1.5f; // Brutal: 1.25f
			} else if (__instance.difficulty > 2) {
				component.speed *= 1.25f;
			} else if (__instance.difficulty == 1) {
				component.speed *= 0.75f;
			} else if (__instance.difficulty == 0) {
				component.speed *= 0.5f;
			}
			component.damage *= __instance.eid.totalDamageModifier;
		} else {
			component.Explode();
		}

		__instance.currentBurst++;
		__instance.readyToShoot = false;
		if (__instance.difficulty == 19) {
			__instance.Invoke("ReadyToShoot", 0.025f / __instance.eid.totalSpeedModifier); // Brutal: 0.05f instead of 0.025f
			return false;
		}
		if (__instance.difficulty >= 4) {
			__instance.Invoke("ReadyToShoot", 0.05f / __instance.eid.totalSpeedModifier);
			return false;
		}
		if (__instance.difficulty > 0) {
			__instance.Invoke("ReadyToShoot", 0.1f / __instance.eid.totalSpeedModifier);
			return false;
		}
		__instance.Invoke("ReadyToShoot", 0.2f / __instance.eid.totalSpeedModifier);
		return false;
	}
}