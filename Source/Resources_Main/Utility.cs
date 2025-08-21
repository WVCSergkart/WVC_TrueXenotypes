using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using static Verse.GeneSymbolPack;

namespace WVC_TrueXenotypes
{

	public static class Utility
	{

        // CustomXenotypes

        public static bool GenesDictionaryEnabled => WVC_TrueXenotypes.settings.enable_GenesGroups;

        public static string GetGeneDefName(string oldGeneDef)
        {
            if (!GenesDictionaryEnabled)
            {
                return oldGeneDef;
            }
            foreach (GeneGroup group in WVC_TrueXenotypes.settings.geneGroups)
            {
                string newName = group.GetGeneDefName(oldGeneDef);
                if (newName != oldGeneDef)
                {
                    return newName;
                }
            }
            return oldGeneDef;
        }

        public static bool GetGroupAndGene(GeneDef geneDef, out GeneDef newGeneDef)
        {
            newGeneDef = null;
            if (!GenesDictionaryEnabled)
            {
                return false;
            }
            foreach (GeneGroup group in WVC_TrueXenotypes.settings.geneGroups)
            {
                if (group.MainGeneDef == geneDef)
                {
                    return false;
                }
                if (group.GeneDefs.Contains(geneDef))
                {
                    newGeneDef = group.MainGeneDef;
                    break;
                }
            }
            return newGeneDef != null;
        }

        public static bool InAnyGroup(GeneDef geneDef)
        {
            return WVC_TrueXenotypes.settings.geneGroups.Where((group) => group.GeneDefs.Contains(geneDef) || group.MainGeneDef == geneDef).Any();
        }


        public static GeneDef ConvertToDef(this string geneDef)
		{
			foreach (GeneDef dataGene in DefDatabase<GeneDef>.AllDefsListForReading)
			{
				if (dataGene.defName.Contains(geneDef))
				{
					return dataGene;
				}
			}
			return null;
		}


		public static List<GeneDef> ConvertToDefs(this List<string> geneDefs)
		{
			List<GeneDef> list = [];
			foreach (GeneDef dataGene in DefDatabase<GeneDef>.AllDefsListForReading)
			{
				foreach (string groupGene in geneDefs)
				{
					if (dataGene.defName.Contains(groupGene))
					{
						list.Add(dataGene);
					}
				}
			}
			return list;
		}

		public static void CustomXenotypesImport()
		{
			if (!WVC_TrueXenotypes.settings.enable_CustomXenotypeImporter || WVC_TrueXenotypes.settings.importedCustomXenotypes.NullOrEmpty())
			{
				return;
			}
			List<GeneDef> geneDefs = DefDatabase<GeneDef>.AllDefsListForReading;
			foreach (XenotypeProp xenotypeProp in WVC_TrueXenotypes.settings.importedCustomXenotypes)
			{
				try
				{
					DefGenerator.AddImpliedDef(GetFromXenotypePropTemplate(xenotypeProp, geneDefs));
				}
				catch
				{
					Log.Error("Failed generate xenotype: " + xenotypeProp.label);
				}
			}
		}

		public static XenotypeDef GetFromXenotypePropTemplate(XenotypeProp prop, List<GeneDef> geneDefs)
		{
			XenotypeDef xenotypeDef = new()
			{
				defName = "WVC_TX_" + prop.label + "_Custom",
				label = prop.label,
				description = "UniqueXenotypeDesc".Translate(),
				genes = GetGenesByNames(prop.geneDefs, geneDefs),
				inheritable = prop.inheritable,
				iconPath = prop.iconPath
			};
            if (!prop.description.NullOrEmpty())
            {
                xenotypeDef.description = prop.description;
            }
            return xenotypeDef;
		}

		public static List<GeneDef> GetGenesByNames(List<string> genes, List<GeneDef> geneDefs)
		{
			List<GeneDef> list = [];
			foreach (GeneDef geneDef in geneDefs)
			{
				if (genes.Contains(geneDef.defName))
				{
					list.Add(geneDef);
				}
			}
			return list;
		}

	}
}
