using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using System.Collections;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: SWORDSMACHINE !!!
[HarmonyPatch(typeof(SwordsMachine))]
public class SwordsMachinePatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(SwordsMachine), nameof(SwordsMachine.Start))]
	public static void StartPostfix(SwordsMachine __instance) {
		if (__instance.difficulty != 19)
			return;

		__instance.gameObject.AddComponent<CounterInt>().maxValue = 2;
		if (!__instance.bossVersion) {
			__instance.phaseChangeHealth = 10f;
		}
		TimerFloat timer = __instance.gameObject.AddComponent<TimerFloat>();
		timer.cooldownMax = 2f;
		timer.reached = true;
		BoolValue bv = __instance.gameObject.AddComponent<BoolValue>();
		bv.description = "overpumping";
		bv.value = false;

		GameObject pumpChargeObject = new GameObject("PumpChargeObject");
		pumpChargeObject.transform.SetParent(__instance.transform);
		pumpChargeObject.transform.localPosition = Vector3.zero;
		AudioSource audio = pumpChargeObject.AddComponent<AudioSource>();
		audio.SetPlayOnAwake(false);
		audio.playOnAwake = false;
		audio.clip = Plugin.PumpCharge;
		audio.SetPitch(1.35f); // default: 1.2
		audio.SetSpatialBlend(0f);
		audio.maxDistance = 50f;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SwordsMachine), nameof(SwordsMachine.KnockdownSetup))]
	public static void KnockdownSetupPrefix(SwordsMachine __instance) {
		if (!Util.IsHardMode())
			return;
		
		GameObject goop = UnityObject.Instantiate<GameObject>(Plugin.Prefabs["GoopLarge"], __instance.transform.position, Quaternion.identity);
		goop.AddComponent<RemoveOnRespawn>();
		goop.transform.Find("Cylinder").GetComponent<HurtZone>().affected = AffectedSubjects.PlayerOnly;
	}

	// SWORDSMACHINE PATH (speed)
	[HarmonyPostfix]
	[HarmonyPatch(typeof(SwordsMachine), nameof(SwordsMachine.SetSpeed))]
	public static void SetSpeedPostfixPostfix(SwordsMachine __instance) {
		if (__instance.difficulty != 19)
			return;
		
		float hardModeMult = (!Util.IsHardMode()) ? 1f : 1.1f;

		__instance.nma.speed = __instance.firstPhase ? 22f : 26f; // Brutal: 19 : 23
		__instance.nma.speed *= hardModeMult;
		__instance.anim.speed = 1.2f * hardModeMult; // Brutal: 1.2f
		__instance.anim.SetFloat("ThrowSpeedMultiplier", 1.45f * hardModeMult); // Brutal: 1.35f
		__instance.anim.SetFloat("AttackSpeedMultiplier", 1.15f * hardModeMult); // Brutal: 1f
		__instance.moveSpeedMultiplier = 1.45f * hardModeMult * __instance.eid.totalSpeedModifier; // Brutal: 1.35f
		__instance.normalMovSpeed = __instance.nma.speed * __instance.eid.totalSpeedModifier;
		__instance.normalAnimSpeed = __instance.anim.speed * __instance.eid.totalSpeedModifier;
	}

	// SWORDSMACHINE PATCH (speed)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(SwordsMachine), nameof(SwordsMachine.GetSpeed))]
	public static bool GetSpeedPrefix(int difficulty, SwordsMachine __instance, ref EnemyMovementData __result) {
		if (difficulty != 19)
			return true;
		float hardModeMult = (!Util.IsHardMode()) ? 1f : 1.1f;
		float num = __instance.firstPhase ? 22f : 26f;
		num *= hardModeMult;
		__result = new EnemyMovementData {
			speed = num,
			angularSpeed = 1200f, // default: 1200f
			acceleration = 160f // default: 160f
		};
		return false;
	}

	public static IEnumerator SwordsMachineOverpump(Vector3 position, float damageMultiplier, SwordsMachine __instance) {
		yield return new WaitForSeconds(0.15f);

		GameObject explosionObject = UnityObject.Instantiate<GameObject>(Plugin.Prefabs["ExplosionSuper"], position, Quaternion.identity);
		explosionObject.transform.LookAt(NewMovement.Instance.transform);
		explosionObject.transform.localScale *= 1.5f;
		foreach (Explosion explosion in explosionObject.GetComponentsInChildren<Explosion>()) {
			explosion.maxSize *= 1.5f;
			explosion.damage = Mathf.RoundToInt(40 * damageMultiplier); // default: 50
			explosion.enemyDamageMultiplier = 0.25f; // default: 1f
		}
		BoolValue.Set("overpumping", false, __instance.gameObject);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SwordsMachine), nameof(SwordsMachine.ShootGun))]
	public static bool ShootGunPrefix(SwordsMachine __instance) {
		if (__instance.difficulty != 19)
			return true;

		CounterInt counter = __instance.GetComponent<CounterInt>();
		TimerFloat timer = __instance.GetComponent<TimerFloat>();

		if (__instance.inAction)
			return false;

		switch (counter.value) {
			case 1:
				if (__instance.hasVision) {
					TargetData value = __instance.vision.CalculateData(__instance.targetHandle);
					__instance.shotgun.UpdateTarget(new TargetData?(value));
				} else if (__instance.target != null) {
					__instance.lastTargetData.position = __instance.target.position;
					__instance.shotgun.UpdateTarget(new TargetData?(__instance.lastTargetData));
				}
				Vector3 direction = __instance.shotgun.shootPoint.transform.position - __instance.VisionSourcePosition;
				PhysicsCastResult physicsCastResult;
				Vector3 vector;
				PortalTraversalV2[] array;
				PortalPhysicsV2.ProjectThroughPortals(__instance.VisionSourcePosition, direction, default(LayerMask), out physicsCastResult, out vector, out array);
				bool instantExplode = false;
				if (array.Length != 0) {
					PortalTraversalV2 portalTraversalV = array[0];
					PortalHandle portalHandle = portalTraversalV.portalHandle;
					Portal portalObject = portalTraversalV.portalObject;
					instantExplode = !portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile) && !portalObject.passThroughNonTraversals;
				}
				__instance.shotgun.Fire(instantExplode);
				counter.Add();
				break;
			case 2:
				__instance.StartCoroutine(
					SwordsMachineOverpump(__instance.shotgun.shootPoint.position, __instance.eid.totalDamageModifier, __instance)
				);
				if (!__instance.gunDelay) {
					__instance.gunDelay = true;
					__instance.Invoke("ShootDelay", (float)Random.Range(2, 5) * 0.5f);
				}
				counter.Add();
				timer.ResetAndRun();
				// __instance.shotgunning = false;
				// __instance.inAction = false;
				break;
		}
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SwordsMachine), nameof(SwordsMachine.CheckToAttack))]
	public static bool CheckToAttackPrefix(SwordsMachine __instance) {
		if (__instance.difficulty != 19)
			return true;
		if (__instance.inAction || __instance.hasDimensionalTarget || !__instance.hasVision)
			return false;

		float distance = Vector3.Distance(__instance.target.position, __instance.transform.position);
		CounterInt counter = __instance.GetComponent<CounterInt>();
		TimerFloat timer = __instance.GetComponent<TimerFloat>();

		if (counter.value != 2 || !timer.reached || distance > 30) {
			counter.value = 1;
			return true;
		}
		
		if (!__instance.targetingStalker) {
			if (__instance.shotgun && __instance.shotgun.gunReady && !__instance.gunDelay && !__instance.shotgunning && (__instance.firstPhase || __instance.bothPhases)) {
				__instance.transform.Find("PumpChargeObject").GetComponent<AudioSource>().Play(tracked: true);
				BoolValue.Set("overpumping", true, __instance.gameObject);

				__instance.shotgunning = true;
				__instance.anim.SetLayerWeight(1, 1f);
				__instance.anim.SetTrigger("Shoot");
				__instance.aimLerp = 0f;
				return false;
			}
		}

		if (BoolValue.Get("overpumping", __instance.gameObject) == true)
			return false;
		return true;
	}

	// SWORDSMACHINE PATCH (hard mode damage resistance)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Enemy), nameof(Enemy.GetHurt))]
	public static void GetHurtPrefix(GameObject target, Vector3 force, ref float multiplier, float critMultiplier, Vector3 hurtPos, GameObject sourceWeapon, bool fromExplosion, Enemy __instance) {
		if (!Util.IsHardMode())
			return;
		if (__instance.EID.enemyType != EnemyType.Swordsmachine || __instance.sm?.isEnraged == true)
			return;
		multiplier *= 0.25f;
	}
}