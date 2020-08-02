using HarmonyLib;
using System.Reflection;
using Verse;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch]
    public static class Patch_DualWield_Ext_Pawn_EquipmentTracker_AddOffHandEquipment
    {
        public static bool Prepare()
            => ModsConfig.IsActive("roolo.dualwield");
        public static MethodBase TargetMethod()
            => AccessTools.Method("DualWield.Ext_Pawn_EquipmentTracker:AddOffHandEquipment");
        public static void AddOffHandEquipment(this Pawn_EquipmentTracker instance, ThingWithComps newEq)
        {

        }
    }
    [HarmonyPatch]
    public static class Patch_DualWield_Ext_Pawn_EquipmentTracker_TryGetOffHandEquipment
    {
        public static bool Prepare()
            => ModsConfig.IsActive("roolo.dualwield");
        public static MethodBase TargetMethod()
            => AccessTools.Method("DualWield.Ext_Pawn_EquipmentTracker:TryGetOffHandEquipment");
        public static bool TryGetOffHandEquipment(this Pawn_EquipmentTracker instance, out ThingWithComps result)
        {
            result = null;
            return false;
        }
    }
}
