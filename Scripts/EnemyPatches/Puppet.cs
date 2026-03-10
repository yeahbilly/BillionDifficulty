using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;


namespace BillionDifficulty.EnemyPatches;

// !!! PATCHGROUP: PUPPET !!!
[HarmonyPatch(typeof(Puppet))]
public class PuppetPatch {
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Puppet), nameof(Puppet.Start))]
	public static void Postfix(Puppet __instance) {
		if (!Util.IsDifficulty(19)) {
			return;
		}
		__instance.gameObject.GetComponent<Animator>().speed = 1.5f;
		__instance.gameObject.GetComponent<NavMeshAgent>().speed *= 1.5f;
	}
}