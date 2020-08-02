using Verse;
using Verse.AI;

namespace ToolsFramework
{
    public static class JobMaker
    {
        public static JobDef otherPutAwayJob;
        public static Job PutAwayTool(this Pawn pawn, Tool tool)
        {
            if (otherPutAwayJob != null)
                return new Job(otherPutAwayJob, tool);
            return HaulAIUtility.HaulToStorageJob(pawn, tool);
        }
    }
}
