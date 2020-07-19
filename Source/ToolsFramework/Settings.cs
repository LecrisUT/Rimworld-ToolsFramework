using UnityEngine;
using Verse;

namespace ToolsFramework
{
    public class Settings : ModSettings
    {
        public static float toolDegradationFactor = 1f;
        public static bool toolDegradation = true;
        
        public void DoWindowContents(Rect wrect)
        {
            Listing_Standard options = new Listing_Standard();
            Color defaultColor = GUI.color;
            options.Begin(wrect);

            GUI.color = defaultColor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            options.Gap();
            options.CheckboxLabeled("Settings_ToolDegredation".Translate(), ref toolDegradation, "Settings_ToolDegredation_Tooltip".Translate());
            options.Gap();
            if (toolDegradation)
            {
                toolDegradationFactor = options.Slider(toolDegradationFactor, 0, 10);
            }
            options.End();

            Mod.GetSettings<Settings>().Write();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref toolDegradationFactor, "toolDegradationFactor", 1f);
        }
    }
}
