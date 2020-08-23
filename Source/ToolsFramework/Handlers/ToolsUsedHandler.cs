using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ToolsFramework
{
    public class ToolsUsedHandler : IExposable
    {
        public bool dirtyCache = true;

        private Dictionary<ToolType, Tool> bestTool = ToolType.allToolTypes.ToDictionary<ToolType, ToolType, Tool>(t => t, t => null);
        public IEnumerable<Tool> UsedTools => BestTool.Values.Where(t => t != null);
        public Dictionary<ToolType, Tool> BestTool
        {
            get
            {
                if (dirtyCache)
                    Update();
                return bestTool;
            }
        }
        private List<Tool> heldTools = new List<Tool>();
        public List<Tool> HeldToolsList
        {
            get
            {
                dirtyCache = true;
                return heldTools;
            }
        }
        public IEnumerable<Tool> HeldTools => heldTools;
        public int HeldToolsCount
        {
            get
            {
                if (dirtyCache)
                    Update();
                return heldTools.Count;
            }
        }
        private void Update()
        {
            bestTool = ToolType.allToolTypes.ToDictionary<ToolType, ToolType, Tool>(t => t, t => null);
            foreach (var currTool in heldTools)
                foreach (var toolType in currTool.ToolTypes)
                {
                    var currVal = currTool.GetValue(toolType);
                    if (currVal > 1f && currVal > bestTool[toolType].GetValue(toolType, 0f))
                        bestTool[toolType] = currTool;
                }
            dirtyCache = false;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref heldTools, "heldTools", LookMode.Reference);
        }
    }
}
