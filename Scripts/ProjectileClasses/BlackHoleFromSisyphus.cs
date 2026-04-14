using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

public class BlackHoleFromSisyphus : MonoBehaviour {
	public Sisyphus sisy = null;
	public void Update() {
		if (sisy == null) {
			gameObject.GetComponent<BlackHoleProjectile>().Explode();
		}
	}
}