using UnityEngine;


namespace BillionDifficulty.EnemyPatches;


public class SlowDownOverTime : MonoBehaviour {
	public float slowRate;

	public float cooldownMax = 0.01f;
	public float cooldown = 0f;
	public bool stop = false;
	public Projectile proj;
	public void Start() {
		proj = GetComponent<Projectile>();
	}
	public void Update() {
		if (proj.parried)
			return;


		if (stop)
			return;

		cooldown += Time.deltaTime;
		if (cooldown < cooldownMax)
			return;

		cooldown = 0f;

		if (proj.speed > 0f) {
			proj.speed -= slowRate;
		} else {
			proj.speed = 0.001f;
			stop = true;
		}
	}
}