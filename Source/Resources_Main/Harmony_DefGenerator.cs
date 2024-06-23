using System;
using System.Collections.Generic;
// using System.Collections.IEnumerable;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;


namespace WVC_TrueXenotypes
{

	[HarmonyPatch(typeof(DefGenerator), "GenerateImpliedDefs_PreResolve")]
	public static class WVC_DefGenerator_GenerateImpliedDefs_PreResolve_Patch
	{

		[HarmonyPostfix]
		public static void Postfix()
		{
			GeneratorUtility.CustomXenotypesImport();
		}

	}

}
