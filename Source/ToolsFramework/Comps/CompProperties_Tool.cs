using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ToolsFramework
{
    public class CompProperties_Tool : CompProperties
    {
        public CompProperties_Tool() { compClass = typeof(ToolComp); }
        public override void PostLoadSpecial(ThingDef parent)
        {
            if (parent.statBases.NullOrEmpty())
                parent.statBases = new List<StatModifier> { new StatModifier() { stat = StatDefOf.ToolEffectivenessFactor, value = 1f } };
            else if (!parent.statBases.StatListContains(StatDefOf.ToolEffectivenessFactor))
                parent.statBases.Add(new StatModifier() { stat = StatDefOf.ToolEffectivenessFactor, value = 1f });
        }
        public readonly List<ToolTypeModifier> toolTypesValue = new List<ToolTypeModifier>();
        public readonly List<JobModifier> jobBonus = new List<JobModifier>();
        public readonly List<BillGiverModifier> billGiverBonus = new List<BillGiverModifier>();
        public List<StatDef> statsAffected;
        public IEnumerable<ToolType> ToolTypes => toolTypesValue.Select(t => t.toolType);
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            if (!req.HasThing)
            {
                // Temporary solution untill StatRequest is expanded
                var defaultTool = ThingMaker.MakeThing((ThingDef)req.Def, req.StuffDef) as ThingWithComps;
                foreach (var toolType in ToolTypes)
                    yield return Utility.ToolTypeDrawEntry(defaultTool, toolType, defaultTool.GetStatValue(toolType, StatDefOf.ToolEffectivenessFactor));
                defaultTool.Destroy();
            }
        }
    }
}
