using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ToolsFramework
{
    public class ToolAssignment_Database : GameComponent
    {
        public ToolAssignment_Database(Game game)
        {
        }

        public override void FinalizeInit()
        {
            if (!initialized)
            {
                GenerateStartingToolAssignments();
                initialized = true;
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref initialized, "initialized", false);
            Scribe_Collections.Look(ref assignments, "assignments", LookMode.Deep, new object[0]);
        }

        public List<ToolAssignment> AllAssignments => assignments;

        public ToolAssignment DefaultAssignment()
        {
            var assignment = assignments.Count == 0 ? MakeNewToolAssignment() : assignments[0];
            return assignment;
        }

        public AcceptanceReport TryDelete(ToolAssignment toolAssignment)
        {
            foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
                if (pawn.CanUseTools(out var tracker) && tracker.ToolAssignment == toolAssignment)
                    return new AcceptanceReport("ToolAssignmentInUse".Translate(pawn));
            foreach (Pawn pawn2 in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
                if (pawn2.CanUseTools(out var tracker2) &&
                    tracker2.ToolAssignment == toolAssignment)
                    tracker2.ToolAssignment = null;
            assignments.Remove(toolAssignment);
            return AcceptanceReport.WasAccepted;
        }
        public ToolAssignment MakeNewToolAssignment()
        {
            int uniqueId = assignments.Any() ? assignments.Max(a => a.uniqueId) + 1 : 1;
            var toolAssignment = new ToolAssignment(uniqueId, $"{"ToolAssignment".Translate()} {uniqueId}");
            toolAssignment.filter.SetAllow(ThingCategoryDefOf.Tools, true);
            assignments.Add(toolAssignment);
            return toolAssignment;
        }

        private void GenerateStartingToolAssignments()
        {
            var anything = MakeNewToolAssignment();
            anything.label = "OutfitAnything".Translate();

            var nothing = MakeNewToolAssignment();
            nothing.label = "FoodRestrictionNothing".Translate();
            nothing.filter.SetDisallowAll(null, null);

            foreach (var category in ThingCategoryDefOf.Tools.childCategories)
            {
                var assingment = MakeNewToolAssignment();
                assingment.label = category.LabelCap;
                assingment.filter.SetDisallowAll();
                foreach (var tDef in Utility.AllToolDefs.Where(t => t.GetModExtension<ToolProperties>()?.ToolTypes.Any(tt => tt.defaultToolAssignmentTags.Contains(category)) ?? false))
                        assingment.filter.SetAllow(tDef, true);
            }

        }

        private bool initialized = false;
        private List<ToolAssignment> assignments = new List<ToolAssignment>();
    }
}