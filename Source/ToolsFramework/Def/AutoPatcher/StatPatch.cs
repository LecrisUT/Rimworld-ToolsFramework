using System;
using System.Reflection;
using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;
using Verse.AI;
using System.Reflection.Emit;

namespace ToolsFramework.AutoPatcher
{
    public static class StatPatch
    {
        private static readonly Type thisType = typeof(StatPatch);
        private static Type JobDriver;
        private static FieldInfo Job_Info;
        private static FieldInfo JobDriver_Info;
        private static List<CodeInstruction> InsertedInstructions
        {
            get
            {
                if (JobDriver_Info == null)
                {
                    return new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Ldarg_0, null),
                        new CodeInstruction(OpCodes.Ldfld, Job_Info)
                    };
                }
                return new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Ldarg_0, null),
                    new CodeInstruction(OpCodes.Ldfld, JobDriver_Info),
                    new CodeInstruction(OpCodes.Ldfld, Job_Info)
                };
            }
        }
        public static bool Prepare(Type type, Type ntype, MethodInfo method, List<StatDef> stats)
        {
            JobDriver = type;
            JobDriver_Info = null;
            if (ntype != null)
            {
                List<FieldInfo> fields = AccessTools.GetDeclaredFields(ntype);
                JobDriver_Info = fields.FirstOrFallback(t => t.FieldType == JobDriver);
                if (JobDriver_Info == null)
                    return false;
            }
            Job_Info = AccessTools.Field(type, "job");
            return Job_Info != null;
        }
        public static bool Transpile(ref List<CodeInstruction> instuctionList, ILGenerator generator, int pos, StatDef stat, out List<(int pos, int offset)> CIOffsets)
        {
            CIOffsets = new List<(int, int)>();
            if (pos + 2 >= instuctionList.Count ||
                   !instuctionList[pos + 2].Is(OpCodes.Call, GetStatValue_Info) ||
                   !InstructionIsStatDef(instuctionList[pos]))
                return false;
            instuctionList[pos + 2].operand = thisGetStatValueJob_Info;
            instuctionList.InsertRange(pos + 1, InsertedInstructions);
            CIOffsets.Add((pos + 1, InsertedInstructions.Count));
            return true;
        }
        private static bool InstructionIsStatDef(CodeInstruction instruction)
        {
            if (instruction.operand is MethodInfo method && method.ReturnType.IsAssignableFrom(typeof(StatDef)))
                return true;
            if (instruction.operand is FieldInfo field && field.FieldType.IsAssignableFrom(typeof(StatDef)))
                return true;
            return false;
        }
        private static readonly MethodInfo GetStatValue_Info = AccessTools.Method(typeof(StatExtension), "GetStatValue");
        private static readonly MethodInfo thisGetStatValueJob_Info = AccessTools.Method(thisType, "GetStatValueJob");
        public static float GetStatValueJob(this Pawn pawn, StatDef stat, Job job, bool applyPostProcess = true)
        {
            var val = pawn.GetStatValue(stat, applyPostProcess);
            if (!ToolType.jobToolType.ContainsKey(job.def))
                return val;
            if (pawn.CanUseTools(out var tracker) && tracker.toolInUse is Tool tool)
                return val * tool.GetValue(job.def, stat);
            return GetStatValueJob_Fallback(val, pawn, stat, job, applyPostProcess);
        }
        public static float GetStatValueJob_Fallback(float orig, Pawn pawn, StatDef stat, Job job, bool applyPostProcess)
            => orig;
    }
}
