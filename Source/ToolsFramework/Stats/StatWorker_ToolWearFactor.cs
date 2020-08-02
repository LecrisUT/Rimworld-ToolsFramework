using RimWorld;
using System.Linq;
using System.Text;
using Verse;

namespace ToolsFramework
{
    public class StatWorker_ToolWearFactor : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req) =>
            req.BuildableDef.IsTool() && Settings.degradation;

        public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
        {
            var tool = (Tool)req.Thing;
            val *= Settings.degradationFactor;
            base.FinalizeValue(req, ref val, applyPostProcess);
            if (tool.HoldingPawn is Pawn pawn && pawn.CanUseTools(out var tracker))
            {
                var count = tracker.usedHandler.HeldTools.Count();
                val *= count;
            }
        }

        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{"TF_degredationFactor".Translate()}: " +
                $"{Settings.degradationFactor.ToStringByStyle(ToStringStyle.FloatTwo, ToStringNumberSense.Factor)}");
            builder.AppendLine();
            builder.AppendLine(base.GetExplanationFinalizePart(req, numberSense, finalVal));
            return builder.ToString();
        }
    }
}
