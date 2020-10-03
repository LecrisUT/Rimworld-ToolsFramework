using HarmonyLib;
using System;
using Verse;
using RimWorld;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch(typeof(ThingOwner))]
    public static class Patch_ThingOwner
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ThingOwner<Thing>), nameof(ThingOwner<Thing>.TryAdd), new Type[] { typeof(Thing), typeof(bool) })]
        public static void Postfix_TryAdd_Thing(ThingOwner<Thing> __instance, bool __result, Thing item)
        {
            if (!__result || !(item is ThingWithComps thing) || !thing.IsTool(out var comp, false))
                return;
            Pawn pawn = __instance.Owner.HoldingPawn(true);
            if (pawn != null && pawn.CanUseTools(out var tracker) && !tracker.transfering)
            {
                tracker.UsedHandler.HeldToolsList.AddDistinct(new ToolInfo(thing, comp));
                if (pawn.CurJobDef.IsTakingTempTool())
                    tracker.memoryTool.Add(thing);
#if DEBUG
                var test = new System.Text.StringBuilder($"Test 1.0: Tool pick-up: {pawn} : {thing}\nHeldTools:\n");
                foreach (var a in tracker.UsedHandler.HeldTools)
                    test.AppendLine($"{a}");
                test.AppendLine($"\nBestTools:");
                foreach (var b in tracker.UsedHandler.BestTool)
                    test.AppendLine($"{b.Key} : {b.Value?.tool}");
                Log.Message(test.ToString());
#endif
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ThingOwner.TryDrop_NewTmp))]
        public static void Postfix_TryDrop_NewTmp(ThingOwner __instance, bool __result, Thing thing)
        {
            if (!__result || !(thing is ThingWithComps twc) || !twc.IsTool())
                return;
            Pawn pawn = __instance.Owner.HoldingPawn(true);
            if (pawn != null && pawn.CanUseTools(out var tracker) == true)
            {
                tracker.UsedHandler.HeldToolsList.RemoveAll(t => t.tool == twc);
                if (pawn.CurJobDef.IsReturningTool())
                    tracker.memoryTool.Remove(twc);
                if (pawn.CurJobDef != null && Dictionaries.jobToolType.ContainsKey(pawn.CurJobDef))
                    pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptOptional);
#if DEBUG
                var test = new System.Text.StringBuilder($"Test 1.1: Tool pick-up: {pawn} : {thing}\nHeldTools:\n");
                foreach (var a in tracker.UsedHandler.HeldTools)
                    test.AppendLine($"{a}");
                test.AppendLine($"\nBestTools:");
                foreach (var b in tracker.UsedHandler.BestTool)
                    test.AppendLine($"{b.Key} : {b.Value?.tool}");
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
            if (!(item is ThingWithComps thing) || !thing.IsTool())
                return;
            Pawn pawn = __instance.Owner.HoldingPawn(true);
            Pawn otherPawn = otherContainer.Owner.HoldingPawn(true);
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
                var tool = (ThingWithComps)item;
                Pawn pawn = __instance.Owner.HoldingPawn(true);
                Pawn otherPawn = otherContainer.Owner.HoldingPawn(true);
                if (pawn != otherPawn)
                {
                    __state.UsedHandler.HeldToolsList.RemoveAll(t => t.tool == tool);
                    if (pawn.CurJobDef.IsReturningTool())
                        __state.memoryTool.Remove(tool);
                }
#if DEBUG
                var test = new System.Text.StringBuilder($"Test 1.2: Tool pick-up: {pawn} : {item}\nHeldTools:\n");
                foreach (var a in __state.UsedHandler.HeldTools)
                    test.AppendLine($"{a}");
                test.AppendLine($"\nBestTools:");
                foreach (var b in __state.UsedHandler.BestTool)
                    test.AppendLine($"{b.Key} : {b.Value?.tool}");
                Log.Message(test.ToString());
#endif
            }
            __state.transfering = false;
        }
    }
}