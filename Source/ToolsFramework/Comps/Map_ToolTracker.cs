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
        private HashSet<ToolInfo> storedTools = new HashSet<ToolInfo>();
        public IEnumerable<ThingWithComps> StoredTools => StoredToolInfos.Select(t => t.tool);
        public IEnumerable<ToolInfo> StoredToolInfos => storedTools;
        public HashSet<ToolInfo> StoredToolsList
        {
            get
            {
                Cached_BestTools = false;
                return storedTools;
            }
        }
        private HashSet<ToolInfo> useableTools = new HashSet<ToolInfo>();
        public IEnumerable<ThingWithComps> UseableTools => UseableToolInfos.Select(t => t.tool);
        public IEnumerable<ToolInfo> UseableToolInfos => useableTools;
        public HashSet<ToolInfo> UseableToolsList
        {
            get
            {
                Cached_BestTools = false;
                return useableTools;
            }
        }

        private Dictionary<ToolType, ToolInfo> bestTools;
        private Dictionary<ToolType, ToolInfo> BestTools
        {
            get
            {
                if (!Cached_BestTools || bestTools == null)
                    FindBestTools();
                return bestTools;
            }
        }
        private Dictionary<ThingDef, ToolInfo> bestToolThings;
        private Dictionary<ThingDef, ToolInfo> BestToolThings
        {
            get
            {
                if (!Cached_BestTools || bestToolThings == null)
                    FindBestTools();
                return bestToolThings;
            }
        }
        private Dictionary<ThingDef, List<ToolInfo>> storedToolThings;
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
            useableTools = storedTools.Where(t => !t.tool.IsForbidden(faction) && !reservation.IsReservedByAnyoneOf(t.tool, faction)).ToHashSet() ?? new HashSet<ToolInfo>();
            if (oldList.SetEquals(useableTools))
                return;
            Cached_BestTools = false;
        }
        public void UpdateStoredTools()
        {
            nextUpdateStoredToolsTick = Find.TickManager.TicksGame + Settings.mapTrackerDelay_StoredTools;
            var oldList = storedTools;
            storedTools = new HashSet<ToolInfo>();
            var things = map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways).OfType<ThingWithComps>()?.Where(t => t.IsInAnyStorage());
            foreach (var thing in things)
                if (thing.IsTool(out var comp, false))
                    storedTools.Add(new ToolInfo(thing, comp));
            if (oldList.SetEquals(storedTools))
                return;
            storedToolThings = Utility.AllToolDefs.ToDictionary(t => t, t => new List<ToolInfo>());
            foreach (var info in storedTools)
                storedToolThings[info.tool.def].Add(info);
            UpdateUseableTools();
        }
        public void FindBestTools()
        {
            if (bestTools == null)
                bestTools = DefDatabase<ToolType>.AllDefs.ToDictionary<ToolType, ToolType, ToolInfo>(t => t, t => null);
            bestToolThings = Utility.AllToolDefs.ToDictionary<ThingDef, ThingDef, ToolInfo>(t => t, t => null);
            foreach (var toolType in DefDatabase<ToolType>.AllDefs)
                FindBestTool(toolType);
            foreach (var info in useableTools)
            {
                var toolDef = info.tool.def;
                if (info.comp.TotalScore > (bestToolThings[toolDef]?.comp.TotalScore ?? 0f))
                    bestToolThings[toolDef] = info;
            }
            Cached_BestTools = true;
        }
        public void FindBestTool(ToolType toolType)
            => bestTools[toolType] = privateBestTool(toolType);
        private ToolInfo privateBestTool(ToolType toolType)
        {
            ToolInfo info = null;
            float val = 0f;
            foreach (var currInfo in useableTools)
            {
                float currVal = currInfo.comp[toolType];
                if (currInfo.comp.TryGetValue(toolType, out var baseVal) && baseVal > 1f && currVal > val)
                {
                    info = currInfo;
                    val = currVal;
                }
            }
            return info;
        }
        public bool ValidateTool(ThingWithComps tool)
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
        public ToolInfo BestTool(ToolType toolType)
        {
            var info = BestTools[toolType];
            if (info == null)
                return null;
            if (!ValidateTool(info.tool))
                info = BestTool(toolType);
            return info;
        }
        public ToolInfo BestToolInfo(ThingDef def)
        {
            var info = BestToolThings[def];
            if (info == null)
                return null;
            if (!ValidateTool(info.tool))
                info = BestToolInfo(def);
            return info;
        }
        public ThingWithComps BestTool(ThingDef def) => BestToolInfo(def)?.tool;
        public IEnumerable<ToolInfo> StoredToolThingInfos(ThingDef def)
        {
            if (storedToolThings == null)
                UpdateStoredTools();
            var list = storedToolThings[def];
            if (list.NullOrEmpty())
                return list;
            if (list.Any(t => !t.tool.IsInAnyStorage()))
            {
                UpdateStoredTools();
                return StoredToolThingInfos(def);
            }
            return list;
        }
        public IEnumerable<ThingWithComps> StoredToolThings(ThingDef def)
            => StoredToolThingInfos(def)?.Select(t => t.tool);
        public ToolInfo ClosestToolInfo(ToolType toolType, IntVec3 pos, Pawn pawn = null)
        {
            var reservation = pawn?.MapHeld.reservationManager;
            var faction = pawn?.Faction;
            var flag = pawn.CanUseTools(out var tracker);
            ThingFilter assignmentFilter = null;
            if (flag)
                assignmentFilter = tracker.ToolAssignment.filter;
            ToolInfo info = null;
            float bestDist = float.MaxValue;
            foreach (var currInfo in UseableToolInfos)
            {
                if (pawn != null)
                {
                    if (flag)
                    {
                        if (currInfo.tool.ToolIsForbidden(pawn, assignmentFilter, reservation, faction))
                            continue;
                    }
                    else if (currInfo.tool.ToolIsForbidden(pawn, reservation, faction))
                        continue;
                }
                if (!currInfo.comp.TryGetValue(toolType, out float val) || val < 1f || !Distance(currInfo.tool, pos, out float dist))
                    continue;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    info = currInfo;
                }
            }
            return info;
        }
        public ThingWithComps ClosestTool(ToolType toolType, IntVec3 pos, Pawn pawn = null) => ClosestToolInfo(toolType, pos, pawn)?.tool;
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
