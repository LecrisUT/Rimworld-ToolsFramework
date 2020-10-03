using Verse;

namespace ToolsFramework
{
    public class ToolInfo
    {
        public ThingWithComps tool;
        public ToolComp comp;
        public ToolInfo() { }
        public ToolInfo(ThingWithComps tool, bool getInfo = true)
        {
            this.tool = tool;
            if (getInfo)
                tool.IsTool(out comp, false);
        }
        public ToolInfo(ThingWithComps tool, ToolComp comp)
        {
            this.tool = tool;
            this.comp = comp;
        }
        public static explicit operator ToolInfo((ThingWithComps, ToolComp) info) => new ToolInfo(info.Item1, info.Item2);
        public static explicit operator (ThingWithComps tool, ToolComp comp)(ToolInfo info) => (info.tool, info.comp);
        public static explicit operator ThingWithComps(ToolInfo info) => info.tool;
        public static explicit operator ToolComp(ToolInfo info) => info.comp;
    }
}
