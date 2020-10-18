using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ToolsFramework
{
    public class JobDriver_ReturnToolToContainer : JobDriver_HaulToContainer
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var tool = TargetThingA as ThingWithComps;
            pawn.DropTool(tool);
            foreach (var toil in base.MakeNewToils())
                yield return toil;
        }
    }
}
