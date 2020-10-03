using System.Collections.Generic;
using Verse;

namespace ToolsFramework
{
    public class ToolsForcedHandler : IExposable
    {
        private HashSet<ThingWithComps> forcedTools = new HashSet<ThingWithComps>();
        public bool SomethingIsForced => !forcedTools.EnumerableNullOrEmpty();
        public HashSet<ThingWithComps> ForcedTools => forcedTools;
        public void Reset() => forcedTools.Clear();
        public void SetForced(ThingWithComps tool, bool forced, bool checkIfTool = true)
        {
            if (checkIfTool && !tool.IsTool())
            {
                if (forcedTools.Contains(tool))
                    forcedTools.Remove(tool);
                return;
            }
            if (forced && !forcedTools.Contains(tool))
                forcedTools.Add(tool);
            else if (!forced && forcedTools.Contains(tool))
                forcedTools.Remove(tool);
        }
        public void ExposeData()
        {
            Scribe_Collections.Look(ref forcedTools, "forcedTools", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
                forcedTools.RemoveWhere(t => t.DestroyedOrNull());
        }
    }
}
