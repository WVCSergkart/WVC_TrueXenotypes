using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Verse;
using static Verse.GeneSymbolPack;

namespace WVC_TrueXenotypes
{

	public static class HarmonyUtility
    {

        public static void HarmonyPatches(WVC_TrueXenotypesSettings settings)
        {
            //Log.Error("0");
            var harmony = new HarmonyLib.Harmony("wvc.sergkart.biotech.truexenotypes");
            //Log.Error("1");
            if (settings.enable_GenesGroups)
            {
                //Log.Error("1.0");
                harmony.Patch(AccessTools.Method(typeof(BackCompatibility), "BackCompatibleDefName"), postfix: new HarmonyMethod(typeof(HarmonyUtility).GetMethod(nameof(Patch_BackCompatibility_BackCompatibleDefName))));
                //Log.Error("1.1");
                harmony.Patch(AccessTools.Method(typeof(Dialog_CreateXenotype), "DrawGene"), prefix: new HarmonyMethod(typeof(HarmonyUtility).GetMethod(nameof(Patch_Dialog_CreateXenotype_DrawGene))));
                //Log.Error("1.2");
                harmony.Patch(AccessTools.Method(typeof(DirectXmlCrossRefLoader), "RegisterObjectWantsCrossRef", [typeof(object), typeof(FieldInfo), typeof(string), typeof(string), typeof(string), typeof(Type)]), prefix: new HarmonyMethod(typeof(HarmonyUtility).GetMethod(nameof(Patch_DirectXmlCrossRefLoader_RegisterObjectWantsCrossRef_PatchOne))));
                //Log.Error("1.3");
                harmony.Patch(AccessTools.Method(typeof(DirectXmlCrossRefLoader), "RegisterObjectWantsCrossRef", [typeof(object), typeof(string), typeof(XmlNode), typeof(string), typeof(string), typeof(Type)]), prefix: new HarmonyMethod(typeof(HarmonyUtility).GetMethod(nameof(Patch_DirectXmlCrossRefLoader_RegisterObjectWantsCrossRef_PatchThree))));
                //Log.Error("1.4");
                harmony.Patch(AccessTools.Method(typeof(DirectXmlCrossRefLoader), "RegisterObjectWantsCrossRef", [typeof(object), typeof(string), typeof(string), typeof(string), typeof(string), typeof(Type)]), prefix: new HarmonyMethod(typeof(HarmonyUtility).GetMethod(nameof(Patch_DirectXmlCrossRefLoader_RegisterObjectWantsCrossRef_PatchTwo))));
                //Log.Error("1.5");
                harmony.Patch(AccessTools.Method(typeof(GenDefDatabase), "GetDef"), postfix: new HarmonyMethod(typeof(HarmonyUtility).GetMethod(nameof(Patch_GenDefDatabase_GetDef))));
                //Log.Error("1.6");
                harmony.Patch(AccessTools.Method(typeof(GenDefDatabase), "GetDefSilentFail"), postfix: new HarmonyMethod(typeof(HarmonyUtility).GetMethod(nameof(Patch_GenDefDatabase_GetDefSilentFail))));
                //Log.Error("1.7");
                harmony.Patch(AccessTools.Method(typeof(GeneMaker), "MakeGene"), prefix: new HarmonyMethod(typeof(HarmonyUtility).GetMethod(nameof(Patch_GeneMaker_MakeGene))));
                //Log.Error("1.8");
                harmony.Patch(AccessTools.Method(typeof(XenotypeDef), "ResolveReferences"), prefix: new HarmonyMethod(typeof(HarmonyUtility).GetMethod(nameof(Patch_XenotypeDef_ResolveReferences))));
                //Log.Error("1.9");
            }
            //Log.Error("2");
            if (settings.enable_TrueXenotypes)
            {
                harmony.Patch(AccessTools.Method(typeof(Gene), "OverrideBy"), postfix: new HarmonyMethod(typeof(HarmonyUtility).GetMethod(nameof(Patch_Gene_OverrideBy))));
                harmony.Patch(AccessTools.PropertyGetter(typeof(XenotypeDef), "Archite"), prefix: new HarmonyMethod(typeof(HarmonyUtility).GetMethod(nameof(Patch_XenotypeDef_Archite))));
            }
            //Log.Error("3");
            if (settings.enable_CustomXenotypeImporter)
            {
                harmony.Patch(AccessTools.Method(typeof(DefGenerator), "GenerateImpliedDefs_PreResolve"), postfix: new HarmonyMethod(typeof(HarmonyUtility).GetMethod(nameof(Patch_DefGenerator_GenerateImpliedDefs_PreResolve))));
            }
            //Log.Error("4");
        }
        public static void Patch_BackCompatibility_BackCompatibleDefName(Type defType, string defName, ref string __result)
        {
            if (typeof(GeneDef).IsAssignableFrom(defType))
            {
                __result = Utility.GetGeneDefName(defName);
            }
        }
        public static void Patch_DefGenerator_GenerateImpliedDefs_PreResolve()
        {
            Utility.CustomXenotypesImport();
        }

