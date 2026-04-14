using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;

namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: V2 !!!
[HarmonyPatch(typeof(V2))]
public class V2Patch {
	// V2 PATCH (speed)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(V2), nameof(V2.SetSpeed))]
	public static bool SetSpeedPrefix(V2 __instance) {
		if (__instance.difficulty != 19)
			return true;

		if (!__instance.nma)
			__instance.nma = __instance.GetComponent<NavMeshAgent>();
		if (!__instance.eid)
			__instance.eid = __instance.GetComponent<EnemyIdentifier>();
		if (__instance.difficulty < 0)
			__instance.difficulty = Enemy.InitializeDifficulty(__instance.eid);
		
		if (__instance.originalMovementSpeed != 0f) {
			__instance.movementSpeed = __instance.originalMovementSpeed;
		} else {
			switch (__instance.difficulty) {
				case 0:
					__instance.movementSpeed *= 0.65f;
					break;
				case 1:
					__instance.movementSpeed *= 0.75f;
					break;
				case 2:
					__instance.movementSpeed *= 0.85f;
					break;
				case 3:
					__instance.movementSpeed *= 1f;
					break;
				case 4:
				case 5:
				case 19:
					__instance.movementSpeed *= 1.5f;
					break;
			}
			__instance.movementSpeed *= __instance.eid.totalSpeedModifier;
			__instance.originalMovementSpeed = __instance.movementSpeed;
		}
		if (__instance.enraged) {
			__instance.movementSpeed *= 2f;
		}
		if (__instance.nma) {
			__instance.nma.speed = __instance.originalMovementSpeed;
		}
		GameObject[] array = __instance.weapons;
		for (int i = 0; i < array.Length; i++) {
			array[i].transform.GetChild(0).SendMessage("UpdateBuffs", __instance.eid, SendMessageOptions.DontRequireReceiver);
		}
		return false;
	}

	// V2 PATCH (attack cooldown)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(V2), nameof(V2.UpdateCooldowns))]
	public static bool UpdateCooldownsPrefix(V2 __instance) {
		if (__instance.difficulty != 19)
			return true;

		if (__instance.patternCooldown > 0f) {
			__instance.patternCooldown = Mathf.MoveTowards(__instance.patternCooldown, 0f, Time.deltaTime);
		}
		if (__instance.shootCooldown > 0f || __instance.altShootCooldown > 0f) {
			float num = 1f;
			// if (__instance.difficulty == 1)
			// 	num = 0.85f;
			// if (__instance.difficulty == 0)
			// 	num = 0.75f;
			if (__instance.difficulty == 19)
				num = 1.25f; // default: 1f
			if (Util.IsHardMode())
				num = 1.45f;

			if (__instance.shootCooldown > 0f) {
				__instance.shootCooldown = Mathf.MoveTowards(__instance.shootCooldown, 0f, Time.deltaTime * num * (__instance.cowardPattern ? 0.5f : 1f) * __instance.eid.totalSpeedModifier);
			}
			if (__instance.altShootCooldown > 0f) {
				__instance.altShootCooldown = Mathf.MoveTowards(__instance.altShootCooldown, 0f, Time.deltaTime * num * __instance.eid.totalSpeedModifier);
			}
		}
		if (__instance.dodgeCooldown < 6f) {
			float num2 = 1f;
			switch (__instance.difficulty) {
				case 0:
				case 1:
				case 2:
					num2 = 0.1f;
					break;
				case 3:
					num2 = 0.5f;
					break;
				case 4:
				case 5:
					num2 = 1f;
					break;
				case 19:
					num2 = 1.25f;
					break;
			}
			if (Util.IsHardMode())
				num2 = 1.4f;
			__instance.dodgeCooldown = Mathf.MoveTowards(__instance.dodgeCooldown, 6f, Time.deltaTime * num2 * __instance.eid.totalSpeedModifier);
		}
		if (__instance.dodgeLeft > 0f) {
			__instance.dodgeLeft = Mathf.MoveTowards(__instance.dodgeLeft, 0f, Time.deltaTime * 3f * __instance.eid.totalSpeedModifier);
			if (__instance.dodgeLeft <= 0f) {
				__instance.DodgeEnd();
			}
		}
		if (__instance.secondEncounter && (__instance.coins.Count == 0 || (__instance.aboutToShoot && __instance.shootingForCoin))) {
			switch (__instance.difficulty) {
				case 0:
					__instance.coinsInSightCooldown = 0.8f;
					break;
				case 1:
					__instance.coinsInSightCooldown = 0.6f;
					break;
				case 2:
					__instance.coinsInSightCooldown = 0.4f;
					break;
				case 3:
					__instance.coinsInSightCooldown = 0.2f;
					break;
				case 4:
				case 5:
				case 19:
					__instance.coinsInSightCooldown = 0f;
					break;
			}
		}
		if (__instance.inPattern) {
			__instance.DistancePatience();
		}
		return false;
	}

