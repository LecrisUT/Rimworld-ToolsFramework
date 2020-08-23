using HarmonyLib;
using System.Reflection;
using Verse;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch]
    public static class Patch_DualWield_Ext_Pawn_EquipmentTracker
    {
        public static void AddOffHandEquipment(this Pawn_EquipmentTracker instance, ThingWithComps newEq)
        {

        }
        public static bool TryGetOffHandEquipment(this Pawn_EquipmentTracker instance, out ThingWithComps result)
        {
            result = null;
            return false;
        }
    }
}
