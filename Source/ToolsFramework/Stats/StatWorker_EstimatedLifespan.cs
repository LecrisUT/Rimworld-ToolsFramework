using RimWorld;
using System.Text;
using UnityEngine;
using Verse;

namespace ToolsFramework
{
    public class StatWorker_EstimatedLifespan : StatWorker
    {
        public static int BaseWearInterval => GenDate.TicksPerHour; // Once per hour of continuous work, or ~40 mins with hardcore

        public override bool ShouldShowFor(StatRequest req) =>
            req.BuildableDef.IsTool() && Settings.degradation;

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            Tool tool = req.Thing as Tool;
            return GetBaseEstimatedLifespan(tool, req.BuildableDef, req.StuffDef);
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            Tool tool = req.Thing as Tool;
            return $"{"StatsReport_BaseValue".Translate()}: {GetBaseEstimatedLifespan(tool, req.BuildableDef, req.StuffDef, false).ToString("F1")}";
        }

        private float GetBaseEstimatedLifespan(Tool tool, BuildableDef def, ThingDef stuffDef, bool withWearFactor = true)
        {
            if (!((ThingDef)def).useHitPoints)
                return float.PositiveInfinity;
            var props = def.GetModExtension<ToolProperties>();
            // For def
            if (tool == null)
                return GenDate.TicksToDays(Mathf.RoundToInt(BaseWearInterval * def.GetStatValueAbstract(RimWorld.StatDefOf.MaxHitPoints, stuffDef) /
                    (withWearFactor ? WearFactor(tool, def, stuffDef) : 1f)));

            // For thing
            return GenDate.TicksToDays(Mathf.RoundToInt(BaseWearInterval * tool.MaxHitPoints / (withWearFactor ? WearFactor(tool, def, stuffDef) : 1f)));
        }

        private float WearFactor(Tool tool, BuildableDef def, ThingDef stuffDef)
        {
            if (tool == null)
                return def.GetStatValueAbstract(StatDefOf.ToolWearFactor, stuffDef);
            return tool.GetStatValue(StatDefOf.ToolWearFactor);
        }

        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            StringBuilder finalBuilder = new StringBuilder();
            finalBuilder.AppendLine($"{StatDefOf.ToolWearFactor.LabelCap}: " +
                $"{(1 / WearFactor(req.Thing as Tool, req.BuildableDef, req.StuffDef)).ToStringByStyle(ToStringStyle.FloatTwo, ToStringNumberSense.Factor)}");
            finalBuilder.AppendLine();
            finalBuilder.AppendLine(base.GetExplanationFinalizePart(req, numberSense, finalVal));
            return finalBuilder.ToString();
        }
    }
}
