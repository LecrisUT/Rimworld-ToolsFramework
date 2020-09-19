using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace ToolsFramework
{
    public class StatPart_Settings : StatPart
    {
        [Obsolete]
        public static Dictionary<StatDef, float> Settings => Dictionaries.StatPartSettings;
        public override void TransformValue(StatRequest req, ref float val)
            => val *= Dictionaries.StatPartSettings[parentStat];
        public override string ExplanationPart(StatRequest req)
        {
            var val = Dictionaries.StatPartSettings[parentStat];
            if (val == 1f)
                return null;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Settings".Translate() + ": x" + val.ToStringPercent("F0"));
            return stringBuilder.ToString();
        }
    }
}
