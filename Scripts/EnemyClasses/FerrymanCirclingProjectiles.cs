using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

class FerrymanCirclingProjectiles : MonoBehaviour {
	public float cooldownMax = 0.3f;
	public float scaleMultiplier = 3f;
	public float projectileSpeed = 80f;

	public float angleStep = 30f;
	public float angle = 0f;
	public bool active = false;
	public bool doubled = false;
	public float cooldown = 0f;
	public GameObject projectile = Plugin.Prefabs["Projectile"];
	public EnemyIdentifier eid;

	public void Start() {
		eid = GetComponent<EnemyIdentifier>();
	}

	public void Update() {
		if (eid == null || eid.dead) {
			if (BoolValue.Get("enraged", gameObject) == true) {
				BoolValue.Set("enraged", false, gameObject);
				EnrageEffect enrageEffect = gameObject.GetComponentInChildren<EnrageEffect>(includeInactive: true);
				UnityObject.Destroy(enrageEffect.gameObject);
			}
			UnityObject.Destroy(this);
			return;
		}
		if (!active)
			return;
		cooldown += Time.deltaTime;
		if (cooldown < cooldownMax)
			return;
		cooldown = 0f;
		
		if (angle >= 360f)
			angle %= 360f;
		
		if (!doubled) {
			Shoot(4f);
		} else {
			Shoot(2.5f);
			Shoot(5.5f);
		}
		angle += angleStep;
	}

	public void Shoot(float heightOffset) {
		GameObject newProjectile = UnityObject.Instantiate<GameObject>(
			projectile,
			transform.position + heightOffset * transform.up,
			Quaternion.LookRotation(Vector3.forward)
		);
		newProjectile.transform.Rotate(new Vector3(0f, angle, 0f), Space.Self);
		newProjectile.transform.position += 2f * newProjectile.transform.forward;
		newProjectile.transform.localScale *= scaleMultiplier;

		Projectile proj = newProjectile.GetComponent<Projectile>();
		proj.unparryable = true;
		proj.speed = projectileSpeed;
		proj.ignoreEnvironment = true;
		proj.safeEnemyType = EnemyType.Ferryman;
		proj.enemyDamageMultiplier = 0f;
		proj.damage = 20f;

		RemoveOnTime remove = newProjectile.GetComponent<RemoveOnTime>();
		remove.time = 1f;

		UnityObject.Destroy(proj.GetComponent<MeshRenderer>());
		MeshRenderer chargeEffect = proj.transform.Find("AlwaysLookAtCamera/ChargeEffect").GetComponent<MeshRenderer>();
		chargeEffect.material.color = Color.white;

		TrailRenderer trail = proj.GetComponent<TrailRenderer>();
		trail.startColor = Color.white;
		trail.endColor = new Color(0.5f, 1f, 1f);

		Light light = proj.GetComponent<Light>();
		light.color = new Color(0f, 0.3886f, 1f);
	}
}