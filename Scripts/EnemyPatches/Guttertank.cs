using Billion = BillionDifficulty.Plugin;

using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: GUTTERTANK !!!
[HarmonyPatch(typeof(Guttertank))]
public class GuttertankPatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Guttertank), nameof(Guttertank.SetSpeed))]
	public static void SetSpeedPostfix(Guttertank __instance) {
		if (__instance.difficulty != 19) {
			return;
		}
		__instance.anim.speed = 1.2f * __instance.eid.totalSpeedModifier; // Brutal: 1f
		__instance.nma.speed = 20f * __instance.anim.speed;
	}

	// GUTTERTANK PATCH (speed)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Guttertank), nameof(Guttertank.GetSpeed))]
	public static bool GetSpeedPrefix(int difficulty, ref EnemyMovementData __result) {
		if (difficulty != 19) {
			return true;
		}
		__result = new EnemyMovementData {
			speed = 24f, // Brutal: 20f
			angularSpeed = 1200f, // default: 1200f
			acceleration = 80f // default: 80f
		};
		return false;
	}
}

// GUTTERTANK PATCH (projectile explosion)
[HarmonyPatch(typeof(Grenade), nameof(Grenade.Explode))]
public class GuttertankGrenadePatch {
	public static void Prefix(bool big, bool harmless, bool super, float sizeMultiplier, bool ultrabooster, GameObject exploderWeapon, bool fup, ref Grenade __instance) {
		if (harmless) {
			return;
		}

		bool isFromGuttertank = __instance.originEnemy != null && __instance.originEnemy.difficulty == 19 && __instance.originEnemy.enemyType == EnemyType.Guttertank;
		if (!isFromGuttertank) {
			return;
		}

		for (int projCount = 0; projCount < 20; projCount++) {
			EnemyIdentifier eid = __instance.originEnemy;
			Vector3 targetPosition = (eid.target == null) ? NewMovement.Instance.transform.position : eid.target.position;
			Vector3 aimDirection = targetPosition - __instance.transform.position;
			if (projCount != 0) {
				aimDirection += new Vector3(
					Random.Range(-15f, 15f),
					Random.Range(-15f, 15f),
					Random.Range(-15f, 15f)
				);
			}
			aimDirection.Normalize();

			// __instance.transform.position + 6f * aimDirection
			Projectile currentProjectile = UnityObject.Instantiate<GameObject>(Billion.Projectile, __instance.transform.position + 6f * aimDirection, Quaternion.LookRotation(aimDirection)).GetComponent<Projectile>();
			currentProjectile.speed = 80f;
			currentProjectile.safeEnemyType = EnemyType.Guttertank;
			//currentProjectile.ignoreExplosions = true; // the projectiles don't do damage with this
			#pragma warning disable CS0618 // Type or member is obsolete
			currentProjectile.target = eid.target;
			#pragma warning restore CS0618 // Type or member is obsolete
			currentProjectile.friendly = false;
			currentProjectile.damage = 30; // default: 30
			currentProjectile.enemyDamageMultiplier = 0.2f;
			currentProjectile.transform.localScale *= 2f;
		}
	}
}