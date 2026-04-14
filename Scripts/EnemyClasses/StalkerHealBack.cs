using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

/// <summary>
/// Added to the sand zone
/// </summary>
class StalkerHealBackMessenger : MonoBehaviour {
	public Enemy source;
	public void Awake() {
		source = null; // to avoid the warning message
	}
	public void HealSelf() {
		source.health += 1.5f;
		source.eid.health += 1.5f;
	}
}

/// <summary>
/// Added to enemies that touch the sand zone
/// </summary>
class StalkerHealBack : MonoBehaviour {
	public Enemy source;
	public bool healed = false;
	public void Update() {
		if (source == null || healed)
			return;
		source.health += 1.5f * source.eid.totalDamageModifier;
		source.eid.health += 1.5f * source.eid.totalDamageModifier;
		healed = true;

		if (!Util.IsHardMode())
			return;

		Enemy thisEnemy = GetComponent<Enemy>();
		thisEnemy?.health += 5f * source.eid.totalDamageModifier;
		EnemyIdentifier thisEid = GetComponent<EnemyIdentifier>();
		thisEid?.health += 5f * source.eid.totalDamageModifier;
	}
}