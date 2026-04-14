using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

class TimerFloat : MonoBehaviour {
	public bool deleteOnReached = false;
	public float cooldownMax = 1f;

	public bool running = false;
	public bool keepRunning = false;
	public bool reached = false;
	public float cooldown = 0f;
	public void Update() {
		if (reached && deleteOnReached) {
			UnityObject.Destroy(this);
			return;
		}
		if (!running)
			return;

		cooldown += Time.deltaTime;
		if (cooldown < cooldownMax)
			return;

		reached = true;
		if (!keepRunning)
			running = false;
	}

	public void Run() {
		running = true;
	}
	public void Reset() {
		cooldown = 0f;
		reached = false;
	}
	public void ResetAndRun() {
		Reset();
		Run();
	}
	public void Stop() {
		running = false;
	}
	public void ResetAndStop() {
		Reset();
		Stop();
	}
}