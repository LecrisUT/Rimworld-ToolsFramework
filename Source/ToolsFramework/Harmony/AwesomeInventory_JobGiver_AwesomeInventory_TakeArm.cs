using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch]
    public static class Patch_JobGiver_AwesomeInventory_TakeArm
    {
        public static bool Prepare()
            => ModsConfig.IsActive("notooshabby.awesomeinventory");
        public static MethodBase TargetMethod()
            => AccessTools.Method("AwesomeInventory.Jobs.JobGiver_AwesomeInventory_TakeArm:Validator");
        public static bool Prefix(ref bool __result, Thing thing)
        {
            if (thing.def.IsTool())
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}