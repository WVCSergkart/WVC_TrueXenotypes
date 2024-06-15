using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace WVC_TrueXenotypes
{

	public class WVC_TrueXenotypes_Main : Mod
	{
		public WVC_TrueXenotypes_Main(ModContentPack content)
			: base(content)
		{
			new Harmony("wvc.sergkart.biotech.truexenotypes").PatchAll();
		}
	}

	[HarmonyPatch(typeof(Gene), "OverrideBy")]
	public static class Patch_Gene_OverrideBy
	{
		public static void Postfix(Gene __instance)
		{
			CompXenotype.ResetPawnXenotype(__instance.pawn);
		}
	}

}
