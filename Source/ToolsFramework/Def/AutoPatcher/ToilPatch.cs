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

namespace ToolsFramework.AutoPatcher
{
    public static class ToilPatch
    {
        private static readonly Type thisType = typeof(ToilPatch);
        private static MethodInfo ChangeToil_Method = AccessTools.Method(thisType, "ChangeToil");
        private static MethodInfo TryAddToil_Method = AccessTools.Method(thisType, "TryAddToil");
        private static FieldInfo Current;
        private static FieldInfo switchField;
        private static LocalVariableInfo JobDriver_local;
        private static FieldInfo JobDriver_field;
        private static List<(Label label, int ToilStart, int ToilEnd)> ToilInfo;
        private static List<CodeInstruction> InsertedInstructions
        {
            get
            {
                var instructions = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Ldarg_0, null),
                    new CodeInstruction(OpCodes.Ldflda, Current),
                };
                if (JobDriver_local != null)
                    instructions.AddRange(new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldloc_S, JobDriver_local.LocalIndex),
                    });
                else
                    instructions.AddRange(new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldarg_0, null),
                        new CodeInstruction(OpCodes.Ldfld, JobDriver_field),
                    });
                instructions.Add(new CodeInstruction(OpCodes.Call, ChangeToil_Method));
                return instructions;
            }
        }
        private static List<CodeInstruction> InsertToilInstructions(int pos, Label thisLabel, Label nextLabel)
        {
            var instructions = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0, null) { labels = new List<Label>{ thisLabel } },
                new CodeInstruction(OpCodes.Ldc_I4, pos + 1),
                new CodeInstruction(OpCodes.Stfld, switchField),
            };
            if (JobDriver_local != null)
                instructions.AddRange(new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldloc_S, JobDriver_local.LocalIndex),
                });
            else
                instructions.AddRange(new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldarg_0, null),
                    new CodeInstruction(OpCodes.Ldfld, JobDriver_field),
                });
            instructions.AddRange(new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0, null),
                new CodeInstruction(OpCodes.Ldflda, Current),
                new CodeInstruction(OpCodes.Call, TryAddToil_Method),
                new CodeInstruction(OpCodes.Brfalse, nextLabel),
                new CodeInstruction(OpCodes.Ldc_I4_1,null),
                new CodeInstruction(OpCodes.Ret, null),
            });
            return instructions;
        }
        public static bool Prepare(Type type, Type ntype, MethodInfo method, (FieldInfo Current, FieldInfo switchField, LocalVar switchLocal) enumInfo, List<MethodInfo> actions, List<(Label label, int ToilStart, int ToilEnd)> toilInfo)
        {
            if (!SearchJobDriver(method, type, ntype, out JobDriver_local, out JobDriver_field))
                return false;
            Current = enumInfo.Current;
            switchField = enumInfo.switchField;
            ToilInfo = toilInfo;
            firstToil = true;
            return true;
        }
        private static bool firstToil = false;
        private static bool SearchJobDriver(MethodInfo searchMethod, Type JobDriver, Type ntype, out LocalVariableInfo JobDriver_local, out FieldInfo JobDriver_field)
        {
            JobDriver_field = null;
            JobDriver_local = searchMethod.GetMethodBody().LocalVariables.FirstOrFallback(t => t.LocalType == JobDriver);
            if (JobDriver_local == null)
            {
                JobDriver_field = ntype.GetFields(AccessTools.all).First(t => t.FieldType == JobDriver);
                return JobDriver_field != null;
            }
            return true;
        }
        public static bool Transpile(ref List<CodeInstruction> instuctionList, ILGenerator generator, int pos, List<MethodInfo> actions,
            out List<(int pos, int offset)> CIOffsets, out List<(Label label, int pos, int length)> newItems)
        {
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
                var offset = insertToil.Count;
                CIOffsets.Add((prevToilPos, offset));
                newItems.Add((label, prevToilPos, insertToil.Count));
                pos += offset;
            }
            // patch Toil
            instuctionList.InsertRange(pos + 1, InsertedInstructions);
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
            if (!pawn.CanUseTools(out var tracker) || !Dictionaries.jobToolType.TryGetValue(job.def, out var toolType))
                return false;
            var tool = tracker.UsedHandler.BestTool[toolType];
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
#if DEBUG
            Log.Message($"Test 3.0: {Dictionaries.jobToolType.TryGetValue(driver.job.def, out var toolType2)} : {toolType2}");
#endif
            if (!pawn.CanUseTools(out var tracker) || !Dictionaries.jobToolType.TryGetValue(driver.job.def, out var toolType))
                return;

            var tool = tracker.UsedHandler.BestTool[toolType];
#if DEBUG
            var test = new System.Text.StringBuilder($"Test 3.1: {tracker.UsedHandler.BestTool[toolType]}\n");
            foreach (var a in tracker.UsedHandler.BestTool)
                test.AppendLine($"{a.Key} : {a.Value}");
            Log.Message(test.ToString());
#endif
            if (tool != null)
                HasTool(ref toil, pawn, tool, tracker);
            else
                NoTool(ref toil, pawn, tracker);
        }
        public static void HasTool(ref Toil toil, Pawn pawn, Tool tool, Pawn_ToolTracker tracker)
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
            toil.AddFailCondition(delegate
            {
                return tool.DestroyedOrNull();
            });
        }
        public static List<JobDef> dropJobs = new List<JobDef>()
        {
            RimWorld.JobDefOf.DropEquipment,
        };
        public static void NoTool(ref Toil toil, Pawn pawn, Pawn_ToolTracker tracker)
        {
        }
    }
}
