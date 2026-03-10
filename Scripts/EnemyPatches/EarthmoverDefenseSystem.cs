using Billion = BillionDifficulty.Plugin;
using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using System.Collections;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: MORTARLAUNCHER !!!
[HarmonyPatch(typeof(MortarLauncher))]
public class MortarLauncherPatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MortarLauncher), nameof(MortarLauncher.Start))]
	public static void StartPostfix(MortarLauncher __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		//  mortar
		if (__instance.mortar.name.Contains("HH")) {
			__instance.firingDelay = 5f; // default: 7?
		}
		// tower
		else {
			__instance.firingDelay = 4.5f; // default: 5?
			//__instance.difficultySpeedModifier = 4f/3f;
			CounterInt counter = __instance.gameObject.AddComponent<CounterInt>();
			counter.maxValue = 2;
		}
	}
	
	// MORTAR LAUNCHER PATCH (changed projectile)
	[HarmonyPrefix]
	[HarmonyPatch(typeof(MortarLauncher), nameof(MortarLauncher.ShootHoming))]
	public static bool ShootHomingPrefix(ref MortarLauncher __instance) {
		if (__instance.difficulty != 19) {
			return true;
		}

		if (__instance.eid.target == null) {
			return false;
		}

		// mortar
		if (__instance.mortar.name.Contains("HH")) {
			Projectile projectile = UnityObject.Instantiate<Projectile>(__instance.mortar, __instance.shootPoint.position, __instance.shootPoint.rotation);
			#pragma warning disable CS0618 // Type or member is obsolete
			projectile.target = __instance.eid.target;
			#pragma warning restore CS0618 // Type or member is obsolete
			//projectile.GetComponent<Rigidbody>().velocity = __instance.shootPoint.forward * __instance.projectileForce;

			Vector3 forwardForce = 0.6f * (NewMovement.Instance.transform.position - __instance.transform.position);
			projectile.predictiveHomingMultiplier = 0.5f; // default: 0
			Rigidbody projectileRigidbody = projectile.gameObject.GetComponent<Rigidbody>();
			projectileRigidbody.drag = -0.5f; // -2.65f; default: 0
			projectileRigidbody.AddForce(Vector3.up * 30f + forwardForce, ForceMode.VelocityChange);

			projectile.damage *= __instance.eid.totalDamageModifier;
			projectile.safeEnemyType = __instance.eid.enemyType;
			projectile.turningSpeedMultiplier *= __instance.difficultySpeedModifier;
			projectile.gameObject.SetActive(true);
			if (__instance.anim) {
				__instance.anim.Play("Shoot", 0, 0f);
			}
		}
		// tower
		else {
			CounterInt counter = __instance.gameObject.GetComponent<CounterInt>();

			// prevents it from shooting the big projectile if there's a wall
			Vector3 direction = (__instance.eid.target != null) ? __instance.eid.target.position - __instance.shootPoint.position : __instance.shootPoint.forward;
			float distance = direction.magnitude;
			direction.Normalize();
			LayerMask groundLayer = LayerMask.GetMask("Outdoors", "OutdoorsBaked", "Environment", "EnvironmentBaked");
			if (Physics.Raycast(__instance.shootPoint.position, direction, out RaycastHit hit, distance, groundLayer)) {
				counter.value = 1;
			}

			Vector3 addedLookRotation = new Vector3(0, 0, 0);
			if (counter != null && counter.value == 2) {
				addedLookRotation = new Vector3(0, 2f, 0);
			}
			Quaternion lookRotation  = (__instance.eid.target != null) ? Quaternion.LookRotation(__instance.eid.target.position - __instance.shootPoint.position + addedLookRotation) : __instance.shootPoint.rotation;

			Projectile projectile = UnityObject.Instantiate<Projectile>(__instance.mortar, __instance.shootPoint.position, lookRotation);
			if (counter != null && counter.value == 1) {
				projectile.GetComponent<Rigidbody>().velocity = __instance.shootPoint.forward * __instance.projectileForce;
			}
			#pragma warning disable CS0618 // Type or member is obsolete
			projectile.target = __instance.eid.target;
			#pragma warning restore CS0618 // Type or member is obsolete

			if (counter != null && counter.value == 2) {
				projectile.homingType = HomingType.Instant;
				projectile.turningSpeedMultiplier = 0.2f;
				projectile.speed = 5f;
				projectile.ignoreEnvironment = true;

				SlowDownOverTime slowDown = projectile.gameObject.AddComponent<SlowDownOverTime>();
				slowDown.slowRate = -2.5f;

				ChangeScaleOverTime changeScale = projectile.gameObject.AddComponent<ChangeScaleOverTime>();
				changeScale.targetScaleMultiplier = 30f;
				changeScale.time = 3f;

				projectile.gameObject.GetComponent<RemoveOnTime>().time = 3f;
			}
			counter.Add();

			projectile.damage *= __instance.eid.totalDamageModifier;
			projectile.safeEnemyType = __instance.eid.enemyType;
			//projectile.turningSpeedMultiplier *= __instance.difficultySpeedModifier;
			projectile.gameObject.SetActive(true);
			if (__instance.anim) {
				__instance.anim.Play("Shoot", 0, 0f);
			}
		}
		return false;
	}
}

