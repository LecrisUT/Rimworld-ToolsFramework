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
            if (tracker.optimizingTool || tracker.NecessaryToolTypes.EnumerableNullOrEmpty() || !(pawn.MapHeld?.GetComponent<Map_ToolTracker>() is Map_ToolTracker mapTracker))
                goto NoJobs;
            // Remove extra Tools
            var jobs = new List<Job>();
            var heldTools = tracker.UsedHandler.HeldTools.ToList();
            var bestTools = tracker.UsedHandler.BestTool;
            var neededToolTypes = tracker.NecessaryToolTypes;
            var neededTools = new List<Tool>();
            foreach (var toolType in neededToolTypes)
            {
                var tool = bestTools[toolType];
                if (tool != null)
                    neededTools.Add(tool);
            }
            neededTools.RemoveDuplicates();
            var extraTools = heldTools.Except(neededTools).ToList();
            // Get better tools
            var toolsToGet = new List<Tool>();
            var mapTools = mapTracker.BestTools;
            var bestMapToolThings = mapTracker.BestToolThings;
            var mapToolThings = mapTracker.StoredToolThings;
            var reservation = pawn.MapHeld.reservationManager;
            var faction = pawn.Faction;
            var assignmentFilter = tracker.ToolAssignment.filter;
            foreach (var toolType in neededToolTypes)
            {
                var mapTool = mapTools[toolType];
                if (mapTool == null)
                    continue;
                if (mapTool.IsForbidden(pawn) || reservation.IsReservedByAnyoneOf(mapTool, faction))
                {
                    mapTool = null;
                    foreach (var thingDef in bestMapToolThings.Keys)
                    {
                        var tool = bestMapToolThings[thingDef];
                        if (tool == null)
                            continue;
                        if (assignmentFilter.Allows(thingDef) && !reservation.IsReservedByAnyoneOf(tool, faction) && thingDef.IsTool(out var prop) && prop.ToolTypes.Contains(toolType))
                        {
                            if (!tool.IsForbidden(pawn))
                                mapTool = tool;
                            else
                                foreach (var otherTool in mapToolThings[thingDef])
                                    if (!otherTool.IsForbidden(pawn) && otherTool.GetValue(toolType) > mapTool.GetValue(toolType, 0f))
                                        mapTool = otherTool;
                        }
                    }
                    if (mapTool == null)
                        continue;
                }
                var heldTool = bestTools[toolType];
                if (heldTool == null || mapTool[toolType] > heldTool[toolType])
                {
                    if (heldTool != null)
                        extraTools.AddDistinct(heldTool);
                    toolsToGet.AddDistinct(mapTool);
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
