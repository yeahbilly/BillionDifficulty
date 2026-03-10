using Billion = BillionDifficulty.Plugin;
using BillionDifficulty.EnemyPatches;
using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.GenaralPatches;

// PLAYER PATCHES
[HarmonyPatch(typeof(NewMovement))]
public class NewMovementPatch {
	// hard damage increase
	[HarmonyPrefix]
	[HarmonyPatch(typeof(NewMovement), nameof(NewMovement.GetHurt))]
	public static bool GetHurtPrefix(NewMovement __instance, out float __state) {
		__state = __instance.antiHp;
		return true;
	}
	[HarmonyPostfix]
	[HarmonyPatch(typeof(NewMovement), nameof(NewMovement.GetHurt))]
	public static void GetHurtPostfix(NewMovement __instance, float __state) {
		if (__instance.difficulty != 19) {
			return;
		}

		float num = __instance.antiHp - __state;
		if (num > 0f) {
			num *= 50f/35f; // Brutal: 35% hard damage
			__instance.antiHp = __state + num;
		}
	}
	
	// stops streetcleaner afterburn when respawning
	[HarmonyPrefix]
	[HarmonyPatch(typeof(NewMovement), nameof(NewMovement.Respawn))]
	public static void RespawnPrefix(NewMovement __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		StreetcleanerAfterburn[] allAfterburns = UnityObject.FindObjectsByType<StreetcleanerAfterburn>(FindObjectsSortMode.None);
		foreach (StreetcleanerAfterburn afterburn in allAfterburns) {
			afterburn.ticks = afterburn.tickCount;
		}
	}

}


// radiance speed patches
[HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.Start))]
public class RadianceSpeedPatch1 {
	public static void Postfix(EnemyIdentifier __instance) {
		if (!Util.IsDifficulty(19)) {
			return;
		}

		switch (__instance.enemyType) {
			case EnemyType.Turret:
			case EnemyType.Virtue:
				__instance.speedBuffModifier = 1.1f;
				break;
			case EnemyType.Ferryman:
			case EnemyType.Sisyphus:
			case EnemyType.Providence:
			case EnemyType.Power:
				__instance.speedBuffModifier = 1.15f;
				break;
			default:
				__instance.speedBuffModifier = 1.25f;
				break;
		}
	}
}
[HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.SpeedBuff), new Type[] {typeof(float)})]
public class RadianceSpeedPatch2 {
	public static bool Prefix(float modifier, EnemyIdentifier __instance) {
		if (__instance.difficulty != 19) {
			return true;
		}

		__instance.speedBuffRequests++;
		switch (__instance.enemyType) {
			case EnemyType.Turret:
			case EnemyType.Virtue:
				__instance.speedBuffModifier = 1.1f;
				break;
			case EnemyType.Ferryman:
			case EnemyType.Sisyphus:
			case EnemyType.Providence:
			case EnemyType.Power:
				__instance.speedBuffModifier = 1.15f;
				break;
			default:
				__instance.speedBuffModifier = 1.25f;
				break;
		}
		__instance.UpdateBuffs(false, true);
		return false;
	}
}


// explosion controller
[HarmonyPatch(typeof(Projectile), nameof(Projectile.Explode))]
public class ExplosionPatch {
	public static bool Prefix(ref Projectile __instance) {
		if (!__instance.active) {
			return false;
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
				BillionExplosionController excon = explosion.gameObject.GetComponent<BillionExplosionController>();
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


// cybergrind wave end cleanup patch
[HarmonyPatch(typeof(EndlessGrid))]
public class EndlessGridPatch {
	[HarmonyPrefix]
	[HarmonyPatch(typeof(EndlessGrid), nameof(EndlessGrid.NextWave))]
	public static void NextWavePrefix() {
		if (!Util.IsDifficulty(19)) {
			return;
		}

		DestroyAllOfType<SandificationZone>(); // removes sand zones
		DestroyAllOfType<SlowDownOverTime>(); // removes mindflayer green orbs
		DestroyAllOfType<SlowDownOverTimeEase>(); // removes schism projectiles
	}

	public static void DestroyAllOfType<T>() where T : UnityObject {
		T[] objs = UnityObject.FindObjectsByType<T>(FindObjectsSortMode.None);
		foreach (T obj in objs) {
			UnityObject.Destroy((obj as Component)?.transform.root.gameObject);
		}
	}
}


// STYLE PATCHES
[HarmonyPatch(typeof(StyleHUD))]
public class StyleHUDPatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(StyleHUD), nameof(StyleHUD.Start))]
	public static void StartPostfix(Dictionary<string, string> ___idNameDict) {
		if (!___idNameDict.ContainsKey("billion.blue")) {
			___idNameDict.Add("billion.blue", "<color=#00ffffff>I'M BLUE</color>");
		}
	}
}


// blue filth
[HarmonyPatch(typeof(ZombieMelee), nameof(ZombieMelee.Start))]
public class BlueFilthPatch {
	public static void Postfix(ZombieMelee __instance) {
		bool gotBlueFilth = __instance.difficulty == 19 && __instance.eid.enemyType == EnemyType.Filth && UnityEngine.Random.Range(0, 300) == 0;
		if (!gotBlueFilth) {
			return;
		}

		Material blueFilthMaterial = UnityObject.Instantiate(__instance.originalMaterial);
		Material blueFilthBiteMaterial = UnityObject.Instantiate(__instance.biteMaterial);
		blueFilthMaterial.mainTexture = Billion.blueFilthTexture;
		blueFilthBiteMaterial.mainTexture = Billion.blueFilthBiteTexture;

		__instance.originalMaterial = blueFilthMaterial;
		__instance.biteMaterial = blueFilthBiteMaterial;

		foreach (SkinnedMeshRenderer renderer in __instance.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>()) {
			renderer.material = blueFilthMaterial;
			var block = new MaterialPropertyBlock();
			renderer.GetPropertyBlock(block);
			block.SetTexture("_MainTex", Billion.blueFilthTexture);
			renderer.SetPropertyBlock(block);
		}
	}
}