using HarmonyLib;
using Verse;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using RimWorld;
using System.Reflection;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch]
    public static class Patch_StatRequest_For
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(StatRequest), "For", new Type[] { typeof(Thing)})]
        public static StatRequest For(ThingWithComps tool, ToolType toolType)
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var Thing_def = AccessTools.Field(typeof(Thing), "def");
                var instructionList = instructions.ToList();
                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];
                    if (instruction.LoadsField(Thing_def))
                    {
                        instructionList[i] = new CodeInstruction(OpCodes.Ldarg_1, null);
                        instructionList.RemoveAt(i - 1);
                        break;
                    }
                }
                return instructionList;
            }
            _ = Transpiler(null);
            return StatRequest.ForEmpty();
        }
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(StatRequest), "For", new Type[] { typeof(BuildableDef), typeof(ThingDef), typeof(QualityCategory) })]
        public static StatRequest For(ToolType toolType, ThingDef stuffDef, QualityCategory quality = QualityCategory.Normal)
        {
            return StatRequest.ForEmpty();
        }
    }
    [HarmonyPatch]
    public static class Patch_StatRequest_StatBases
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(StatRequest), "StatBases");
        }
        public static bool Prefix(ref List<StatModifier> __result, Def ___defInt, Thing ___thingInt)
        {
            if (___defInt is ToolType toolType)
            {
                var tool = ___thingInt as ThingWithComps;
                var thingDef = ___thingInt?.def;
                var modifiers = new List<StatModifier>(toolType.workStatFactors);
                var effectiveness = new StatModifier() { stat = StatDefOf.ToolEffectivenessFactor, value = 1f };
                effectiveness.value *= thingDef?.statBases.GetStatFactorFromList(StatDefOf.ToolEffectivenessFactor) ?? 1f;
                effectiveness.value *= tool?.ToolCompProp()?.toolTypesValue.GetToolTypeValueFromList(toolType, 1f) ?? 1f;
                modifiers.Add(effectiveness);
                __result = modifiers;
                return false;
            }
            return true;
        }
    }
}
