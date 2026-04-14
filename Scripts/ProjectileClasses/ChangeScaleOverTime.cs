using UnityEngine;


namespace BillionDifficulty.EnemyPatches;

class ChangeScaleOverTime : MonoBehaviour {
	public float delay = 0f;
	public float targetScaleMultiplier;
	public float time;

	public float timeSinceStart;
	public Projectile proj;
	public Vector3 currentScale;
	public Vector3 initialScale;
	public Vector3 targetScale;
	public LineRenderer line;
	public bool scaleBeam = false;
	public float currentBeamScale;
	public float initialBeamScale;
	public float targetBeamScale;
	public void Start() {
		proj = GetComponent<Projectile>();
		line = proj.GetComponentInChildren<LineRenderer>();

		currentScale = proj.transform.localScale;
		initialScale = proj.transform.localScale;
		targetScale = initialScale * targetScaleMultiplier;
		timeSinceStart = 0;

		if (!scaleBeam)
			return;

		currentBeamScale = line.startWidth;
		initialBeamScale = line.startWidth;
		targetBeamScale = initialBeamScale * targetScaleMultiplier;
	}
	
	public void Update() {
		if (line == null) {
			scaleBeam = false;
		}

		timeSinceStart += Time.deltaTime;
		if (timeSinceStart < delay)
			return;

		float newX = Mathf.MoveTowards(currentScale.x, targetScale.x, Time.deltaTime * Mathf.Abs(targetScale.x - initialScale.x) / time);
		float newY = Mathf.MoveTowards(currentScale.y, targetScale.y, Time.deltaTime * Mathf.Abs(targetScale.y - initialScale.y) / time);
		float newZ = Mathf.MoveTowards(currentScale.z, targetScale.z, Time.deltaTime * Mathf.Abs(targetScale.z - initialScale.z) / time);
		proj.transform.localScale = new Vector3(newX, newY, newZ);
		currentScale = new Vector3(newX, newY, newZ);

		if (!scaleBeam)
			return;

		float newWidth = Mathf.MoveTowards(currentBeamScale, targetBeamScale, Time.deltaTime * Mathf.Abs(targetBeamScale - initialBeamScale) / time);
		line.startWidth = newWidth;
		line.endWidth = newWidth;
		currentBeamScale = newWidth;
	}
}