	// V2 PATCH (fix dodges)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(V2), nameof(V2.Dodge))]
	public static bool DodgePrefix(Transform projectile, V2 __instance) {
		if (__instance.difficulty != 19)
			return true;
		if (__instance.target == null || !__instance.active || __instance.dodgeLeft > 0f || __instance.chargingAlt)
			return false;
		if (Vector3.Distance(__instance.transform.position, __instance.target.position) <= 15f)
			return false;

		if (__instance.dodgeCooldown >= 1.5f /*6 - __instance.difficulty*/) {
			__instance.dodgeCooldown -= 1.5f /*6 - __instance.difficulty*/;
			Vector3 direction = new Vector3(__instance.transform.position.x - projectile.position.x, 0f, __instance.transform.position.z - projectile.position.z);
			if (__instance.currentPattern == V2Pattern.Chase) {
				direction = direction.normalized + (__instance.targetPos - __instance.transform.position).normalized;
			}
			__instance.DodgeNow(direction);
			__instance.ChangeDirection((float)((Random.Range(0f, 1f) > 0.5f) ? 90 : -90));
			return false;
		}
		if (__instance.gc.onGround && !__instance.jumping && !__instance.slideOnly) {
			if (__instance.cowardPattern) {
				__instance.Jump();
				return false;
			}
			float num = Random.Range(0f, (__instance.difficulty >= 3) ? 2f : 3f);
			if (num > 1f)
				return false;

			if (num > 0.75f) {
				__instance.Jump();
				return false;
			}
			__instance.Slide();
		}
		return false;
	}

