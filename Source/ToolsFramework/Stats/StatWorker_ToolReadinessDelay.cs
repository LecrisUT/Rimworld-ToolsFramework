﻿using RimWorld;
using System.Text;
using Verse;

namespace ToolsFramework
{
    public class StatWorker_ToolReadinessDelay : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req) =>
            req.BuildableDef.IsTool() && Settings.equipDelay;

        public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
        {
            val *= Settings.equipDelayFactor;
            base.FinalizeValue(req, ref val, applyPostProcess);
        }

        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{"TF_equipDelayFactor".Translate()}: " +
                $"{Settings.equipDelayFactor.ToStringByStyle(ToStringStyle.FloatTwo, ToStringNumberSense.Factor)}");
            builder.AppendLine();
            builder.AppendLine(base.GetExplanationFinalizePart(req, numberSense, finalVal));
            return builder.ToString();
        }
    }
}
