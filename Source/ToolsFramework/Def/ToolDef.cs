using System.Collections.Generic;
using Verse;

namespace ToolsFramework
{
    public class ToolDef : ThingDef
    {
        private CompProperties_Tool compProp;
        public CompProperties_Tool CompProp
        {
            get
            {
                if (compProp == null)
                    compProp = GetCompProperties<CompProperties_Tool>();
                return compProp;
            }
        }
        public IEnumerable<ToolType> ToolTypes => CompProp.ToolTypes;
        public int ToolTypesCount => CompProp.toolTypesValue.Count;
        /*public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (var stat in base.SpecialDisplayStats(req))
                yield return stat;
            var defaultTool = new Tool() { def = this };
            defaultTool.SetStuffDirect(req.StuffDef);
            foreach (var toolType in ToolTypes)
                yield return Utility.ToolTypeDrawEntry(defaultTool, toolType, defaultTool.GetStatValue(toolType, StatDefOf.ToolEffectivenessFactor));
        }*/
    }
}
