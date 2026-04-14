using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

class FerrymanStuff : MonoBehaviour {
	public float initialSpeed = 0f;
	public float maxSpeed = 0f;
	public float speedChangeMultiplier = 1f;
	public bool changeColor = true;
	public Color targetFerrymanColor;
	public Color targetCloakColor;

	public bool reached = false;
	public bool addedStyle = false;
	public float currentValue;
	public float time = 0f;
	public EnemyIdentifier eid;
	public Material ferrymanMaterial;
	public Material cloakMaterial;
	public Color initialFerrymanColor;
	public Color initialCloakColor;
	public Color enragedColorMultiplier = new Color(1f, 0.25f, 0.25f);
	public void Start() {
		currentValue = initialSpeed;
		
		if (targetFerrymanColor != null) {
			ferrymanMaterial = gameObject.GetComponentInChildren<SkinnedMeshRenderer>().materials[0];
			initialFerrymanColor = ferrymanMaterial.color;
		}
		if (targetCloakColor != null) {
			cloakMaterial = gameObject.GetComponentInChildren<SkinnedMeshRenderer>().materials[1];
			initialCloakColor = cloakMaterial.color;
		}
	}
	public void Update() {
		bool enraged = BoolValue.Get("enraged", gameObject) == true;

		if (currentValue < maxSpeed)
			time += Time.deltaTime;
		currentValue = Mathf.MoveTowards(currentValue, maxSpeed, speedChangeMultiplier * Time.deltaTime * maxSpeed);

		if (changeColor && targetFerrymanColor != null && eid != null && !eid.dead) {
			ferrymanMaterial.color = Color.Lerp(initialFerrymanColor, targetFerrymanColor, (currentValue - initialSpeed) / (maxSpeed - initialSpeed));
			if (enraged) {
				ferrymanMaterial.color = new Color(
					ferrymanMaterial.color.r * enragedColorMultiplier.r,
					ferrymanMaterial.color.g * enragedColorMultiplier.g,
					ferrymanMaterial.color.b * enragedColorMultiplier.b
				);
			}
		}
		if (changeColor && targetCloakColor != null && eid != null && !eid.dead) {
			cloakMaterial.color = Color.Lerp(initialCloakColor, targetCloakColor, (currentValue - initialSpeed) / (maxSpeed - initialSpeed));
			if (enraged) {
				cloakMaterial.color = new Color(
					cloakMaterial.color.r * enragedColorMultiplier.r,
					cloakMaterial.color.g * enragedColorMultiplier.g,
					cloakMaterial.color.b * enragedColorMultiplier.b
				);
			}
		}

		if (currentValue < maxSpeed)
			return;

		if (!addedStyle)
			reached = true;
	}
}