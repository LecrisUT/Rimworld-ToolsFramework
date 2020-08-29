using RimWorld;

namespace ToolsFramework
{
    public class StatWorker_ToolWearFactor : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req) =>
             Settings.degradation && req.BuildableDef.IsTool();
    }
}
