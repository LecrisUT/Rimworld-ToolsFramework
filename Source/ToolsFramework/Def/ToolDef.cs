using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ToolsFramework
{
    public class ToolDef : ThingDef
    {
        private ToolProperties toolProperties;
        public ToolProperties ToolProperties
        {
            get
            {
                if (toolProperties == null)
                    toolProperties = GetModExtension<ToolProperties>();
                return toolProperties;
            }
        }
        public IEnumerable<ToolType> ToolTypes => ToolProperties.ToolTypes;
        public int ToolTypesCount => ToolProperties.toolTypesValue.Count;
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (var stat in base.SpecialDisplayStats(req))
                yield return stat;
            var defaultTool = new Tool() { def = this };
            defaultTool.SetStuffDirect(req.StuffDef);
            foreach (var toolType in ToolTypes)
                yield return Utility.ToolTypeDrawEntry(defaultTool, toolType, defaultTool.GetStatValue(toolType, StatDefOf.ToolEffectivenessFactor));
        }
    }
}
