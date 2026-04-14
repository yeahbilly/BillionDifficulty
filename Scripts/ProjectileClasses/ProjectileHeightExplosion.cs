using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

class ProjectileHeightExplosion : MonoBehaviour {
	public float maxDistance = -1f;
	public EnemyTarget target;
	public Projectile proj;

	public void Start() {
		proj = GetComponent<Projectile>();
	}

	public void Update() {
		if (target == null)
			return;
		if (target.position.y > transform.position.y)
			return;

		float distance = Vector3.Distance(target.position, transform.position);
		bool isWithinMaxDistance =
			maxDistance > 0f
			&& distance <= maxDistance
			&& distance > maxDistance/2f;
		
		if (maxDistance < 0f || isWithinMaxDistance) {
			proj.Explode();
		}
	}
}