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
        public static int mapTrackerDelay = 250;

        public void DoWindowContents(Rect wrect)
        {
            Listing_Standard options = new Listing_Standard();
            Color defaultColor = GUI.color;
            options.Begin(wrect);

            GUI.color = defaultColor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            int days;
            float hours;
            int tickUnits = 250;
            options.Gap();
            options.CheckboxLabeled("TF_degredation".Translate(), ref degradation, "TF_degredation_tooltip".Translate());
            if (degradation)
            {
                options.Gap();
                options.Label("TF_degredationFactor".Translate() + $"\tx{degradationFactor.ToString("F02")}", tooltip: "TF_degredationFactor_tooltip".Translate());
                degradationFactor = options.Slider(degradationFactor, 0.01f, 10f);
                options.Gap();
                options.CheckboxLabeled("TF_alertToolNeedsReplacing".Translate(), ref alertToolNeedsReplacing, "TF_alertToolNeedsReplacing_tooltip".Translate());
                if (alertToolNeedsReplacing)
                {
                    options.Gap();
                    days = Mathf.FloorToInt((float)alertToolNeedsReplacing_Delay / GenDate.TicksPerDay);
                    hours = ((float)alertToolNeedsReplacing_Delay - days * GenDate.TicksPerDay) / GenDate.TicksPerHour;
                    options.Label("TF_alertToolNeedsReplacing_Delay".Translate() + $"\t{days} " + "DaysLower".Translate() +
                        $"  {hours.ToString("F02")} " + "HoursLower".Translate(),
                        tooltip: "TF_alertToolNeedsReplacing_Delay_tooltip".Translate());
                    alertToolNeedsReplacing_Delay = Mathf.RoundToInt(options.Slider((float)alertToolNeedsReplacing_Delay / tickUnits, 1, 100f) * tickUnits);
                    options.Gap();
                    days = Mathf.FloorToInt(alertToolNeedsReplacing_Treshold);
                    hours = alertToolNeedsReplacing_Treshold % 1f * 24f;
                    options.Label("TF_alertToolNeedsReplacing_Treshold".Translate() + $"\t{days} " + "DaysLower".Translate() +
                        $"  {hours.ToString("F02")} " + "HoursLower".Translate(),
                        tooltip: "TF_alertToolNeedsReplacing_Treshold_tooltip".Translate());
                    alertToolNeedsReplacing_Treshold = options.Slider(alertToolNeedsReplacing_Treshold, 0.01f, 10f);
                }
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
            if (optimization)
            {
                options.Gap();
                days = Mathf.FloorToInt((float)optimizationDelay * 250 / GenDate.TicksPerDay);
                hours = ((float)optimizationDelay * 250 - days * GenDate.TicksPerDay) / GenDate.TicksPerHour;
                options.Label("TF_optimizationDelay".Translate() + $"\t{days} " + "DaysLower".Translate() + 
                    $"  {hours.ToString("F02")} " + "HoursLower".Translate(),
                    tooltip: "TF_optimizationDelay_tooltip".Translate());
                optimizationDelay = Mathf.RoundToInt(options.Slider(optimizationDelay, 1, 200));
                options.Gap();
                days = Mathf.FloorToInt((float)mapTrackerDelay / GenDate.TicksPerDay);
                hours = ((float)mapTrackerDelay - days * GenDate.TicksPerDay) / GenDate.TicksPerHour;
                options.Label("TF_mapTrackerDelay".Translate() + $"\t{days} " + "DaysLower".Translate() +
                    $"  {hours.ToString("F02")} " + "HoursLower".Translate(),
                    tooltip: "TF_mapTrackerDelay_tooltip".Translate());
                mapTrackerDelay = Mathf.RoundToInt(options.Slider((float)mapTrackerDelay / tickUnits, 1, 200f) * tickUnits);
            }
            options.End();

            Mod.GetSettings<Settings>().Write();
        }
        #region Curves
        public static float ToolTotalScorePow = 1f;
        public static readonly SimpleCurve LifespanScoreCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0.04f),
            new CurvePoint(0.5f, 0.2f),
            new CurvePoint(1f, 0.5f),
            new CurvePoint(2f, 1f),
            new CurvePoint(4f, 1f),
            new CurvePoint(999f, 10f)
        };
        #endregion

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
            Scribe_Values.Look(ref mapTrackerDelay, "mapTrackerDelay", 250);
        }
    }
}
