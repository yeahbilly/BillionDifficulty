using ULTRAKILL.Cheats;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using UnityEngine.AI;
using System;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: MALICIOUSFACE !!!
[HarmonyPatch(typeof(MaliciousFace))]
public class MaliciousFacePatch {
	// MAURICE PATCH (burst number and cooldown)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MaliciousFace), nameof(MaliciousFace.Start))]
	public static void StartPostfix(MaliciousFace __instance) {
		if (__instance.difficulty != 19)
			return;
		
		float hardModeMult = (!Util.IsHardMode()) ? 1f : 1.2f;
		__instance.coolDownMultiplier = 1.4f * hardModeMult; // Brutal: 1.25f
		__instance.maxBurst = 13; // Brutal: 10

		if (!Util.IsHardMode())
			return;

		__instance.maxBurst = 18; // Brutal: 10
		Plugin.SetEnemyWeakness("hammer", 0.6f, __instance.eid); // default: 1.5f
	}

	// MAURICE PATCH (explosion parry and projectile damage reduction)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(MaliciousFace), nameof(MaliciousFace.OnParry))]
	public static bool OnParryPrefix(ref DamageData data, bool isShotgun, MaliciousFace __instance) {
		if (__instance.difficulty != 19)
			return true;
		if (!__instance.spiderParryable)
			return false;

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

	// MAURICE PATCH (more beam)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MaliciousFace), nameof(MaliciousFace.AttackCheck))]
	public static void AttackCheckPostfix(TargetData targetData, MaliciousFace __instance) {
		if (!Util.IsDifficulty(19))
			return;
		
		__instance.beamsAmount = 2;
		if (__instance.isEnraged) {
			__instance.beamsAmount = 3;
		}
	}

	// MAURICE PATCH (hard mode shockwave)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(MaliciousFace), nameof(MaliciousFace.HandleCollision))]
	public static bool HandleCollisionPrefix(Collision other, MaliciousFace __instance) {
		if (!Util.IsHardMode())
			return true;

		if (other.gameObject.CompareTag("Moving")) {
			__instance.BreakCorpse();
			MonoSingleton<CameraController>.Instance.CameraShake(2f);
			return false;
		}
		if (LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment)) {
			Breakable breakable;
			if (other.gameObject.CompareTag("Floor")) {
				__instance.rb.isKinematic = true;
				__instance.rb.SetGravityMode(false);
				Transform transform = __instance.transform;
				UnityObject.Instantiate<GameObject>(__instance.impactParticle, transform.position, transform.rotation);
				__instance.spriteRot.eulerAngles = new Vector3(other.contacts[0].normal.x + 90f, other.contacts[0].normal.y, other.contacts[0].normal.z);
				__instance.spritePos = new Vector3(other.contacts[0].point.x, other.contacts[0].point.y + 0.1f, other.contacts[0].point.z);
				// AudioSource componentInChildren = UnityObject.Instantiate<GameObject>(__instance.shockwave.ToAsset(), __instance.spritePos, Quaternion.identity).GetComponentInChildren<AudioSource>();
				// if (componentInChildren) {
				// 	Object.Destroy(componentInChildren);
				// }

				GameObject shockwaveObject = UnityObject.Instantiate<GameObject>(Plugin.Prefabs["PhysicalShockwave"], __instance.transform.position - 2f * __instance.transform.up, Quaternion.identity);
				PhysicalShockwave shockwave = shockwaveObject.GetComponent<PhysicalShockwave>();
				shockwave.speed = 50f; //50f; // Brutal (cerberus): 75f
				shockwave.damage = Mathf.RoundToInt(25f * __instance.eid.totalDamageModifier);
				shockwave.maxSize = 75f; // default (cerberus): 100f
				shockwave.enemy = true;
				shockwave.enemyType = EnemyType.MaliciousFace;

				Transform transform2 = __instance.transform;
				transform2.position -= transform2.up * 1.5f;
				__instance.spiderFalling = false;
				__instance.rb.excludeLayers = default(LayerMask);
				MaliciousFaceCatcher maliciousFaceCatcher;
				if (!other.gameObject.TryGetComponent<MaliciousFaceCatcher>(out maliciousFaceCatcher)) {
					UnityObject.Instantiate<GameObject>(__instance.impactSprite, __instance.spritePos, __instance.spriteRot).transform.SetParent(__instance.gz.goreZone, true);
				}
				SphereCollider obj;
				if (__instance.TryGetComponent<SphereCollider>(out obj)) {
					UnityObject.Destroy(obj);
				}
				SpiderBodyTrigger componentInChildren2 = __instance.transform.parent.GetComponentInChildren<SpiderBodyTrigger>(true);
				if (componentInChildren2) {
					UnityObject.Destroy(componentInChildren2.gameObject);
				}
				__instance.rb.GetComponent<NavMeshObstacle>().enabled = true;
				MonoSingleton<CameraController>.Instance.CameraShake(2f);
				if (__instance.fallEnemiesHit.Count > 0) {
					foreach (EnemyIdentifier enemyIdentifier in __instance.fallEnemiesHit) {
						Collider collider;
						if (enemyIdentifier != null && !enemyIdentifier.dead && enemyIdentifier.TryGetComponent<Collider>(out collider)) {
							Physics.IgnoreCollision(__instance.headCollider, collider, false);
						}
					}
					__instance.fallEnemiesHit.Clear();
					return false;
				}
			} else if (other.gameObject.TryGetComponent<Breakable>(out breakable) && !breakable.playerOnly && !breakable.specialCaseOnly) {
				breakable.Break();
			}
		}
		return false;
	}

