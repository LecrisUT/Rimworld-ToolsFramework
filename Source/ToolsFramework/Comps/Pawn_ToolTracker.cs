using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace ToolsFramework
{
    public class Pawn_ToolTracker : ThingComp
    {
        private Pawn Pawn => (Pawn)parent;
        public Pawn_ToolTracker()
        {
            nextOptimizationTick = Find.TickManager.TicksGame;
        }

        public ThingWithComps toolInUse = null;
        public List<ThingWithComps> memoryTool = new List<ThingWithComps>();
        public List<ThingWithComps> memoryEquipment = new List<ThingWithComps>();

        public bool transfering = false;

        public int nextOptimizationTick = 0;
        public bool optimizingTool = false;

        private ToolAssignment toolAssignment;
        public ToolAssignment ToolAssignment
        {
            get
            {
                if (toolAssignment == null)
                    toolAssignment = Current.Game.GetComponent<ToolAssignment_Database>().DefaultAssignment();
                return toolAssignment;
            }
            set => toolAssignment = value;
        }

        private ToolsUsedHandler usedHandler;
        public ToolsUsedHandler UsedHandler
        {
            get
            {
                if (usedHandler == null)
                    usedHandler = new ToolsUsedHandler(Pawn);
                return usedHandler;
            }
        }
        public ToolsForcedHandler forcedHandler = new ToolsForcedHandler();
        public bool dirtyCache_necessaryToolTypes = true;
        private HashSet<ToolType> necessaryToolTypes = new HashSet<ToolType>();
        public IEnumerable<ToolType> NecessaryToolTypes
        {
            get
            {
                if (dirtyCache_necessaryToolTypes)
                    UpdateNecessaryToolTypes();
                return necessaryToolTypes;
            }
        }

        public void UpdateNecessaryToolTypes()
        {
            necessaryToolTypes = new HashSet<ToolType>();
            foreach (var workGiver in Pawn.workSettings.WorkGiversInOrderNormal.Select(t => t.def))
                if (Utility.ToolWorkGivers.TryGetValue(workGiver, out var toolTypes))
                    foreach (var toolType in toolTypes.Where(t => !necessaryToolTypes.Contains(t)))
                        necessaryToolTypes.Add(toolType);
            dirtyCache_necessaryToolTypes = false;
        }
        public override void PostExposeData()
        {
            Scribe_References.Look(ref toolInUse, "toolInUse");
            Scribe_Collections.Look(ref memoryTool, "memoryTool", LookMode.Reference);
            Scribe_Collections.Look(ref memoryEquipment, "memoryEquipment", LookMode.Reference);

            Scribe_Values.Look(ref nextOptimizationTick, "nextOptimizationTick", 0);
            Scribe_Values.Look(ref optimizingTool, "optimizingTool", false);

            Scribe_References.Look(ref toolAssignment, "toolAssignment");

            // Scribe_Deep.Look(ref usedHandler, "usedHandler");
            Scribe_Deep.Look(ref forcedHandler, "forecedHandler");
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                toolInUse = toolInUse.DestroyedOrNull() ? null : toolInUse;
                memoryTool.RemoveAll(t => t.DestroyedOrNull());
                memoryEquipment.RemoveAll(t => t.DestroyedOrNull());
            }
        }
    }
}
