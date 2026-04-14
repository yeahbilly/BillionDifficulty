using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

public class StrayAttack : MonoBehaviour {
	public float animSpeedFast = 4f;
	public float animSpeedSlow = 1.75f;

	public int projectilesLeft = 0;
	public Animator anim = null;
	public EnemyIdentifier eid = null;
	public void Update() {
		if (eid == null || eid.dead || anim == null) {
			UnityObject.Destroy(this);
			return;
		}
		if (projectilesLeft > 0 && eid.zombie != null && eid.zombie.grounded) {
			anim.speed = animSpeedFast; // Brutal: 1.5f
		} else if (eid.zombie != null && !eid.zombie.grounded) {
			projectilesLeft = 0;
		}
		if (projectilesLeft == 0) {
			anim.speed = animSpeedSlow; // Brutal: 1.5f
		}
	}
}