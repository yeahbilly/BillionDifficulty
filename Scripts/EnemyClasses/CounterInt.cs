using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

class CounterInt : MonoBehaviour {
	public int maxValue;
	public bool randomized = false;
	public int randomMin;
	public int randomMax;

	public int value = 1;
	public void Start() {
		TryRandomize();
	}
	public void Add() {
		if (value < maxValue) {
			value += 1;
			return;
		}
		value = 1;
		TryRandomize();
	}
	public void TryRandomize() {
		if (randomized) {
			maxValue = Random.Range(randomMin, randomMax + 1);
		}
	}
}