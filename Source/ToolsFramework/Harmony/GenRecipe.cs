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
    public static class Patch_GenRecipe_DoRecipeWork
    {
        public static MethodBase TargetMethod()
            => AccessTools.Method(typeof(GenRecipe).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First(t => t.GetInterfaces().Any(tt => tt == typeof(IEnumerator<Thing>))), "MoveNext");
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionList = instructions.ToList();
            var GetStatValue = AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue));
            var efficiencyStat = AccessTools.Field(typeof(RecipeDef), nameof(RecipeDef.efficiencyStat));
            for (int i = 0; i < instructionList.Count; i++)
            {
                var instruction = instructionList[i];
                if (instruction.Calls(GetStatValue) && instructionList[i - 2].LoadsField(efficiencyStat))
                {
                    var GetStatValueJob = AccessTools.Method(typeof(StatPatch), nameof(StatPatch.GetStatValueJob));
                    var CurrJob = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.CurJob));
                    bool flag = false;
                    int endIns = -2;
                    int startIns = -1;
                    for (int j = i - 1; j > 0; j--)
                    {
                        var instruction2 = instructionList[j];
                        if (instruction2.IsLdarg(0))
                        {
                            if (flag)
                            {
                                startIns = j;
                                break;
                            }
                            else
                            {
                                endIns = j;
                                flag = true;
                            }
                        }
                    }
                    var insertInstruction = instructionList.GetRange(startIns, endIns - startIns);
                    insertInstruction.Add(new CodeInstruction(OpCodes.Call, CurrJob));
                    instructionList[i].operand = GetStatValueJob;
                    instructionList.InsertRange(i - 1, insertInstruction);
                    return instructionList;
                }
            }
            return null;
        }
    }
}
