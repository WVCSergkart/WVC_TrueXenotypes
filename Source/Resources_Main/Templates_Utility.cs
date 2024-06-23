using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using static Verse.GeneSymbolPack;

namespace WVC_TrueXenotypes
{

	public static class GeneratorUtility
	{

		// CustomXenotypes

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
			return xenotypeDef;
		}

		public static List<GeneDef> GetGenesByNames(List<string> genes, List<GeneDef> geneDefs)
		{
			List<GeneDef> list = new();
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