// ROCKET LAUNCHER PATCH (sets up counter)
[HarmonyPatch(typeof(DroneFlesh))]
public class DefenseSystemDroneFleshPatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(DroneFlesh), nameof(DroneFlesh.Start))]
	public static void StartPostfix(DroneFlesh __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		if (__instance.eid.enemyType != EnemyType.Centaur) {
			return;
		}

		CounterInt counter = __instance.transform.parent.gameObject.AddComponent<CounterInt>();
		counter.maxValue = 2;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DroneFlesh), nameof(DroneFlesh.ShootBeam))]
	public static void ShootBeamPostfix(DroneFlesh __instance) {
		if (__instance.difficulty != 19) {
			return;
		}

		if (__instance.eid.enemyType != EnemyType.Centaur) {
			return;
		}

		CounterInt counter = __instance.transform.parent.GetComponent<CounterInt>();
		counter.Add();
	}
}

// ROCKET LAUNCHER PATCH (projectile shoots oil every n shots)
[HarmonyPatch(typeof(Grenade))]
public class DefenseSystemGrenadePatch {
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Grenade), nameof(Grenade.Start))]
	public static void StartPostfix(Grenade __instance) {
		if (!Util.IsDifficulty(19)) {
			return;
		}

		bool isFromRocketLauncher = __instance.originEnemy != null && __instance.originEnemy.enemyType == EnemyType.Centaur;
		if (!isFromRocketLauncher) {
			return;
		}

		BoolValue bv = __instance.gameObject.AddComponent<BoolValue>();
		bv.description = "shotOil";
	}
	
	public static IEnumerator ShootOil(Grenade __instance) {
		float angleToUp = Vector3.Angle(__instance.transform.forward, Vector3.up);
		bool shiftDown = angleToUp > 90f;

		for (int oilIndex = 0; oilIndex < 25; oilIndex++) {
			GameObject oil = UnityObject.Instantiate<GameObject>(Billion.GasolineProjectile, __instance.transform.position + Vector3.up * 1f, __instance.transform.rotation); //Quaternion.LookRotation(aimDirection)
			oil.transform.Rotate(
				!shiftDown ? Random.Range(-50f, 50f) : Random.Range(-50f, 50f) + 3f * oilIndex,
				!shiftDown ? Random.Range(-50f, 50f) : Random.Range(-50f, 50f) + 3f * oilIndex,
				Random.Range(-50f, 50f)
			);
			oil.GetComponent<Rigidbody>().AddForce(35f * oil.transform.forward, ForceMode.Impulse);
			BoolValue bv = oil.AddComponent<BoolValue>();
			bv.description = "toIgnite";
			bv.value = true;
		}
		BoolValue.Set("shotOil", true, __instance.gameObject);
		__instance.rocketSpeed = -1f; // the rocket grazes the ground otherwise (default speed is 150f)
		yield return new WaitForSeconds(0.1f);
		//yield return new WaitForSeconds(0.125f);
		__instance.Explode();
		//yield return new WaitForSeconds(0.175f);
		//StainVoxelManager.Instance.TryIgniteAt(__instance.transform.position, 5);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Grenade), nameof(Grenade.Explode))]
	public static bool ExplodePrefix(bool big, bool harmless, bool super, float sizeMultiplier, bool ultrabooster, GameObject exploderWeapon, bool fup, Grenade __instance) {
		if (!Util.IsDifficulty(19)) {
			return true;
		}

		bool isFromRocketLauncher = __instance.originEnemy != null && __instance.originEnemy.enemyType == EnemyType.Centaur;
		if (!isFromRocketLauncher) {
			return true;
		}

		CounterInt counter = __instance.originEnemy.gameObject.GetComponent<CounterInt>();
		if (counter.value == 2) {
			BoolValue.Set("shotOil", true, __instance.gameObject); // skips the oil
		}

		if (BoolValue.Get("shotOil", __instance.gameObject) == false) {
			__instance.StartCoroutine(ShootOil(__instance));
			return false;
		} else {
			return true;
		}
	}
}

