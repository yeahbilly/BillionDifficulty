using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: GUTTERTANK !!!
[HarmonyPatch(typeof(Guttertank))]
public class GuttertankPatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Guttertank), nameof(Guttertank.Start))]
	public static void StartPostfix(Guttertank __instance) {
		if (!Util.IsHardMode())
			return;

		TimerFloat timer = __instance.gameObject.AddComponent<TimerFloat>();		
		timer.cooldownMax = 3f;
		timer.cooldown = 3f;
		timer.Run();
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Guttertank), nameof(Guttertank.SetSpeed))]
	public static void SetSpeedPostfix(Guttertank __instance) {
		if (__instance.difficulty != 19)
			return;

		float hardModeMult = (!Util.IsHardMode()) ? 1f : 1.1f;
		__instance.anim.speed = 1.2f * hardModeMult * __instance.eid.totalSpeedModifier; // Brutal: 1f
		__instance.nma.speed = 20f * __instance.anim.speed;
	}

	// GUTTERTANK PATCH (speed)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Guttertank), nameof(Guttertank.GetSpeed))]
	public static bool GetSpeedPrefix(int difficulty, ref EnemyMovementData __result) {
		if (difficulty != 19)
			return true;
		
		float hardModeMult = (!Util.IsHardMode()) ? 1f : 1.1f;
		__result = new EnemyMovementData {
			speed = 24f * hardModeMult, // Brutal: 20f
			angularSpeed = 1200f, // default: 1200f
			acceleration = 80f // default: 80f
		};
		return false;
	}

	// GUTTERTANK PATCH (mirror reaper hand)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Guttertank), nameof(Guttertank.PlaceMine))]
	public static void PlaceMinePrefix(Guttertank __instance) {
		if (!Util.IsHardMode())
			return;
		
		TimerFloat timer = __instance.GetComponent<TimerFloat>();
		if (!timer.reached)
			return;
		timer.ResetAndRun();
		
		GroundWave groundWave = UnityObject.Instantiate<GameObject>(Plugin.Prefabs["MirrorReaperGroundWave"], __instance.transform.position, __instance.transform.rotation)
			.GetComponent<GroundWave>();
		groundWave.target = __instance.eid.target;
		groundWave.transform.SetParent(__instance.transform.parent ? __instance.transform.parent : GoreZone.ResolveGoreZone(__instance.transform).transform);
		groundWave.lifetime = 15f;
		Breakable componentInChildren = groundWave.GetComponentInChildren<Breakable>();
		if (componentInChildren) {
			componentInChildren.durability = 5f;
		}
		groundWave.eid = __instance.eid;
		groundWave.difficulty = __instance.difficulty;
	}

	// GUTTERTANK PATCH (mine placement)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Guttertank), nameof(Guttertank.CheckMines))]
	public static void CheckMinesPostfix(Guttertank __instance, ref bool __result) {
		if (!Util.IsHardMode())
			return;
		if (UnityEngine.Random.Range(1, 101) > 22)
			__result = true;
	}

	// GUTTERTANK PATCH (mine cooldown)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Guttertank), nameof(Guttertank.PrepMine))]
	public static void PrepMinePostfix(Guttertank __instance) {
		if (!Util.IsHardMode())
			return;
		__instance.mineCooldown = 1.25f;
	}
}


// GUTTERTANK PATCH (projectile explosion)
[HarmonyPatch(typeof(Grenade), nameof(Grenade.Explode))]
public class GuttertankGrenadePatch {
	public static void Prefix(bool big, bool harmless, bool super, float sizeMultiplier, bool ultrabooster, GameObject exploderWeapon, bool fup, Grenade __instance) {
		if (harmless)
			return;
		bool isFromGuttertank = __instance.originEnemy != null && __instance.originEnemy.difficulty == 19 && __instance.originEnemy.enemyType == EnemyType.Guttertank;
		if (!isFromGuttertank)
			return;

		for (int projCount = 0; projCount < 12; projCount++) {
			EnemyIdentifier eid = __instance.originEnemy;
			Vector3 targetPosition = (eid.target == null) ? NewMovement.Instance.transform.position : eid.target.position;
			Vector3 aimDirection = targetPosition - __instance.transform.position;
			// if (projCount != 0) {
			// 	aimDirection += new Vector3(
			// 		Random.Range(-15f, 15f),
			// 		Random.Range(-15f, 15f),
			// 		Random.Range(-15f, 15f)
			// 	);
			// }
			aimDirection.Normalize();

			// __instance.transform.position + 6f * aimDirection
			Projectile currentProjectile = UnityObject.Instantiate<GameObject>(
				Plugin.Prefabs["Projectile"],
				__instance.transform.position,// + 6f * aimDirection,
				Quaternion.LookRotation(aimDirection)
			).GetComponent<Projectile>();

			if (projCount != 0) {
				currentProjectile.transform.Rotate(
					new Vector3(
						3f * Mathf.RoundToInt(Random.Range(-12f, 12f)),
						3f * Mathf.RoundToInt(Random.Range(-12f, 12f)),
						3f * Mathf.RoundToInt(Random.Range(-12f, 12f))
					),
					Space.Self
				);
			}
			currentProjectile.transform.position += 6f * currentProjectile.transform.forward;

			currentProjectile.transform.localScale *= 3f;
			currentProjectile.speed = 80f;
			currentProjectile.safeEnemyType = EnemyType.Guttertank;
			currentProjectile.ignoreEnvironment = true;
			//currentProjectile.ignoreExplosions = true; // the projectiles don't do damage with this
			#pragma warning disable CS0618 // Type or member is obsolete
			currentProjectile.target = eid.target;
			#pragma warning restore CS0618 // Type or member is obsolete
			currentProjectile.friendly = false;
			currentProjectile.damage = 30; // default: 30
			currentProjectile.enemyDamageMultiplier = 0f;
			currentProjectile.GetComponent<RemoveOnTime>().time = 2f;
		}
	}
}