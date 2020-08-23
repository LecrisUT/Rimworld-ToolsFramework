using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch(typeof(ThingDef))]
    [HarmonyPatch(nameof(ThingDef.SpecialDisplayStats))]
    public static class Patch_ThingDef_SpecialDisplayStats
    {
        public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> values, ThingDef __instance, StatRequest req)
        {
            foreach (var stat in values)
                yield return stat;
            if (!req.HasThing && __instance.IsTool(out var toolProp))
            {
                var defaultTool = new Tool() { def = (ThingDef)req.Def };
                defaultTool.SetStuffDirect(req.StuffDef);
                foreach (var toolType in toolProp.ToolTypes)
                    yield return Utility.ToolTypeDrawEntry(defaultTool, toolType, defaultTool.GetStatValue(toolType,StatDefOf.ToolEffectivenessFactor));
            }
        }
    }
}