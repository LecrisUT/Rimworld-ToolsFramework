using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ToolsFramework.AutoPatcher;
using Verse;
using Verse.AI;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch]
    public static class Patch_Toil
    {
        public static List<(MethodInfo method, FieldInfo action, CodeInstruction pawnCI)> targets = new List<(MethodInfo, FieldInfo, CodeInstruction)>
        {
            { (AccessTools.Method(typeof(Toils_Interpersonal), nameof(Toils_Interpersonal.TryTrain)), AccessTools.Field(typeof(Toil), nameof(Toil.initAction)), new CodeInstruction(OpCodes.Ldloc_0, null)) },
        };
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var Action_ctor = AccessTools.Constructor(typeof(Action), new[] { typeof(object), typeof(IntPtr) });
            foreach (var item in targets)
            {
                MethodInfo actionMethod = null;
                var toilMethod = item.method;
                var field = item.action;
                var instructionList = PatchProcessor.GetCurrentInstructions(toilMethod);
                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];
                    if (instruction.StoresField(field) && instructionList[i - 1].Is(OpCodes.Newobj, Action_ctor) && instructionList[i - 2].opcode == OpCodes.Ldftn)
                    {
                        actionMethod = (MethodInfo)instructionList[i - 2].operand;
                        pawnCI = item.pawnCI;
                        break;
                    }
                }
                yield return actionMethod;
            }
        }
        private static CodeInstruction pawnCI;
        public static bool Prepare(MethodBase original)
        {
            return true;
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
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
                        pawnCI,
                        new CodeInstruction(OpCodes.Call, CurJob),
                    });
                    i += 2;
                }
            }
            return instructionList;
        }
    }
}
