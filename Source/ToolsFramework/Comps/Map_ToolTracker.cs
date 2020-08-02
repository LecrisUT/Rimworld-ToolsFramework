using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ToolsFramework
{
    public class Map_ToolTracker : MapComponent
    {
        public Map_ToolTracker(Map map) : base(map)
        {

        }
        public bool dirtyCache = false;
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
        private Dictionary<ToolType, Tool> bestTools = new Dictionary<ToolType, Tool>(DefDatabase<ToolType>.DefCount);
        public Dictionary<ToolType, Tool> BestTools
        {
            get
            {
                if (dirtyCache)
                    FindBestTools();
                ConfirmTools();
                return bestTools;
            }
        }
        public int tick = 0;
        public override void MapComponentTick()
        {
            if (++tick < 0)
                tick = 0;
            if (Settings.optimization && tick % Settings.mapTrackerDelay == 0)
            {
                UpdateStoredTools();
                tick += Rand.Range(0, Mathf.CeilToInt(Settings.mapTrackerDelay / 1000));
            }
        }
        public void UpdateStoredTools()
            => StoredToolsList = map.listerThings.AllThings?.OfType<Tool>()?.Where(t => t.IsInAnyStorage()).ToList() ?? new List<Tool>();
        public void ConfirmTools()
        {
            if (bestTools.Values.Any(t => !t.IsInAnyStorage()))
            {
                UpdateStoredTools();
                FindBestTools();
            }

        }
        public void FindBestTools()
        {
            bestTools = new Dictionary<ToolType, Tool>(DefDatabase<ToolType>.DefCount);
            foreach (var toolType in DefDatabase<ToolType>.AllDefs)
                FindBestTool(toolType);
        }
        public void FindBestTool(ToolType toolType)
        {
            var tool = BestTool(toolType);
            if (tool != null)
                bestTools.SetOrAdd(toolType, tool);
            else if (bestTools.ContainsKey(toolType))
                bestTools.Remove(toolType);
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
        public override void ExposeData()
        {
            Scribe_Values.Look(ref tick, "tick");
            Scribe_Collections.Look(ref storedTools, "storedTools");
        }
    }
}
