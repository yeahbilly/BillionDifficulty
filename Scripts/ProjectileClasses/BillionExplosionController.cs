using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

class BillionExplosionController : MonoBehaviour {
	public float maxSizeMultiplier = 1f;
	public float speedMultiplier = 1f;
	public float enemyDamageMultiplier = 1f;
	public int damage = -1;
	public float damageMultiplier = 1f;
	public void Start() {
		maxSizeMultiplier = 1f;
		speedMultiplier = 1f;
		enemyDamageMultiplier = 1f;
		damage = -1;
		damageMultiplier = 1f;
	}
}