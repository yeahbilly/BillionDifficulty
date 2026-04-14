using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

public class SlowDownOverTimeEase : MonoBehaviour {
	public float slowRate;
	public float rateEase;
	public bool makeUnparryableOnStop = false;
	public bool changeColorOnStop = false;
	public bool hasBeam = false;
	public float volumeMultiplier = 1f;
	public Color newProjectileColor;
	public int reverseTimes = 0;

	public float cooldownMax = 0.01f;
	public float cooldown = 0f;
	public bool stop = false;
	public int currentReverseTimes = 1;
	public Projectile proj;
	public LineRenderer line;
	public float initialProjectileSpeed = 0f;
	public float initialSlowRate = 0f;

	public void Start() {
		proj = GetComponent<Projectile>();
		line = proj.GetComponentInChildren<LineRenderer>();
		initialProjectileSpeed = proj.speed;
		initialSlowRate = slowRate;
	}

	public void Update() {
		if (proj.parried) {
			proj.ignoreEnvironment = false;
			return;
		}
		if (line == null) {
			hasBeam = false;
		}

		if (stop)
			return;

		cooldown += Time.deltaTime;
		if (cooldown < cooldownMax)
			return;

		cooldown = 0f;
		if (proj.speed > 0f) {
			proj.speed -= slowRate;
			slowRate += rateEase;
			return;
		}

		// projectile stopped
		proj.speed = 0.001f;
		proj.enemyDamageMultiplier = 0f;

		if (currentReverseTimes >= reverseTimes) {
			stop = true;
		}
		if (makeUnparryableOnStop) {
			proj.unparryable = true;
		}
		if (changeColorOnStop) {
			// projectile and charge effect
			MeshRenderer[] renderers = proj.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer renderer in renderers) {
				renderer.material.color = newProjectileColor;
			}
			// beam
			if (hasBeam) {
				line.startColor = newProjectileColor;
				line.endColor = newProjectileColor;
			}
			// projectile light
			Light light = proj.GetComponent<Light>();
			if (light != null) {
				//light.intensity = 2.5f; // default: 5f
				light.color = newProjectileColor;
			}
		}
		if (volumeMultiplier != 1f) {
			foreach (AudioSource audio in proj.GetComponentsInChildren<AudioSource>()) {
				audio.volume *= volumeMultiplier;
			}
		}
		// slowRate += rateEase;

		if (currentReverseTimes < reverseTimes) {
			currentReverseTimes++;
			transform.Rotate(0f, 180f, 0f);
			slowRate = initialSlowRate;
			proj.speed = initialProjectileSpeed;
		}
	}
}