using HarmonyLib;
using RimWorld;
using Verse;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch(typeof(ThingFilter))]
    [HarmonyPatch(nameof(ThingFilter.SetFromPreset))]
    public static class Patch_ThingFilter_SetFromPreset
    {
        public static void Postfix(ThingFilter __instance, StorageSettingsPreset preset)
        {
            if (preset == StorageSettingsPreset.DefaultStockpile)
                __instance.SetAllow(ThingCategoryDefOf.Tools, true);
        }
    }
}