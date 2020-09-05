using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ToolsFramework
{
    public class Map_ToolTracker : MapComponent
    {
        public Map_ToolTracker(Map map) : base(map)
        {
            nextOptimizationTick = Find.TickManager.TicksGame;
        }
        public bool dirtyCache = true;
        private List<Tool> storedTools = new List<Tool>();
        public IEnumerable<Tool> StoredTools => storedTools;
        public List<Tool> StoredToolsList
        {
            get
            {
                dirtyCache = true;
                return storedTools;
            }
            set
            {
                dirtyCache = true;
                storedTools = value;
            }
        }

        private Dictionary<ToolType, Tool> bestTools = ToolType.allToolTypes.ToDictionary<ToolType, ToolType, Tool>(t => t, t => null);
        public Dictionary<ToolType, Tool> BestTools
        {
            get
            {
                if (dirtyCache)
                    FindBestTools();
                return bestTools;
            }
        }
        private Dictionary<ThingDef, Tool> bestToolThings = Utility.AllToolDefs.ToDictionary<ThingDef, ThingDef, Tool>(t => t, t => null);
        public Dictionary<ThingDef, Tool> BestToolThings
        {
            get
            {
                if (dirtyCache)
                    FindBestTools();
                return bestToolThings;
            }
        }
        private Dictionary<ThingDef, List<Tool>> storedToolThings = Utility.AllToolDefs.ToDictionary(t => t, t => new List<Tool>());
        public Dictionary<ThingDef, List<Tool>> StoredToolThings
        {
            get
            {
                if (dirtyCache)
                    FindBestTools();
                return storedToolThings;
            }
        }
        public int nextOptimizationTick = 0;
        public override void MapComponentTick()
        {
            if (Settings.optimization && Find.TickManager.TicksGame > nextOptimizationTick)
            {
                UpdateStoredTools();
                nextOptimizationTick = Find.TickManager.TicksGame + Settings.mapTrackerDelay;
            }
        }
        public void UpdateStoredTools()
        {
            var faction = Find.FactionManager.OfPlayer;
            var reservation = map.reservationManager;
            storedTools = map.listerThings.AllThings?.OfType<Tool>()?.Where(t => t.IsInAnyStorage() && !t.IsForbidden(faction) && !reservation.IsReservedByAnyoneOf(t, faction)).ToList() ?? new List<Tool>();
            storedToolThings = Utility.AllToolDefs.ToDictionary(t => t, t => new List<Tool>());
            foreach (var tool in storedTools)
                storedToolThings[tool.def].Add(tool);
            dirtyCache = true;
        }
        public void FindBestTools()
        {
            bestToolThings = Utility.AllToolDefs.ToDictionary<ThingDef, ThingDef, Tool>(t => t, t => null);
            foreach (var toolType in DefDatabase<ToolType>.AllDefs)
                FindBestTool(toolType);
            foreach (var tool in storedTools)
            {
                var toolDef = tool.def;
                if (tool.TotalScore > (bestToolThings[toolDef]?.TotalScore ?? 0f))
                    bestToolThings[toolDef] = tool;
            }
            dirtyCache = false;
        }
        public void FindBestTool(ToolType toolType)
        {
            var tool = BestTool(toolType);
            bestTools[toolType] = tool;
        }
        public Tool BestTool(ToolType toolType)
        {
            Tool tool = null;
            float val = 0f;
            foreach (var currTool in storedTools)
            {
                float currVal = currTool[toolType];
                if (currTool.TryGetValue(toolType, out var baseVal) && baseVal > 1f && currVal > val)
                {
                    tool = currTool;
                    val = currVal;
                }
            }
            return tool;
        }
        public Tool ClosestTool(ToolType toolType, IntVec3 pos, Pawn pawn = null)
        {
            var reservation = pawn?.MapHeld.reservationManager;
            var faction = pawn?.Faction;
            Tool tool = null;
            float bestDist = float.MaxValue;
            foreach (var currTool in StoredTools)
            {
                if (currTool.IsForbidden(pawn) || (reservation?.IsReservedByAnyoneOf(currTool, faction) ?? false) || !currTool.TryGetValue(toolType, out float val) || val < 1f || !Distance(currTool, pos, out float dist))
                    continue;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    tool = currTool;
                }
            }
            return tool;
        }
        private static bool Distance(Thing target, IntVec3 source, out float dist)
        {
            dist = float.MaxValue;
            if (Settings.opportunisticTakeTool_calcPath)
            {
                var path = target.Map.pathFinder.FindPath(source, target, TraverseParms.For(TraverseMode.PassDoors, Danger.Some), PathEndMode.Touch);
                bool found = path.Found;
                if (found)
                    dist = path.TotalCost * 2;
                path.ReleaseToPool();
                return found;
            }
            dist = Mathf.Sqrt(source.DistanceToSquared(target.Position)) * 2;
            return true;
        }
        public override void ExposeData()
        {
            Scribe_Values.Look(ref nextOptimizationTick, "nextOptimizationTick");
            Scribe_Collections.Look(ref storedTools, "storedTools", LookMode.Reference);
        }
    }
}
