using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;

namespace WVC_TrueXenotypes
{

	public class CompProperties_Xenotype : CompProperties
	{

		// public bool shouldResurrect = true;

		// public int recacheFrequency = 60;

		public float minMatchPercent = 0.75f;

		// public IntRange resurrectionDelay = new(6000, 9000);

		public string uniqueTag = "trueXenotype";

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

		private int nextTick = 10;

		// =================

		public override void CompTickInterval(int delta)
		{
			Tick(delta);
		}

		// public override void CompTickRare()
		// {
			// Tick();
		// }

		// public override void CompTickLong()
		// {
			// Tick();
		// }

		public void Tick(int delta)
		{
			if (xenotypeUpdated)
			{
				return;
			}
			if (!WVC_TrueXenotypes.settings.enable_TrueXenotypes)
			{
				xenotypeUpdated = true;
				return;
			}
			nextTick -= delta;
			if (nextTick > 0 || parent is not Pawn pawn)
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
                yield return new Command_Action
				{
					defaultLabel = "DEV: TryUpdatedXenotype",
					action = delegate
					{
						TryUpdatedXenotype(parent as Pawn);
					}
				};
				//yield return new Command_Action
				//{
				//	defaultLabel = "DEV: AddGene",
				//	action = delegate
				//	{
				//		(parent as Pawn).genes.AddGene(DefDatabase<GeneDef>.GetNamed("PsychicAbility_Dull"), true);
				//	}
				//};
            }
		}

		public static List<GeneDef> ConvertGenesInGeneDefs(List<Gene> genes)
		{
			List<GeneDef> geneDefs = [];
			foreach (Gene item in genes)
			{
				geneDefs.Add(item.def);
			}
			return geneDefs;
		}

		public static void ResetPawnXenotype(Pawn pawn)
		{
            pawn.TryGetComp<CompXenotype>()?.ResetXenotype();
        }

		public void ResetXenotype()
		{
			// Log.Error("Reset");
			nextTick = 60;
			xenotypeUpdated = false;
		}

		//[Obsolete]
		//public bool TryUpdatedXenotype(Pawn pawn)
		//{
		//	if (pawn != null)
		//	{
		//		List<GeneDef> pawnGenes = ConvertGenesInGeneDefs(pawn.genes.GenesListForReading);
		//		Dictionary<XenotypeDef, float> matchedXenotypes = new();
		//		bool xenos = !pawn.genes.Xenogenes.NullOrEmpty();
		//		foreach (XenotypeDef xenotypeDef in DefDatabase<XenotypeDef>.AllDefsListForReading.OrderBy((XenotypeDef xeno) => xeno.inheritable ? 1 : 2))
		//		{
		//			if (xenotypeDef.genes.NullOrEmpty())
		//			{
		//				continue;
		//			}
		//			if (!xenos && !xenotypeDef.inheritable)
		//			{
		//				continue;
		//			}
		//			bool match = true;
		//			float matchingGenesCount = 0f;
		//			foreach (GeneDef geneDef in xenotypeDef.genes)
		//			{
		//				if (!pawnGenes.Contains(geneDef))
		//				{
		//					match = false;
		//					break;
		//				}
		//				matchingGenesCount++;
		//			}
		//			if (!match)
		//			{
		//				continue;
		//			}
		//			matchedXenotypes[xenotypeDef] = matchingGenesCount - xenotypeDef.genes.Count;
		//		}
		//		XenotypeDef resultXenotype = XenotypeDefOf.Baseliner;
		//		float currentMatchValue = -999f;
		//		if (!matchedXenotypes.NullOrEmpty())
		//		{
		//			foreach (var item in matchedXenotypes)
		//			{
		//				if (item.Value > currentMatchValue || item.Value == currentMatchValue && !item.Key.inheritable)
		//				{
		//					currentMatchValue = item.Value;
		//					resultXenotype = item.Key;
		//				}
		//			}
		//		}
		//		bool baseliner = true;
		//		foreach (GeneDef geneDef in pawnGenes)
		//		{
		//			if (geneDef.passOnDirectly)
		//			{
		//				baseliner = false;
		//				break;
		//			}
		//		}
		//		if (!baseliner && resultXenotype != XenotypeDefOf.Baseliner)
		//		{
		//			pawn.genes?.SetXenotypeDirect(resultXenotype);
		//		}
		//		else if (!baseliner)
		//		{
		//			pawn.genes?.SetXenotypeDirect(XenotypeDefOf.Baseliner);
		//			if (pawn.genes.xenotypeName == null)
		//			{
		//				pawn.genes.xenotypeName = GeneUtility.GenerateXenotypeNameFromGenes(pawnGenes);
		//			}
		//			if (pawn.genes.iconDef == null)
		//			{
		//				pawn.genes.iconDef = DefDatabase<XenotypeIconDef>.AllDefsListForReading.RandomElement();
		//			}
		//		}
		//		else if (baseliner)
		//		{
		//			pawn.genes?.SetXenotypeDirect(XenotypeDefOf.Baseliner);
		//		}
		//		return true;
		//	}
		//	return false;
		//}

