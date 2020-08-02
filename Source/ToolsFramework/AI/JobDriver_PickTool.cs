using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ToolsFramework
{
    public class JobDriver_PickTool : JobDriver_TakeInventory
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            bool flag = pawn.CanUseTools(out var tracker);
            Tool toolA = null;
            if (TargetThingA is Tool)
            {
                toolA = (Tool)TargetThingA;
                foreach (var toil in PickTool(toolA))
                    yield return toil;
            }
            if (!job.targetQueueA.NullOrEmpty())
            {
                foreach (var target in job.targetQueueA)
                {
                    job.targetA = target;
                    var tool = (Tool)TargetThingA;
                    if (tool == null || tool == toolA)
                        continue;
                    foreach (var toil in PickTool(tool))
                        yield return toil;

                }
            }

            // Sealed function
            IEnumerable<Toil> PickTool(Tool tool)
            {
                foreach (var toil in base.MakeNewToils())
                    yield return toil;
                if (flag && job.playerForced)
                    tracker.forcedHandler.SetForced(tool, true);
            }
        }
    }
}