// ROCKET LAUNCHER PATCH (sets the oil on fire)
[HarmonyPatch(typeof(GasolineProjectile))]
public class GasolineProjectilePatch {
	[HarmonyPrefix]
	[HarmonyPatch(typeof(GasolineProjectile), nameof(GasolineProjectile.OnTriggerEnter))]
	public static bool OnTriggerEnterPrefix(Collider other, GasolineProjectile __instance) {
		// hit enemy
		if (other.gameObject.layer == 10 || other.gameObject.layer == 11) {
			if (__instance.hitSomething) {
				return false;
			}
			EnemyIdentifierIdentifier enemyIdentifierIdentifier;
			if (other.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out enemyIdentifierIdentifier) && enemyIdentifierIdentifier.eid && !enemyIdentifierIdentifier.eid.dead) {
				__instance.hitSomething = true;
				enemyIdentifierIdentifier.eid.AddFlammable(0.1f);
				UnityObject.Destroy(__instance.gameObject);
			}
			return false;
		}

		if (!LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment)) {
			return false;
		}
		Vector3 vector = __instance.transform.position;
		Vector3 a = -__instance.rb.velocity;

		Ray ray = new Ray(__instance.transform.position - __instance.rb.velocity.normalized * Mathf.Max(2.5f, __instance.rb.velocity.magnitude * Time.fixedDeltaTime), __instance.rb.velocity.normalized);
		RaycastHit raycastHit;
		if (!other.Raycast(ray, out raycastHit, 10f)) {
			return false;
		}

		if (!LayerMaskDefaults.IsMatchingLayer(raycastHit.transform.gameObject.layer, LMD.Environment)) {
			return false;
		}
		vector = raycastHit.point;
		a = raycastHit.normal;
		bool clipToSurface = true;
		MeshRenderer meshRenderer;
		if (!PostProcessV2_Handler.Instance.usedComputeShadersAtStart) {
			vector += a * 0.2f;
			clipToSurface = false;
		} else if (other.TryGetComponent<MeshRenderer>(out meshRenderer)) {
			Material sharedMaterial = meshRenderer.sharedMaterial;
			if (sharedMaterial != null && sharedMaterial.IsKeywordEnabled("VERTEX_DISPLACEMENT")) {
				vector += a * 0.2f;
				clipToSurface = false;
			}
		}
		GasolineStain gasolineStain = UnityObject.Instantiate<GasolineStain>(__instance.stain, vector, __instance.transform.rotation);
		Transform transform = gasolineStain.transform;
		transform.forward = a * -1f;
		transform.Rotate(Vector3.forward * UnityEngine.Random.Range(0f, 360f));
		gasolineStain.AttachTo(other, clipToSurface);

		if (Util.IsDifficulty(19)) {
			if (BoolValue.Get("toIgnite", __instance.gameObject) == true) {
				StainVoxelManager.Instance.TryIgniteAt(__instance.transform.position, 7);
			}
		}


		UnityObject.Destroy(__instance.gameObject);
		return false;
	}
}