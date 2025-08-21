using RimWorld;
using System.Collections.Generic;
using Verse;

namespace WVC_TrueXenotypes
{

	public class HediffCompProperties_TrueParentGenes : HediffCompProperties
	{

		public bool addSurrogateGenes = false;

		public string uniqueTag = "trueParentGenes";

		public HediffCompProperties_TrueParentGenes()
		{
			compClass = typeof(HediffComp_TrueParentGenes);
		}

	}

	public class HediffComp_TrueParentGenes : HediffComp
	{

		public HediffCompProperties_TrueParentGenes Props => (HediffCompProperties_TrueParentGenes)props;

		private bool xenotypeUpdated = false;

		private int nextTick = 2;

        public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			UpdateGeneSet(false);
		}

        public override void CompPostTick(ref float severityAdjustment)
        {
            UpdateGeneSet(true);
        }

        private void UpdateGeneSet(bool ticker = true)
        {
            if (xenotypeUpdated)
            {
                return;
            }
            if (!WVC_TrueXenotypes.settings.enable_TrueParentGenes)
            {
                xenotypeUpdated = true;
                return;
            }
            nextTick--;
            if (nextTick > 0 && ticker)
            {
                return;
            }
            if (parent is Hediff_Pregnant pregnancy)
            {
                GeneSet newGeneSet = new();
                //AddParentGenes(pregnancy.Mother, newGeneSet);
                //AddParentGenes(pregnancy.Father, newGeneSet);
                AddParentsGenes(pregnancy.Mother, pregnancy.Father, newGeneSet);
                if (pregnancy.Mother == null && pregnancy.Father == null || Props.addSurrogateGenes)
                {
                    AddParentGenes(parent.pawn, newGeneSet);
                }
                if (!newGeneSet.GenesListForReading.NullOrEmpty())
                {
                    newGeneSet.SortGenes();
                    pregnancy.geneSet = newGeneSet;
                }
            }
            xenotypeUpdated = true;
        }

        public static void AddParentGenes(Pawn parent, GeneSet geneSet)
		{
			if (parent?.genes == null)
			{
				return;
			}
			List<GeneDef> genes = CompXenotype.ConvertGenesInGeneDefs(parent.genes.Endogenes);
			foreach (GeneDef gene in genes)
			{
				if (geneSet.GenesListForReading.Contains(gene))
				{
					continue;
				}
				geneSet.AddGene(gene);
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmos()
		{
			if (DebugSettings.ShowDevGizmos)
			{
				Command_Action command_Action = new()
				{
					defaultLabel = "DEV: TryChildUpdatedXenotype",
					action = delegate
					{
						xenotypeUpdated = false;
						UpdateGeneSet(true);
					}
				};
				yield return command_Action;
			}
		}

		public static void AddParentsGenes(Pawn mother, Pawn father, GeneSet geneSet)
        {
            List<GeneDef> newGenes = [];
            List<GeneDef> matchGenes = [];
            AddGenes(father, newGenes, matchGenes);
            AddGenes(mother, newGenes, matchGenes);
            if (newGenes.NullOrEmpty())
            {
                return;
            }
            foreach (GeneDef gene in newGenes)
            {
                if (gene.biostatArc != 0 && !matchGenes.Contains(gene))
                {
                    continue;
                }
                if (geneSet.GenesListForReading.Contains(gene))
                {
                    continue;
                }
                geneSet.AddGene(gene);
            }

            static void AddGenes(Pawn parent, List<GeneDef> newGenes, List<GeneDef> matchGenes)
            {
                if ((parent?.genes) == null)
                {
                    return;
                }
                List<GeneDef> genes = CompXenotype.ConvertGenesInGeneDefs(parent.genes.Endogenes);
                foreach (GeneDef gene in genes)
                {
                    if (newGenes.Contains(gene))
                    {
                        matchGenes.Add(gene);
                        continue;
                    }
                    newGenes.Add(gene);
                }
            }
        }

        public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_Values.Look(ref xenotypeUpdated, Props.uniqueTag + "_xenotypeUpdated", false);
		}

	}

}
