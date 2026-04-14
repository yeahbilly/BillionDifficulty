using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

public class TimedRotator : MonoBehaviour {
	public float cooldownMax = 0.35f;
	public float angle = 45f;
	public Vector3 spinDirection = new Vector3(0f, 0f, 1f);

	public float cooldown = 0f;

	public void Update() {
		cooldown += Time.deltaTime;
		if (cooldown < cooldownMax)
			return;
		
		cooldown = 0f;
		transform.Rotate(angle * spinDirection.normalized, Space.Self);
	}
}