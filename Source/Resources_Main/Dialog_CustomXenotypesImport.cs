using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace WVC_TrueXenotypes
{

	public class Dialog_CustomXenotypes_Import : Dialog_XenotypeList
	{


		public Dialog_CustomXenotypes_Import()
		{
			interactButLabel = "WVC_XaG_DialogCustomXenotypesImport".Translate();
			deleteTipKey = "DeleteThisXenotype";
		}

		protected override void DoFileInteraction(string fileName)
		{
			string filePath = GenFilePaths.AbsFilePathForXenotype(fileName);
			PreLoadUtility.CheckVersionAndLoad(filePath, ScribeMetaHeaderUtility.ScribeHeaderMode.Xenotype, delegate
			{
				if (GameDataSaveLoader.TryLoadXenotype(filePath, out var xenotype))
				{
					if (WVC_TrueXenotypes.settings.importedCustomXenotypes == null)
					{
						WVC_TrueXenotypes.settings.importedCustomXenotypes = new();
					}
					XenotypeProp newXenotype = new();
					newXenotype.label = xenotype.name;
					newXenotype.iconPath = xenotype.iconDef.texPath;
					newXenotype.inheritable = xenotype.inheritable;
					newXenotype.geneDefs = new();
					foreach (GeneDef geneDef in xenotype.genes)
					{
						newXenotype.geneDefs.Add(geneDef.defName);
					}
					WVC_TrueXenotypes.settings.importedCustomXenotypes.Add(newXenotype);
					Messages.Message("WVC_XaG_DialogCustomXenotypesImport_Success".Translate(), MessageTypeDefOf.TaskCompletion, historical: false);
				}
				// Close();
			});
		}

	}

}
