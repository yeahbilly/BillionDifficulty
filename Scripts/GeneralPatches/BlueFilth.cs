using HarmonyLib;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace BillionDifficulty.GenaralPatches;

[HarmonyPatch(typeof(ZombieMelee), nameof(ZombieMelee.Start))]
public class BlueFilthPatch {
	public static void Postfix(ZombieMelee __instance) {
		if (__instance.difficulty != 19)
			return;
		
		bool gotBlueFilth = __instance.eid.enemyType == EnemyType.Filth && Random.Range(0, Plugin.BlueFilthRarity) == 0;
		if (!gotBlueFilth)
			return;

		Material blueFilthMaterial = UnityObject.Instantiate(__instance.originalMaterial);
		Material blueFilthBiteMaterial = UnityObject.Instantiate(__instance.biteMaterial);
		blueFilthMaterial.mainTexture = Plugin.blueFilthTexture;
		blueFilthBiteMaterial.mainTexture = Plugin.blueFilthBiteTexture;

		__instance.originalMaterial = blueFilthMaterial;
		__instance.biteMaterial = blueFilthBiteMaterial;

		foreach (SkinnedMeshRenderer renderer in __instance.GetComponentsInChildren<SkinnedMeshRenderer>()) {
			renderer.material = blueFilthMaterial;
			var block = new MaterialPropertyBlock();
			renderer.GetPropertyBlock(block);
			block.SetTexture("_MainTex", Plugin.blueFilthTexture);
			renderer.SetPropertyBlock(block);
		}
	}
}