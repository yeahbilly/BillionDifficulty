using Billion = BillionDifficulty.Plugin;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

public class SlowDownOverTimeEase : MonoBehaviour {
	public float slowRate;
	public float rateEase;
	public bool makeUnparryableOnStop = false;
	public bool changeColorOnStop = false;
	public bool hasBeam = false;
	public float volumeMultiplier = 1f;
	public Color newProjectileColor;

	public float cooldownMax = 0.01f;
	public float cooldown = 0f;
	public bool stop = false;
	public Projectile proj;
	public LineRenderer line;

	public void Start() {
		proj = this.GetComponent<Projectile>();
		line = proj.GetComponentInChildren<LineRenderer>();
	}

	public void Update() {
		if (proj.parried) {
			return;
		}
		if (line == null) {
			hasBeam = false;
		}

		if (stop) {
			return;
		}
		cooldown += Time.deltaTime;
		if (cooldown < cooldownMax) {
			return;
		}
		
		cooldown = 0f;
		if (proj.speed > 0f) {
			proj.speed -= slowRate;
			slowRate += rateEase;
			return;
		}

		// projectile stopped
		proj.speed = 0.001f;
		stop = true;
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
		slowRate += rateEase;
	}
}

public class SlowDownOverTime : MonoBehaviour {
	public float slowRate;

