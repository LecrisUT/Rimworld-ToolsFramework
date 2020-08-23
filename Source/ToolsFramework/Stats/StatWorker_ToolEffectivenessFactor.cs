using RimWorld;
using Verse;

namespace ToolsFramework
{
    public class StatWorker_ToolEffectivenessFactor : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
            => req.Thing is Tool && req.Def is ToolType;
    }
}
