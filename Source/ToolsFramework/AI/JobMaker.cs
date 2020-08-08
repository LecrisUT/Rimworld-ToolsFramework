using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace ToolsFramework
{
    public static class JobMaker
    {
        public static Job PutAwayTool(this Pawn pawn, Tool tool, bool bestLoc = true)
        {
            Job job;
            if (bestLoc)
                job = HaulAIUtility.HaulToStorageJob(pawn, tool);
            else
                job = HaulAIUtility.HaulToStorageJob(pawn, tool);
            if (job != null)
                pawn.DropTool(tool);
            return job;
        }

        public static Job TakeTool(this Pawn pawn, Tool tool)
        {
            return new Job(JobDefOf.PickTool, tool)
            {
                count = 1,
                checkEncumbrance = true,
            };
        }
        public static Job TakeTool(this Pawn pawn, List<Tool> tools)
        {
            if (tools.NullOrEmpty())
                return null;
            var job = new Job(JobDefOf.PickTool)
            {
                count = 1,
                checkEncumbrance = true,
            };
            foreach (var tool in tools)
                job.AddQueuedTarget(TargetIndex.A, tool);
            job.targetA = job.targetQueueA.First();
            return job;
        }
    }
}
