using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ToolsFramework
{
    public sealed class PawnColumnWorker_ToolAssignment : PawnColumnWorker
    {
        public override void DoHeader(Rect rect, PawnTable table)
        {
            base.DoHeader(rect, table);
            Rect rect2 = new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f);
            if (Widgets.ButtonText(rect2, "ManageToolAssignments".Translate(), true, false, true))
            {
                Find.WindowStack.Add(new Dialog_ManageToolAssignments(null));
            }
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (!pawn.CanUseTools(out var tracker))
                return;
            var assignment = tracker.ToolAssignment;
            int num = Mathf.FloorToInt((rect.width - 4f) * 0.714285731f);
            int num2 = Mathf.FloorToInt((rect.width - 4f) * 0.2857143f);
            float num3 = rect.x;
            bool somethingIsForced = tracker.forcedHandler.SomethingIsForced;
            Rect rect2 = new Rect(num3, rect.y + 2f, (float)num, rect.height - 4f);
            if (somethingIsForced)
            {
                rect2.width -= 4f + (float)num2;
            }
            Rect rect3 = rect2;
            Pawn pawn2 = pawn;
            Func<Pawn, ToolAssignment> getPayload = (Pawn p) => p.TryGetComp<Pawn_ToolTracker>()?.ToolAssignment;
            Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<ToolAssignment>>> menuGenerator = new Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<ToolAssignment>>>(Button_GenerateMenu);
            string buttonLabel = assignment.label.Truncate(rect2.width, null);
            string label = assignment.label;
            Widgets.Dropdown(rect3, pawn2, getPayload, menuGenerator, buttonLabel, null, label, null, null, true);
            num3 += rect2.width;
            num3 += 4f;
            Rect rect4 = new Rect(num3, rect.y + 2f, (float)num2, rect.height - 4f);

            if (somethingIsForced)
            {
                if (Widgets.ButtonText(rect4, "ClearForcedApparel".Translate(), true, false, true))
                {
                    tracker.forcedHandler.Reset();
                }
                TooltipHandler.TipRegion(rect4, new TipSignal(delegate ()
                {
                    string text = "ForcedTools".Translate() + ":\n";
                    foreach (Thing tool in tracker.forcedHandler.ForcedTools)
                    {
                        text = text + "\n   " + tool.LabelCap;
                    }
                    return text;
                }, pawn.GetHashCode() * 128));
                num3 += (float)num2;
                num3 += 4f;
            }

            Rect rect5 = new Rect(num3, rect.y + 2f, (float)num2, rect.height - 4f);
            if (Widgets.ButtonText(rect5, "AssignTabEdit".Translate(), true, false, true))
            {
                Find.WindowStack.Add(new Dialog_ManageToolAssignments(assignment));
            }
            num3 += (float)num2;
        }

        private IEnumerable<Widgets.DropdownMenuElement<ToolAssignment>> Button_GenerateMenu(Pawn pawn)
        {
            if (!pawn.CanUseTools(out var tracker))
                yield break;
            foreach (var assignment in Current.Game.GetComponent<ToolAssignment_Database>().AllAssignments)
                yield return new Widgets.DropdownMenuElement<ToolAssignment>
                {
                    option = new FloatMenuOption(assignment.label, () => tracker.ToolAssignment = assignment),
                    payload = assignment,
                };
        }

        public override int GetMinWidth(PawnTable table)
            => Mathf.Max(base.GetMinWidth(table), Mathf.CeilToInt(194f));

        public override int GetOptimalWidth(PawnTable table)
            => Mathf.Clamp(Mathf.CeilToInt(251f), GetMinWidth(table), GetMaxWidth(table));

        public override int GetMinHeaderHeight(PawnTable table)
            => Mathf.Max(base.GetMinHeaderHeight(table), PawnColumnWorker_Outfit.TopAreaHeight);

        public override int Compare(Pawn a, Pawn b)
            => GetValueToCompare(a).CompareTo(GetValueToCompare(b));

        private int GetValueToCompare(Pawn pawn)
            => (pawn.CanUseTools(out var tracker) && tracker.ToolAssignment != null) ?
                tracker.ToolAssignment.uniqueId : int.MinValue;
    }
}