using RimWorld;

namespace ToolsFramework
{
    public class StatWorker_ToolEffectivenessFactor : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
            => req.Thing is Tool && req.Def is ToolType;
        /*public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
        {
            base.FinalizeValue(req, ref val, applyPostProcess);
        }*/
    }
}
