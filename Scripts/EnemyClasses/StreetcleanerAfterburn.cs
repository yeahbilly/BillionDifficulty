using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

class StreetcleanerAfterburn : MonoBehaviour {
	public int damage = 0;
	public int tickCount = 4;
	
	public float cooldownMax = 0.5f;
	public float cooldown = 0f;
	public bool damagedThePlayer = false;
	public bool stoppedAttacking = false;
	public bool destroyOnEnd = false;
	public int ticks = 0;
	public void Update() {
		if (!damagedThePlayer || !stoppedAttacking)
			return;

		cooldown += Time.deltaTime;
		if (cooldown < cooldownMax)
			return;
		cooldown = 0f;

		if (ticks < tickCount) {
			ticks += 1;
			if (NewMovement.Instance.boost && !NewMovement.Instance.sliding)
				return;

			NewMovement.Instance.GetHurt(damage: damage, invincible: false, scoreLossMultiplier: 0f);
			return;
		}

		if (destroyOnEnd) {
			UnityObject.Destroy(this);
			return;
		}
		ticks = 0;
		damagedThePlayer = false;
		stoppedAttacking = false;
	}
}