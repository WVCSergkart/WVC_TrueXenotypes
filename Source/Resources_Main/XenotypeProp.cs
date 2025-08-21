using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace WVC_TrueXenotypes
{

	public class XenotypeProp : IExposable
	{

		public string label;

		public string description;

		public bool inheritable;

		public List<string> geneDefs = [];

		public string iconPath;

		public void ExposeData()
		{
			Scribe_Values.Look(ref label, "label");
            Scribe_Values.Look(ref description, "description");
            Scribe_Values.Look(ref inheritable, "inheritable");
			Scribe_Values.Look(ref iconPath, "iconPath");
			Scribe_Collections.Look(ref geneDefs, "geneDefs", LookMode.Value);
			// if (Scribe.mode == LoadSaveMode.PostLoadInit && geneDefs.RemoveAll((GeneDef x) => x == null) > 0)
			// {
				// Log.Error("Removed null genes");
			// }
		}

	}

}
