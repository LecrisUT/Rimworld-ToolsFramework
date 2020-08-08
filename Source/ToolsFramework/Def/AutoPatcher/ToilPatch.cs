using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Verse;
using HarmonyLib;
using Verse.AI;
using System.Reflection.Emit;

namespace ToolsFramework.AutoPatcher
{
    public static class ToilPatch
    {
        private static readonly Type thisType = typeof(ToilPatch);
        private static MethodInfo ChangeToil_Method = AccessTools.Method(thisType, "ChangeToil");
        private static FieldInfo Current;
        private static LocalVariableInfo JobDriver;
        private static List<CodeInstruction> InsertedInstructions => new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Ldarg_0, null),
            new CodeInstruction(OpCodes.Ldflda, Current),
            new CodeInstruction(OpCodes.Ldloc_S, JobDriver.LocalIndex),
            new CodeInstruction(OpCodes.Call, ChangeToil_Method),
        };
        public static bool Prepare(Type type, Type ntype, MethodInfo method, List<MethodInfo> actions)
        {
            var get_Current = ntype.GetInterfaceMap(typeof(IEnumerator<Toil>)).TargetMethods.First();
            Current = GetFieldInfo(get_Current);
            if (Current == null || !SearchJobDriver(method, type, out JobDriver))
                return false;
            return true;
        }
        private static FieldInfo GetFieldInfo(MethodInfo getterMethod)
        {
            List<CodeInstruction> instructionList;
            try { instructionList = PatchProcessor.GetCurrentInstructions(getterMethod); }
            catch { instructionList = PatchProcessor.GetOriginalInstructions(getterMethod); }
            var length = instructionList.Count;
            for (int i = 0; i < length; i++)
            {
                CodeInstruction instruction = instructionList[length - 1 - i];
                if (instruction.opcode == OpCodes.Ldfld && instructionList[length - 2 - i].IsLdarg(0))
                    return instruction.operand as FieldInfo;
            }
            return null;
        }
        private static bool SearchJobDriver(MethodInfo searchMethod, Type JobDriver, out LocalVariableInfo JobDriver_local)
        {
            JobDriver_local = null;
            var local = searchMethod.GetMethodBody().LocalVariables.FirstOrFallback(t => t.LocalType == JobDriver);
            if (local == null)
                return false;
            JobDriver_local = local;
            return true;
        }
        public static bool Transpile(ref List<CodeInstruction> instuctionList, int pos, List<MethodInfo> actions)
        {
            instuctionList.InsertRange(pos + 1, InsertedInstructions);
            return true;
        }

        public static void ChangeToil(ref Toil toil, JobDriver driver)
        {
            Pawn pawn = toil.actor ?? driver.pawn;
            if (pawn == null)
            {
                Log.Error("TF_BaseMessage".Translate() + "TF_Error_ChangeToil".Translate(toil, driver.job.def, driver));
                return;
            }
            if (!pawn.CanUseTools(out var tracker) || !ToolType.jobToolType.TryGetValue(driver.job.def, out var toolType))
                return;

            if (tracker.usedHandler.BestTool.TryGetValue(toolType, out var tool))
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
                tool.ToolUse.inUse = true;
                Log.Message($"Test 0: {pawn} : {tool} : {tracker.memoryTool.FirstOrFallback()}: {tool.ToolUse.inUse}");
            });
            toil.AddFinishAction(delegate
            {
                if (equipTool && (!(pawn.jobs.jobQueue.FirstOrDefault() is QueuedJob nextJob) || !dropJobs.Contains(nextJob.job.def)))
                    pawn.DeequipTool();
                tool.ToolUse.inUse = false;
                tracker.toolInUse = null;
                tracker.memoryEquipment = null;
                tracker.memoryEquipmentOffHand = null;
            });
        }
        public static List<JobDef> dropJobs = new List<JobDef>()
        {
            RimWorld.JobDefOf.DropEquipment
        };
        private static void NoTool(ref Toil toil, Pawn pawn, Pawn_ToolTracker tracker)
        {

        }
    }
}
