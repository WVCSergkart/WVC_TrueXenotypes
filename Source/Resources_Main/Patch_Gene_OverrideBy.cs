using HarmonyLib;
using Verse;

namespace WVC_TrueXenotypes
{
    [HarmonyPatch(typeof(Gene), "OverrideBy")]
	public static class Patch_Gene_OverrideBy
	{
		public static void Postfix(Gene __instance)
		{
			CompXenotype.ResetPawnXenotype(__instance.pawn);
		}
	}

}
