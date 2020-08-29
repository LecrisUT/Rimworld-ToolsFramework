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
    public class WorkGiver_EndNode : EndNode<(Type type, Type ntype, MethodInfo method), List<(int pos, JobDef job)>>
    {
        public override void Initialize(bool fromSave = false)
        {
            base.Initialize(fromSave);
            foreach (var wgDef in DefDatabase<WorkGiverDef>.AllDefs.Where(t => t.HasModExtension<WorkGiver_Extension>()))
                Utility.toolWorkGivers.Add(wgDef, wgDef.GetModExtension<WorkGiver_Extension>().toolTypes);
        }
        public override bool Perform(Node node)
        {
            if (!base.Perform(node))
                return false;
            var workGivers = BaseInput(node.inputPorts).GetData<(Type type, Type ntype, MethodInfo method)>().Select(t => t.type).ToList();
            var jobDefs = InputA(node.inputPorts).GetData<List<(int pos, JobDef job)>>().Select(t => t.ConvertAll(tt => tt.job)).ToList();
            for (int i = 0; i < workGivers.Count; i++)
            {
                var wg = workGivers[i];
                var toolTypes = jobDefs[i].Where(t => ToolType.jobToolType.ContainsKey(t))?.Select(t => ToolType.jobToolType[t])?.ToList() ?? new List<ToolType>();
                toolTypes.RemoveDuplicates();
                if (!toolTypes.NullOrEmpty())
                    foreach (var wgDef in DefDatabase<WorkGiverDef>.AllDefs.Where(t => t.giverClass.IsAssignableFrom(wg)))
                    {
                        if (wgDef.modExtensions.NullOrEmpty())
                            wgDef.modExtensions = new List<DefModExtension>();
                        if (!wgDef.HasModExtension<WorkGiver_Extension>())
                        {
                            Utility.toolWorkGivers.Add(wgDef, toolTypes);
                            wgDef.modExtensions.Add(new WorkGiver_Extension() { toolTypes = toolTypes });
                        }
                    }
            }
            if (node.DebugLevel > 0)
            {
                node.DebugMessage.AppendLine("TF_BaseMessage".Translate() + " WorkGiver used ToolTypes");
                foreach (var wgDef in DefDatabase<WorkGiverDef>.AllDefs.Where(t => t.HasModExtension<WorkGiver_Extension>()))
                {
                    var ext = wgDef.GetModExtension<WorkGiver_Extension>();
                    node.DebugMessage.Append($"{wgDef} : ");
                    ext.toolTypes.Do(t => node.DebugMessage.Append($"{t}, "));
                    node.DebugMessage.Length -= 2;
                    node.DebugMessage.AppendLine();
                }
            }
            return true;
        }
    }
}
