using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ToolsFramework.AutoPatcher;
using Verse;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch]
    public static class Patch_InteractionWorker_Interacted
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(InteractionWorker_RecruitAttempt), nameof(InteractionWorker_RecruitAttempt.Interacted));
        }
        public static bool Prepare(MethodBase original)
        {
            return true;
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionList = instructions.ToList();
            var GetStatValue = AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue));
            var GetStatValueJob = AccessTools.Method(typeof(StatPatch), nameof(StatPatch.GetStatValueJob));
            var CurJob = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.CurJob));
            for (int i = 0; i < instructionList.Count; i++)
            {
                var instruction = instructionList[i];
                if (instruction.Calls(GetStatValue))
                {
                    instructionList[i].operand = GetStatValueJob;
                    instructionList.InsertRange(i - 1, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldarg_1, null),
                        new CodeInstruction(OpCodes.Call, CurJob),
                    });
                    i += 2;
                }
            }
            return instructionList;
        }
    }
}
