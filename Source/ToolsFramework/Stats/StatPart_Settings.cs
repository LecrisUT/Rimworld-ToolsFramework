using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace ToolsFramework
{
    public class StatPart_Settings : StatPart
    {
        public static Dictionary<StatDef, float> Settings = new Dictionary<StatDef, float>();
        public override void TransformValue(StatRequest req, ref float val)
            => val *= Settings[parentStat];
        public override string ExplanationPart(StatRequest req)
        {
            var val = Settings[parentStat];
            if (val == 1f)
                return null;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Settings".Translate() + ": x" + val.ToStringPercent("F0"));
            return stringBuilder.ToString();
        }
    }
}
