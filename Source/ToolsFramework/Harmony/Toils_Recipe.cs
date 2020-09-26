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
    public static class Patch_Toils_Recipe_DoRecipeWork
    {
        public static MethodBase TargetMethod()
        {
            var instructionList = PatchProcessor.GetCurrentInstructions(AccessTools.Method(typeof(Toils_Recipe), nameof(Toils_Recipe.DoRecipeWork)));
            var tickAction = AccessTools.Field(typeof(Toil), nameof(Toil.tickAction));
            var Action_ctor = AccessTools.Constructor(typeof(Action), new[] { typeof(object), typeof(IntPtr) });
            MethodInfo method = null;
            for (int i = 0; i < instructionList.Count; i++)
            {
                var instruction = instructionList[i];
                if (instruction.StoresField(tickAction) && instructionList[i - 1].Is(OpCodes.Newobj, Action_ctor) && instructionList[i - 2].opcode == OpCodes.Ldftn)
                {
                    method = (MethodInfo)instructionList[i - 2].operand;
                    break;
                }
            }
            return method;
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionList = instructions.ToList();
            var GetStatValue = AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue));
            var workSpeedStat = AccessTools.Field(typeof(RecipeDef), nameof(RecipeDef.workSpeedStat));
            var get_RecipeDef = AccessTools.PropertyGetter(typeof(Job), nameof(Job.RecipeDef));
            for (int i = 0; i < instructionList.Count; i++)
            {
                var instruction = instructionList[i];
                if (instruction.Calls(GetStatValue) && instructionList[i - 2].LoadsField(workSpeedStat) && instructionList[i - 3].Calls(get_RecipeDef))
                {
                    if (instructionList[i - 4].IsLdloc() && instructionList[i - 5].IsLdloc())
                    {
                        var job = instructionList[i - 4];
                        var actor = instructionList[i - 5];
                        var GetStatValueJob = AccessTools.Method(typeof(StatPatch), nameof(StatPatch.GetStatValueJob));
                        instructionList[i].operand = GetStatValueJob;
                        instructionList.Insert(i - 1, job);
                        return instructionList;
                    }
                    else
                    {
                        Log.Error($"[[LC]ToolsFramework] Patch_Toils_Recipe_DoRecipeWork failed to patch");
                        return null;
                    }
                }
            }
            return null;
        }
    }
}
