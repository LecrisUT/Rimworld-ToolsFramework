using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace ToolsFramework.Harmony
{
    [HarmonyPatch]
    public static class Patch_PriorityTracker_InvalidateCache
    {
        public static bool Prepare()
            => ModsConfig.IsActive("fluffy.worktab") || ModsConfig.IsActive("fluffy.worktab_local") || ModsConfig.IsActive("fluffy.worktab_steam") || ModsConfig.IsActive("fluffy.worktab_copy");
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method("WorkTab.PriorityTracker:InvalidateCache", new Type[] { typeof(WorkGiverDef), typeof(bool) });
            yield return AccessTools.Method("WorkTab.PriorityTracker:InvalidateCache", new Type[] { typeof(WorkTypeDef), typeof(bool) });
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var instructionList = instructions.ToList();
            var labelFinish = generator.DefineLabel();
            var labelLoop = generator.DefineLabel();
            var pawn = generator.DeclareLocal(typeof(Pawn));
            var tracker = generator.DeclareLocal(typeof(Pawn_ToolTracker));
            instructionList[0].labels.Add(labelFinish);
            instructionList.InsertRange(0, new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_2, null),
                new CodeInstruction(OpCodes.Brfalse, labelFinish),
                new CodeInstruction(OpCodes.Ldarg_0, null),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(AccessTools.TypeByName("WorkTab.PriorityTracker"),"Pawn")),
                new CodeInstruction(OpCodes.Stloc, pawn),
                new CodeInstruction(OpCodes.Ldloc, pawn),
                new CodeInstruction(OpCodes.Brfalse, labelLoop),
                new CodeInstruction(OpCodes.Ldloc, pawn),
                new CodeInstruction(OpCodes.Ldloca, tracker),
                new CodeInstruction(OpCodes.Ldc_I4_1, null),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RimworldExtensions), nameof(RimworldExtensions.CanUseTools), new Type[]{ typeof(Pawn), typeof(Pawn_ToolTracker).MakeByRefType(), typeof(bool)})),
                new CodeInstruction(OpCodes.Brfalse, labelFinish),
                new CodeInstruction(OpCodes.Ldloc, tracker),
                new CodeInstruction(OpCodes.Ldc_I4_1, null),
                new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(Pawn_ToolTracker),"dirtyCache_necessaryToolTypes")),
#if DEBUG
                new CodeInstruction(OpCodes.Ldloc, pawn),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PriorityTracker_InvalidateCache), nameof(Test))),
#endif
                new CodeInstruction(OpCodes.Br, labelFinish),
                new CodeInstruction(OpCodes.Ldarg_0, null){ labels = new List<Label>{ labelLoop } },
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(AccessTools.TypeByName("WorkTab.FavouriteManager"),"_favourites")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PriorityTracker_InvalidateCache), nameof(SetFavourites))),
            });
            return instructionList;
        }
        public static void SetFavourites(object favourite, Dictionary<Pawn, object> dictionary)
        {
            foreach (var pawn in dictionary.Where(t => t.Value == favourite).Select(t => t.Key))
                if (pawn.CanUseTools(out var tracker))
                    tracker.dirtyCache_necessaryToolTypes = true;
        }
#if DEBUG
        public static void Test(Pawn pawn)
        {
            Log.Message($"Test DirtyCache: {pawn}");
        }
#endif
    }
}
