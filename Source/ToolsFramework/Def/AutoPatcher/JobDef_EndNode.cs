using AutoPatcher;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace ToolsFramework.AutoPatcher
{
    public class JobDef_EndNode : EndNode<(Type type, Type ntype, MethodInfo method), List<(int pos, StatDef stat)>>
    {
        public override bool Perform(Node node)
        {
            if (!base.Perform(node))
                return false;
            var typeMethods = BaseInput(node.inputPorts).GetData<(Type type, Type ntype, MethodInfo method)>().ToList();
            var statListIndexed = InputA(node.inputPorts).GetData<List<(int pos, StatDef stat)>>().ToList();
            var statJobDef = new Dictionary<StatDef, List<JobDef>>();
            var statList = statListIndexed.SelectMany(t => t.Select(tt => tt.stat)).ToList();
            statList.RemoveDuplicates();
            statList.Do(t => statJobDef.Add(t, new List<JobDef>()));
            for (int i = 0; i < typeMethods.Count; i++)
            {
                var jobDriver = typeMethods[i].type;
                var jobList = DefDatabase<JobDef>.AllDefs.Where(t => jobDriver.IsAssignableFrom(t.driverClass));
                foreach (var stat in statListIndexed[i].Select(t => t.stat))
                    statJobDef[stat].AddRange(jobList);
            }
            statJobDef.Do(t => t.Value.RemoveDuplicates());
            foreach (var toolType in DefDatabase<ToolType>.AllDefs)
            {
                toolType.Initialize();
                if (!toolType.jobList.NullOrEmpty())
                    toolType.jobList.Do(t => ToolType.jobToolType.Add(t, toolType));
                else
                {
                    var jobList = new List<JobDef>();
                    foreach (var stat in toolType.workStatFactors.Select(t => t.stat))
                        if (statJobDef.TryGetValue(stat, out var jobs))
                            jobList.AddRange(jobs);
                    jobList.RemoveDuplicates();
                    foreach (var job in jobList.Where(t => !toolType.jobException.Contains(t)))
                        ToolType.jobToolType.Add(job, toolType);
                }
            }
            if (node.DebugLevel > 0)
            {
                node.DebugMessage.AppendLine("TF_BaseMessage".Translate() + " JobDef <-> ToolType assignment");
                ToolType.jobToolType.Do(t => node.DebugMessage.AppendLine($"{t.Key} : {t.Value}"));
                node.DebugMessage.AppendLine();
            }
            return true;
        }
    }
}
