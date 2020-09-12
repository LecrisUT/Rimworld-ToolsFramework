using RimWorld;
using System.Text;
using Verse;

namespace ToolsFramework
{
    public class StatWorker_ToolReadinessDelay : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req) =>
            Settings.equipDelay && req.BuildableDef.IsTool();

        public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
        {
            val *= LoadOutEffect((Tool)req.Thing);
            base.FinalizeValue(req, ref val, applyPostProcess);
        }
        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{"TF_LoadoutToolReadiness".Translate()}: " +
                $"{LoadOutEffect((Tool)req.Thing).ToStringByStyle(ToStringStyle.FloatTwo, ToStringNumberSense.Factor)}");
            builder.AppendLine();
            builder.AppendLine(base.GetExplanationFinalizePart(req, numberSense, finalVal));
            return builder.ToString();
        }
        private float LoadOutEffect(Tool tool)
        {
            if (tool == null || !(tool.HoldingPawn is Pawn pawn) || !pawn.CanUseTools(out var tracker))
                return 1f;
            var count = tracker.UsedHandler.HeldToolsCount;
            return Settings.ToolReadinessCurve.Evaluate(count);
        }
    }
}
