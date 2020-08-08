using RimWorld;
using Verse;

namespace ToolsFramework
{
    [DefOf]
    public static class JobDefOf
    {
        public static JobDef OptimizeTools;
        public static JobDef PickTool;

        // Opportunistic Jobs
        public static JobDef TakeTempTool;
        public static JobDef ReturnToolToCell;
        public static JobDef ReturnToolToContainer;
    }
}
