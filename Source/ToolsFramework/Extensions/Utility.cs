using HarmonyLib;
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
        private static HashSet<ThingDef> allToolDefs = new HashSet<ThingDef>();
        public static HashSet<ThingDef> AllToolDefs
        {
            get
            {
                if (allToolDefs.EnumerableNullOrEmpty())
                    allToolDefs = DefDatabase<ThingDef>.AllDefs.Where(t => t.IsTool()).ToHashSet();
                return allToolDefs;
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
        public static bool IsTool(this BuildableDef def)
            => def is ThingDef tdef && typeof(Tool).IsAssignableFrom(tdef.thingClass);
        public static string ToolStatDrawEntry(Tool tool, ToolType toolType, float value)
        {
            var stat = StatDefOf.ToolEffectivenessFactor;
            var builder = new StringBuilder(stat.description);
            builder.AppendLine("\n\n" + stat.Worker.GetExplanationFull(Patch_StatRequest_For.For(tool, toolType), stat.toStringNumberSense, value));
            return builder.ToString();
        }
        /*public static StatDrawEntry ToolTypeDrawEntry(Tool tool, ToolType toolType, float value)
            => new StatDrawEntry(StatCategoryDefOf.Tools, StatDefOf.ToolEffectivenessFactor, value, StatRequest_For.For(tool, toolType));*/
        public static StatDrawEntry ToolTypeDrawEntry(Tool tool, ToolType toolType, float value)
            => new StatDrawEntry(StatCategoryDefOf.Tools, toolType.LabelCap, value.ToStringPercent("F0"), ToolStatDrawEntry(tool, toolType, value), 99999,
                overrideReportTitle: "[[LC]ToolsFramework] ToolStatDrawEntry", hyperlinks: null, forceUnfinalizedMode: false);

    }
}
