using RimWorld;
using Verse;

namespace ToolsFramework
{
    public interface ITool
    {
        ToolDef ToolDef { get; }
        ToolComp ToolComp { get; }
    }
    public class Tool : ThingWithComps , ITool
    {
        public ToolDef ToolDef => def as ToolDef;
        private ToolComp toolComp;
        public ToolComp ToolComp
        {
            get
            {
                if (toolComp == null)
                    toolComp = GetComp<ToolComp>();
                return toolComp;
            }
        }
        public override string LabelNoCount
        {
            get
            {
                string label = base.LabelNoCount;
                if (ToolComp.InUse)
                    label = "InUse".Translate() + label;
                if (this.IsForced())
                    label += $" ({"ApparelForcedLower".Translate()})";
                return label;
            }
        }
    }
    public class ToolApparel : Apparel, ITool
    {
        public ToolDef ToolDef => def as ToolDef;
        private ToolComp toolComp;
        public ToolComp ToolComp
        {
            get
            {
                if (toolComp == null)
                    toolComp = GetComp<ToolComp>();
                return toolComp;
            }
        }
        public override string LabelNoCount
        {
            get
            {
                string label = base.LabelNoCount;
                if (ToolComp.InUse)
                    label = "InUse".Translate() + label;
                if (this.IsForced())
                    label += $" ({"ApparelForcedLower".Translate()})";
                return label;
            }
        }
    }
}
