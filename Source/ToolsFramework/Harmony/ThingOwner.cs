using HarmonyLib;
using System;
using Verse;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch(typeof(ThingOwner))]
    public static class Patch_ThingOwner
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ThingOwner<Thing>), nameof(ThingOwner<Thing>.TryAdd), new Type[] { typeof(Thing), typeof(bool) })]
        public static void Postfix_TryAdd(ThingOwner<Thing> __instance, bool __result, Thing item)
        {
            if (!__result || !(item is Tool tool))
                return;
            Pawn pawn = GetPawn(__instance.Owner);
            if (pawn != null && pawn.CanUseTools(out var tracker) && !tracker.transfering)
            {
                tracker.UsedHandler.HeldToolsList.AddDistinct(tool);
                if (pawn.CurJobDef.IsTakingTempTool())
                    tracker.memoryTool.Add(tool);
#if DEBUG
                var test = new System.Text.StringBuilder($"Test 1.0: Tool pick-up: {pawn} : {tool}\nHeldTools:\n");
                foreach (var a in tracker.UsedHandler.HeldTools)
                    test.AppendLine($"{a}");
                test.AppendLine($"\nBestTools:");
                foreach (var b in tracker.UsedHandler.BestTool)
                    test.AppendLine($"{b.Key} : {b.Value}");
                Log.Message(test.ToString());
#endif
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ThingOwner.TryDrop_NewTmp))]
        public static void Postfix_TryDrop_NewTmp(ThingOwner __instance, bool __result, Thing thing)
        {
            if (!__result || !(thing is Tool tool))
                return;
            Pawn pawn = GetPawn(__instance.Owner);
            if (pawn != null && pawn.CanUseTools(out var tracker) == true)
            {
                tracker.UsedHandler.HeldToolsList.Remove(tool);
                if (pawn.CurJobDef.IsReturningTool())
                    tracker.memoryTool.Remove(tool);
                if (pawn.CurJobDef != null && Dictionaries.jobToolType.ContainsKey(pawn.CurJobDef))
                    pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptOptional);
#if DEBUG
                var test = new System.Text.StringBuilder($"Test 1.1: Tool pick-up: {pawn} : {tool}\nHeldTools:\n");
                foreach (var a in tracker.UsedHandler.HeldTools)
                    test.AppendLine($"{a}");
                test.AppendLine($"\nBestTools:");
                foreach (var b in tracker.UsedHandler.BestTool)
                    test.AppendLine($"{b.Key} : {b.Value}");
                Log.Message(test.ToString());
#endif
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ThingOwner.TryTransferToContainer), new Type[] { typeof(Thing), typeof(ThingOwner), typeof(int), typeof(Thing), typeof(bool) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
        public static void Prefix_TryTransferToContainer(ThingOwner __instance, Thing item, ThingOwner otherContainer, out Pawn_ToolTracker __state)
        {
            __state = null;
            if (!(item is Tool tool))
                return;
            Pawn pawn = GetPawn(__instance.Owner);
            Pawn otherPawn = GetPawn(otherContainer.Owner);
            if (pawn != null && pawn.CanUseTools(out __state) && otherPawn == pawn)
                __state.transfering = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ThingOwner.TryTransferToContainer), new Type[] { typeof(Thing), typeof(ThingOwner), typeof(int), typeof(Thing), typeof(bool) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
        public static void Postfix_TryTransferToContainer(ThingOwner __instance, int __result, Thing item, ThingOwner otherContainer, Pawn_ToolTracker __state)
        {
            if (__state == null)
                return;
            if (__result > 0)
            {
                var tool = item as Tool;
                Pawn pawn = GetPawn(__instance.Owner);
                Pawn otherPawn = GetPawn(otherContainer.Owner);
                if (pawn != otherPawn)
                {
                    __state.UsedHandler.HeldToolsList.Remove(tool);
                    if (pawn.CurJobDef.IsReturningTool())
                        __state.memoryTool.Remove(tool);
                }
#if DEBUG
                var test = new System.Text.StringBuilder($"Test 1.2: Tool pick-up: {pawn} : {tool}\nHeldTools:\n");
                foreach (var a in __state.UsedHandler.HeldTools)
                    test.AppendLine($"{a}");
                test.AppendLine($"\nBestTools:");
                foreach (var b in __state.UsedHandler.BestTool)
                    test.AppendLine($"{b.Key} : {b.Value}");
                Log.Message(test.ToString());
#endif
            }
            __state.transfering = false;
        }
        private static Pawn GetPawn(IThingHolder tOwner)
        {
            // Failsafe if other ThingHolders are defined
            Pawn pawn = null;
            switch (tOwner)
            {
                case Pawn_EquipmentTracker eq:
                    pawn = eq.pawn;
                    break;
                case Pawn_InventoryTracker inv:
                    pawn = inv.pawn;
                    break;
                case Pawn_CarryTracker car:
                    pawn = car.pawn;
                    break;
            }
            return pawn;
        }
    }
}