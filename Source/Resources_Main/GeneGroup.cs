using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WVC_TrueXenotypes
{

    public class GeneGroup : IExposable
    {

        public string geneDef;

        public List<string> geneDefs;

        private string cachedGroupKey = null;
        public string GroupName
        {
            get
            {
                if (cachedGroupKey == null)
                {
                    if (MainGeneDef != null)
                    {
                        cachedGroupKey = MainGeneDef.LabelCap;
                    }
                    else
                    {
                        cachedGroupKey = "Unknown";
                    }
                }
                return cachedGroupKey;
            }
        }

        private GeneDef cachedMainDef;
        public GeneDef MainGeneDef
        {
            get
            {
                cachedMainDef ??= geneDef.ConvertToDef();
                return cachedMainDef;
            }
        }

        private List<GeneDef> cachedGeneDefs;
        public List<GeneDef> GeneDefs
        {
            get
            {
                cachedGeneDefs ??= geneDefs.ConvertToDefs();
                return cachedGeneDefs;
            }
        }

        public string GetGeneDefName(string oldGeneDef)
        {
            if (geneDefs.Contains(oldGeneDef))
            {
                return geneDef;
            }
            return oldGeneDef;
        }

        public void AddGene(GeneDef geneDef)
        {
            if (geneDefs.NullOrEmpty())
            {
                geneDefs = [];
            }
            if (!geneDefs.Contains(geneDef.defName))
            {
                geneDefs.Add(geneDef.defName);
            }
            cachedGeneDefs = null;
        }

        public void RemoveGene(GeneDef geneDef)
        {
            if (geneDefs.NullOrEmpty())
            {
                geneDefs = [];
            }
            if (geneDefs.Contains(geneDef.defName))
            {
                geneDefs.Remove(geneDef.defName);
            }
            cachedGeneDefs = null;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref geneDef, "geneDef");
            Scribe_Collections.Look(ref geneDefs, "geneDefs", LookMode.Value);
        }
    }

}
