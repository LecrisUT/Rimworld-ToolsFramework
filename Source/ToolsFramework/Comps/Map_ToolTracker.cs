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
            nextUpdateStoredToolsTick = Find.TickManager.TicksGame;
            nextUpdateUseableToolsTick = Find.TickManager.TicksGame;
        }
        public bool Cached_BestTools = false;
        private HashSet<Tool> storedTools = new HashSet<Tool>();
        public IEnumerable<Tool> StoredTools => storedTools;
        public HashSet<Tool> StoredToolsList
        {
            get
            {
                Cached_BestTools = false;
                return storedTools;
            }
        }
        private HashSet<Tool> useableTools = new HashSet<Tool>();
        public IEnumerable<Tool> UseableTools => useableTools;
        public HashSet<Tool> UseableToolsList
        {
            get
            {
                Cached_BestTools = false;
                return useableTools;
            }
        }

        private Dictionary<ToolType, Tool> bestTools;
        private Dictionary<ToolType, Tool> BestTools
        {
            get
            {
                if (!Cached_BestTools || bestTools == null)
                    FindBestTools();
                return bestTools;
            }
        }
        private Dictionary<ThingDef, Tool> bestToolThings;
        private Dictionary<ThingDef, Tool> BestToolThings
        {
            get
            {
                if (!Cached_BestTools || bestToolThings == null)
                    FindBestTools();
                return bestToolThings;
            }
        }
        private Dictionary<ThingDef, List<Tool>> storedToolThings;
        public int nextUpdateStoredToolsTick = 0;
        public int nextUpdateUseableToolsTick = 0;
        public override void MapComponentTick()
        {
            if (Settings.optimization)
            {
                if (Find.TickManager.TicksGame > nextUpdateStoredToolsTick)
                    UpdateStoredTools();
                else if (Find.TickManager.TicksGame > nextUpdateUseableToolsTick)
                    UpdateUseableTools();
            }
        }
        public void UpdateUseableTools()
        {
            nextUpdateUseableToolsTick = Find.TickManager.TicksGame + Settings.mapTrackerDelay_UseableTools;
            var faction = Find.FactionManager.OfPlayer;
            var reservation = map.reservationManager;
            var oldList = useableTools;
            useableTools = storedTools.Where(t => !t.IsForbidden(faction) && !reservation.IsReservedByAnyoneOf(t, faction)).ToHashSet() ?? new HashSet<Tool>();
            if (oldList.SetEquals(useableTools))
                return;
            Cached_BestTools = false;
        }
        public void UpdateStoredTools()
        {
            nextUpdateStoredToolsTick = Find.TickManager.TicksGame + Settings.mapTrackerDelay_StoredTools;
            var oldList = storedTools;
            storedTools = map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways).OfType<Tool>()?.Where(t => t.IsInAnyStorage()).ToHashSet() ?? new HashSet<Tool>();
            if (oldList.SetEquals(storedTools))
                return;
            storedToolThings = Utility.AllToolDefs.ToDictionary(t => t, t => new List<Tool>());
            foreach (var tool in storedTools)
                storedToolThings[tool.def].Add(tool);
            UpdateUseableTools();
        }
        public void FindBestTools()
        {
            if (bestTools == null)
                bestTools = DefDatabase<ToolType>.AllDefs.ToDictionary<ToolType, ToolType, Tool>(t => t, t => null);
            bestToolThings = Utility.AllToolDefs.ToDictionary<ThingDef, ThingDef, Tool>(t => t, t => null);
            foreach (var toolType in DefDatabase<ToolType>.AllDefs)
                FindBestTool(toolType);
            foreach (var tool in useableTools)
            {
                var toolDef = tool.def;
                if (tool.TotalScore > (bestToolThings[toolDef]?.TotalScore ?? 0f))
                    bestToolThings[toolDef] = tool;
            }
            Cached_BestTools = true;
        }
        public void FindBestTool(ToolType toolType)
        {
            var tool = privateBestTool(toolType);
            bestTools[toolType] = tool;
        }
        private Tool privateBestTool(ToolType toolType)
        {
            Tool tool = null;
            float val = 0f;
            foreach (var currTool in useableTools)
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
        public bool ValidateTool(Tool tool)
        {
            if (!tool.IsInAnyStorage())
            {
                UpdateStoredTools();
                return false;
            }
            var faction = Find.FactionManager.OfPlayer;
            if (tool.IsForbidden(faction) || map.reservationManager.IsReservedByAnyoneOf(tool, faction))
            {
                UpdateUseableTools();
                return false;
            }
            return true;
        }
        public Tool BestTool(ToolType toolType)
        {
            var tool = BestTools[toolType];
            if (tool == null)
                return null;
            if (!ValidateTool(tool))
                tool = BestTool(toolType);
            return tool;
        }
        public Tool BestTool(ThingDef def)
        {
            var tool = BestToolThings[def];
            if (tool == null)
                return null;
            if (!ValidateTool(tool))
                tool = BestTool(def);
            return tool;
        }
        public IEnumerable<Tool> StoredToolThings(ThingDef def)
        {
            if (storedToolThings == null)
                UpdateStoredTools();
            var list = storedToolThings[def];
            if (list.NullOrEmpty())
                return list;
            if (list.Any(t => !t.IsInAnyStorage()))
            {
                UpdateStoredTools();
                return StoredToolThings(def);
            }
            return list;
        }
        public Tool ClosestTool(ToolType toolType, IntVec3 pos, Pawn pawn = null)
        {
            var reservation = pawn?.MapHeld.reservationManager;
            var faction = pawn?.Faction;
            Tool tool = null;
            float bestDist = float.MaxValue;
            foreach (var currTool in UseableTools)
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
            var pathFinder = target?.MapHeld?.pathFinder;
            if (pathFinder == null)
                return false;
            if (Settings.opportunisticTakeTool_calcPath)
            {
                var path = pathFinder.FindPath(source, target, TraverseParms.For(TraverseMode.PassDoors, Danger.Some), PathEndMode.Touch);
                bool found = path.Found;
                if (found)
                    dist = path.TotalCost * 2;
                path.ReleaseToPool();
                return found;
            }
            dist = Mathf.Sqrt(source.DistanceToSquared(target.PositionHeld)) * 2;
            return true;
        }
    }
}
