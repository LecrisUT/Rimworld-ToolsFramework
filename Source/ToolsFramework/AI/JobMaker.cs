using Verse;
using Verse.AI;

namespace ToolsFramework
{
    public static class JobMaker
    {
        public static Job PutAwayTool(this Pawn pawn, ThingWithComps tool, bool bestLoc = true)
        {
            if (bestLoc)
                return HaulAIUtility.HaulToStorageJob(pawn, tool);
            else
                return HaulAIUtility.HaulToStorageJob(pawn, tool);
        }

        public static Job TakeTool(this Pawn pawn, ThingWithComps tool)
        {
            return new Job(JobDefOf.PickTool, tool)
            {
                count = 1,
                checkEncumbrance = true,
            };
        }
    }
}
