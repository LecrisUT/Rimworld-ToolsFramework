using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch(typeof(StatWorker))]
    [HarmonyPatch(nameof(StatWorker.GetExplanationFinalizePart))]
    public static class Patch_StatWorker_GetExplanationFinalizePart
    {
        public static void Postfix(ref string __result, StatDef ___stat, float finalVal, StatRequest req)
        {
            if (Utility.StatsToToolType.TryGetValue(___stat, out var toolTypes))
            {
                var builder = new StringBuilder("\n\n"+"ToolEffect".Translate().CapitalizeFirst());
                builder.AppendLine();
                var pawn = req.Thing as Pawn ?? req.Pawn;
                if (pawn == null)
                {
                    Log.ErrorOnce("TF_BaseMessage".Translate() + "TF_Error_StatWorkerNoPawn".Translate(___stat), ___stat.GetHashCode());
                    return;
                }
                if (!pawn.CanUseTools(out var tracker))
                    return;
                foreach (var toolType in toolTypes)
                {
                    var tool = tracker.usedHandler.BestTool[toolType];
                    ReportText(ref builder, pawn, ___stat, tool, toolType, finalVal);
                }
                __result += builder.ToString();
            }
        }
        private static void ReportText(ref StringBuilder stringBuilder, Pawn pawn, StatDef stat, Tool tool, ToolType toolType, float origVal)
        {
            var toolVal = tool.GetStatValue(toolType, stat, fallback: 1f);
            stringBuilder.Append("  " + toolType.LabelCap + " : x" + toolVal + " = " + origVal * toolVal);
            if (tool == null)
                stringBuilder.AppendLine(" [" + "NoTool".Translate() + "]");
            else
                stringBuilder.AppendLine(" [" + tool.LabelCap + "]");
        }
    }
}