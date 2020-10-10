using RimWorld;
using System.Text;
using UnityEngine;
using Verse;

namespace ToolsFramework
{
    public class StatWorker_EstimatedLifespan : StatWorker
    {
        public static int BaseWearInterval => GenDate.TicksPerHour; // Once per hour of continuous work, or ~40 mins with hardcore

        public override bool ShouldShowFor(StatRequest req)
            => Settings.degradation && (req.HasThing ? req.Thing.IsTool() : req.BuildableDef.IsTool());

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            return GetBaseEstimatedLifespan(req.Thing, req.BuildableDef, req.StuffDef);
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            return $"{"StatsReport_BaseValue".Translate()}: {GetBaseEstimatedLifespan(req.Thing, req.BuildableDef, req.StuffDef, false).ToString("F1")}";
        }

        private float GetBaseEstimatedLifespan(Thing thing, BuildableDef def, ThingDef stuffDef, bool withWearFactor = true)
        {
            if (!((ThingDef)def).useHitPoints)
                return float.PositiveInfinity;
            // For def
            if (thing == null)
                return GenDate.TicksToDays(Mathf.RoundToInt(BaseWearInterval * def.GetStatValueAbstract(RimWorld.StatDefOf.MaxHitPoints, stuffDef) /
                    (withWearFactor ? WearFactor(null, def, stuffDef) : 1f)));

            // For thing
            return GenDate.TicksToDays(Mathf.RoundToInt(BaseWearInterval * thing.MaxHitPoints / (withWearFactor ? WearFactor(thing, def, stuffDef) : 1f)));
        }

        private float WearFactor(Thing thing, BuildableDef def, ThingDef stuffDef)
        {
            if (thing == null)
                return def.GetStatValueAbstract(StatDefOf.ToolWearFactor, stuffDef);
            return thing.GetStatValue(StatDefOf.ToolWearFactor);
        }

        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            StringBuilder finalBuilder = new StringBuilder();
            finalBuilder.AppendLine($"{StatDefOf.ToolWearFactor.LabelCap}: " +
                $"{(1 / WearFactor(req.Thing, req.BuildableDef, req.StuffDef)).ToStringByStyle(ToStringStyle.FloatTwo, ToStringNumberSense.Factor)}");
            finalBuilder.AppendLine();
            finalBuilder.AppendLine(base.GetExplanationFinalizePart(req, numberSense, finalVal));
            return finalBuilder.ToString();
        }
    }
}
