using HarmonyLib;
using Verse;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch(typeof(PawnRenderer))]
    [HarmonyPatch("CarryWeaponOpenly")]
    public static class Patch_PawnRenderer_CarryWeaponOpenly
    {
        public static bool Prefix(ref bool __result, Pawn ___pawn)
        {
            if (___pawn.CanUseTools(out var tracker) && tracker.toolInUse.ToolComp().DrawTool)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
