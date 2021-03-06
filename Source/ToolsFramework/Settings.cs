﻿using RimWorld;
using UnityEngine;
using Verse;

namespace ToolsFramework
{
    public class Settings : ModSettings
    {
        public static bool degradation = true;
        public static float degradationFactor = 1f;

        public static bool alertToolNeedsReplacing = true;
        public static int alertToolNeedsReplacing_Delay = 1;
        public static float alertToolNeedsReplacing_Treshold = 0.5f;

        public static bool equipTool = true;
        public static bool draw = true;

        public static bool equipDelay = true;
        public static float equipDelayFactor = 1f;

        public static bool optimization = true;
        public static int optimizationDelay = 1;
        public static int mapTrackerDelay_StoredTools = 250;
        public static int mapTrackerDelay_UseableTools = 250;

        public static bool opportunisticToolJobs = true;
        public static bool opportunisticReturnTool = true;
        public static bool opportunisticReturnTool_onlyMemory = true;
        public static bool opportunisticTakeTool = true;
        public static bool opportunisticTakeTool_calcPath = true;

        public static int DictionaryCache = 3000;
        public static bool AllITool = false;
        public static int ResetStatPartTick = 5000;

        private Vector2 scrollPosition = Vector2.zero;
        public void DoWindowContents(Rect wrect)
        {
            float days;
            float hours;
            // wrect.yMin += 15f;
            // wrect.yMax -= 15f;

            var options = new Listing_Standard();
            // 
            var outRect = new Rect(wrect.x, wrect.y, wrect.width, wrect.height);            // Size of viewed area
            var scrollRect = new Rect(0f, 0f, wrect.width - 16f, wrect.height * 1.5f);      // Size of content data
            Widgets.BeginScrollView(outRect, ref scrollPosition, scrollRect, true);

            options.Begin(scrollRect);

            Header(options, "Performance_Settings".Translate(), false);

            if (degradation)
            {
                options.CheckboxLabeled("TF_alertToolNeedsReplacing".Translate(), ref alertToolNeedsReplacing, "TF_alertToolNeedsReplacing_tooltip".Translate());
                if (alertToolNeedsReplacing)
                {
                    options.Gap();
                    days = Mathf.FloorToInt((float)alertToolNeedsReplacing_Delay / GenDate.TicksPerDay);
                    hours = ((float)alertToolNeedsReplacing_Delay - days * GenDate.TicksPerDay) / GenDate.TicksPerHour;
                    options.Label("TF_alertToolNeedsReplacing_Delay".Translate() + $"\t{days} " + "DaysLower".Translate() +
                        $"  {hours.ToString("F02")} " + "HoursLower".Translate(),
                        tooltip: "TF_alertToolNeedsReplacing_Delay_tooltip".Translate());
                    alertToolNeedsReplacing_Delay = Mathf.RoundToInt(options.Slider(alertToolNeedsReplacing_Delay, 1, GenDate.TicksPerDay));
                    options.Gap();
                    days = Mathf.FloorToInt(alertToolNeedsReplacing_Treshold);
                    hours = alertToolNeedsReplacing_Treshold % 1f * 24f;
                    options.Label("TF_alertToolNeedsReplacing_Treshold".Translate() + $"\t{days} " + "DaysLower".Translate() +
                        $"  {hours.ToString("F02")} " + "HoursLower".Translate(),
                        tooltip: "TF_alertToolNeedsReplacing_Treshold_tooltip".Translate());
                    alertToolNeedsReplacing_Treshold = options.Slider(alertToolNeedsReplacing_Treshold, 0.01f, 10f);
                }
            }
            if (optimization)
            {
                options.Gap();
                days = Mathf.FloorToInt((float)optimizationDelay / GenDate.TicksPerDay);
                hours = ((float)optimizationDelay - days * GenDate.TicksPerDay) / GenDate.TicksPerHour;
                options.Label("TF_optimizationDelay".Translate() + $"\t{days} " + "DaysLower".Translate() +
                    $"  {hours.ToString("F02")} " + "HoursLower".Translate(),
                    tooltip: "TF_optimizationDelay_tooltip".Translate());
                optimizationDelay = Mathf.RoundToInt(options.Slider(optimizationDelay, GenDate.TicksPerHour, GenDate.TicksPerYear));
                options.Gap();
                days = Mathf.FloorToInt((float)mapTrackerDelay_StoredTools / GenDate.TicksPerDay);
                hours = ((float)mapTrackerDelay_StoredTools - days * GenDate.TicksPerDay) / GenDate.TicksPerHour;
                options.Label("TF_mapTrackerDelay_Stored".Translate() + $"\t{days} " + "DaysLower".Translate() +
                    $"  {hours.ToString("F02")} " + "HoursLower".Translate(),
                    tooltip: "TF_mapTrackerDelay_Stored_tooltip".Translate());
                mapTrackerDelay_StoredTools = Mathf.RoundToInt(options.Slider(mapTrackerDelay_StoredTools, GenDate.TicksPerHour, GenDate.TicksPerYear));
                days = Mathf.FloorToInt((float)mapTrackerDelay_UseableTools / GenDate.TicksPerDay);
                hours = ((float)mapTrackerDelay_UseableTools - days * GenDate.TicksPerDay) / GenDate.TicksPerHour;
                options.Label("TF_mapTrackerDelay_Useable".Translate() + $"\t{days} " + "DaysLower".Translate() +
                    $"  {hours.ToString("F02")} " + "HoursLower".Translate(),
                    tooltip: "TF_mapTrackerDelay_Useable_tooltip".Translate());
                mapTrackerDelay_UseableTools = Mathf.RoundToInt(options.Slider(mapTrackerDelay_UseableTools, GenDate.TicksPerHour, GenDate.TicksPerYear));
                if (mapTrackerDelay_UseableTools > mapTrackerDelay_StoredTools)
                    mapTrackerDelay_UseableTools = mapTrackerDelay_StoredTools;
            }
            if (opportunisticTakeTool)
            {
                options.Gap();
                if (ModsConfig.IsActive("fluffy.colonymanager"))
                {
                    options.CheckboxLabeled("TF_opportunisticTakeTool_calcPath".Translate(), ref opportunisticTakeTool_calcPath, "TF_opportunisticTakeTool_calcPath_tooltip".Translate());
                }
                else
                    options.CheckboxLabeled("TF_opportunisticTakeTool_calcPath".Translate(), ref opportunisticTakeTool_calcPath, "TF_opportunisticTakeTool_calcPath_tooltip".Translate());
            }
            if (!degradation && !optimization && !opportunisticTakeTool)
            {
                GUI.color = Color.gray;
                options.Label("NoPerformanceTweaks".Translate());
                GUI.color = Color.white;
            }

            Header(options, "Gameplay_Settings".Translate(), false);

            options.CheckboxLabeled("TF_degredation".Translate(), ref degradation, "TF_degredation_tooltip".Translate());
            if (degradation)
            {
                options.Gap();
                options.Label("TF_degredationFactor".Translate() + $"\tx{degradationFactor.ToString("F02")}", tooltip: "TF_degredationFactor_tooltip".Translate());
                degradationFactor = options.Slider(degradationFactor, 0.01f, 10f);
                options.Gap();
            }
            options.Gap();
            var equipString = equipTool ? "TF_equipTool_equip".Translate() : "TF_usefrominv".Translate();
            options.CheckboxLabeled("TF_equipTool".Translate() + $": {equipString}", ref equipTool, "TF_equipTool_tooltip".Translate());
            if (equipTool)
            {
                options.Gap();
                options.CheckboxLabeled("TF_draw".Translate(), ref draw, "TF_draw_tooltip".Translate());
            }
            options.Gap();
            options.CheckboxLabeled("TF_equipDelay".Translate(), ref equipDelay, "TF_equipDelay_tooltip".Translate());
            if (equipDelay)
            {
                options.Gap();
                options.Label("TF_equipDelayFactor".Translate() + $"\tx{equipDelayFactor.ToString("F02")}", tooltip: "TF_equipDelayFactor_tooltip".Translate());
                equipDelayFactor = options.Slider(equipDelayFactor, 0.01f, 10);
            }
            options.Gap();
            options.CheckboxLabeled("TF_optimization".Translate(), ref optimization, "TF_optimization_tooltip".Translate());
            options.Gap();
            options.CheckboxLabeled("TF_opportunisticToolJobs".Translate(), ref opportunisticToolJobs, "TF_opportunisticToolJobs_tooltip".Translate());
            if (opportunisticToolJobs)
            {
                options.Gap();
                options.CheckboxLabeled("TF_opportunisticReturnTool".Translate(), ref opportunisticReturnTool, "TF_opportunisticReturnTool_tooltip".Translate());
                options.Gap();
                options.CheckboxLabeled("TF_opportunisticTakeTool".Translate(), ref opportunisticTakeTool, "TF_opportunisticTakeTool_tooltip".Translate());
            }
            options.End();
            Widgets.EndScrollView();
            // Mod.GetSettings<Settings>().Write();
        }
        #region Curves
        public static float ToolTotalScorePow = 0.5f;
        public static readonly SimpleCurve LifespanScoreCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0.04f),
            new CurvePoint(0.5f, 0.2f),
            new CurvePoint(1f, 0.5f),
            new CurvePoint(2f, 1f),
            new CurvePoint(4f, 1f),
            new CurvePoint(999f, 10f)
        };
        public static readonly SimpleCurve ToolReadinessCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0.00f),
            new CurvePoint(1f, 1f),
            new CurvePoint(2f, 2f),
            new CurvePoint(4f, 4f),
            new CurvePoint(999f, 10f)
        };
        #endregion

        private void Header(Listing_Standard options, string text, bool GapLine = true)
        {
            Header();
            if (GapLine)
                options.GapLine();
            else
                options.Gap();
            options.Label(text);
            options.Gap();
            NormalText();
        }
        private void Header()
        {
            GUI.color = Color.cyan;
            Text.Anchor = TextAnchor.UpperCenter;
            Text.Font = GameFont.Medium;
        }
        private void NormalText()
        {
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }
        public override void ExposeData()
        {
            Scribe_Values.Look(ref degradation, "degradation", true);
            Scribe_Values.Look(ref degradationFactor, "degradationFactor", 1f);

            Scribe_Values.Look(ref alertToolNeedsReplacing, "alertToolNeedsReplacing", true);
            Scribe_Values.Look(ref alertToolNeedsReplacing_Delay, "alertToolNeedsReplacing_Delay", 1);
            Scribe_Values.Look(ref alertToolNeedsReplacing_Treshold, "alertToolNeedsReplacing_Treshold", 0.5f);

            Scribe_Values.Look(ref equipTool, "equipTool", true);
            Scribe_Values.Look(ref draw, "draw", true);

            Scribe_Values.Look(ref equipDelay, "equipDelay", true);
            Scribe_Values.Look(ref equipDelayFactor, "equipDelayFactor", 1f);

            Scribe_Values.Look(ref optimization, "optimization", true);
            Scribe_Values.Look(ref optimizationDelay, "optimizationDelay", 1);
            Scribe_Values.Look(ref mapTrackerDelay_StoredTools, "mapTrackerDelay_StoredTools", 250);
            Scribe_Values.Look(ref mapTrackerDelay_UseableTools, "mapTrackerDelay_UseableTools", 250);

            Scribe_Values.Look(ref opportunisticToolJobs, "opportunisticToolJobs", true);
            Scribe_Values.Look(ref opportunisticReturnTool, "opportunisticReturnTool", true);
            Scribe_Values.Look(ref opportunisticReturnTool_onlyMemory, "opportunisticReturnTool_onlyMemory", true);
            Scribe_Values.Look(ref opportunisticTakeTool, "opportunisticTakeTool", true);
            Scribe_Values.Look(ref opportunisticTakeTool_calcPath, "opportunisticTakeTool_calcPath", true);
            Dictionaries.StatPartSettings.SetOrAdd(StatDefOf.ToolReadinessDelay, equipDelayFactor);
            Dictionaries.StatPartSettings.SetOrAdd(StatDefOf.ToolWearFactor, degradationFactor);
        }
    }
}
