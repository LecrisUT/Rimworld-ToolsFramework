using RimWorld;
using System.Text;
using System.Collections.Generic;
using Verse;

namespace ToolsFramework
{
    public class StatPart_Tool : StatPart
    {
        public List<ToolType> ToolTypes = new List<ToolType>();
        public Dictionary<Pawn, (float factor, float offset, int resetTick)> CachedRequests = new Dictionary<Pawn, (float, float, int)>();
        public override void TransformValue(StatRequest req, ref float val)
        {
            var pawn = req.Thing as Pawn ?? req.Pawn;
            if (pawn == null || !pawn.CanUseTools(out var tracker))
                return;
#if DEBUG
            var orig = val;
            TryTransform(pawn, tracker, ref val);
            Log.Message($"[[LC]ToolsFramework] {pawn} : {pawn?.CurJobDef}\n{tracker?.toolInUse} -> {orig} : {val}");
#else
            TryTransform(pawn, tracker, ref val);
#endif
        }
        private void TryTransform(Pawn pawn, Pawn_ToolTracker tracker, ref float val)
        {
            float factor;
            float offset;
            if (tracker.toolInUse == null)
            {
                FallBack(ref val, pawn, tracker);
                return;
            }
            if (CachedRequests.TryGetValue(pawn, out var values) && values.resetTick - Find.TickManager.TicksGame > 0)
            {
                factor = values.factor;
                offset = values.offset;
            }
            else
            {
                var resetTick = Find.TickManager.TicksGame + Settings.ResetStatPartTick;
                var toolType = GetToolType(pawn, tracker, out var def);
                if (toolType == null)
                {
                    factor = 1f;
                    offset = 0f;
                }
                else
                    switch (def)
                    {
                        case JobDef jobDef:
                            tracker.toolInUse.TryGetToolValue(jobDef, parentStat, out factor, out offset, toolType);
                            break;
                        case ThingDef billGiver:
                            tracker.toolInUse.TryGetToolValue(billGiver, parentStat, out factor, out offset, toolType);
                            break;
                        default:
                            throw new System.Exception("Unknown def");
                    }
#if DEBUG
                Log.Message($"[[LC]ToolsFramework] Cached value: {pawn} : {toolType} : {def} : {parentStat}\n+{offset} : x{factor} : {resetTick}");
#endif
                CachedRequests.Add(pawn, (factor, offset, resetTick));
            }
            val = (val + offset) * factor;
        }
        public ToolType GetToolType(Pawn pawn, Pawn_ToolTracker tracker, out Def def)
        {
            ToolType toolType;
            var jobDef = pawn.CurJobDef;
            if (jobDef == RimWorld.JobDefOf.DoBill)
            {
                var billGiver = pawn.CurJob.targetA.Thing.def;
                def = billGiver;
                if (!Dictionaries.billGiverToolType.TryGetValue(pawn.CurJob.targetA.Thing.def, out toolType))
                    return null;
                return toolType;
            }
            def = jobDef;
            if (!Dictionaries.jobStatToolType.TryGetValue((jobDef, parentStat), out toolType))
                return null;
            return toolType;
        }
        public override string ExplanationPart(StatRequest req)
        {
            var pawn = req.Thing as Pawn ?? req.Pawn;
            if (pawn == null || !pawn.CanUseTools(out var tracker))
                return null;
            var currToolType = GetToolType(pawn, tracker, out _);
            var builder = new StringBuilder("ToolEffect".Translate().CapitalizeFirst());
            builder.AppendLine();
            foreach (var toolType in ToolTypes)
            {
                var info = tracker.UsedHandler.BestTool[toolType];
                if (currToolType == toolType)
                    builder.Append("InUse".Translate());
                ReportText(ref builder, pawn, info, toolType);
            }
            return builder.ToString();
        }
        private void FallBack(ref float val, Pawn pawn, Pawn_ToolTracker tracker) { }
        private void ReportText(ref StringBuilder stringBuilder, Pawn pawn, ToolInfo info, ToolType toolType)
        {
            stringBuilder.Append("  " + toolType.LabelCap);
            if (info == null)
            {
                stringBuilder.AppendLine(" [" + "NoTool".Translate() + "]");
                return;
            }
            info.comp.TryGetValue(toolType, parentStat, out var factor, out var offset);
            stringBuilder.AppendLine(" [" + info.tool.LabelCap + "]: ( val + " + offset.ToStringPercent("F2") + " ) x " + factor.ToStringPercent("F2"));
            var jobBonus = info.comp.CompProp.jobBonus;
            if (jobBonus.NullOrEmpty())
                stringBuilder.AppendLine("  [" + "NoJobBonus".Translate() + "]");
            else
            {
                stringBuilder.AppendLine("  " + "JobBonus".Translate() + ":");
                foreach (var bonus in jobBonus)
                    stringBuilder.AppendLine("    " + bonus.job + ": x" + bonus.value);
            }
        }
    }
}
