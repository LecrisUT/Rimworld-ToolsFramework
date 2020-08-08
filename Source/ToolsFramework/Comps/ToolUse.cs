using RimWorld;
using Verse;

namespace ToolsFramework
{
    public class ToolUse : ThingComp
    {
        private Tool tool => (Tool)parent;
        public int workTicksDone = 0;
        public bool inUse = false;
        public bool DrawTool
            => Settings.draw && inUse;
        public Pawn HoldingPawn
        {
            get
            {
                if (ParentHolder is Pawn_EquipmentTracker eq)
                    return eq.pawn;
                if (ParentHolder is Pawn_InventoryTracker inv)
                    return inv.pawn;
                return null;
            }
        }
        public override void CompTick()
        {
            if (inUse)
                Use();
        }
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref workTicksDone, "workTicksDone", 0);
            Scribe_Values.Look(ref inUse, "inUse", false);
        }
        public virtual void Use()
        {
            Log.Message($"Test 0.2: {HoldingPawn} : {tool} : {HoldingPawn.GetComp<Pawn_ToolTracker>().memoryTool.FirstOrFallback()}");
            if (workTicksDone++ < 0)
                workTicksDone = 0;
            if (Settings.degradation && tool.def.useHitPoints && workTicksDone % tool.WorkTicksToDegrade == 0)
                Degrade(tool, HoldingPawn);
        }
        private void Degrade(Thing item, Pawn pawn)
        {
            item.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1));
        }
    }
}
