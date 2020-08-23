using RimWorld;
using System.Text;
using Verse;

namespace ToolsFramework
{
    public class StatPart_StuffEffect : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (GetMultiplier(req, out float mod))
                val *= mod;
        }
        public override string ExplanationPart(StatRequest req)
        {
            if (!GetMultiplier(req, out float mod))
                return null;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("StatsReport_Material".Translate() + " (" + req.StuffDef.label + "): x" + mod.ToStringPercent("F0"));
            return stringBuilder.ToString();
        }
        private bool GetMultiplier(StatRequest req, out float mod)
        {
            mod = 0f;
            if (!(req.StuffDef is ThingDef stuffDef) || !(req.Def is ToolType toolType) || toolType.efficiencyModifiers.NullOrEmpty())
                return false;
            mod = toolType.GetStuffEfficiency(stuffDef);
            return true;
        }
    }
}
