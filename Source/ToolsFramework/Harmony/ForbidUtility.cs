using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch(typeof(ForbidUtility))]
    [HarmonyPatch("IsForbidden")]
    [HarmonyPatch(new Type[] { typeof(Thing), typeof(Pawn)})]
    public static class Patch_ForbidUtility_IsForbidden
    {
        public static void Postfix(ref bool __result, Thing t, Pawn pawn)
        {
            if (!__result && t is Tool && pawn.CanUseTools(out var tracker))
                __result = !tracker.ToolAssignment?.filter?.Allows(t) ?? false;
        }
    }
}
