using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsFramework.Harmony;
using Verse;

namespace ToolsFramework
{
    public static class Utility
    {
        private static HashSet<ThingDef> allToolDefs;
        public static HashSet<ThingDef> AllToolDefs
        {
            get
            {
                if (allToolDefs.EnumerableNullOrEmpty())
                    allToolDefs = DefDatabase<ThingDef>.AllDefs.Where(t => t.IsTool()).ToHashSet();
                return allToolDefs;
            }
        }
        private static Dictionary<StatDef, List<ToolType>> statsToToolType;
        public static Dictionary<StatDef, List<ToolType>> StatsToToolType
        {
            get
            {
                if (statsToToolType.EnumerableNullOrEmpty())
                {
                    statsToToolType = new Dictionary<StatDef, List<ToolType>>();
                    foreach (var toolType in ToolType.allToolTypes)
                    {
                        foreach (var stat in toolType.workStatFactors.Select(t => t.stat))
                        {
                            if (!statsToToolType.TryGetValue(stat, out var list))
                            {
                                statsToToolType.Add(stat, new List<ToolType> { toolType });
                                continue;
                            }
                            list.AddDistinct(toolType);
                        }
                        foreach (var stat in toolType.workStatOffset.Select(t => t.stat))
                        {
                            if (!statsToToolType.TryGetValue(stat, out var list))
                            {
                                statsToToolType.Add(stat, new List<ToolType> { toolType });
                                continue;
                            }
                            list.AddDistinct(toolType);
                        }
                    }
                }
                return statsToToolType;
            }
        }
        public static Dictionary<WorkGiverDef, List<ToolType>> toolWorkGivers = new Dictionary<WorkGiverDef, List<ToolType>>();
        public static Dictionary<WorkGiverDef, List<ToolType>> ToolWorkGivers
        {
            get
            {
                if (toolWorkGivers.EnumerableNullOrEmpty())
                {
                    foreach (var wg in DefDatabase<WorkGiverDef>.AllDefs)
                        if (wg.GetModExtension<WorkGiver_Extension>() is WorkGiver_Extension extension)
                            toolWorkGivers.Add(wg, extension.toolTypes);
                }
                return toolWorkGivers;
            }
        }
        public static string ToolStatDrawEntry(ThingWithComps tool, ToolType toolType, float value, ThingDef stuffDef = null)
        {
            var stat = StatDefOf.ToolEffectivenessFactor;
            var builder = new StringBuilder(stat.description);
            var statReq = tool == null ? Patch_StatRequest_For.For(toolType, stuffDef) : Patch_StatRequest_For.For(tool, toolType);
            builder.AppendLine("\n\n" + stat.Worker.GetExplanationFull(statReq, stat.toStringNumberSense, value));
            return builder.ToString();
        }
        public static StatDrawEntry ToolTypeDrawEntry(ThingWithComps tool, ToolType toolType, float value, ThingDef stuffDef = null)
            => new StatDrawEntry(StatCategoryDefOf.Tools, toolType.LabelCap, value.ToStringPercent("F0"), ToolStatDrawEntry(tool, toolType, value, stuffDef), 99999,
                overrideReportTitle: toolType.description, hyperlinks: null, forceUnfinalizedMode: false);

    }
}
