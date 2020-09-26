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
    public static class Patch_JobDriver_DoBill_MakeNewToils
    {
        public static MethodBase TargetMethod()
            => AccessTools.Method(typeof(JobDriver_DoBill).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First(t => t.GetInterfaces().Any(tt => tt == typeof(IEnumerator<Toil>))), "MoveNext");
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            var instructionList = instructions.ToList();
            var DoRecipeWork = AccessTools.Method(typeof(Toils_Recipe), nameof(Toils_Recipe.DoRecipeWork));
            var type = original.DeclaringType;
            var get_Current = type.GetInterfaceMap(typeof(IEnumerator<Toil>)).TargetMethods.First();
            var Current = GetFieldInfo(get_Current);
            bool flag = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                var instruction = instructionList[i];
                if (instruction.Calls(DoRecipeWork))
                {
                    flag = true;
                    continue;
                }
                if (flag && instruction.StoresField(Current))
                {
                    var changeToil = AccessTools.Method(typeof(Patch_JobDriver_DoBill_MakeNewToils), nameof(ChangeToil));
                    instructionList.InsertRange(i + 1, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldarg_0, null),
                        new CodeInstruction(OpCodes.Ldflda, Current),
                        new CodeInstruction(OpCodes.Ldloc_1, null),
                        new CodeInstruction(OpCodes.Call, changeToil),
                    });
                    return instructionList;
                }
            }
            return null;
        }
        public static void ChangeToil(ref Toil toil, JobDriver driver)
        {

            Pawn pawn = toil.actor ?? driver.pawn;
            if (pawn == null)
            {
                Log.Error("TF_BaseMessage".Translate() + "TF_Error_ChangeToil".Translate(toil.Named(null), "", typeof(JobDriver_DoBill).Named(null)));
                return;
            }
            var billGiver = pawn.CurJob.targetA.Thing.def;
#if DEBUG
            Log.Message($"Test 3.0: {Dictionaries.billGiverToolType.TryGetValue(billGiver, out var toolType2)} : {toolType2}");
#endif
            if (!pawn.CanUseTools(out var tracker) || !Dictionaries.billGiverToolType.TryGetValue(billGiver, out var toolType))
                return;

            var tool = tracker.UsedHandler.BestTool[toolType];
#if DEBUG
            var test = new System.Text.StringBuilder($"Test 3.1: {tracker.UsedHandler.BestTool[toolType]}\n");
            foreach (var a in tracker.UsedHandler.BestTool)
                test.AppendLine($"{a.Key} : {a.Value}");
            Log.Message(test.ToString());
#endif
            if (tool != null)
                ToilPatch.HasTool(ref toil, pawn, tool, tracker);
            else
                ToilPatch.NoTool(ref toil, pawn, tracker);
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
