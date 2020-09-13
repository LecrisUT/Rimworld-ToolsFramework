using HarmonyLib;
using RimWorld;
using Verse;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch(typeof(Pawn_WorkSettings))]
    [HarmonyPatch("CacheWorkGiversInOrder")]
    public static class Patch_Pawn_WorkSettings_CacheWorkGiversInOrder
    {
        public static bool Prepare()
            => !ModsConfig.IsActive("fluffy.worktab") && !ModsConfig.IsActive("fluffy.worktab_local") && !ModsConfig.IsActive("fluffy.worktab_steam") && !ModsConfig.IsActive("fluffy.worktab_copy");
        public static void Postfix(Pawn ___pawn)
        {
            if (___pawn.CanUseTools(out var tracker))
            {
#if DEBUG
                Log.Message($"Test 0: {___pawn}");
#endif
                tracker.dirtyCache_necessaryToolTypes = true;
            }
        }
    }
}
