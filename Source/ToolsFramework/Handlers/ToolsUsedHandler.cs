using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ToolsFramework
{
    public class ToolsUsedHandler : IExposable
    {
        public bool dirtyCache = true;

        private Dictionary<ToolType, Tool> bestTool = new Dictionary<ToolType, Tool>(DefDatabase<ToolType>.DefCount);
        public IEnumerable<Tool> UsedTools => BestTool.Values;
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
        private void Update()
        {
            bestTool = new Dictionary<ToolType, Tool>();
            var prevVal = new Dictionary<ToolType, float>(DefDatabase<ToolType>.DefCount);
            DefDatabase<ToolType>.AllDefs.Do(t => prevVal.Add(t, 0f));
            foreach (var currTool in heldTools)
            {
                var currProp = currTool.ToolProperties;
                foreach (var toolType in currTool.ToolProperties.toolTypesValue.Select(t=>t.toolType))
                {
                    var currVal = currTool.GetValue(toolType);
                    if (currVal > 1f && currVal > prevVal[toolType])
                        bestTool.SetOrAdd(toolType, currTool);
                }
            }
            dirtyCache = false;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref bestTool, "bestTool", LookMode.Reference, LookMode.Reference);
            Scribe_Collections.Look(ref heldTools, "heldTools", LookMode.Reference);
        }
    }
}
