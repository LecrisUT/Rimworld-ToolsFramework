using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ToolsFramework
{
    public class Tool : ThingWithComps
    {
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
        private ToolProperties toolProperties;
        public ToolProperties ToolProperties
        {
            get
            {
                if (toolProperties == null)
                    toolProperties = def.GetModExtension<ToolProperties>();
                return toolProperties;
            }
        }
        private ToolUse toolUse;
        public ToolUse ToolUse
        {
            get
            {
                if (toolUse == null)
                    toolUse = GetComp<ToolUse>();
                return toolUse;
            }
        }

        public int WorkTicksToDegrade => Mathf.FloorToInt(
                (this.GetStatValue(Tools_StatDefOf.ToolEstimatedLifespan) * GenDate.TicksPerDay) / MaxHitPoints);


        public override string LabelNoCount
        {
            get
            {
                string label = base.LabelNoCount;
                if (toolUse.inUse)
                    label = $"{"ToolInUse".Translate()}: " + label;
                return label;
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            foreach (var modifier in def.GetModExtension<ToolProperties>().toolTypesValue)
                yield return Utility.ToolTypeDrawEntry(this, modifier.toolType, modifier.value);
        }
    }
}
