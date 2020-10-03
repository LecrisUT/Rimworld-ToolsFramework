using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch(typeof(FloatMenuMakerMap),"AddHumanlikeOrders")]
    public static class Patch_FloatMenuMakerMap_AddHumanlikeOrders
    {
        public static void Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                return;
            IntVec3 position = IntVec3.FromVector3(clickPos);
            var tools = position.GetThingList(pawn.Map).OfType<ThingWithComps>().Where(t => t.IsTool(true)).ToList();
            foreach (var tool in tools)
            {
                if (!pawn.CanReach(tool, PathEndMode.ClosestTouch, Danger.Deadly))
                    return;
                if (!MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, tool, 1))
                {
                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUp".Translate(tool.Label, tool) + "AsTool".Translate() + " (" + "ApparelForcedLower".Translate() + ")", delegate
                    {
                        tool.SetForbidden(value: false, warnOnFail: false);
                        var job = pawn.TakeTool(tool);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }, MenuOptionPriority.High), pawn, tool));
                }
            }
        }
    }
}