	public float cooldownMax = 0.01f;
	public float cooldown = 0f;
	public bool stop = false;
	public Projectile proj;
	public void Start() {
		proj = this.GetComponent<Projectile>();
	}
	public void Update() {
		if (proj.parried) {
			return;
		}

		if (stop) {
			return;
		}

		cooldown += Time.deltaTime;
		if (cooldown < cooldownMax) {
			return;
		}
		cooldown = 0f;

		if (proj.speed > 0f) {
			proj.speed -= slowRate;
		} else {
			proj.speed = 0.001f;
			stop = true;
		}
	}
}

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
	public float originalSpeed;

	public void Start() {
		proj = this.GetComponent<Projectile>();
		originalSpeed = proj.speed;

		if (homing) {
			proj.homingType = HomingType.Instant;
		}

		if (!changeColor) {
			return;
		}

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
		if (proj.parried) {
			return;
		}

		if (homing) {
			proj.turnSpeed = 0f;
		}

		cooldown += Time.deltaTime;
		if (cooldown < cooldownMax) {
			return;
		}
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

		if (!explodeWhenClose) {
			return;
		}

		float distanceToPlayer = Vector3.Distance(NewMovement.Instance.transform.position, this.transform.position);
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

class ChangeScaleOverTime : MonoBehaviour {
	public float delay = 0f;
	public float targetScaleMultiplier;
	public float time;

	public float timeSinceStart;
	public Projectile proj;
	public Vector3 currentScale;
	public Vector3 initialScale;
	public Vector3 targetScale;
	public LineRenderer line;
	public bool scaleBeam = false;
	public float currentBeamScale;
	public float initialBeamScale;
	public float targetBeamScale;
	public void Start() {
		proj = this.GetComponent<Projectile>();
		line = proj.GetComponentInChildren<LineRenderer>();

		currentScale = proj.transform.localScale;
		initialScale = proj.transform.localScale;
		targetScale = initialScale * targetScaleMultiplier;
		timeSinceStart = 0;

		if (!scaleBeam) {
			return;
		}
		currentBeamScale = line.startWidth;
		initialBeamScale = line.startWidth;
		targetBeamScale = initialBeamScale * targetScaleMultiplier;
	}
	public void Update() {
		if (line == null) {
			scaleBeam = false;
		}

		timeSinceStart += Time.deltaTime;
		if (timeSinceStart < delay) {
			return;
		}

		float newX = Mathf.MoveTowards(currentScale.x, targetScale.x, Time.deltaTime * Mathf.Abs(targetScale.x - initialScale.x) / time);
		float newY = Mathf.MoveTowards(currentScale.y, targetScale.y, Time.deltaTime * Mathf.Abs(targetScale.y - initialScale.y) / time);
		float newZ = Mathf.MoveTowards(currentScale.z, targetScale.z, Time.deltaTime * Mathf.Abs(targetScale.z - initialScale.z) / time);
		proj.transform.localScale = new Vector3(newX, newY, newZ);
		currentScale = new Vector3(newX, newY, newZ);

		if (!scaleBeam) {
			return;
		}
		float newWidth = Mathf.MoveTowards(currentBeamScale, targetBeamScale, Time.deltaTime * Mathf.Abs(targetBeamScale - initialBeamScale) / time);
		line.startWidth = newWidth;
		line.endWidth = newWidth;
		currentBeamScale = newWidth;
	}
}

class ProjectileHeightExplosion : MonoBehaviour {
	public float maxDistance = -1f;
	public EnemyTarget target;

	public Transform proj;
	public void Start() {
		proj = this.transform;
	}
	public void Update() {
		if (target == null) {
			return;
		}

		if (target.position.y > proj.position.y) {
			return;
		}

		float distance = Vector3.Distance(target.position, proj.position);
		bool isWithinMaxDistance =
			maxDistance != -1f
			&& distance <= maxDistance
			&& distance > maxDistance/2f;
		if ( maxDistance == -1f || isWithinMaxDistance) {
			proj.GetComponent<Projectile>().Explode();
		}
	}
}

public class AlternatingProjectileSplits : MonoBehaviour {
	public int maxLayer;
	public float projectileSpeed;
	public float cooldownMax = 0.15f;

	public int currentLayer = 1;
	public GameObject proj;
	public Projectile projComp;
	public float cooldown = 0f;
	public void Start() {
		proj = this.gameObject;
		projComp = proj.GetComponent<Projectile>();
	}
	public void Update() {
		cooldown += Time.deltaTime;
		if (cooldown < cooldownMax) {
			return;
		}
		cooldown = 0f;
		
		if (currentLayer >= maxLayer) {
			return;
		}

		projComp.speed = 1f;
		ChangeScaleOverTime scale = proj.AddComponent<ChangeScaleOverTime>();
		scale.delay = 0.25f;
		scale.time = 1f;
		scale.targetScaleMultiplier = 0f;
		UnityObject.Destroy(proj, 1.25f);

		if (currentLayer % 2 == 1) {
			SplitOddLayer();
		} else {
			SplitEvenLayer();
		}
	}

	public void SplitOddLayer() {
		Vector3 newRotation = new Vector3(0,0,0);
		for (int i = 0; i < 4; i++) {
			switch (i) {
				case 0:
					// up
					newRotation = new Vector3(12.5f, 0f, 0f);
					break;
				case 1:
					// right
					newRotation = new Vector3(0f, 12.5f, 0f);
					break;
				case 2:
					// down
					newRotation = new Vector3(-12.5f, 0f, 0f);
					break;
				case 3:
					// left
					newRotation = new Vector3(0f, -12.5f, 0f);
					break;
				
			}
			GameObject newProj = UnityObject.Instantiate(Billion.Projectile, this.transform.position, this.transform.rotation);
			newProj.transform.Rotate(newRotation);
			newProj.GetComponent<Projectile>().speed = this.projectileSpeed;

			if (currentLayer >= maxLayer) {
				return;
			}
			AlternatingProjectileSplits newSplits = newProj.AddComponent<AlternatingProjectileSplits>();
			newSplits.currentLayer = this.currentLayer + 1;
			newSplits.maxLayer = this.maxLayer;
			newSplits.projectileSpeed = this.projectileSpeed;
		}
	}

	public void SplitEvenLayer() {
		Vector3 newRotation = new Vector3(0,0,0);
		for (int i = 0; i < 4; i++) {
			switch (i) {
				case 0:
					// up + right
					newRotation = new Vector3(12.5f, 12.5f, 0f);
					break;
				case 1:
					// right + down
					newRotation = new Vector3(-12.5f, 12.5f, 0f);
					break;
				case 2:
					// down + left
					newRotation = new Vector3(-12.5f, -12.5f, 0f);
					break;
				case 3:
					// left + up
					newRotation = new Vector3(12.5f, -12.5f, 0f);
					break;
				
			}
			GameObject newProj = UnityObject.Instantiate(Billion.Projectile, this.transform.position, this.transform.rotation);
			newProj.transform.Rotate(newRotation);
			newProj.GetComponent<Projectile>().speed = this.projectileSpeed;

			if (currentLayer >= maxLayer) {
				return;
			}
			AlternatingProjectileSplits newSplits = newProj.AddComponent<AlternatingProjectileSplits>();
			newSplits.currentLayer = this.currentLayer + 1;
			newSplits.maxLayer = this.maxLayer;
			newSplits.projectileSpeed = this.projectileSpeed;
		}
	}
}