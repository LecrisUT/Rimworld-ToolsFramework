using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ToolsFramework
{
    public class ToolType : Def
    {
        public static Dictionary<JobDef, ToolType> jobToolType = new Dictionary<JobDef, ToolType>();

        public List<StatModifier> efficiencyModifiers = new List<StatModifier>();
        public List<StatModifier> workStatFactors = new List<StatModifier>();

        public List<JobDef> jobList = new List<JobDef>();
        public List<JobDef> jobException = new List<JobDef>();
        private List<Type> jobDriverList;
        private List<Type> jobDriverException;

        public bool emergencyTool = false;

        public List<ThingCategoryDef> defaultToolAssignmentTags = new List<ThingCategoryDef>();

        public void Initialize()
        {
            if (!jobList.NullOrEmpty())
                return;
            if (!jobDriverException.NullOrEmpty())
                jobException = DefDatabase<JobDef>.AllDefs.Where(t => jobDriverException.Any(tt => tt.IsAssignableFrom(t.driverClass))).ToList();
            if (!jobDriverList.NullOrEmpty())
            {
                jobList = DefDatabase<JobDef>.AllDefs.Where(t => (jobException.NullOrEmpty() || !jobException.Contains(t)) && jobDriverList.Any(tt => tt.IsAssignableFrom(t.driverClass))).ToList();
                return;
            }
        }
    }
}
