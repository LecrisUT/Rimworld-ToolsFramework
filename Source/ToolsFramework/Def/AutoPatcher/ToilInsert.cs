using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace ToolsFramework.AutoPatcher
{
    public static class ToilInsert
    {
        private static Dictionary<Type, int> toilIndex = new Dictionary<Type, int>();
        public static bool Prepare(Type type, Type ntype, MethodInfo method, List<(int pos, int ToilIndex, List<MethodInfo> actions)> positions)
        {
            if (positions.NullOrEmpty())
                return false;
            toilIndex.Add(type, positions.First().ToilIndex);
            var oClasses = type.AllSubclasses().Where(t => t.GetMethod("MakeNewToils") != null).ToList();
            foreach (var dType in type.AllSubclasses().Where(t => !oClasses.Any(tt => t.IsSubclassOf(tt))))
                toilIndex.Add(dType, positions.First().ToilIndex);
            return true;
        }
        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> values, JobDriver __instance, Pawn ___pawn, Job ___job)
        {
            int ind = -1;
            // var driver = ___job.def.driverClass;
            foreach(Toil toil in values)
            {
                ind++;
                if (ind == toilIndex[__instance.GetType()] && ReadyTool(___pawn, ___job, TargetIndex.A, out Toil ReadyToolToil))
                    yield return ReadyToolToil;
                yield return toil;
            }
        }

        public static bool ReadyTool(Pawn pawn, Job job, TargetIndex targetInd, out Toil toil)
        {
            toil = null;
            bool newToil = false;
            if (!pawn.CanUseTools(out var tracker) || !ToolType.jobToolType.TryGetValue(job.def, out var toolType) || !tracker.usedHandler.BestTool.TryGetValue(toolType, out var tool))
                return newToil;
            if (Settings.equipDelay)
            {
                toil = Toils_General.WaitWith(targetInd, Mathf.CeilToInt(tool.GetStatValueForPawn(StatDefOf.ToolReadinessDelay, pawn)), true, true);
                newToil = true;
            }
            return newToil;
        }
    }
}
