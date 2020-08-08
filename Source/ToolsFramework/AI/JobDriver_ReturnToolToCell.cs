using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ToolsFramework
{
    public class JobDriver_ReturnToolToCell : JobDriver_HaulToCell
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
