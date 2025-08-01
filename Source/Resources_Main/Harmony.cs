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

}
