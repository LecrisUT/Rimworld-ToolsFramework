using AutoPatcher;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace ToolsFramework.AutoPatcher
{
    public class JobDef_EndNode : EndNode<TypeMethod, SavedList<ItemPos<StatDef>>>
    {
        public override bool Perform(Node node)
        {
            if (!base.Perform(node))
                return false;
            var typeMethods = BaseInput(node.inputPorts).GetData<TypeMethod>().ToList();
            var statListIndexed = InputA(node.inputPorts).GetData<List<ItemPos<StatDef>>>().ToList();
            var statJobDef = new Dictionary<StatDef, List<JobDef>>();
            var statList = statListIndexed.SelectMany(t => t.Select(tt => tt.target)).ToList();
            var duplicateToolType = new Dictionary<JobDef, List<ToolType>>();
            statList.RemoveDuplicates();
            statList.Do(t => statJobDef.Add(t, new List<JobDef>()));
            for (int i = 0; i < typeMethods.Count; i++)
            {
                var jobDriver = typeMethods[i].type;
                var jobList = DefDatabase<JobDef>.AllDefs.Where(t => jobDriver.IsAssignableFrom(t.driverClass));
                foreach (var stat in statListIndexed[i].Select(t => t.target))
                    statJobDef[stat].AddRange(jobList);
            }
            statJobDef.Do(t => t.Value.RemoveDuplicates());
            foreach (var toolType in DefDatabase<ToolType>.AllDefs)
            {
                toolType.Initialize();
                if (!toolType.jobList.NullOrEmpty())
                    toolType.jobList.Do(t => Dictionaries.jobToolType.Add(t, toolType));
                else
                {
                    var jobList = new List<JobDef>();
                    foreach (var stat in toolType.workStatFactors.Select(t => t.stat))
                        if (statJobDef.TryGetValue(stat, out var jobs))
                            jobList.AddRange(jobs);
                    jobList.RemoveDuplicates();
                    foreach (var job in jobList.Where(t => !toolType.jobException.Contains(t)))
                    {
                        if (Dictionaries.jobToolType.ContainsKey(job))
                        {
                            if (duplicateToolType.TryGetValue(job, out var list))
                            {
                                list.Add(toolType);
                            }
                            else
                                duplicateToolType.Add(job, new List<ToolType>
                                {
                                    Dictionaries.jobToolType[job],
                                    toolType,
                                });
                        }
                        else
                            Dictionaries.jobToolType.Add(job, toolType);
                    }
                }
            }
            if (!duplicateToolType.EnumerableNullOrEmpty())
            {
                duplicateToolType.Keys.Do(t => Dictionaries.jobToolType.Remove(t));
                if (node.DebugLevel > -1)
                {
                    var warn = new StringBuilder("TF_BaseMessage".Translate() + ": Following jobs are ignored due to duplicate ToolTypes assigned:\n");
                    foreach (var item in duplicateToolType)
                    {
                        warn.Append($"{item.Key} : ");
                        foreach (var toolType in item.Value)
                            warn.Append($"{toolType}, ");
                        warn.Length -= 2;
                        warn.AppendLine();
                    }
                    Log.Warning(warn.ToString());
                }

            }
            if (node.DebugLevel > 0)
            {
                node.DebugMessage.AppendLine("TF_BaseMessage".Translate() + " JobDef <-> ToolType assignment");
                Dictionaries.jobToolType.Do(t => node.DebugMessage.AppendLine($"{t.Key} : {t.Value}"));
                node.DebugMessage.AppendLine();
            }
            return true;
        }
    }
}
