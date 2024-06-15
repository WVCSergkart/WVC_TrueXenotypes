using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace WVC_TrueXenotypes
{

	public class CompProperties_Xenotype : CompProperties
	{

		// public bool shouldResurrect = true;

		// public int recacheFrequency = 60;

		// public IntRange resurrectionDelay = new(6000, 9000);

		public string uniqueTag = "TrueXenotype";

		public CompProperties_Xenotype()
		{
			compClass = typeof(CompXenotype);
		}

		// public override void ResolveReferences(ThingDef parentDef)
		// {
			// if (shouldResurrect && parentDef.race?.corpseDef != null)
			// {
				// if (parentDef.race.corpseDef.GetCompProperties<CompProperties_UndeadCorpse>() != null)
				// {
					// return;
				// }
				// CompProperties_UndeadCorpse undead_comp = new();
				// undead_comp.resurrectionDelay = resurrectionDelay;
				// undead_comp.uniqueTag = uniqueTag;
				// parentDef.race.corpseDef.comps.Add(undead_comp);
			// }
		// }

	}

	public class CompXenotype : ThingComp
	{

		public CompProperties_Xenotype Props => (CompProperties_Xenotype)props;

		private bool xenotypeUpdated = false;

		private int nextTick = 60;

		// =================

		public override void CompTick()
		{
			Tick();
		}

		public override void CompTickRare()
		{
			Tick();
		}

		public override void CompTickLong()
		{
			Tick();
		}

		public void Tick()
		{
			if (xenotypeUpdated)
			{
				return;
			}
			nextTick--;
			if (parent is not Pawn pawn || !pawn.Spawned || nextTick > 0)
			{
				return;
			}
			if (TryUpdatedXenotype(pawn))
			{
				xenotypeUpdated = true;
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (DebugSettings.ShowDevGizmos)
			{
				Command_Action command_Action = new()
				{
					defaultLabel = "DEV: TryUpdatedXenotype",
					action = delegate
					{
						TryUpdatedXenotype(parent as Pawn);
					}
				};
				yield return command_Action;
			}
		}

		public static List<GeneDef> ConvertGenesInGeneDefs(List<Gene> genes)
		{
			List<GeneDef> geneDefs = new();
			foreach (Gene item in genes)
			{
				geneDefs.Add(item.def);
			}
			return geneDefs;
		}

		public static void ResetPawnXenotype(Pawn pawn)
		{
			// Log.Error("Check CompHumanlike");
			CompXenotype humanlike = pawn.TryGetComp<CompXenotype>();
			if (humanlike != null)
			{
				// Log.Error("CompHumanlike Reset");
				humanlike.ResetXenotype();
			}
		}

		public void ResetXenotype()
		{
			// Log.Error("Reset");
			nextTick = 60;
			xenotypeUpdated = false;
		}

		public bool TryUpdatedXenotype(Pawn pawn)
		{
			if (pawn != null)
			{
				// List<XenotypeDef> xenotypes = DefDatabase<XenotypeDef>.AllDefsListForReading.OrderBy((XenotypeDef xeno) => xeno.inheritable ? 1 : 2);
				List<GeneDef> pawnGenes = ConvertGenesInGeneDefs(pawn.genes.GenesListForReading);
				Dictionary<XenotypeDef, float> matchedXenotypes = new();
				foreach (XenotypeDef xenotypeDef in DefDatabase<XenotypeDef>.AllDefsListForReading.OrderBy((XenotypeDef xeno) => xeno.inheritable ? 1 : 2))
				{
					if (xenotypeDef.genes.NullOrEmpty())
					{
						continue;
					}
					bool match = true;
					float matchingGenesCount = 0f;
					foreach (GeneDef geneDef in xenotypeDef.genes)
					{
						if (!pawnGenes.Contains(geneDef))
						{
							match = false;
							break;
						}
						matchingGenesCount++;
					}
					if (!match)
					{
						continue;
					}
					// float matchingGenesCount = XaG_GeneUtility.GetMatchingGenesList(pawn.genes.GenesListForReading, xenotypeDef.genes).Count;
					matchedXenotypes[xenotypeDef] = matchingGenesCount / xenotypeDef.genes.Count;
				}
				XenotypeDef resultXenotype = XenotypeDefOf.Baseliner;
				float currentMatchValue = 0f;
				if (!matchedXenotypes.NullOrEmpty())
				{
					foreach (var item in matchedXenotypes)
					{
						// Log.Error(item.Key.defName + " | Match: " + item.Value.ToString());
						if (item.Value > currentMatchValue || item.Value == currentMatchValue && !item.Key.inheritable)
						{
							currentMatchValue = item.Value;
							resultXenotype = item.Key;
						}
					}
				}
				bool baseliner = true;
				// GeneDef melanin = pawn.genes.GetMelaninGene();
				// GeneDef hairColor = pawn.genes.GetHairColorGene();
				foreach (GeneDef geneDef in pawnGenes)
				{
					if (geneDef.passOnDirectly)
					{
						baseliner = false;
						break;
					}
				}
				if (!baseliner && resultXenotype != XenotypeDefOf.Baseliner)
				{
					pawn.genes?.SetXenotypeDirect(resultXenotype);
				}
				else if (!baseliner)
				{
					pawn.genes?.SetXenotypeDirect(XenotypeDefOf.Baseliner);
					if (pawn.genes.xenotypeName == null)
					{
						pawn.genes.xenotypeName = GeneUtility.GenerateXenotypeNameFromGenes(pawnGenes);
					}
					if (pawn.genes.iconDef == null)
					{
						pawn.genes.iconDef = DefDatabase<XenotypeIconDef>.AllDefsListForReading.RandomElement();
					}
				}
				return true;
			}
			return false;
		}

		// public override void PostExposeData()
		// {
			// base.PostExposeData();
			// Scribe_Values.Look(ref xenotypeUpdated, Props.uniqueTag + "_xenotypeUpdated", false);
		// }

	}

}
