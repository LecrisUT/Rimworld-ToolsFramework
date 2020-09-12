using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ToolsFramework
{
    public class Tool : ThingWithComps
    {
        public ToolDef ToolDef => def as ToolDef;
        public Pawn HoldingPawn => ParentHolder?.ParentHolder as Pawn;
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
        public ToolProperties ToolProperties => ToolDef?.ToolProperties ?? def.GetModExtension<ToolProperties>();
        public IEnumerable<ToolType> ToolTypes => ToolDef?.ToolTypes ?? ToolProperties?.ToolTypes;
        public int ToolTypesCount => ToolDef?.ToolTypesCount ?? ToolProperties?.toolTypesValue.Count ?? 0;

        private int workTicksToDegrade = -1;
        public int WorkTicksToDegrade
        {
            get
            {
                if (workTicksToDegrade > 0)
                    goto Ret;
                if (!def.useHitPoints)
                {
                    workTicksToDegrade = int.MaxValue;
                    goto Ret;
                }
                workTicksToDegrade = Mathf.FloorToInt(this.GetStatValue(StatDefOf.ToolEstimatedLifespan) * GenDate.TicksPerDay / MaxHitPoints);
            Ret:
                return workTicksToDegrade;
            }
        }
        public float LifeSpan => def.useHitPoints ? 10f : this.GetStatValue(StatDefOf.ToolEstimatedLifespan) * ((float)HitPoints / MaxHitPoints);
        private float baseTotalScore = -1f;
        public float TotalScore
        {
            get
            {
                if (baseTotalScore < 0)
                {
                    baseTotalScore = 0f;
                    foreach (var tooltype in ToolTypes)
                        baseTotalScore += this.GetValue(tooltype);
                    baseTotalScore /= Mathf.Pow(ToolTypesCount, Settings.ToolTotalScorePow);
                }
                return baseTotalScore * Settings.LifespanScoreCurve.Evaluate(LifeSpan);
            }
        }
        public float this[ToolType toolType]
        {
            get => !ToolTypes.Contains(toolType) ? 0f : this.GetValue(toolType) * Settings.LifespanScoreCurve.Evaluate(LifeSpan);
        }

        public override string LabelCapNoCount => Label;

        public override string Label
        {
            get
            {
                string label = base.LabelNoCount;
                if (ToolUse.InUse)
                    label = "InUse".Translate() + label;
                if (this.IsForced())
                    label += $" ({"ApparelForcedLower".Translate()})";
                return label;
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            foreach (var toolType in ToolTypes)
                yield return Utility.ToolTypeDrawEntry(this, toolType, this.GetStatValue(toolType, StatDefOf.ToolEffectivenessFactor));
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            if (!HoldingPawn.CanUseTools(out var tracker))
                return;
            tracker.UsedHandler.HeldToolsList.Remove(this);
            tracker.forcedHandler.ForcedTools.Remove(this);
        }
    }
}
