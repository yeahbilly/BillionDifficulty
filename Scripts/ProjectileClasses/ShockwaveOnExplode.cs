using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;


class ShockwaveOnExplode : MonoBehaviour {
	public float speed = 50f;
	public float damage = 25f;
	public float maxSize = 75f;
	public float height = 15f;
	public EnemyType enemyType;
	public float totalDamageModifier;

	public void SpawnShockwave(Vector3 position) {
		GameObject shockwave = UnityObject.Instantiate<GameObject>(Plugin.Prefabs["PhysicalShockwave"], position, Quaternion.identity);
		Vector3 scale = shockwave.transform.localScale;
		shockwave.transform.localScale = new Vector3(scale.x, height, scale.z);
		PhysicalShockwave component = shockwave.GetComponent<PhysicalShockwave>();
		component.speed = speed;// Brutal (cerberus): 75f
		component.damage = Mathf.RoundToInt(damage * totalDamageModifier);
		component.maxSize = maxSize; // default (cerberus): 100f
		component.enemyType = enemyType;
		component.enemy = true;
	}
}