	// V2 PATCH (shoot fix)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(V2), nameof(V2.ShootCheck))]
	public static bool ShootCheckPrefix(V2 __instance) {
		if (__instance.difficulty != 19)
			return true;

		float num = Vector3.Distance(__instance.target.position, __instance.transform.position);
		if (!__instance.aboutToShoot) {
			if (num <= 15f) {
				__instance.SwitchWeapon(1);
			} else if (__instance.weapons.Length > 2 && num < 25f && __instance.eid.stuckMagnets.Count <= 0) {
				__instance.SwitchWeapon(2);
			} else {
				__instance.SwitchWeapon(0);
			}
		}
		if (Physics.Raycast(__instance.transform.position + Vector3.up * 2f, __instance.target.position - __instance.transform.position, out __instance.rhit, Vector3.Distance(__instance.transform.position, __instance.target.position), LayerMaskDefaults.Get(LMD.Environment))) {
			if (__instance.altShootCooldown <= 0f && __instance.rhit.transform != null && __instance.rhit.transform.gameObject.CompareTag("Breakable")) {
				__instance.predictAmount = 0f;
				__instance.aimAtGround = false;
				if (__instance.distancePatience >= 4f) {
					__instance.shootCooldown = 1f;
				} else {
					__instance.shootCooldown = (__instance.difficulty > 2) ? Random.Range(1f, 2f) : 2f;
				}
				__instance.altShootCooldown = 5f;
				__instance.weapons[__instance.currentWeapon].transform.GetChild(0).SendMessage("PrepareAltFire");
				__instance.aboutToShoot = true;
				__instance.chargingAlt = true;
				__instance.Invoke("AltShootWeapon", 1f / __instance.eid.totalSpeedModifier);
			}
			return false;
		}
		__instance.aboutToShoot = true;
		if (__instance.altShootCooldown <= 0f || (__instance.distancePatience >= 8f && __instance.currentWeapon == 0 && !__instance.dontEnrage)) {
			__instance.aimAtGround = __instance.currentWeapon != 0 || __instance.weapons.Length == 1;
			if (__instance.currentWeapon == 0) {
				__instance.predictAmount = 0.15f / __instance.eid.totalSpeedModifier;
			} else if (__instance.currentWeapon == 1 || __instance.difficulty > 2) {
				__instance.predictAmount = 0.25f / __instance.eid.totalSpeedModifier;
			} else {
				__instance.predictAmount = -0.25f / __instance.eid.totalSpeedModifier;
			}
			__instance.shootCooldown = (__instance.difficulty > 2) ? Random.Range(1f, 2f) : 2f;
			__instance.altShootCooldown = 5f;
			if (__instance.secondEncounter && num >= 8f && !__instance.enraged && Random.Range(0f, 1f) < 0.5f) {
				__instance.SwitchWeapon(0);
				__instance.coinsToThrow = (__instance.difficulty >= 2) ? 3 : 1;
				__instance.ThrowCoins();
				return false;
			}
			__instance.chargingAlt = true;
			__instance.weapons[__instance.currentWeapon].transform.GetChild(0).SendMessage("PrepareAltFire", SendMessageOptions.DontRequireReceiver);
			float num2 = 1f;
			switch (__instance.difficulty) {
				case 0:
					num2 = 1.5f;
					break;
				case 1:
					num2 = 1.25f;
					break;
				case 2:
				case 3:
				case 4:
				case 5:
					num2 = 1f;
					break;
				case 19:
					num2 = 0.8f;
					break;
			}
			if (Util.IsHardMode())
				num2 = 0.55f;
			__instance.Invoke("AltShootWeapon", num2 / __instance.eid.totalSpeedModifier);
		} else {
			if (__instance.currentWeapon == 0) {
				__instance.predictAmount = 0f;
			} else if (__instance.currentWeapon == 1 || __instance.difficulty > 2) {
				__instance.predictAmount = 0.15f / __instance.eid.totalSpeedModifier;
			} else {
				__instance.predictAmount = -0.25f / __instance.eid.totalSpeedModifier;
			}
			if (__instance.currentWeapon == 0 && __instance.distancePatience >= 4f) {
				__instance.shootCooldown = 1f;
			} else {
				__instance.shootCooldown = (__instance.difficulty > 2) ? Random.Range(1.5f, 2f) : 2f;
			}
			__instance.weapons[__instance.currentWeapon].transform.GetChild(0).SendMessage("PrepareFire", SendMessageOptions.DontRequireReceiver);
			if (__instance.currentWeapon == 0) {
				__instance.shootingForCoin = false;
				__instance.Flash();
				if (__instance.difficulty >= 2) {
					__instance.Invoke("ShootWeapon", 0.75f / __instance.eid.totalSpeedModifier);
				}
				if (__instance.difficulty >= 1) {
					__instance.Invoke("ShootWeapon", 0.95f / __instance.eid.totalSpeedModifier);
				}
				__instance.Invoke("ShootWeapon", 1.15f / __instance.eid.totalSpeedModifier);
				return false;
			}
			float num3 = 1f;
			switch (__instance.difficulty) {
				case 0:
					num3 = 1.25f;
					break;
				case 1:
					num3 = 1f;
					break;
				case 2:
				case 3:
				case 4:
				case 5:
					num3 = 0.75f;
					break;
				case 19:
					num3 = 0.6f;
					break;
			}
			if (Util.IsHardMode())
				num3 = 0.4f;
			__instance.Invoke("ShootWeapon", num3 / __instance.eid.totalSpeedModifier);
		}
		return false;
	}
}