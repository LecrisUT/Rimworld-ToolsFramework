using HarmonyLib;
using RimWorld;
using System.Text;
using Verse;

namespace ToolsFramework.Harmony
{
    // Temporary solution until swithing to statpart
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
                    var info = tracker.UsedHandler.BestTool[toolType];
                    ReportText(ref builder, pawn, ___stat, info, toolType, finalVal);
                }
                __result += builder.ToString();
            }
        }
        private static void ReportText(ref StringBuilder stringBuilder, Pawn pawn, StatDef stat, ToolInfo info, ToolType toolType, float origVal)
        {
            info.comp.TryGetValue(toolType, stat, out var fac, out var off);
#if DEBUG
            stringBuilder.Append("  " + toolType.LabelCap + " : + " + off.ToStringPercent("F2") + " x " + fac.ToStringPercent("F2") + " = " + ((origVal + off) * fac).ToStringPercent("F2"));
#else
            stringBuilder.Append("  " + toolType.LabelCap + " : = " + ((origVal + off) * fac).ToStringPercent("F2"));
#endif
            if (info == null)
                stringBuilder.AppendLine(" [" + "NoTool".Translate() + "]");
            else
            {
                stringBuilder.AppendLine(" [" + info.tool.LabelCap + "]");
                var jobBonus = info.comp.CompProp.jobBonus;
                if (jobBonus.NullOrEmpty())
                    stringBuilder.AppendLine("  [" + "NoJobBonus".Translate() + "]");
                else
                {
                    stringBuilder.AppendLine("  " + "JobBonus".Translate() + ":");
                    foreach (var bonus in jobBonus)
                        stringBuilder.AppendLine("    " + bonus.job + ": x" + bonus.value);
                }
            }
        }
    }
}