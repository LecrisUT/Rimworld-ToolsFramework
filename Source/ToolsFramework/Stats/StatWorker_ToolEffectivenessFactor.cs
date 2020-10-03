using RimWorld;

namespace ToolsFramework
{
    public class StatWorker_ToolEffectivenessFactor : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
            => req.Thing.IsTool() && req.Def is ToolType;
    }
}
