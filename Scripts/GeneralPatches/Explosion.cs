using BillionDifficulty.EnemyPatches;
using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.GenaralPatches;

// explosion controller
[HarmonyPatch(typeof(Projectile), nameof(Projectile.Explode))]
public class ExplosionPatch {
	public static bool Prefix(Projectile __instance) {
		if (!Util.IsDifficulty(19))
			return true;
		if (!__instance.active)
			return false;

		var shockwaveOnExplode = __instance.GetComponent<ShockwaveOnExplode>();
		if (shockwaveOnExplode != null) {
			shockwaveOnExplode.SpawnShockwave(__instance.transform.position);
		}

		__instance.active = false;
		if (__instance.keepTrail) {
			__instance.KeepTrail();
		}
		GameObject wholeExplosion = UnityObject.Instantiate<GameObject>(
			__instance.explosionEffect,
			__instance.transform.position - __instance.rb.velocity * 0.02f,
			__instance.transform.rotation
		);
		foreach (Explosion explosion in wholeExplosion.GetComponentsInChildren<Explosion>()) {
			explosion.sourceWeapon = (__instance.sourceWeapon ?? explosion.sourceWeapon);
			if (__instance.bigExplosion) {
				explosion.maxSize *= 1.5f;
			}
			if (explosion.damage != 0) {
				explosion.damage = Mathf.RoundToInt(__instance.damage);
			}
			explosion.enemy = true;

			if (__instance.difficulty == 19) {
				BillionExplosionController excon = explosion.GetComponent<BillionExplosionController>();
				if (excon != null) {
					explosion.maxSize *= excon.maxSizeMultiplier;
					explosion.speed *= excon.speedMultiplier;
					explosion.enemyDamageMultiplier *= excon.enemyDamageMultiplier;
					if (excon.damage == -1) {
						explosion.damage = Mathf.RoundToInt(excon.damageMultiplier * (float)explosion.damage);
					} else {
						explosion.damage = excon.damage;
					}
				}
			}
		}
		StainVoxelManager.Instance.TryIgniteAt(__instance.transform.position, 3);
		UnityObject.Destroy(__instance.gameObject);
		return false;
	}
}