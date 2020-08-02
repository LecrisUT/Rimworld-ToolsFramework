using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ToolsFramework
{
    public class Dialog_ManageToolAssignments : Window
    {
        private Vector2 scrollPosition;
        private static ThingFilter globalFilter;
        private ToolAssignment toolAssignment;

        public const float TopAreaHeight = 40f;
        public const float TopButtonHeight = 35f;
        public const float TopButtonWidth = 150f;

        public Dialog_ManageToolAssignments(ToolAssignment selectedToolAssignment)
        {
            forcePause = true;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
            if (globalFilter == null)
            {
                globalFilter = new ThingFilter();
                globalFilter.SetAllow(ThingCategoryDefOf.Tools, true);
            }
            ToolAssignment = selectedToolAssignment;
        }

        private ToolAssignment ToolAssignment
        {
            get => toolAssignment;
            set
            {
                CheckAssignmentHasName();
                toolAssignment = value;
            }
        }

        private void CheckAssignmentHasName()
        {
            if (ToolAssignment?.label.NullOrEmpty() == true)
                ToolAssignment.label = "Unnamed";
        }

        public override Vector2 InitialSize => new Vector2(700f, 700f);

        public override void DoWindowContents(Rect inRect)
        {
            var assignmentDatabase = Current.Game.GetComponent<ToolAssignment_Database>();
            float num = 0f;
            if (Widgets.ButtonText(new Rect(0f, 0f, TopButtonWidth, TopButtonHeight), "SelectToolAssignment".Translate()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (var localOut3 in assignmentDatabase.AllAssignments)
                {
                    var localOut = localOut3;
                    list.Add(new FloatMenuOption(localOut.label, delegate ()
                    {
                        ToolAssignment = localOut;
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            num += 10f + TopButtonWidth;
            if (Widgets.ButtonText(new Rect(num, 0f, TopButtonWidth, TopButtonHeight), "NewToolAssignment".Translate(), true, false, true))
            {
                toolAssignment = assignmentDatabase.MakeNewToolAssignment();
            }
            num += 10f + TopButtonWidth;
            if (Widgets.ButtonText(new Rect(num, 0f, TopButtonWidth, TopButtonHeight), "DeleteToolAssignment".Translate(), true, false, true))
            {
                List<FloatMenuOption> list2 = new List<FloatMenuOption>();
                foreach (var localOut2 in assignmentDatabase.AllAssignments)
                {
                    var localOut = localOut2;
                    list2.Add(new FloatMenuOption(localOut.label, delegate ()
                    {
                        AcceptanceReport acceptanceReport = assignmentDatabase.TryDelete(localOut);
                        if (!acceptanceReport.Accepted)
                        {
                            Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, false);
                        }
                        else if (localOut == ToolAssignment)
                        {
                            ToolAssignment = null;
                        }
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(list2));
            }
            Rect rect4 = new Rect(0f, TopAreaHeight, inRect.width, inRect.height - TopAreaHeight - CloseButSize.y).ContractedBy(10f);
            if (ToolAssignment == null)
            {
                GUI.color = Color.grey;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect4, "NoToolAssignmentSelected".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
                return;
            }
            GUI.BeginGroup(rect4);
            Dialog_ManageOutfits.DoNameInputRect(new Rect(0f, 0f, 200f, 30f), ref ToolAssignment.label);
            ThingFilterUI.DoThingFilterConfigWindow(new Rect(0f, TopAreaHeight, 300f, rect4.height - 45f - 10f), ref scrollPosition, toolAssignment.filter, globalFilter, 16);
            GUI.EndGroup();
        }

        public override void PreClose()
        {
            base.PreClose();
            CheckAssignmentHasName();
        }
    }
}