	// MAURICE PATCH (projectile burst)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(MaliciousFace), nameof(MaliciousFace.ShootProj))]
	public static bool ShootProjPrefix(ref TargetData targetData, MaliciousFace __instance) {
		if (__instance.difficulty != 19)
			return true;
		
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
		Projectile proj = __instance.currentProj.GetComponent<Projectile>();
		if (__instance.difficulty == 19) {
			proj.GetComponent<RemoveOnTime>().time = 3f;
			proj.enemyDamageMultiplier = 0.175f; // default: 1
			proj.damage = 20f; // default: 25f
			if (Util.IsHardMode()) {
				proj.enemyDamageMultiplier = 0f;
			}
		}

		float projOffset = (float)(1 + __instance.currentBurst / 5 * 2);
		if (__instance.difficulty == 19) {
			switch (__instance.currentBurst % 9) {
				case 0:
					__instance.currentProj.transform.LookAt(vector);
					__instance.currentProj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); // default: 1, 1, 1
					proj.damage = 40f;
					break;
				case 1:
					__instance.currentProj.transform.LookAt(vector + __instance.transform.right * projOffset);
					break;
				case 2:
					__instance.currentProj.transform.LookAt(
						vector + __instance.transform.right * projOffset
						+ __instance.transform.up * projOffset
					);
					break;
				case 3:
					__instance.currentProj.transform.LookAt(vector + __instance.transform.up * projOffset);
					break;
				case 4:
					__instance.currentProj.transform.LookAt(
						vector + __instance.transform.up * projOffset
						- __instance.transform.right * projOffset
					);
					break;
				case 5:
					__instance.currentProj.transform.LookAt(vector - __instance.transform.right * projOffset);
					break;
				case 6:
					__instance.currentProj.transform.LookAt(
						vector - __instance.transform.right * projOffset
						- __instance.transform.up * projOffset
						);
					break;
				case 7:
					__instance.currentProj.transform.LookAt(vector - __instance.transform.up * projOffset);
					break;
				case 8:
					__instance.currentProj.transform.LookAt(
						vector - __instance.transform.up * projOffset
						+ __instance.transform.right * projOffset
					);
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

		if (!flag) {
			proj.safeEnemyType = EnemyType.MaliciousFace;
			proj.targetHandle = targetData.handle;
			if (__instance.difficulty == 19)
				proj.speed *= 1.5f; // Brutal: 1.25f
			else if (__instance.difficulty > 2)
				proj.speed *= 1.25f;
			else if (__instance.difficulty == 1)
				proj.speed *= 0.75f;
			else if (__instance.difficulty == 0)
				proj.speed *= 0.5f;
			proj.damage *= __instance.eid.totalDamageModifier;
		} else {
			proj.Explode();
		}

		__instance.currentBurst++;
		__instance.readyToShoot = false;

		float invokeTime;
		if (Util.IsHardMode()) 
			invokeTime = 0.02f;
		else if (__instance.difficulty == 19)
			invokeTime = 0.025f; // Brutal: 0.05f
		else if (__instance.difficulty >= 4)
			invokeTime = 0.05f;
		else if (__instance.difficulty > 0)
			invokeTime = 0.1f;
		else
			invokeTime = 0.2f;

		__instance.Invoke("ReadyToShoot", invokeTime / __instance.eid.totalSpeedModifier);
		return false;
	}
}