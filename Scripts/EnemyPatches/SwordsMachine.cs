using Billion = BillionDifficulty.Plugin;
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
	public static void StartPostfix(ref SwordsMachine __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		__instance.gameObject.AddComponent<CounterInt>().maxValue = 2;
		if (!__instance.bossVersion) {
			__instance.phaseChangeHealth = 10f;
		}
		TimerFloat timer = __instance.gameObject.AddComponent<TimerFloat>();
		timer.target = 2f;
		timer.reached = true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(SwordsMachine), nameof(SwordsMachine.SetSpeed))]
	public static void SetSpeedPostfixPostfix(SwordsMachine __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		__instance.nma.speed = __instance.firstPhase ? 22 : 26; // Brutal: 19 : 23
		__instance.anim.speed = 1.2f; // Brutal: 1.2f
		__instance.anim.SetFloat("ThrowSpeedMultiplier", 1.45f); // Brutal: 1.35f
		__instance.anim.SetFloat("AttackSpeedMultiplier", 1.15f); // Brutal: 1f
		__instance.moveSpeedMultiplier = 1.45f * __instance.eid.totalSpeedModifier; // Brutal: 1.35f
		__instance.normalMovSpeed = __instance.nma.speed * __instance.eid.totalSpeedModifier;
		__instance.normalAnimSpeed = __instance.anim.speed * __instance.eid.totalSpeedModifier;
	}

	// SWORDSMACHINE PATCH (speed)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(SwordsMachine), nameof(SwordsMachine.GetSpeed))]
	public static bool GetSpeedPrefix(int difficulty, SwordsMachine __instance, ref EnemyMovementData __result) {
		if (difficulty != 19) {
			return true;
		}
		int num = __instance.firstPhase ? 22 : 26;
		__result = new EnemyMovementData {
			speed = num,
			angularSpeed = 1200f, // default: 1200f
			acceleration = 160f // default: 160f
		};
		return false;
	}

	public static IEnumerator SwordsMachineOverpump(Vector3 position, float damageMultiplier) {
		yield return new WaitForSeconds(0.15f);

		GameObject explosionObject = UnityObject.Instantiate<GameObject>(Billion.ExplosionSuper, position, Quaternion.identity);
		explosionObject.transform.LookAt(NewMovement.Instance.transform);
		explosionObject.transform.localScale *= 1.5f;
		foreach (Explosion explosion in explosionObject.GetComponentsInChildren<Explosion>()) {
			explosion.maxSize *= 1.5f;
			explosion.damage = Mathf.RoundToInt(40 * damageMultiplier); // default: 50
			explosion.enemyDamageMultiplier = 0.25f; // default: 1f
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SwordsMachine), nameof(SwordsMachine.ShootGun))]
	public static bool ShootGunPrefix(SwordsMachine __instance) {
		if (__instance.difficulty != 19) {
			return true;
		}

		CounterInt counter = __instance.gameObject.GetComponent<CounterInt>();
		TimerFloat timer = __instance.gameObject.GetComponent<TimerFloat>();

		if (__instance.inAction) {
			return false;
		}

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
				float distance = Vector3.Distance(__instance.target.position, __instance.transform.position);
				if (distance > 20f) {
					if (__instance.hasVision) {
						TargetData value = __instance.vision.CalculateData(__instance.targetHandle);
						__instance.shotgun.UpdateTarget(new TargetData?(value));
					} else if (__instance.target != null) {
						__instance.lastTargetData.position = __instance.target.position;
						__instance.shotgun.UpdateTarget(new TargetData?(__instance.lastTargetData));
					}
					__instance.shotgun.Fire();
				} else {
					AudioSource pumpSound = UnityObject.Instantiate<GameObject>(Billion.PumpChargeSound).GetComponent<AudioSource>();
					pumpSound.pitch = 1.35f; // default: 1.2
					pumpSound.Play(tracked: true);
					__instance.StartCoroutine(
						SwordsMachineOverpump(__instance.shotgun.shootPoint.position, __instance.eid.totalDamageModifier)
					);
					counter.Add();
					timer.ResetAndRun();
					__instance.shotgunning = false;
					__instance.inAction = false;
				}
				break;
		}
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SwordsMachine), nameof(SwordsMachine.CheckToAttack))]
	public static bool CheckToAttackPrefix(SwordsMachine __instance) {
		if (__instance.difficulty != 19) {
			return true;
		}

		if (__instance.inAction) {
			return false;
		}
		if (__instance.hasDimensionalTarget || !__instance.hasVision) {
			return false;
		}

		CounterInt counter = __instance.gameObject.GetComponent<CounterInt>();
		TimerFloat timer = __instance.gameObject.GetComponent<TimerFloat>();

		if (counter.value != 2) {
			return true;
		}
		if (!timer.reached) {
			return false;
		}

		// Vector3 targetPosition = __instance.targetPosition;
		// float num = Vector3.Distance(targetPosition, __instance.transform.position);
		if (!__instance.targetingStalker) {
			if (__instance.shotgun && __instance.shotgun.gunReady && !__instance.gunDelay && !__instance.shotgunning && (__instance.firstPhase || __instance.bothPhases)/* && num > 5f*/) {
				__instance.shotgunning = true;
				__instance.anim.SetLayerWeight(1, 1f);
				__instance.anim.SetTrigger("Shoot");
				__instance.aimLerp = 0f;
			}
		}
		return false;
	}
}