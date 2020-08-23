using RimWorld;
using System.Collections.Generic;
using Verse.AI;

namespace ToolsFramework
{
    public class JobDriver_PickTool : JobDriver_TakeInventory
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (!(TargetThingA is Tool tool))
                yield break;
            AddFailCondition(() => tool.IsForbidden(pawn));
            foreach (var toil in base.MakeNewToils())
                yield return toil;
            if (pawn.CanUseTools(out var tracker) && job.playerForced)
                tracker.forcedHandler.SetForced(tool, true);
        }
    }
}
