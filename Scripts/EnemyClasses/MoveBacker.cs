using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.EnemyPatches;

public class MoveBacker : MonoBehaviour {
	public Rigidbody rb;
	public NavMeshAgent nma;
	public EnemyIdentifier eid;
	public float strength = 150f;
	public bool moving = false;
	public void Start() {
		rb = GetComponent<Rigidbody>();
		nma = GetComponent<NavMeshAgent>();
		eid = GetComponent<EnemyIdentifier>();
	}
	public void Update() {
		if (eid.dead) {
			UnityObject.Destroy(this);
		}
	}
	public IEnumerator MoveBack() {
		moving = true;

		if (!eid || eid.dead) {
			yield break;
		}

		Vector3 target = (eid.target == null) ? NewMovement.Instance.transform.position : eid.target.position;
		Vector3 forward = target - transform.position;
		forward.y = 0f;
		forward.Normalize();

		Vector3 targetPosition = transform.position - 15f * forward;

		NavMeshHit hit;
		LayerMask groundLayer = LayerMask.GetMask("Outdoors", "OutdoorsBaked", "Environment", "EnvironmentBaked");
		float navMeshSampleRadius = 1f;
		float groundCheckDistance = 2f;

		if (!NavMesh.SamplePosition(targetPosition, out hit, navMeshSampleRadius, NavMesh.AllAreas)) {
			moving = false;
			yield break;
		}

		// checks if floor exists below the navmesh position
		Vector3 rayOrigin = hit.position + transform.up * 0.1f; // slightly above to avoid self-collision
		if (!Physics.Raycast(rayOrigin, -1f * transform.up, groundCheckDistance, groundLayer)) {
			moving = false;
			yield break;
		}
		
		if (!nma) {
			moving = false;
			yield break;
		}

		nma.enabled = false;
		rb.isKinematic = false;
		rb.useGravity = true;
		rb.drag = 12f;
		rb.AddForce(-forward * strength, ForceMode.VelocityChange);
		yield return new WaitForSeconds(0.5f);
		//yield return new WaitUntil(() => rb.velocity.magnitude < 0.05f);
		rb.isKinematic = true;
		rb.useGravity = false;

		if (!nma) {
			moving = false;
			yield break;
		}

		nma.enabled = true;
		rb.drag = 0f;
		nma.Warp(rb.position);
		moving = false;
	}
}