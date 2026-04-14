using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: FERRYMAN !!!
[HarmonyPatch(typeof(Ferryman))]
public class FerrymanPatch {
	// FERRYMAN PATCH (setup blueing)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Ferryman), nameof(Ferryman.Start))]
	public static void StartPrefix(Ferryman __instance) {
		__instance.difficulty = Util.GetDifficulty();
		if (__instance.difficulty != 19)
			return;

		if (__instance.eid == null) {
			__instance.eid = __instance.GetComponent<EnemyIdentifier>();
		}

		FerrymanStuff fs = __instance.gameObject.AddComponent<FerrymanStuff>();
		if (!Util.IsHardMode()) {
			fs.initialSpeed = 1.1f;
			fs.maxSpeed = 1.3f;
		} else {
			fs.initialSpeed = 1.15f;
			fs.maxSpeed = 1.35f;
		}
		fs.changeColor = true;
		fs.targetFerrymanColor = new Color(0.4f, 0.4f, 1f); // (0.5f, 0.5f, 1f)
		fs.targetCloakColor = new Color(0.55f, 0.55f, 1f); // (0.65f, 0.65f, 1f)
		fs.eid = __instance.eid;

		OriginalHealth oh = __instance.gameObject.AddComponent<OriginalHealth>();
		oh.health = __instance.mach.health;

		BoolValue bv = __instance.gameObject.AddComponent<BoolValue>();
		bv.description = "enraged";
		bv.value = false;

		if (Util.IsHardMode())
			__instance.gameObject.AddComponent<FerrymanCirclingProjectiles>();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Ferryman), nameof(Ferryman.Update))]
	public static void UpdatePrefix(Ferryman __instance) {
		if (__instance.difficulty != 19)
			return;
		__instance.SetSpeed();

		if (!Util.IsHardMode())
			return;
		
		OriginalHealth oh = __instance.GetComponent<OriginalHealth>();
		if (__instance.eid.health > 0.5f * oh.health || BoolValue.Get("enraged", __instance.gameObject) == true)
			return;

		BoolValue.Set("enraged", true, __instance.gameObject);
		FerrymanCirclingProjectiles circling = __instance.GetComponent<FerrymanCirclingProjectiles>();
		circling.active = true;

		EnemySimplifier es = __instance.GetComponentInChildren<EnemySimplifier>();
		GameObject rage = UnityObject.Instantiate<GameObject>(
			DefaultReferenceManager.Instance.enrageEffect,
			es.transform.parent
		);
		rage.transform.localScale = 0.4f * Vector3.one;
		rage.transform.localPosition = new Vector3(0f, 0.75f, 0f);
	}

	// FERRYMAN PATCH (speed up)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Ferryman), nameof(Ferryman.SetSpeed))]
	public static void SetSpeedPostfix(Ferryman __instance) {
		if (__instance.eid.dead)
			return;
		if (__instance.difficulty != 19)
			return;

		OriginalHealth oh = __instance.GetComponent<OriginalHealth>();
		FerrymanStuff fs = __instance.GetComponent<FerrymanStuff>();
		float hardModeMult = (!Util.IsHardMode()) ? 1f : 1.15f;
		fs.speedChangeMultiplier = hardModeMult * 0.4f / (oh.health * __instance.eid.totalHealthModifier); // more health = slower

		__instance.defaultMovementSpeed = 32f * fs.currentValue;
		__instance.anim.speed = fs.currentValue * __instance.eid.totalSpeedModifier;
		__instance.nma.speed = __instance.defaultMovementSpeed * __instance.eid.totalSpeedModifier;

		if (fs.reached && !fs.addedStyle) {
			MonoSingleton<StyleHUD>.Instance.AddPoints(25, "billion.blue", null, __instance.eid, -1, "", "");
			fs.addedStyle = true;
			if (Util.IsHardMode()) {
				FerrymanCirclingProjectiles circling = __instance.GetComponent<FerrymanCirclingProjectiles>();
				circling.active = true;
				circling.doubled = true;
			}
		}
	}


	// FERRYMAN PATCH (hard mode oar slam attack)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Ferryman), nameof(Ferryman.SlamHit))]
	public static void SlamHitPostfix(Ferryman __instance) {
		if (!Util.IsHardMode())
			return;
		__instance.StartCoroutine(SlamHitExtra(__instance));
	}

	public static IEnumerator SlamHitExtra(Ferryman __instance) {
		yield return new WaitForSeconds(0.1f);
		Transform tr = __instance.transform;
		Vector3 mainOffset = (tr.forward + tr.up).normalized;
		
		SpawnHoming(__instance, __instance.transform.position + 1.5f * mainOffset);
		yield return new WaitForSeconds(0.1f);
		SpawnHoming(__instance, __instance.transform.position + 1.5f * (Quaternion.AngleAxis(120f, tr.up) * mainOffset));
		yield return new WaitForSeconds(0.1f);
		SpawnHoming(__instance, __instance.transform.position + 1.5f * (Quaternion.AngleAxis(-120f, tr.up) * mainOffset));

		// yield return new WaitForSeconds(0.1f);
		yield break;
	}

	public static GameObject SpawnHoming(Ferryman __instance, Vector3 position) {
		Vector3 direction = (position - __instance.transform.position).normalized;
		Quaternion rotation = Quaternion.LookRotation(direction);

		GameObject proj = UnityObject.Instantiate<GameObject>(Plugin.Prefabs["ProjectileHoming"], position, rotation);
		Projectile comp = proj.GetComponent<Projectile>();
		comp.safeEnemyType = EnemyType.Ferryman;
		comp.speed = 30f;
		comp.ignoreExplosions = true;
		#pragma warning disable CS0618 // Type or member is obsolete
		comp.target = __instance.eid.target;
		#pragma warning restore CS0618 // Type or member is obsolete
		return proj;
	}


	// FERRYMAN PATCH (hard mode backstep attack)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Ferryman), nameof(Ferryman.BackstepAttack))]
	public static void BackstepAttackPostfix(Ferryman __instance) {
		if (!Util.IsHardMode())
			return;
		__instance.StartCoroutine(BackstepAttackExtra(__instance));
	}

	public static IEnumerator BackstepAttackExtra(Ferryman __instance) {
		yield return new WaitForSeconds(1.35f / __instance.anim.speed);

		GameObject explosion = UnityObject.Instantiate<GameObject>(
			Plugin.Prefabs["ExplosionSisyphusPrimeCharged"],
			__instance.transform.position + 2f * __instance.transform.up,
			Quaternion.identity
		);
		explosion.GetComponentInChildren<ExplosionController>(includeInactive: true).transform.localScale = Vector3.one;
		foreach (Explosion component in explosion.GetComponentsInChildren<Explosion>(includeInactive: true)) {
			component.speed *= 20f;
			component.maxSize *= 0.75f;
			component.enemyDamageMultiplier = 0f;
			component.originEnemy = __instance.eid;
		}

		// yield return new WaitForSeconds(0.1f);
		yield break;
	}


	// FERRYMAN PATCH (hard mode vault swing attack)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Ferryman), nameof(Ferryman.VaultSwing))]
	public static void VaultSwingPostfix(Ferryman __instance) {
		if (!Util.IsHardMode())
			return;
		__instance.StartCoroutine(VaultSwingExtra(__instance));
	}

	public static IEnumerator VaultSwingExtra(Ferryman __instance) {
		yield return new WaitForSeconds(1.125f / __instance.anim.speed);
		ShootMinosSnake(__instance);
		// yield return new WaitForSeconds(0.1f);
		yield break;
	}


	// FERRYMAN PATCH (hard mode stinger attack)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Ferryman), nameof(Ferryman.Stinger))]
	public static void StingerPostfix(Ferryman __instance) {
		if (!Util.IsHardMode())
			return;
		__instance.StartCoroutine(StingerExtra(__instance));
	}

	public static IEnumerator StingerExtra(Ferryman __instance) {
		yield return new WaitForSeconds(0.95f / __instance.anim.speed);
		ShootMinosSnake(__instance);
		// yield return new WaitForSeconds(0.1f);
		yield break;
	}


	public static void ShootMinosSnake(Ferryman __instance) {
		GameObject proj = UnityObject.Instantiate<GameObject>(
			Plugin.Prefabs["ProjectileMinosPrimeSnake"],
			__instance.transform.position + 3.5f *__instance.transform.up,
			Quaternion.LookRotation(__instance.transform.up)
		);
		Projectile comp = proj.GetComponentInChildren<Projectile>();
		comp.speed = 75f;
		comp.turningSpeedMultiplier *= 2.5f;
		comp.ignoreEnvironment = true;
		comp.ignoreExplosions = true;
		comp.stopTrackingAfterSeconds = 1f;
	}
}