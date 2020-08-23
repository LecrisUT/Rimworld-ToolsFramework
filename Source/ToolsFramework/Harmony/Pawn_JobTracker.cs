using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine.SocialPlatforms;
using Verse;
using Verse.AI;

namespace ToolsFramework.Harmony
{
    // Alternative implementation used with copy-pasted WorkGiver

    [HarmonyPatch(typeof(Pawn_JobTracker))]
    [HarmonyPatch(nameof(Pawn_JobTracker.TryOpportunisticJob))]
    public static class Patch_Pawn_JobTracker_TryOpportunisticJob
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            FieldInfo allowOpportunisticPrefix_Field = AccessTools.Field(typeof(JobDef), "allowOpportunisticPrefix");
            List<CodeInstruction> instuctionList = instructions.ToList();
            for (int i = 0; i < instuctionList.Count; i++)
            {
                CodeInstruction instruction = instuctionList[i];
                if (instruction.LoadsField(allowOpportunisticPrefix_Field))
                {
                    while (!instuctionList[i].IsLdarg(1))
                    {
                        i--;
                    }
                    // i -= 2;
                    Label brLabel = generator.DefineLabel();
                    Label oldLabel = instuctionList[i].labels.First();
                    LocalBuilder localVar = generator.DeclareLocal(typeof(Job));
                    instuctionList[i].labels[0] = brLabel;
                    List<CodeInstruction> insertedInstructions = new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Ldarg_0, null) { labels=new List<Label>(){ oldLabel} },
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_JobTracker), "pawn")),
                        new CodeInstruction(OpCodes.Ldarg_1, null),
                        new CodeInstruction(OpCodes.Ldloca_S, localVar),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_Pawn_JobTracker_TryOpportunisticJob), "CheckOportunisticToolJob")),
                        new CodeInstruction(OpCodes.Brfalse_S, brLabel),
                        new CodeInstruction(OpCodes.Ldloc_S, localVar),
                        new CodeInstruction(OpCodes.Ret, null)
                    };
                    instuctionList.InsertRange(i, insertedInstructions);
                    break;
                }
            }
            return instuctionList;
        }
        public static bool CheckOportunisticToolJob(Pawn pawn, Job currJob, out Job toolJob)
        {
            toolJob = null;
            if (!pawn.CanUseTools(out var tracker) || pawn.InMentalState || pawn.IsBurning())
                return false;
            var jobDef = currJob.def;
            var toolType = ToolType.jobToolType.TryGetValue(jobDef);
            var toolList = tracker.memoryTool.ToList();
            if (toolType != null)
                toolList.RemoveAll(t => t.ToolTypes.Contains(toolType));
            // Return tool
            if (!toolList.NullOrEmpty() && jobDef.allowOpportunisticPrefix && !jobDef.IsReturningTool())
            {
                bool foundPos = false;
                IntVec3 bestPos = pawn.PositionHeld;
                Tool tool = null;
                IntVec3 pawnPos = pawn.PositionHeld;
                foreach (var currTool in toolList)
                    if (HaulAIUtility.CanHaulAside(pawn, currTool, out var pos))
                    {
                        if (!foundPos)
                        {
                            bestPos = pos;
                            tool = currTool;
                            foundPos = true;
                            continue;
                        }
                        if (pos.DistanceTo(pawnPos) < bestPos.DistanceTo(pawnPos))
                        {
                            bestPos = pos;
                            tool = currTool;
                        }
                    }
                if (foundPos)
                {
                    toolJob = pawn.PutAwayTool(tool, false);
                    TransformJob(ref toolJob);
                    return true;
                }
                return false;
            }
            // Take tool
            else if (toolType != null)
            {
                if (!toolType.emergencyTool && !jobDef.allowOpportunisticPrefix)
                    return false;
                if (tracker.usedHandler.BestTool[toolType] != null)
                    return false;
                var mapTracker = pawn.MapHeld.GetComponent<Map_ToolTracker>();
                var tool = mapTracker.ClosestTool(toolType, pawn.PositionHeld, pawn);
                if (tool != null)
                {
                    toolJob = pawn.TakeTool(tool);
                    TransformJob(ref toolJob);
                    return true;
                }
            }
            return false;
        }
        private static void TransformJob(ref Job job)
        {
            if (job == null)
                return;
            if (job.def == RimWorld.JobDefOf.HaulToCell)
            {
                job.def = JobDefOf.ReturnToolToCell;
                return;
            }
            if (job.def == RimWorld.JobDefOf.HaulToContainer)
            {
                job.def = JobDefOf.ReturnToolToContainer;
                return;
            }
            if (job.def == JobDefOf.PickTool)
            {
                job.def = JobDefOf.TakeTempTool;
                return;
            }
        }
        public static bool IsReturningTool(this JobDef jobDef)
        {
            var ext = jobDef?.GetModExtension<Job_Extension>();
            if (ext == null)
                return false;
            return ext.isOpportunistic && ext.isPutAwayToolJob;
        }
        public static bool IsTakingTempTool(this JobDef jobDef)
        {
            var ext = jobDef?.GetModExtension<Job_Extension>();
            if (ext == null)
                return false;
            return ext.isOpportunistic && ext.isTakeToolJob;
        }
    }
}