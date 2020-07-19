using RimWorld;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ToolsFramework
{
    public class ToolProperties : DefModExtension
    {
        public readonly List<ToolTypeModifier> toolTypesValue = new List<ToolTypeModifier>();
        private readonly List<JobModifier> jobBonus = new List<JobModifier>();

        public HashSet<ToolType> toolTypes;
        public HashSet<JobDef> jobList = new HashSet<JobDef>();
        private Dictionary<JobDef, List<StatModifier>> jobStatFactors = new Dictionary<JobDef, List<StatModifier>>();
        private Dictionary<JobDef, ToolType> jobToolType = new Dictionary<JobDef, ToolType>();
        private Dictionary<JobDef, float> jobFactors = new Dictionary<JobDef, float>();
        public void Initialize()
        {
            toolTypes = toolTypesValue.Select(t => t.toolType).ToHashSet();
            jobList = toolTypes.SelectMany(t => t.jobList).ToHashSet();
            foreach (var modifier in toolTypesValue)
            {
                var toolType = modifier.toolType;
                var toolTypeFactor = modifier.value;
                foreach (var job in toolType.jobList)
                {
                    if (jobFactors.ContainsKey(job))
                    {
                        Log.Warning($"[[LC]ToolsFramework] Multiple tool types affect job {job}");
                        continue;
                    }
                    float bonus = jobBonus.FirstOrFallback(t => t.job == job)?.value ?? 1f;
                    var statModifiers = new List<StatModifier>(toolType.workStatFactors);
                    statModifiers.Do(t => t.value *= bonus * toolTypeFactor);
                    jobStatFactors.Add(job, statModifiers);
                    jobFactors.Add(job, toolTypeFactor * bonus);
                }
            }
        }
        public float GetValue_Job(JobDef job, StatDef stat, Tool tool)
        {
            GetValue_Job(job, stat, tool, out float val);
            return val;
        }
        public float GetValue_Job(JobDef job, Tool tool)
        {
            GetValue_Job(job, tool, out float val);
            return val;
        }
        public bool GetValue_Job(JobDef job, StatDef stat, Tool tool, out float val)
        {
            val = 0f;
            if (!jobList.Contains(job))
                return false;
            var statModifiers = jobStatFactors[job];
            if (!statModifiers.StatListContains(stat))
                return false;
            var toolType = jobToolType[job];
            val = statModifiers.GetStatFactorFromList(stat) * toolType.StuffEffect(tool.Stuff) * tool.GetStatValue(Tools_StatDefOf.ToolEffectivenessFactor);
            return true;
        }
        public bool GetValue_Job(JobDef job, Tool tool, out float val)
        {
            val = 0f;
            if (!jobList.Contains(job))
                return false;
            var toolType = jobToolType[job];
            val = jobFactors[job] * toolType.StuffEffect(tool.Stuff) * tool.GetStatValue(Tools_StatDefOf.ToolEffectivenessFactor);
            return true;
        }
        public bool AffectsJob(JobDef jobDef)
            => jobList.Contains(jobDef);
    }
}