		public bool TryUpdatedXenotype(Pawn pawn)
		{
			if (pawn == null)
			{
				return true;
			}
			if (pawn.genes.Xenotype != XenotypeDefOf.Baseliner && (pawn.genes.Xenotype.inheritable || !pawn.genes.Xenogenes.NullOrEmpty()))
			{
				return true;
			}
			// List<XenotypeDef> xenotypes = DefDatabase<XenotypeDef>.AllDefsListForReading.OrderBy((XenotypeDef xeno) => xeno.inheritable ? 1 : 2);
			List<GeneDef> pawnGenes = ConvertGenesInGeneDefs(pawn.genes.GenesListForReading);
			Dictionary<XenotypeDef, float> matchedXenotypes = [];
			bool xenos = !pawn.genes.Xenogenes.NullOrEmpty();
			foreach (XenotypeDef xenotypeDef in DefDatabase<XenotypeDef>.AllDefsListForReading.OrderBy((XenotypeDef xeno) => xeno.inheritable ? 1 : 2))
			{
				if (xenotypeDef.genes.NullOrEmpty())
				{
					continue;
				}
				if (!xenos && !xenotypeDef.inheritable)
				{
					continue;
				}
				float matchingGenesCount = 0f;
				foreach (GeneDef geneDef in xenotypeDef.genes)
				{
					if (pawnGenes.Contains(geneDef))
					{
						matchingGenesCount++;
					}
				}
				matchedXenotypes[xenotypeDef] = matchingGenesCount;
			}
			XenotypeDef resultXenotype = XenotypeDefOf.Baseliner;
			float currentMatchValue = -999f;
			if (!matchedXenotypes.NullOrEmpty())
			{
				//string text = "";
				//foreach (var item in matchedXenotypes)
				//{
				//	text += "\n" + item.Key.defName + ": " + item.Value.ToString();
				//}
				// Log.Error("MatchedXenotypesRate:" + text);
				foreach (var item in matchedXenotypes)
				{
					// Log.Error(item.Key.defName + " | Match: " + item.Value.ToString());
					if (item.Value < pawnGenes.Count * Props.minMatchPercent)
					{
						// Log.Error(item.Key.defName + " skipped.");
						continue;
					}
					if (item.Value > currentMatchValue || item.Value == currentMatchValue && !item.Key.inheritable)
					{
						currentMatchValue = item.Value;
						resultXenotype = item.Key;
					}
				}
			}
			// Log.Error("Greatest match: " + resultXenotype.defName);
			if (resultXenotype == XenotypeDefOf.Baseliner && pawn.genes.Xenotype != XenotypeDefOf.Baseliner && pawn.genes.Xenotype.genes.Where((GeneDef geneDef) => geneDef.selectionWeight > 0 && geneDef.canGenerateInGeneSet).ToList().Count == 0)
			{
				// Log.Error("Return current xeno: " + pawn.genes.Xenotype.defName);
				return true;
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
				//if (!TryGetCustomXenotyte(pawn, out CustomXenotype customXenotype))
				//{
				//}
				//else
				//{

				//}
				pawn.genes.xenotypeName ??= GeneUtility.GenerateXenotypeNameFromGenes(pawnGenes);
				pawn.genes.iconDef ??= DefDatabase<XenotypeIconDef>.AllDefsListForReading.RandomElement();
			}
			else if (baseliner)
			{
				pawn.genes?.SetXenotypeDirect(XenotypeDefOf.Baseliner);
			}
			return true;
		}

		//private bool TryGetCustomXenotyte(Pawn pawn, out CustomXenotype customXenotype)
		//{
		//	customXenotype = null;
		//	foreach (CustomXenotype item in GetCustomXenotypesList())
		//	{
		//		if (GeneUtility.PawnIsCustomXenotype(pawn, item))
		//		{
		//			customXenotype = item;
		//			break;
		//		}
		//	}
		//	return customXenotype != null;
		//}

		//private List<CustomXenotype> GetCustomXenotypesList()
		//{
		//	List<CustomXenotype> xenotypes = new();
		//	foreach (FileInfo item in GenFilePaths.AllCustomXenotypeFiles.OrderBy((FileInfo f) => f.LastWriteTime))
		//	{
		//		string filePath = GenFilePaths.AbsFilePathForXenotype(Path.GetFileNameWithoutExtension(item.Name));
		//		PreLoadUtility.CheckVersionAndLoad(filePath, ScribeMetaHeaderUtility.ScribeHeaderMode.Xenotype, delegate
		//		{
		//			if (GameDataSaveLoader.TryLoadXenotype(filePath, out var xenotype))
		//			{
		//				xenotypes.Add(xenotype);
		//			}
		//		}, skipOnMismatch: true);
		//	}
		//	return xenotypes;
		//}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref xenotypeUpdated, Props.uniqueTag + "_xenotypeUpdated", false);
		}

	}

}
