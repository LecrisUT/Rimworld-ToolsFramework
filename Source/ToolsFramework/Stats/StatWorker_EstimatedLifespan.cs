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
            req.BuildableDef.IsTool() && Settings.toolDegradation;

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            Tool tool = req.Thing as Tool;
            return GetBaseEstimatedLifespan(tool, req.BuildableDef);
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            Tool tool = req.Thing as Tool;
            return $"{"StatsReport_BaseValue".Translate()}: {GetBaseEstimatedLifespan(tool, req.BuildableDef).ToString("F1")}";
        }

        private float GetBaseEstimatedLifespan(Tool tool, BuildableDef def)
        {
            if (!((ThingDef)def).useHitPoints)
                return float.PositiveInfinity;
            var props = def.GetModExtension<ToolProperties>();
            // For def
            if (tool == null)
                return GenDate.TicksToDays(Mathf.RoundToInt((BaseWearInterval * def.GetStatValueAbstract(StatDefOf.MaxHitPoints)) / def.GetStatValueAbstract(Tools_StatDefOf.ToolWearFactor)));

            // For thing
            return GenDate.TicksToDays(Mathf.RoundToInt((BaseWearInterval * tool.MaxHitPoints) / tool.GetStatValue(Tools_StatDefOf.ToolWearFactor)));
        }

        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            StringBuilder finalBuilder = new StringBuilder();
            finalBuilder.AppendLine($"{"Settings_ToolDegradationRate".Translate()}: " +
                $"{(1 / Settings.toolDegradationFactor).ToStringByStyle(ToStringStyle.FloatTwo, ToStringNumberSense.Factor)}");
            finalBuilder.AppendLine();
            finalBuilder.AppendLine(base.GetExplanationFinalizePart(req, numberSense, finalVal));
            return finalBuilder.ToString();
        }
    }
}
