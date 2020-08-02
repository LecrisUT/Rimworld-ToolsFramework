using HarmonyLib;
using RimWorld;
using Verse;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch(typeof(Pawn_WorkSettings))]
    [HarmonyPatch("CacheWorkGiversInOrder")]
    public static class Patch_Pawn_WorkSettings_CacheWorkGiversInOrder
    {
        public static void Postfix(Pawn ___pawn)
        {
            if (___pawn.CanUseTools(out var tracker))
            {
                tracker.dirtyCache_necessaryToolTypes = true;
            }
        }
    }
}
