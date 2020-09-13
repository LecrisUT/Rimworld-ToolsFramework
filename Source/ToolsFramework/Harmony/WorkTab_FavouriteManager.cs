using HarmonyLib;
using System.Reflection;
using Verse;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch]
    public static class Patch_FavouriteManager
    {
        public static bool Prepare()
            => ModsConfig.IsActive("fluffy.worktab") || ModsConfig.IsActive("fluffy.worktab_local") || ModsConfig.IsActive("fluffy.worktab_steam") || ModsConfig.IsActive("fluffy.worktab_copy");
        public static MethodBase TargetMethod()
            => AccessTools.PropertySetter(AccessTools.TypeByName("WorkTab.FavouriteManager"), "Item");
        public static void Postfix(Pawn pawn)
        {
            if (pawn != null && pawn.CanUseTools(out var tracker))
                tracker.dirtyCache_necessaryToolTypes = true;
        }
    }
}
