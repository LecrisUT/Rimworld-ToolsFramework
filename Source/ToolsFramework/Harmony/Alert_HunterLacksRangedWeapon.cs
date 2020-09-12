using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using Verse.AI;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch]
    public static class Alert_HunterLacksRangedWeapon
    {
        public static MethodBase TargetMethod()
            => AccessTools.PropertyGetter(typeof(RimWorld.Alert_HunterLacksRangedWeapon), "HuntersWithoutRangedWeapon");
        public static void Postfix(ref List<Pawn> __result)
        {
            __result.RemoveAll(t => t.CanUseTools(out var tracker) && tracker.toolInUse != null);
        }
    }
}
