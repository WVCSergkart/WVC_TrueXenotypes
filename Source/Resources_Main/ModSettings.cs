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
		public bool enable_GenesGroups = false;
		public List<XenotypeProp> importedCustomXenotypes = new();
		public List<GeneGroup> geneGroups = new();

		public bool GetGroupAndGene(GeneDef geneDef, out GeneDef newGeneDef)
        {
			newGeneDef = null;
			if (!WVC_TrueXenotypes.settings.enable_GenesGroups)
			{
				return false;
			}
			foreach (GeneGroup group in geneGroups)
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

		public bool InAnyGroup(GeneDef geneDef)
		{
			return geneGroups.Where((group) => group.GeneDefs.Contains(geneDef) || group.MainGeneDef == geneDef).Any();
		}

		public IEnumerable<string> GetEnabledSettings => from specificSetting in GetType().GetFields()
														 where specificSetting.FieldType == typeof(bool) && (bool)specificSetting.GetValue(this)
														 select specificSetting.Name;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref enable_CustomXenotypeImporter, "enable_CustomXenotypeImporter", defaultValue: false);
			Scribe_Values.Look(ref enable_TrueParentGenes, "enable_TrueParentGenes", defaultValue: false);
			Scribe_Values.Look(ref enable_TrueXenotypes, "enable_TrueXenotypes", defaultValue: false);
			Scribe_Values.Look(ref enable_GenesGroups, "enable_GenesGroups", defaultValue: false);
			Scribe_Collections.Look(ref importedCustomXenotypes, "importedCustomXenotypes", LookMode.Deep);
			Scribe_Collections.Look(ref geneGroups, "geneGroups", LookMode.Deep);
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
			if (WVC_TrueXenotypes.settings.enable_GenesGroups)
			{
				TabRecord newTab1 = new("WVC_TrueXenotypes_Tab_GenesSettings".Translate(), delegate
				{
					PageIndex = 1;
					WriteSettings();
				}, PageIndex == 1);
				tabs.Add(newTab1);
				TabRecord newTab2 = new("WVC_TrueXenotypes_Tab_GenesGroupSettings".Translate(), delegate
				{
					PageIndex = 2;
					WriteSettings();
				}, PageIndex == 2);
				tabs.Add(newTab2);
			}
			TabDrawer.DrawTabs(rect, tabs);
			switch (PageIndex)
			{
				case 0:
					GeneralSettings(rect.ContractedBy(15f));
					break;
				case 1:
					GenesSettings(rect.ContractedBy(15f));
					break;
				case 2:
					GroupsSettings(rect.ContractedBy(15f));
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
			listingStandard.CheckboxLabeled("WVC_Label_enable_GenesGroups".Translate(), ref settings.enable_GenesGroups, "WVC_ToolTip_enable_GenesGroups".Translate());
			if (listingStandard.ButtonText("WVC_XaG_ButtonCustomXenotypesImport".Translate()))
			{
				Find.WindowStack.Add(new Dialog_CustomXenotypes_Import());
			}
			if (listingStandard.ButtonText("WVC_XaG_ButtonCustomXenotypesImport_Reset".Translate()))
			{
				Find.WindowStack.Add(new Dialog_CustomXenotypes_Remove());
			}
			// =============== Buttons ===============
			listingStandard.End();
			Widgets.EndScrollView();
		}

		// Genes Settings
		// Genes Settings
		// Genes Settings

		private string searchKey;

		private List<GeneDef> allGeneDefs;

		public void GenesSettings(Rect inRect)
		{
			if (settings.geneGroups == null)
			{
				settings.geneGroups = new();
				return;
			}
			if (allGeneDefs == null)
            {
                UpdLists();
                return;
            }
            var rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			var searchLabel = new Rect(rect.x + 5, rect.y, 60, 24);
			Widgets.Label(searchLabel, "WVC_TX_GeneGroup_Search".Translate());
			var searchRect = new Rect(searchLabel.xMax + 5, searchLabel.y, 200, 24f);
			searchKey = Widgets.TextField(searchRect, searchKey);
			Text.Anchor = TextAnchor.UpperLeft;

			IEnumerable<GeneDef> searchXenotypes;
			if (!searchKey.NullOrEmpty())
			{
				searchXenotypes = allGeneDefs.Where((GeneDef x) => x.label.ToLower().Contains(searchKey.ToLower()));
			}
			else
			{
				IEnumerable<GeneDef> enumerable = allGeneDefs;
				searchXenotypes = enumerable;
			}
			List<GeneDef> list = (from x in searchXenotypes
									  orderby x.label
									  select x).ToList();

			// Log.Error("0");
			var resetRect = new Rect(searchLabel.x, searchLabel.yMax + 5, 265, 24f);
			if (Widgets.ButtonText(resetRect, "WVC_TX_ResetGeneGroup".Translate()))
			{
				settings.geneGroups = new();
			}

			var explanationTitleRect = new Rect(resetRect.xMax + 15, resetRect.y, inRect.width - (resetRect.width + 35), 24f);
			Widgets.Label(explanationTitleRect, "WVC_TX_Title".Translate());

			// Log.Error("1");
			float height = GetScrollHeight(list);
			var outerRect = new Rect(rect.x, searchRect.yMax + 35, rect.width, rect.height - 70);
			var viewArea = new Rect(rect.x, outerRect.y, rect.width - 16, height);
			// Log.Error("2");
			Widgets.BeginScrollView(outerRect, ref scrollPosition, viewArea);
			var outerPos = new Vector2(rect.x + 5, outerRect.y);
			float num = 0;
			int entryHeight = 200;
			// Log.Error("3");
			foreach (GeneDef def in list)
			{
				bool canDrawGroup = num >= scrollPosition.y - entryHeight && num <= (scrollPosition.y + outerRect.height);
				float curNum = outerPos.y;
				if (canDrawGroup)
				{
					// Log.Error("1");
					var infoRect = new Rect(outerPos.x + 5, outerPos.y + 5, 24, 24);
					Widgets.InfoCardButton(infoRect, def);
					var iconRect = new Rect(infoRect.xMax + 5, outerPos.y + 5, 24, 24);
					Widgets.DefIcon(iconRect, def);
					var labelRect = new Rect(iconRect.xMax + 15, outerPos.y + 5, viewArea.width - 85, 24f);
					Widgets.Label(labelRect, def.LabelCap);
					Widgets.DrawHighlightIfMouseover(labelRect);
					TaggedString label = "WVC_TX_AddGeneToGeneGroup".Translate();
					var firstRect = new Rect(labelRect.xMax / 2 - label.GetWidthCached(), labelRect.y, label.GetWidthCached() * 1.2f, 24f);
					if (Widgets.ButtonText(firstRect, label))
					{
						if (settings.geneGroups.Where((group) => group.GeneDefs.Contains(def) || group.MainGeneDef == def).Any())
						{
							Messages.Message("WVC_TX_GeneGroupExist".Translate().CapitalizeFirst(), null, MessageTypeDefOf.RejectInput, historical: false);
						}
						else
                        {
                            GetGroupList(def);
						}
					}
					label = "WVC_TX_CreateGeneGroup".Translate();
                    float width = label.GetWidthCached() * 1.2f;
                    var secondRect = new Rect(firstRect.x + width + 5, firstRect.y, width, 24f);
					if (Widgets.ButtonText(secondRect, "WVC_TX_CreateGeneGroup".Translate()))
                    {
                        if (settings.geneGroups.Where((group) => group.GeneDefs.Contains(def) || group.MainGeneDef == def).Any())
						{
							Messages.Message("WVC_TX_GeneGroupExist".Translate().CapitalizeFirst(), null, MessageTypeDefOf.RejectInput, historical: false);
						}
                        else
                        {
                            CreateGroup(def);
							UpdLists();
							Messages.Message("WVC_TX_GeneGroupCreateSucces".Translate().CapitalizeFirst(), null, MessageTypeDefOf.PositiveEvent, historical: false);
						}
                    }
                }
				var innerPos = new Vector2(outerPos.x + 10, outerPos.y);
				outerPos.y += 24;
				num += outerPos.y - curNum;
			}
			// Log.Error("4");
			Widgets.EndScrollView();

            void GetGroupList(GeneDef def)
            {
                List<FloatMenuOption> floatList = new();
                List<GeneGroup> abilities = settings.geneGroups;
                for (int i = 0; i < abilities.Count; i++)
                {
                    GeneGroup mode = abilities[i];
                    floatList.Add(new FloatMenuOption(mode.GroupName, delegate
                    {
						mode.AddGene(def);
						UpdLists();
						Messages.Message("WVC_TX_GeneGroupAddToGroupSucces".Translate().CapitalizeFirst(), null, MessageTypeDefOf.PositiveEvent, historical: false);
					}, orderInPriority: 0 - i));
                }
                Find.WindowStack.Add(new FloatMenu(floatList));
            }

            static void CreateGroup(GeneDef def)
            {
                GeneGroup newGroup = new();
                newGroup.geneDef = def.defName;
				newGroup.geneDefs = new();
                settings.geneGroups.Add(newGroup);
            }

            void UpdLists()
            {
                allGeneDefs = DefDatabase<GeneDef>.AllDefsListForReading.Where((def) => !settings.InAnyGroup(def)).ToList();
            }
        }

		private float GetScrollHeight(List<GeneDef> defs)
		{
			float num = 0;
			foreach (var def in defs)
			{
				num += 24;
			}
			return num + 5;
		}

		private float GetScrollHeight(List<GeneGroup> defs)
		{
			float num = 0;
			foreach (var def in defs)
			{
				num += 24;
			}
			return num + 5;
		}

		// Groups Settings
		// Groups Settings
		// Groups Settings

		public void GroupsSettings(Rect inRect)
		{
			if (settings.geneGroups == null)
			{
				settings.geneGroups = new();
				return;
			}
			var rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			var searchLabel = new Rect(rect.x + 5, rect.y, 60, 24);
			Widgets.Label(searchLabel, "WVC_TX_GeneGroup_Search".Translate());
			var searchRect = new Rect(searchLabel.xMax + 5, searchLabel.y, 200, 24f);
			searchKey = Widgets.TextField(searchRect, searchKey);
			Text.Anchor = TextAnchor.UpperLeft;
			List<GeneGroup> geneGroups = settings.geneGroups.ToList();
			IEnumerable<GeneGroup> searchXenotypes;
			if (!searchKey.NullOrEmpty())
			{
				searchXenotypes = geneGroups.Where((GeneGroup x) => x.GroupName.ToLower().Contains(searchKey.ToLower()));
			}
			else
			{
                IEnumerable<GeneGroup> enumerable = geneGroups;
				searchXenotypes = enumerable;
			}
			List<GeneGroup> list = (from x in searchXenotypes
								  orderby x.GroupName
								  select x).ToList();

			// Log.Error("0");
			var resetRect = new Rect(searchLabel.x, searchLabel.yMax + 5, 265, 24f);
			if (Widgets.ButtonText(resetRect, "WVC_TX_ResetGeneGroup".Translate()))
			{
				settings.geneGroups = new();
			}

			var explanationTitleRect = new Rect(resetRect.xMax + 15, resetRect.y, inRect.width - (resetRect.width + 35), 24f);
			Widgets.Label(explanationTitleRect, "WVC_TX_Title".Translate());

			// Log.Error("1");
			float height = GetScrollHeight(list);
			var outerRect = new Rect(rect.x, searchRect.yMax + 35, rect.width, rect.height - 70);
			var viewArea = new Rect(rect.x, outerRect.y, rect.width - 16, height);
			// Log.Error("2");
			Widgets.BeginScrollView(outerRect, ref scrollPosition, viewArea);
			var outerPos = new Vector2(rect.x + 5, outerRect.y);
			float num = 0;
			int entryHeight = 200;
			// Log.Error("3");
			foreach (GeneGroup def in list)
			{
				bool canDrawGroup = num >= scrollPosition.y - entryHeight && num <= (scrollPosition.y + outerRect.height);
				float curNum = outerPos.y;
				if (canDrawGroup)
				{
					// Log.Error("1");
					var infoRect = new Rect(outerPos.x + 5, outerPos.y + 5, 24, 24);
					Widgets.InfoCardButton(infoRect, def.MainGeneDef);
					var iconRect = new Rect(infoRect.xMax + 5, outerPos.y + 5, 24, 24);
					Widgets.DefIcon(iconRect, def.MainGeneDef);
					var labelRect = new Rect(iconRect.xMax + 15, outerPos.y + 5, viewArea.width - 85, 24f);
					Widgets.Label(labelRect, def.GroupName);
					Widgets.DrawHighlightIfMouseover(labelRect);
					TaggedString label = "WVC_TX_RemoveGeneFromGroup".Translate();
					var firstRect = new Rect(labelRect.xMax / 2 - label.GetWidthCached(), labelRect.y, label.GetWidthCached() * 1.2f, 24f);
					if (Widgets.ButtonText(firstRect, label))
					{
						GetGroupList(def);
					}
					//label = "WVC_TX_CreateGeneGroup".Translate();
					//var secondRect = new Rect(firstRect.xMax + label.GetWidthCached(), firstRect.y, label.GetWidthCached() * 1.6f, 24f);
					//if (Widgets.ButtonText(secondRect, "WVC_TX_CreateGeneGroup".Translate()))
					//{

					//}
				}
				var innerPos = new Vector2(outerPos.x + 10, outerPos.y);
				outerPos.y += 24;
				num += outerPos.y - curNum;
			}
			// Log.Error("4");
			Widgets.EndScrollView();

			static void GetGroupList(GeneGroup def)
			{
				//if (def.GeneDefs.NullOrEmpty())
				//{
				//	settings.geneGroups.Remove(def);
				//	return;
				//}
				List<FloatMenuOption> floatList = new();
				List<GeneDef> abilities = def.GeneDefs;
				for (int i = 0; i < abilities.Count; i++)
				{
					GeneDef mode = abilities[i];
					floatList.Add(new FloatMenuOption(mode.LabelCap, delegate
					{
						def.RemoveGene(mode);
						Messages.Message("WVC_TX_GeneRemoveSucces".Translate().CapitalizeFirst(), null, MessageTypeDefOf.PositiveEvent, historical: false);
					}, orderInPriority: 0 - i));
				}
				floatList.Add(new FloatMenuOption("WVC_TX_RemoveGroup".Translate(), delegate
				{
					settings.geneGroups.Remove(def);
					Messages.Message("WVC_TX_GeneRemoveSucces".Translate().CapitalizeFirst(), null, MessageTypeDefOf.PositiveEvent, historical: false);
				}, orderInPriority: -999));
				Find.WindowStack.Add(new FloatMenu(floatList));
			}
		}

	}

}
