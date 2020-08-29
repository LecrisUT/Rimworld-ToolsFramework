using HugsLib;
using System.Collections.Generic;
using System.Linq;
using Verse;
using HarmonyLib;
using ToolsFramework.Harmony;
using Verse.AI;

namespace ToolsFramework
{
    internal class Controller : ModBase
    {
        public override void DefsLoaded()
        {
            foreach (ThingDef tDef in DefDatabase<ThingDef>.AllDefs.Where(t => t.race?.Humanlike == true))
            {
                if (tDef.comps == null)
                    tDef.comps = new List<CompProperties>();
                tDef.comps.Add(new CompProperties(typeof(Pawn_ToolTracker)));
            }
            var harmony = new HarmonyLib.Harmony("ToolsFramework");
            if (ModsConfig.IsActive("roolo.dualwield"))
            {
                var myAddOffHand = AccessTools.Method(typeof(Patch_DualWield_Ext_Pawn_EquipmentTracker), nameof(Patch_DualWield_Ext_Pawn_EquipmentTracker.AddOffHandEquipment));
                var myTryGetOffHand = AccessTools.Method(typeof(Patch_DualWield_Ext_Pawn_EquipmentTracker), nameof(Patch_DualWield_Ext_Pawn_EquipmentTracker.TryGetOffHandEquipment));
                var AddOffHand = AccessTools.Method("DualWield.Ext_Pawn_EquipmentTracker:AddOffHandEquipment");
                var TryGetOffHand = AccessTools.Method("DualWield.Ext_Pawn_EquipmentTracker:TryGetOffHandEquipment");
                HarmonyLib.Harmony.ReversePatch(TryGetOffHand, new HarmonyMethod(myTryGetOffHand), null);
                HarmonyLib.Harmony.ReversePatch(AddOffHand, new HarmonyMethod(myAddOffHand), null);
            }
            if (ModsConfig.IsActive("petetimessix.simplesidearms"))
                harmony.Unpatch(AccessTools.Constructor(typeof(Toil)), HarmonyPatchType.Postfix, "PeteTimesSix.SimpleSidearms");
            ToolsFramework.thisMod.GetSettings<Settings>();
        }
    }
}
