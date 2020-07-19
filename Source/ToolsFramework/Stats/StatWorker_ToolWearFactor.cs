using RimWorld;

namespace ToolsFramework
{
    public class StatWorker_ToolWearFactor : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req) =>
            req.BuildableDef.IsTool() && Settings.toolDegradation;

        public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
        {
            val *= Settings.toolDegradationFactor;
            base.FinalizeValue(req, ref val, applyPostProcess);
        }
    }
}
