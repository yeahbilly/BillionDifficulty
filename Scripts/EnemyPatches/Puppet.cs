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
		if (!Util.IsDifficulty(19))
			return;
		float hardModeMult = (!Util.IsHardMode()) ? 1.5f : 2.25f;
		__instance.GetComponent<Animator>().speed = hardModeMult;
		__instance.GetComponent<NavMeshAgent>().speed *= hardModeMult;
	}
}