using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ToolsFramework
{
    public class ToolComp : ThingComp
    {
        private ITool Tool => parent as ITool;
        public CompProperties_Tool CompProp => (CompProperties_Tool)props;
        public int workTicksDone = 0;
        private bool inUse = false;
        public bool InUse
        {
            get => inUse;
            set => inUse = value;
        }
        public bool DrawTool
            => Settings.draw && inUse;
        public Pawn HoldingPawn => ParentHolder.HoldingPawn();

        private int workTicksToDegrade = -1;
        public int WorkTicksToDegrade
        {
            get
            {
                if (workTicksToDegrade > 0)
                    goto Ret;
                if (!parent.def.useHitPoints)
                {
                    workTicksToDegrade = int.MaxValue;
                    goto Ret;
                }
                workTicksToDegrade = Mathf.FloorToInt(parent.GetStatValue(StatDefOf.ToolEstimatedLifespan) * GenDate.TicksPerDay / parent.MaxHitPoints);
            Ret:
                return workTicksToDegrade;
            }
        }
        public IEnumerable<ToolType> ToolTypes => Tool?.ToolDef?.ToolTypes ?? CompProp.ToolTypes;
        public int ToolTypesCount => Tool?.ToolDef?.ToolTypesCount ?? CompProp.toolTypesValue.Count;
        public float LifeSpan => parent.def.useHitPoints ? 10f : parent.GetStatValue(StatDefOf.ToolEstimatedLifespan) * ((float)parent.HitPoints / parent.MaxHitPoints);
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
            if (!Settings.degradation || !parent.def.useHitPoints)
                return;
            workTicksDone++;
            if (workTicksDone > WorkTicksToDegrade)
            {
                workTicksDone = 0;
                Degrade(parent, HoldingPawn);
            }
        }
        private void Degrade(Thing item, Pawn pawn)
        {
            item.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1));
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            foreach (var toolType in ToolTypes)
                yield return Utility.ToolTypeDrawEntry(parent, toolType, parent.GetStatValue(toolType, StatDefOf.ToolEffectivenessFactor));
        }
        private (Pawn pawn, Pawn_ToolTracker tracker) tempPawnTracker = (null, null);
        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            // ParentHolder is nullified in PostDestroy
            if (totalDamageDealt < parent.HitPoints || !HoldingPawn.CanUseTools(out var tracker))
                return;
            tempPawnTracker = (HoldingPawn, tracker);
        }
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (tempPawnTracker.pawn == null || tempPawnTracker.tracker == null)
                return;
            tempPawnTracker.tracker.UsedHandler.HeldToolsList.RemoveAll(t => t.tool == parent);
            tempPawnTracker.tracker.forcedHandler.SetForced(parent, false, false);
            var jobDef = tempPawnTracker.pawn.CurJobDef;
            if (jobDef != null && (jobDef == RimWorld.JobDefOf.DoBill || Dictionaries.jobToolType.ContainsKey(jobDef)))
                tempPawnTracker.pawn.jobs.EndCurrentJob(JobCondition.InterruptOptional, true, true);
        }
    }
}
