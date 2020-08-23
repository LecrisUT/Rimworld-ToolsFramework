﻿using System.Collections.Generic;
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
            if (!(TargetThingA is Tool tool))
                yield break;
            pawn.DropTool(tool);
            foreach (var toil in base.MakeNewToils())
                yield return toil;
        }
    }
}
