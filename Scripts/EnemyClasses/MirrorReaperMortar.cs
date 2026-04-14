using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

public class MirrorReaperMortar : MonoBehaviour {
	public bool canShootMortar = false;
	public float cooldown = 0f;
	public float cooldownMax = 2.5f;

	public void Start() {
		if (Util.IsHardMode())
			cooldownMax = 1.75f;
	}
	public void Update() {
		if (canShootMortar)
			return;
		cooldown += Time.deltaTime;
		if (cooldown < cooldownMax)
			return;
		cooldown = 0f;
		canShootMortar = true;
	}
}