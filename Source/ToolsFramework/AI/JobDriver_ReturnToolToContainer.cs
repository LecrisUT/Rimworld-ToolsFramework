using System.Collections.Generic;
using Verse.AI;

namespace ToolsFramework
{
    public class JobDriver_ReturnToolToContainer : JobDriver_HaulToContainer
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var tool = (Tool)TargetThingA;
            if (tool == null)
                yield break;
            pawn.DropTool(tool);
            foreach (var toil in base.MakeNewToils())
                yield return toil;
        }
    }
}
