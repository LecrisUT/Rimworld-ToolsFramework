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
    public static class Patch_JobDriver_MakeNewToils
    {
        private static MethodInfo ToilMethod;
        private static CodeInstruction JobDriverCI;
        private static FieldInfo Current;
        public static Dictionary<Type, (MethodInfo toil, CodeInstruction jobDriver)> targets = new Dictionary<Type, (MethodInfo, CodeInstruction)>
        {
            { typeof(JobDriver_InteractAnimal), (AccessTools.Method(typeof(JobDriver_InteractAnimal), "FinalInteractToil"), new CodeInstruction(OpCodes.Ldloc_2, null)) },
        };
        public static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (var jobDriver in targets.Keys)
                yield return AccessTools.Method(jobDriver.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First(t => t.GetInterfaces().Any(tt => tt == typeof(IEnumerator<Toil>))), "MoveNext");
        }
        public static bool Prepare(MethodBase original)
        {
            if (original == null)
                return true;
            var ntype = original.DeclaringType;
            var type = ntype.DeclaringType;
            var target = targets[type];
            ToilMethod = target.toil;
            JobDriverCI = target.jobDriver;
            var get_Current = ntype.GetInterfaceMap(typeof(IEnumerator<Toil>)).TargetMethods.First();
            Current = GetFieldInfo(get_Current);
            return true;
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            var instructionList = instructions.ToList();
            bool flag = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                var instruction = instructionList[i];
                if (instruction.Calls(ToilMethod))
                {
                    flag = true;
                    continue;
                }
                if (flag && instruction.StoresField(Current))
                {
                    var changeToil = AccessTools.Method(typeof(ToilPatch), nameof(ToilPatch.ChangeToil));
                    instructionList.InsertRange(i + 1, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldarg_0, null),
                        new CodeInstruction(OpCodes.Ldflda, Current),
                        JobDriverCI,
                        new CodeInstruction(OpCodes.Call, changeToil),
                    });
                    return instructionList;
                }
            }
            return null;
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
    }
}
