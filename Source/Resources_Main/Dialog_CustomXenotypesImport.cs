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
					WVC_TrueXenotypes.settings.importedCustomXenotypes ??= [];
                    XenotypeProp newXenotype = new()
                    {
                        label = xenotype.name,
                        iconPath = xenotype.iconDef.texPath,
                        inheritable = xenotype.inheritable,
                        geneDefs = []
                    };
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

	public class Dialog_CustomXenotypes_Remove : Window
	{

		public Dialog_CustomXenotypes_Remove()
		{
			//remoteContoller.RemoteControl_Recache();
			forcePause = true;
			doCloseButton = true;
		}

		protected Vector2 scrollPosition;
		protected float bottomAreaHeight;

		public override void DoWindowContents(Rect inRect)
		{
			Vector2 vector = new(inRect.width - 16f, 40f);
			float y = vector.y;
			float height = (float)WVC_TrueXenotypes.settings.importedCustomXenotypes.Count * y;
			Rect viewRect = new(0f, 0f, inRect.width - 16f, height);
			float num = inRect.height - Window.CloseButSize.y - bottomAreaHeight - 18f;
			Rect outRect = inRect.TopPartPixels(num);
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
			float num2 = 0f;
			int num3 = 0;
			foreach (XenotypeProp controller in WVC_TrueXenotypes.settings.importedCustomXenotypes.ToList())
			{
				if (controller is XenotypeProp prop && num2 + vector.y >= scrollPosition.y && num2 <= scrollPosition.y + outRect.height)
				{
					Rect rect = new(0f, num2, vector.x, vector.y);
					TooltipHandler.TipRegion(rect, controller.label);
					if (num3 % 2 == 0)
					{
						Widgets.DrawAltRect(rect);
					}
					Widgets.BeginGroup(rect);
					GUI.color = Color.white;
					Text.Font = GameFont.Small;
					Rect rect3 = new(rect.width - 100f, (rect.height - 36f) / 2f, 100f, 36f);
					if (Widgets.ButtonText(rect3, "Remove".Translate().CapitalizeFirst()))
					{
						Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation("WVC_XaG_ButtonCustomXenotypesImport_ResetWarning".Translate(), delegate
						{
							WVC_TrueXenotypes.settings.importedCustomXenotypes.Remove(prop);
							Messages.Message("WVC_XaG_ButtonCustomXenotypesImport_Reseted".Translate(), MessageTypeDefOf.TaskCompletion, historical: false);
						});
						Find.WindowStack.Add(window);
						break;
					}
					Rect rect4 = new(40f, 0f, 200f, rect.height);
					Text.Anchor = TextAnchor.MiddleLeft;
					Widgets.Label(rect4, prop.label.CapitalizeFirst().Truncate(rect4.width * 1.8f));
					Text.Anchor = TextAnchor.UpperLeft;
					Rect rect5 = new(0f, 0f, 36f, 36f);
					Widgets.DrawTextureFitted(rect5, ContentFinder<Texture2D>.Get(prop.iconPath), 1.2f);
					Widgets.EndGroup();
				}
				num2 += vector.y;
				num3++;
			}
			Widgets.EndScrollView();
		}

	}

}
