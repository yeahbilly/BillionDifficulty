using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

class SentryMortar : MonoBehaviour {
	public bool canShootOrb = true;
	public Rigidbody rb;
	public void FixedUpdate() {
		if (rb != null) {
			rb.AddForce(0, -5f * Mathf.Abs(rb.velocity.y), 0);
		}
	}
}