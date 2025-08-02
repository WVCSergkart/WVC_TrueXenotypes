using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WVC_TrueXenotypes
{

    [StaticConstructorOnStartup]
    public static class XaG_PostInitialization
    {

        static XaG_PostInitialization()
        {
            try
            {
               FilterList();
            }
            catch (Exception arg)
            {
                Log.Error("Initial error. Reason: " + arg);
            }
        }

        private static void FilterList()
        {
            if (!Utility.GenesDictionaryEnabled)
            {
                return;
            }
            foreach (GeneDef geneDef in DefDatabase<GeneDef>.AllDefsListForReading)
            {
                if (Utility.GetGroupAndGene(geneDef, out _))
                {
                    StaticCollectionsClass.hidedGeneDefs.Add(geneDef);
                }
            }
        }
    }

}
