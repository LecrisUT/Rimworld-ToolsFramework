using RimWorld;
using Verse;

namespace ToolsFramework
{
    public class ToolUse : ThingComp
    {
        private Tool Tool => (Tool)parent;
        public int workTicksDone = 0;
        private bool inUse = false;
        public bool InUse
        {
            get => inUse;
            set => inUse = value;
        }
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
        }
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref workTicksDone, "workTicksDone", 0);
            Scribe_Values.Look(ref inUse, "inUse", false);
        }
        public virtual void Use()
        {
            if (workTicksDone++ < 0)
                workTicksDone = 0;
            if (Settings.degradation && Tool.def.useHitPoints && workTicksDone % Tool.WorkTicksToDegrade == 0)
                Degrade(Tool, HoldingPawn);
        }
        private void Degrade(Thing item, Pawn pawn)
        {
            item.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1));
        }
    }
}