        public static bool Patch_Dialog_CreateXenotype_DrawGene(GeneDef geneDef, ref bool __result)
        {
            if (Utility.GetGroupAndGene(geneDef, out _))
            {
                __result = false;
                return false;
            }
            return true;
        }
        public static void Patch_DirectXmlCrossRefLoader_RegisterObjectWantsCrossRef_PatchOne(FieldInfo fi, ref string targetDefName, Type assumeFieldType = null)
        {
            Type c = assumeFieldType ?? fi.FieldType;
            if (typeof(GeneDef).IsAssignableFrom(c))
            {
                targetDefName = Utility.GetGeneDefName(targetDefName);
            }
        }

        public static void Patch_DirectXmlCrossRefLoader_RegisterObjectWantsCrossRef_PatchThree(object wanter, ref string fieldName, Type overrideFieldType = null)
        {
            Type c = overrideFieldType ?? wanter.GetType().GetField(fieldName, AccessTools.all).FieldType;
            if (typeof(GeneDef).IsAssignableFrom(c))
            {
                fieldName = Utility.GetGeneDefName(fieldName);
            }
        }
        public static void Patch_DirectXmlCrossRefLoader_RegisterObjectWantsCrossRef_PatchTwo(object wanter, string fieldName, ref string targetDefName, Type overrideFieldType = null)
        {
            Type c = overrideFieldType ?? wanter.GetType().GetField(fieldName, AccessTools.all).FieldType;
            if (typeof(GeneDef).IsAssignableFrom(c))
            {
                targetDefName = Utility.GetGeneDefName(targetDefName);
            }
        }
        public static void Patch_GenDefDatabase_GetDef(ref Def __result)
        {
            if (__result is GeneDef oldGeneDef && Utility.GetGroupAndGene(oldGeneDef, out GeneDef newGeneDef))
            {
                __result = newGeneDef;
            }
        }
        public static void Patch_GenDefDatabase_GetDefSilentFail(ref Def __result)
        {
            if (__result is GeneDef oldGeneDef && Utility.GetGroupAndGene(oldGeneDef, out GeneDef newGeneDef))
            {
                __result = newGeneDef;
            }
        }
        public static void Patch_Gene_OverrideBy(Gene __instance)
        {
            CompXenotype.ResetPawnXenotype(__instance.pawn);
        }

        public static void Patch_GeneMaker_MakeGene(ref GeneDef def)
        {
            if (def != null && Utility.GetGroupAndGene(def, out GeneDef newGeneDef))
            {
                def = newGeneDef;
            }
        }

        public static bool Patch_XenotypeDef_Archite(ref bool __result, XenotypeDef __instance)
        {
            __result = __instance.genes.Any((geneDef) => geneDef.biostatArc > 0);
            return false;
        }

        public static void Patch_XenotypeDef_ResolveReferences(XenotypeDef __instance)
        {
            foreach (GeneDef geneDef in __instance.genes.ToList())
            {
                //if (Utility.GetGroupAndGene(geneDef, out GeneDef newGeneDef) && newGeneDef != geneDef)
                //{
                //    __instance.genes.Remove(geneDef);
                //    if (!__instance.genes.Contains(newGeneDef))
                //    {
                //        __instance.genes.Add(newGeneDef);
                //    }
                //}
                if (__instance.genes.Where((def) => def == geneDef).ToList().Count > 1)
                {
                    __instance.genes.Remove(geneDef);
                }
            }
        }

    }
}
