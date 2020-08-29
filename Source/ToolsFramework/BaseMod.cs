using UnityEngine;
using Verse;

namespace ToolsFramework
{
    public class ToolsFramework : Mod
    {
        public static ToolsFramework thisMod;
        public Settings settings;

        public ToolsFramework(ModContentPack content) : base(content)
        {
            thisMod = this;
        }

        public override string SettingsCategory() => "ToolsFramework".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            GetSettings<Settings>().DoWindowContents(inRect);
        }
    }
}
