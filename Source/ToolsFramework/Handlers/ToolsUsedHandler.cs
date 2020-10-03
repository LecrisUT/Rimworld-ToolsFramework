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

        private Dictionary<ToolType, ToolInfo> bestTool = ToolType.allToolTypes.ToDictionary<ToolType, ToolType, ToolInfo>(t => t, t => null);
        public IEnumerable<ThingWithComps> UsedTools => UsedToolInfos.Select(t => t.tool);
        public IEnumerable<ToolInfo> UsedToolInfos => BestTool.Values.Where(t => t != null && !t.tool.DestroyedOrNull());
        public Dictionary<ToolType, ToolInfo> BestTool
        {
            get
            {
                if (dirtyCache)
                    Update();
                return bestTool;
            }
        }
        private List<ToolInfo> heldTools;
        public List<ToolInfo> HeldToolsList
        {
            get
            {
                if (heldTools == null)
                    UpdateHeldTools();
                dirtyCache = true;
                return heldTools;
            }
        }
        public IEnumerable<ThingWithComps> HeldTools
        {
            get
            {
                if (heldTools == null)
                    UpdateHeldTools();
                return heldTools.Select(t => t.tool);
            }
        }
        public IEnumerable<ToolInfo> HeldToolInfos
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
            bestTool = ToolType.allToolTypes.ToDictionary<ToolType, ToolType, ToolInfo>(t => t, t => null);
            foreach (var currInfo in HeldToolInfos)
            {
                foreach (var toolType in currInfo.comp.ToolTypes)
                {
                    var currVal = currInfo.comp.GetValue(toolType);
                    if (currVal > 1f)
                    {
                        var info = bestTool[toolType];
                        if (currVal > (info?.comp.GetValue(toolType, 0f) ?? 0f))
                            bestTool[toolType] = currInfo;
                    }
                }
            }
            dirtyCache = false;
        }
        public void UpdateHeldTools()
        {
            heldTools = new List<ToolInfo>();
            var equipment = pawn.equipment?.AllEquipmentListForReading;
            if (!equipment.NullOrEmpty())
                foreach (var thing in equipment)
                    if (thing.IsTool(out var comp, true))
                        heldTools.Add(new ToolInfo(thing, comp));
            foreach (var thing in pawn.inventory.innerContainer.OfType<ThingWithComps>())
                if (thing.IsTool(out var comp, false))
                    heldTools.Add(new ToolInfo(thing, comp));
        }
    }
}
