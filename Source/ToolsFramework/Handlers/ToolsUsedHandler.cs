using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ToolsFramework
{
    public class ToolsUsedHandler
    {
        public ToolsUsedHandler(Pawn pawn)
        {
            this.pawn = pawn;
        }
        private Pawn pawn;
        public bool dirtyCache = true;

        private Dictionary<ToolType, Tool> bestTool = ToolType.allToolTypes.ToDictionary<ToolType, ToolType, Tool>(t => t, t => null);
        public IEnumerable<Tool> UsedTools => BestTool.Values.Where(t => !t.DestroyedOrNull());
        public Dictionary<ToolType, Tool> BestTool
        {
            get
            {
                if (dirtyCache)
                    Update();
                return bestTool;
            }
        }
        private List<Tool> heldTools;
        public List<Tool> HeldToolsList
        {
            get
            {
                if (heldTools == null)
                    UpdateHeldTools();
                dirtyCache = true;
                return heldTools;
            }
        }
        public IEnumerable<Tool> HeldTools
        {
            get
            {
                if (heldTools == null)
                    UpdateHeldTools();
                return heldTools;
            }
        }
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
            foreach (var currTool in HeldTools)
                foreach (var toolType in currTool.ToolTypes)
                {
                    var currVal = currTool.GetValue(toolType);
                    if (currVal > 1f && currVal > bestTool[toolType].GetValue(toolType, 0f))
                        bestTool[toolType] = currTool;
                }
            dirtyCache = false;
        }
        public void UpdateHeldTools()
        {
            heldTools = new List<Tool>();
            var list = pawn.equipment?.AllEquipmentListForReading.Where(t => t is Tool);
            if (!list.EnumerableNullOrEmpty())
                foreach (var t in list)
                    heldTools.Add((Tool)t);
            var list2 = pawn.inventory.innerContainer.Where(t => t is Tool);
            if (!list2.EnumerableNullOrEmpty())
                foreach (var t in list2)
                    heldTools.Add((Tool)t);
        }
    }
}
