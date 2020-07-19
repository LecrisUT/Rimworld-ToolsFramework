using RimWorld;
using System.Text;
using Verse;

namespace ToolsFramework
{
    public static class Utility
    {
        public static bool IsTool(this BuildableDef def)
            => def is ThingDef tdef && tdef.thingClass.IsAssignableFrom(typeof(Tool));
        public static string ToolStatDrawEntry(Tool tool, ToolType toolType)
        {
            StringBuilder builder = new StringBuilder("[TempText] ToolStatDrawEntry");
            return builder.ToString();
        }
        public static StatDrawEntry ToolTypeDrawEntry(Tool tool, ToolType toolType, float value)
            => new StatDrawEntry(Tools_StatCategoryDefOf.Tools, toolType.LabelCap, value.ToString(), toolType.description, 99999, ToolStatDrawEntry(tool, toolType),
                    hyperlinks: null, forceUnfinalizedMode: false);
    }
}
