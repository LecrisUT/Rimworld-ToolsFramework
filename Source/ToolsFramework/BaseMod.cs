using UnityEngine;
using Verse;

namespace ToolsFramework
{
    public class BaseMod : Mod
    {
        public static BaseMod thisMod;
        public Settings settings;

        public BaseMod(ModContentPack content) : base(content)
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
