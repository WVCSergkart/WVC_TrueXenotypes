using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using Verse;

namespace WVC_TrueXenotypes
{

	public class WVC_TrueXenotypesSettings : ModSettings
	{

		public bool enable_CustomXenotypeImporter = false;
		public bool enable_TrueParentGenes = false;
		public bool enable_TrueXenotypes = false;
		public List<XenotypeProp> importedCustomXenotypes = new();

		public IEnumerable<string> GetEnabledSettings => from specificSetting in GetType().GetFields()
														 where specificSetting.FieldType == typeof(bool) && (bool)specificSetting.GetValue(this)
														 select specificSetting.Name;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref enable_CustomXenotypeImporter, "enable_CustomXenotypeImporter", defaultValue: false);
			Scribe_Collections.Look(ref importedCustomXenotypes, "importedCustomXenotypes", LookMode.Deep);
			Scribe_Values.Look(ref enable_TrueParentGenes, "enable_TrueParentGenes", defaultValue: false);
			Scribe_Values.Look(ref enable_TrueXenotypes, "enable_TrueXenotypes", defaultValue: false);
		}
	}

	public class WVC_TrueXenotypes : Mod
	{
		public static WVC_TrueXenotypesSettings settings;

		private int PageIndex = 0;

		private static Vector2 scrollPosition = Vector2.zero;

		public WVC_TrueXenotypes(ModContentPack content) : base(content)
		{
			settings = GetSettings<WVC_TrueXenotypesSettings>();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Rect rect = new(inRect);
			rect.y = inRect.y + 40f;
			// Rect baseRect = rect;
			rect = new Rect(inRect)
			{
				height = inRect.height - 40f,
				y = inRect.y + 40f
			};
			// Rect rect2 = rect;
			Widgets.DrawMenuSection(rect);
			List<TabRecord> tabs = new()
			{
				new TabRecord("WVC_TrueXenotypes_Tab_XenotypesSettings".Translate(), delegate
				{
					PageIndex = 0;
					WriteSettings();
				}, PageIndex == 0)
			};
			TabDrawer.DrawTabs(rect, tabs);
			switch (PageIndex)
			{
				case 0:
					GeneralSettings(rect.ContractedBy(15f));
					break;
			}
		}

		public override string SettingsCategory()
		{
			return "WVC - " + "WVC_TrueXenotypesSettings".Translate();
		}

		// General Settings
		// General Settings
		// General Settings

		private void GeneralSettings(Rect inRect)
		{
			Rect outRect = new(inRect.x, inRect.y, inRect.width, inRect.height);
			// Rect rect = new(0f, 0f, inRect.width, inRect.height);
			Rect rect = new(0f, 0f, inRect.width - 30f, inRect.height * 2.0f);
			Widgets.BeginScrollView(outRect, ref scrollPosition, rect);
			Listing_Standard listingStandard = new();
			listingStandard.Begin(rect);
			// =============== Buttons ===============
			listingStandard.CheckboxLabeled("WVC_Label_enable_TrueParentGenes".Translate(), ref settings.enable_TrueParentGenes, "WVC_ToolTip_enable_TrueParentGenes".Translate());
			listingStandard.CheckboxLabeled("WVC_Label_enable_TrueXenotypes".Translate(), ref settings.enable_TrueXenotypes, "WVC_ToolTip_enable_TrueXenotypes".Translate());
			listingStandard.CheckboxLabeled("WVC_Label_enable_CustomXenotypeImporter".Translate(), ref settings.enable_CustomXenotypeImporter, "WVC_ToolTip_enable_CustomXenotypeImporter".Translate());
			if (listingStandard.ButtonText("WVC_XaG_ButtonCustomXenotypesImport".Translate()))
			{
				Find.WindowStack.Add(new Dialog_CustomXenotypes_Import());
			}
			if (listingStandard.ButtonText("WVC_XaG_ButtonCustomXenotypesImport_Reset".Translate()))
			{
				Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation("WVC_XaG_ButtonCustomXenotypesImport_ResetWarning".Translate(), delegate
				{
					WVC_TrueXenotypes.settings.importedCustomXenotypes = new();
					Messages.Message("WVC_XaG_ButtonCustomXenotypesImport_Reseted".Translate(), MessageTypeDefOf.TaskCompletion, historical: false);
				});
				Find.WindowStack.Add(window);
			}
			// =============== Buttons ===============
			listingStandard.End();
			Widgets.EndScrollView();
		}

	}

}
