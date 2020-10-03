using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ToolsFramework
{
    public class JobGiver_OptimizeTools : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!Settings.optimization || !pawn.CanUseTools(out var tracker))
                return null;
            if (Find.TickManager.TicksGame < tracker.nextOptimizationTick)
                return null;
            if (tracker.optimizingTool && (pawn.CurJobDef?.HasModExtension<Job_Extension>() ?? false))
                return null;
            if (tracker.optimizingTool || tracker.NecessaryToolTypes.EnumerableNullOrEmpty() || !pawn.MapHeld.TryGetMapToolTracker(out var mapTracker))
                goto NoJobs;
            // Remove extra Tools
            var jobs = new List<Job>();
            var heldTools = tracker.UsedHandler.HeldTools.ToList();
            var bestTools = tracker.UsedHandler.BestTool;
            var neededToolTypes = tracker.NecessaryToolTypes;
            var neededTools = new List<ThingWithComps>();
            foreach (var toolType in neededToolTypes)
            {
                var info = bestTools[toolType];
                if (info != null)
                    neededTools.Add(info.tool);
            }
            neededTools.RemoveDuplicates();
            var extraTools = heldTools.Except(neededTools).ToList();
            // Get better tools
            var toolsToGet = new List<ThingWithComps>();
            var reservation = pawn.MapHeld.reservationManager;
            var faction = pawn.Faction;
            var assignmentFilter = tracker.ToolAssignment.filter;
            foreach (var toolType in neededToolTypes)
            {
                var mapToolInfo = mapTracker.BestTool(toolType);
                if (mapToolInfo == null)
                    continue;
                var mapTool = mapToolInfo.tool;
                if (mapTool.ToolIsForbidden(pawn, assignmentFilter, reservation, faction))
                {
                    mapTool = null;
                    foreach (var thingDef in Utility.AllToolDefs)
                    {
                        var toolInfo = mapTracker.BestToolInfo(thingDef);
                        if (toolInfo == null)
                            continue;
                        if (assignmentFilter.Allows(thingDef) && !reservation.IsReservedByAnyoneOf(toolInfo.tool, faction) && thingDef.IsTool(out var compProp) && compProp.ToolTypes.Contains(toolType))
                        {
                            if (!toolInfo.tool.IsForbidden(pawn))
                                mapToolInfo = toolInfo;
                            else
                                foreach (var otherInfo in mapTracker.StoredToolThingInfos(thingDef))
                                    if (!otherInfo.tool.IsForbidden(pawn) && otherInfo.comp.GetValue(toolType) > mapToolInfo.comp.GetValue(toolType, 0f))
                                        mapToolInfo = otherInfo;
                        }
                    }
                    if (mapToolInfo == null)
                        continue;
                }
                var heldToolInfo = bestTools[toolType];
                if (heldToolInfo == null || mapToolInfo.comp[toolType] > heldToolInfo.comp[toolType])
                {
                    if (heldToolInfo != null)
                        extraTools.AddDistinct(heldToolInfo.tool);
                    toolsToGet.AddDistinct(mapToolInfo.tool);
                }
            }
            // Make JobList
            foreach (var tool in extraTools)
            {
                var job = pawn.PutAwayTool(tool);
                if (job != null)
                    jobs.Add(job);
            }
            foreach (var tool in toolsToGet)
            {
                var job = pawn.TakeTool(tool);
                if (job == null)
                    continue;
                if (reservation.Reserve(pawn, job, tool))
                    jobs.Add(job);
            }
            if (jobs.Count > 0)
            {
                tracker.optimizingTool = true;
                if (jobs.Count > 1)
                    foreach (var job in jobs.GetRange(1, jobs.Count - 1))
                        pawn.jobs.jobQueue.EnqueueFirst(job);
                return jobs.First();
            }
        NoJobs:
            tracker.optimizingTool=false;
            tracker.nextOptimizationTick = Find.TickManager.TicksGame + Mathf.RoundToInt(Settings.optimizationDelay * Rand.Range(0.9f, 1.1f));
            return null;
        }
    }
}
