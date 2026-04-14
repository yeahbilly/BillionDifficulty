using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

public class BoolValue : MonoBehaviour {
	public bool value = false;
	public string description;

	public static bool? Get(string target, GameObject obj) {
		foreach (BoolValue bv in obj.GetComponents<BoolValue>()) {
			if (bv.description == target) {
				return bv.value;
			}
		}
		return null;
	}
	public static void Set(string target, bool newValue, GameObject obj) {
		foreach (BoolValue bv in obj.GetComponents<BoolValue>()) {
			if (bv.description == target) {
				bv.value = newValue;
			}
		}
	}
}