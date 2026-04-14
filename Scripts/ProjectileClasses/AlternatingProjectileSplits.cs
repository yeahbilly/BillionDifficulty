using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

public class AlternatingProjectileSplits : MonoBehaviour {
	public int maxLayer;
	public float projectileSpeed;
	public float projectileScaleMultiplier = 1f;
	public float cooldownMax = 0.125f;
	public float projectileSpeedIncreasePerLayer = 10f;
	public float projectileScaleIncreasePerLayer = 1.15f;
	public float rotationOffset = 12.5f;
	public bool keepFirstProjectile = true;

	public int currentLayer = 1;
	public GameObject proj;
	public Projectile projComp;
	public float cooldown = 0f;
	public void Start() {
		proj = gameObject;
		projComp = proj.GetComponent<Projectile>();
	}
	public void Update() {
		cooldown += Time.deltaTime;
		if (cooldown < cooldownMax)
			return;

		cooldown = 0f;
		
		if (currentLayer >= maxLayer)
			return;

		bool keepingThis = keepFirstProjectile && currentLayer == 1;
		if (!keepingThis) {
			projComp.speed = 1f;
			// ChangeScaleOverTime scale = proj.AddComponent<ChangeScaleOverTime>();
			// scale.delay = 0.25f;
			// scale.time = 1f;
			// scale.targetScaleMultiplier = 0f;
		}

		if (currentLayer % 2 == 1) {
			SplitOddLayer();
		} else {
			SplitEvenLayer();
		}

		if (!keepingThis) {
			UnityObject.Destroy(proj);
		} else {
			UnityObject.Destroy(this);
		}
	}

	public void SplitOddLayer() {
		Vector3 newRotation = Vector3.zero;
		for (int i = 0; i < 4; i++) {
			switch (i) {
				case 0:
					// up
					newRotation = new Vector3(rotationOffset, 0f, 0f);
					break;
				case 1:
					// right
					newRotation = new Vector3(0f, rotationOffset, 0f);
					break;
				case 2:
					// down
					newRotation = new Vector3(-rotationOffset, 0f, 0f);
					break;
				case 3:
					// left
					newRotation = new Vector3(0f, -rotationOffset, 0f);
					break;
				
			}
			CreateProjectile(newRotation);
		}
	}

	public void SplitEvenLayer() {
		Vector3 newRotation = Vector3.zero;
		for (int i = 0; i < 4; i++) {
			switch (i) {
				case 0:
					// up + right
					newRotation = new Vector3(rotationOffset, rotationOffset, 0f);
					break;
				case 1:
					// right + down
					newRotation = new Vector3(-rotationOffset, rotationOffset, 0f);
					break;
				case 2:
					// down + left
					newRotation = new Vector3(-rotationOffset, -rotationOffset, 0f);
					break;
				case 3:
					// left + up
					newRotation = new Vector3(rotationOffset, -rotationOffset, 0f);
					break;
				
			}
			CreateProjectile(newRotation);
		}
	}

	public void CreateProjectile(Vector3 projectileRotation) {
		GameObject newProj = UnityObject.Instantiate(Plugin.Prefabs["Projectile"], transform.position, transform.rotation);
		newProj.transform.localScale *= projectileScaleMultiplier * projectileScaleIncreasePerLayer;
		newProj.transform.Rotate(projectileRotation);
		Projectile newProjComp = newProj.GetComponent<Projectile>();
		newProjComp.speed = projectileSpeed + projectileScaleIncreasePerLayer;
		newProjComp.enemyDamageMultiplier = 0f;
		newProj.GetComponent<AudioSource>().volume = 0.5f * GetComponent<AudioSource>().volume;

		if (currentLayer >= maxLayer)
			return;

		AlternatingProjectileSplits newSplits = newProj.AddComponent<AlternatingProjectileSplits>();
		newSplits.currentLayer = currentLayer + 1;
		newSplits.maxLayer = maxLayer;
		newSplits.projectileScaleMultiplier = projectileScaleMultiplier;
		newSplits.projectileSpeed = projectileSpeed + projectileSpeedIncreasePerLayer;
		newSplits.projectileSpeedIncreasePerLayer = projectileSpeedIncreasePerLayer;
		newSplits.projectileScaleIncreasePerLayer = projectileScaleIncreasePerLayer * projectileScaleIncreasePerLayer;
	}
}