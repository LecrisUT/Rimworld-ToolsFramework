using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Verse;
using HarmonyLib;
using Verse.AI;
using System.Reflection.Emit;
using UnityEngine;
using RimWorld;
using AutoPatcher;
using Mono.Posix;

namespace ToolsFramework.AutoPatcher
{
    public static class ToilPatch
    {
        private static readonly Type thisType = typeof(ToilPatch);
        private static MethodInfo ChangeToil_Method = AccessTools.Method(thisType, "ChangeToil");
        private static MethodInfo TryAddToil_Method = AccessTools.Method(thisType, "TryAddToil");
        private static FieldInfo Current;
        private static LocalBuilder switchLocal;
        private static FieldInfo switchField;
        private static LocalVariableInfo JobDriver;
        private static List<(Label label, int ToilStart, int ToilEnd)> ToilInfo;
        private static List<CodeInstruction> InsertedInstructions => new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Ldarg_0, null),
            new CodeInstruction(OpCodes.Ldflda, Current),
            new CodeInstruction(OpCodes.Ldloc_S, JobDriver.LocalIndex),
            new CodeInstruction(OpCodes.Call, ChangeToil_Method),
        };
        private static List<CodeInstruction> InsertToilInstructions(int pos, Label thisLabel, Label nextLabel)
        {
            var instructions = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0, null) { labels = new List<Label>{ thisLabel } },
                new CodeInstruction(OpCodes.Ldc_I4, pos + 1),
                new CodeInstruction(OpCodes.Stfld, switchField),
                new CodeInstruction(OpCodes.Ldloc_S, JobDriver.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_0, null),
                new CodeInstruction(OpCodes.Ldflda, Current),
                new CodeInstruction(OpCodes.Call, TryAddToil_Method),
                new CodeInstruction(OpCodes.Brfalse, nextLabel),
                new CodeInstruction(OpCodes.Ldc_I4_1,null),
                new CodeInstruction(OpCodes.Ret, null),
            };
            return instructions;
        }
        public static bool Prepare(Type type, Type ntype, MethodInfo method, (FieldInfo Current, FieldInfo switchField, LocalVar switchLocal) enumInfo, List<MethodInfo> actions, List<(Label label, int ToilStart, int ToilEnd)> toilInfo)
        {
            if (!SearchJobDriver(method, type, out JobDriver))
                return false;
            Current = enumInfo.Current;
            switchLocal = enumInfo.switchLocal;
            switchField = enumInfo.switchField;
            ToilInfo = toilInfo;
            firstToil = true;
            return true;
        }
        private static bool firstToil = false;
        private static bool SearchJobDriver(MethodInfo searchMethod, Type JobDriver, out LocalVariableInfo JobDriver_local)
        {
            JobDriver_local = null;
            var local = searchMethod.GetMethodBody().LocalVariables.FirstOrFallback(t => t.LocalType == JobDriver);
            if (local == null)
                return false;
            JobDriver_local = local;
            return true;
        }
        public static bool Transpile(ref List<CodeInstruction> instuctionList, ILGenerator generator, int pos, List<MethodInfo> actions,
            out List<(int pos, int offset)> CIOffsets, out List<(Label label, int pos, int length)> newItems)
        {
            int offset = 0;
            CIOffsets = new List<(int pos, int offset)>();
            newItems = new List<(Label, int, int)>();
            // insert Toil
            int toilpos = 0;
            int prevToilPos = pos;
            if (firstToil)
            {
                var label = generator.DefineLabel();
                var nextLabel = generator.DefineLabel();
                for (int i = 0; i < ToilInfo.Count; i++)
                {
                    var item = ToilInfo[i];
                    if (pos >= item.ToilStart && pos <= item.ToilEnd)
                    {
                        toilpos = i;
                        prevToilPos = item.ToilStart;
                        nextLabel = item.label;
                        break;
                    }
                }
                var insertToil = InsertToilInstructions(toilpos, label, nextLabel);
                instuctionList.InsertRange(prevToilPos, insertToil);
                offset = insertToil.Count;
                CIOffsets.Add((prevToilPos, offset));
                newItems.Add((label, prevToilPos, insertToil.Count));
            }
            // patch Toil
            instuctionList.InsertRange(pos + 1 + offset, InsertedInstructions);
            CIOffsets.Add((pos + 1, InsertedInstructions.Count));
            firstToil = false;
            return true;
        }
        private static bool TryAddToil(JobDriver driver, out Toil toil)
        {
            // toil = Toils_General.WaitWith(TargetIndex.A, 1);
            toil = new Toil();
            var pawn = driver.pawn;
            var job = driver.job;
            bool newToil = false;
            if (!pawn.CanUseTools(out var tracker) || !ToolType.jobToolType.TryGetValue(job.def, out var toolType))
                return false;
            var tool = tracker.usedHandler.BestTool[toolType];
            if (tool == null)
                return false;
            if (Settings.equipDelay)
            {
                toil = Toils_General.WaitWith(TargetIndex.A, Mathf.CeilToInt(tool.GetStatValueForPawn(StatDefOf.ToolReadinessDelay, pawn)), true, true);
                newToil = true;
            }
            return newToil;
        }

        public static void ChangeToil(ref Toil toil, JobDriver driver)
        {
            Pawn pawn = toil.actor ?? driver.pawn;
            if (pawn == null)
            {
                Log.Error("TF_BaseMessage".Translate() + "TF_Error_ChangeToil".Translate(toil.Named(null), driver.job.def, driver.Named(null)));
                return;
            }
            if (!pawn.CanUseTools(out var tracker) || !ToolType.jobToolType.TryGetValue(driver.job.def, out var toolType))
                return;

            var tool = tracker.usedHandler.BestTool[toolType];
            if (tool != null)
                HasTool(ref toil, pawn, tool, tracker);
            else
                NoTool(ref toil, pawn, tracker);
        }
        private static void HasTool(ref Toil toil, Pawn pawn, Tool tool, Pawn_ToolTracker tracker)
        {
            bool equipTool = Settings.equipTool;
            toil.AddPreInitAction(delegate
            {
                tracker.toolInUse = tool;
                if (equipTool)
                    pawn.EquipTool(tool);
                tool.ToolUse.InUse = true;
            });
            toil.AddPreTickAction(delegate
            {
                tool.ToolUse.Use();
            });
            toil.AddFinishAction(delegate
            {
                if (equipTool && (!(pawn.jobs.jobQueue.FirstOrDefault() is QueuedJob nextJob) || !dropJobs.Contains(nextJob.job.def)))
                    pawn.DeequipTool();
                tool.ToolUse.InUse = false;
                tracker.toolInUse = null;
                tracker.memoryEquipment = null;
                tracker.memoryEquipmentOffHand = null;
            });
        }
        public static List<JobDef> dropJobs = new List<JobDef>()
        {
            RimWorld.JobDefOf.DropEquipment,
        };
        private static void NoTool(ref Toil toil, Pawn pawn, Pawn_ToolTracker tracker)
        {
        }
    }
}
