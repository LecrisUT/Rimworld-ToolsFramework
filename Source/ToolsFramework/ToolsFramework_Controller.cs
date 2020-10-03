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
            if (ModsConfig.IsActive("roolo.dualwield") || ModsConfig.IsActive("roolo.dualwield_local") || ModsConfig.IsActive("roolo.dualwield_steam") || ModsConfig.IsActive("roolo.dualwield_copy"))
            {
                var myAddOffHand = AccessTools.Method(typeof(Patch_DualWield_Ext_Pawn_EquipmentTracker), nameof(Patch_DualWield_Ext_Pawn_EquipmentTracker.AddOffHandEquipment));
                var myTryGetOffHand = AccessTools.Method(typeof(Patch_DualWield_Ext_Pawn_EquipmentTracker), nameof(Patch_DualWield_Ext_Pawn_EquipmentTracker.TryGetOffHandEquipment));
                var AddOffHand = AccessTools.Method("DualWield.Ext_Pawn_EquipmentTracker:AddOffHandEquipment");
                var TryGetOffHand = AccessTools.Method("DualWield.Ext_Pawn_EquipmentTracker:TryGetOffHandEquipment");
                HarmonyLib.Harmony.ReversePatch(TryGetOffHand, new HarmonyMethod(myTryGetOffHand), null);
                HarmonyLib.Harmony.ReversePatch(AddOffHand, new HarmonyMethod(myAddOffHand), null);
            }
            if (ModsConfig.IsActive("petetimessix.simplesidearms") || ModsConfig.IsActive("petetimessix.simplesidearms_local") || ModsConfig.IsActive("petetimessix.simplesidearms_steam") || ModsConfig.IsActive("petetimessix.simplesidearms_copy"))
                harmony.Unpatch(AccessTools.Constructor(typeof(Toil)), HarmonyPatchType.Postfix, "PeteTimesSix.SimpleSidearms");
            BaseMod.thisMod.GetSettings<Settings>();
            Dictionaries.StatPartSettings.SetOrAdd(StatDefOf.ToolReadinessDelay, ToolsFramework.Settings.equipDelayFactor);
            Dictionaries.StatPartSettings.SetOrAdd(StatDefOf.ToolWearFactor, ToolsFramework.Settings.degradationFactor);
            ToolsFramework.Settings.AllITool = false;
            var unoptToolDef = DefDatabase<ThingDef>.AllDefs.Where(t => t.IsTool() && !typeof(ITool).IsAssignableFrom(t.thingClass));
            if (unoptToolDef.Any())
            {
                ToolsFramework.Settings.AllITool = false;
                Log.Warning("TF_BaseMessage" + "TF_Warning_Unoptimized".Translate(unoptToolDef.ToStringSafeEnumerable()));
            }
            else
                ToolsFramework.Settings.AllITool = true;
        }
    }
}
