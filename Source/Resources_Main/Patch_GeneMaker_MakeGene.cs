// using System.Collections.IEnumerable;
using RimWorld;
using HarmonyLib;
using Verse;

namespace WVC_TrueXenotypes
{

    [HarmonyPatch(typeof(GeneMaker), "MakeGene")]
	public static class Patch_GeneMaker_MakeGene
	{

		public static void Prefix(ref GeneDef def)
		{
			if (def != null && WVC_TrueXenotypes.settings.GetGroupAndGene(def, out GeneDef newGeneDef))
			{
				def = newGeneDef;
			}
		}

	}

}
