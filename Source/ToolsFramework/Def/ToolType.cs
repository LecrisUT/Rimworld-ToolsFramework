using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ToolsFramework
{
    public class ToolType : Def
    {
        public List<StatModifier> efficiencyModifiers = new List<StatModifier>();
        public List<StatModifier> workStatFactors = new List<StatModifier>();

        public List<JobDef> jobList = new List<JobDef>();
        public List<JobDef> jobException = new List<JobDef>();
        public List<Type> jobDriverList = new List<Type>();
        public List<Type> jobDriverException = new List<Type>();

        public List<string> defaultToolAssignmentTags;

        public void Initialize()
        {
            if (!jobList.NullOrEmpty())
                return;
            if (!jobDriverList.NullOrEmpty())
            {
                jobList = DefDatabase<JobDef>.AllDefs.Where(t => jobDriverList.Any(tt => tt.IsAssignableFrom(t.driverClass))).ToList();
                return;
            }
            if (!jobDriverException.NullOrEmpty())
                jobException = DefDatabase<JobDef>.AllDefs.Where(t => jobDriverException.Any(tt => tt.IsAssignableFrom(t.driverClass))).ToList();
        }
        public float StuffEffect(ThingDef stuff)
        {
            if (stuff == null)
                return 1f;
            float factor = 0f;
            foreach (StatModifier modifier in efficiencyModifiers)
                factor += modifier.value * stuff.stuffProps.statFactors.GetStatFactorFromList(modifier.stat);
            return factor;
        }
    }
}
