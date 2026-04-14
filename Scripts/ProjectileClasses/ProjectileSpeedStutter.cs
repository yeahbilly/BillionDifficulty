using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

public class ProjectileSpeedStutter : MonoBehaviour {
	public float cooldownMax = 0.01f;
	public float cooldown = 0f;
	public float targetSpeedMultiplier;
	public float slowRate;
	public bool homing = false;
	public bool explodeWhenClose = false;
	public float explodeDistance;
	public int explodeStutterDelay;
	public int explodeStutterCounter = -2;
	public bool changeColor = false;
	public AudioSource audio = null;
	public Projectile proj;
	public float originalSpeed = -1f;

	public void Start() {
		proj = GetComponent<Projectile>();

		if (homing) {
			proj.homingType = HomingType.Instant;
		}

		if (!changeColor)
			return;

		// projectile and charge effect
		MeshRenderer[] renderers = proj.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer renderer in renderers) {
			renderer.material.color = new Color(1f, 1f, 1f);
			renderer.material.renderQueue = 3000; // default: 2000
		}
		// trail
		TrailRenderer trail = proj.GetComponent<TrailRenderer>();
		trail.startColor = new Color(1f, 0.6902f, 0.0196f);
		trail.endColor = new Color(1f, 1f, 0.0431f);
		// projectile light
		Light light = proj.GetComponent<Light>();
		light.color = new Color(1f, 1f, 1f);
	}

	public void Update() {
		if (proj.parried)
			return;

		if (homing) {
			proj.turnSpeed = 0f;
		}

		cooldown += Time.deltaTime;
		if (cooldown < cooldownMax)
			return;

		cooldown = 0f;

		if (proj.speed == originalSpeed && audio != null) {
			audio.Play(tracked: true);
		}
		if (proj.speed > originalSpeed * targetSpeedMultiplier) {
			proj.speed -= slowRate;
			return;
		}

		proj.speed = originalSpeed;
		if (homing) {
			proj.turningSpeedMultiplier = 100f;
		}

		if (!explodeWhenClose)
			return;

		float distanceToPlayer = Vector3.Distance(NewMovement.Instance.transform.position, transform.position);
		if (explodeStutterCounter == -2 && distanceToPlayer <= explodeDistance) {
			explodeStutterCounter = -1;
			return;
		}
		if (explodeStutterCounter >= -1 && changeColor) {
			// projectile and charge effect
			MeshRenderer[] renderers = proj.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer renderer in renderers) {
				Color color = renderer.material.color;
				renderer.material.color = new Color(color.r - (1/((float)explodeStutterDelay + 1)), color.g, color.b);
				renderer.material.renderQueue = 3000; // default: 2000
			}
			// trail
			TrailRenderer trail = proj.GetComponent<TrailRenderer>();
			Color trailStartColor = trail.startColor;
			Color trailEndColor = trail.endColor;
			trail.startColor = new Color(trailStartColor.r - (1/((float)explodeStutterDelay + 1)), trailStartColor.g, trailStartColor.b);
			trail.endColor = new Color(trailEndColor.r - (1/((float)explodeStutterDelay + 1)), trailEndColor.g, trailEndColor.b);
			// projectile light
			Light light = proj.GetComponent<Light>();
			Color lightColor = light.color;
			light.color = new Color(lightColor.r - (float)(1/(explodeStutterDelay + 1)), lightColor.g, lightColor.b);
		}

		if (explodeStutterCounter > explodeStutterDelay - 1) {
			proj.Explode();
		} else if (explodeStutterCounter >= -1 && explodeStutterCounter < explodeStutterDelay) {
			explodeStutterCounter += 1;
		}
	}
}