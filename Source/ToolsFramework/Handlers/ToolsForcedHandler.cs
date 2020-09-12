using System.Collections.Generic;
using Verse;

namespace ToolsFramework
{
    public class ToolsForcedHandler : IExposable
    {
        private HashSet<Tool> forcedTools = new HashSet<Tool>();
        public bool SomethingIsForced => !forcedTools.EnumerableNullOrEmpty();
        public HashSet<Tool> ForcedTools => forcedTools;
        public void Reset() => forcedTools.Clear();
        public void SetForced(Tool tool, bool forced)
        {
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
