using RimWorld;
using System.Collections.Generic;
using System.Linq;
using ToolsFramework.Harmony;
using Verse;

namespace ToolsFramework
{
    public static class ToolExtensions
    {
        public static bool ToolTypeListContains(this List<ToolTypeModifier> modList, ToolType toolType, out ToolTypeModifier modifier)
        {
            modifier = null;
            if (modList != null)
                for (int i = 0; i < modList.Count; i++)
                    if (modList[i].toolType == toolType)
                    {
                        modifier = modList[i];
                        return true;
                    }
            return false;
        }
        public static bool ToolTypeListContains(this List<ToolTypeModifier> modList, ToolType toolType)
            => modList.ToolTypeListContains(toolType, out _);
        public static float GetToolTypeValueFromList(this List<ToolTypeModifier> modList, ToolType toolType, float defaultValue = 0f)
        {
            if (modList != null)
                for (int i = 0; i < modList.Count; i++)
                    if (modList[i].toolType == toolType)
                        return modList[i].value;
            return defaultValue;
        }
        public static float GetJobValueFromList(this List<JobModifier> modList, JobDef job, float defaultValue = 0f)
        {
            if (modList != null)
                for (int i = 0; i < modList.Count; i++)
                    if (modList[i].job == job)
                        return modList[i].value;
            return defaultValue;
        }
        public static float GetBillGiverValueFromList(this List<BillGiverModifier> modList, ThingDef billGiver, float defaultValue = 0f)
        {
            if (modList != null)
                for (int i = 0; i < modList.Count; i++)
                    if (modList[i].billGiver == billGiver)
                        return modList[i].value;
            return defaultValue;
        }
        #region ToolDef extenstion
        public static bool TryGetValue(this CompProperties_Tool toolProp, ToolType toolType, out float val, ThingDef stuffDef = null)
        {
            val = 0f;
            if (!toolProp.toolTypesValue.ToolTypeListContains(toolType, out var modifier))
                return false;
            val = modifier.value * toolType.GetStuffEfficiency(stuffDef);
            return true;
        }
        public static float GetValue(this CompProperties_Tool toolProp, ToolType toolType, float fallback = 0f, ThingDef stuffDef = null)
        {
            if (toolProp.TryGetValue(toolType, out float val, stuffDef))
                return val;
            return fallback;
        }
        public static bool TryGetValue(this ToolDef toolDef, ToolType toolType, out float val, ThingDef stuffDef = null)
        {
            val = 0f;
            if (!toolDef.CompProp.toolTypesValue.ToolTypeListContains(toolType, out var modifier))
                return false;
            val = modifier.value * toolType.GetStuffEfficiency(stuffDef);
            return true;
        }
        public static float GetValue(this ToolDef toolDef, ToolType toolType, float fallback = 0f, ThingDef stuffDef = null)
        {
            if (toolDef.TryGetValue(toolType, out float val, stuffDef))
                return val;
            return fallback;
        }
        #endregion
        #region ToolComp Extension
        public static bool TryGetValue(this ToolComp comp, ToolType toolType, out float val)
        {
            val = 0f;
            if (!comp.ToolTypes.Contains(toolType))
                return false;
            val = comp.parent.GetStatValue(toolType, StatDefOf.ToolEffectivenessFactor);
            return true;
        }
        public static bool TryGetValue(this ToolComp comp, JobDef job, out float val, ToolType toolType = null)
        {
            val = 0f;
            if (toolType == null && !Dictionaries.jobToolType.TryGetValue(job, out toolType))
                return false;
            if (!comp.TryGetValue(toolType, out val))
                return false;
            val *= comp.CompProp.jobBonus.GetJobValueFromList(job, 1f);
            return true;
        }
        public static bool TryGetValue(this ToolComp comp, ThingDef billGiver, out float val, ToolType toolType = null)
        {
            val = 0f;
            if (toolType == null && !Dictionaries.billGiverToolType.TryGetValue(billGiver, out toolType))
                return false;
            if (!comp.TryGetValue(toolType, out val))
                return false;
            val *= comp.CompProp.billGiverBonus.GetBillGiverValueFromList(billGiver, 1f);
            return true;
        }
        public static bool TryGetValue(this ToolComp comp, ThingDef billGiver, StatDef stat, out float factor, out float offset, ToolType toolType = null)
        {
            factor = 1f;
            offset = 0f;
            if (toolType == null && !Dictionaries.billGiverToolType.TryGetValue(billGiver, out toolType))
                return false;
            if (!comp.TryGetValue(billGiver, out var val, toolType))
                return false;
            factor = toolType.workStatFactors.GetStatValueFromList(stat, 0f);
            if (factor != 0f)
                factor *= val;
            else
                factor = 1f;
            offset = toolType.workStatOffset.GetStatValueFromList(stat, 0f);
            if (offset != 0f)
                offset = val - offset;
            return true;
        }
        public static bool TryGetValue(this ToolComp comp, JobDef job, StatDef stat, out float factor, out float offset, ToolType toolType = null)
        {
            factor = 1f;
            offset = 0f;
            if (toolType == null && !Dictionaries.jobToolType.TryGetValue(job, out toolType))
                return false;
            if (!comp.TryGetValue(job, out var val, toolType))
                return false;
            bool flag = false;
            factor = toolType.workStatFactors.GetStatValueFromList(stat, 0f);
            if (factor != 0f)
            {
                factor *= val;
                flag = true;
            }
            else
                factor = 1f;
            offset = toolType.workStatOffset.GetStatValueFromList(stat, 0f);
            if (offset != 0f)
            {
                flag = true;
                offset = val - offset;
            }
            return flag;
        }
        public static bool TryGetValue(this ToolComp comp, ToolType toolType, StatDef stat, out float factor, out float offset)
        {
            factor = 1f;
            offset = 0f;
            if (!comp.TryGetValue(toolType, out var val))
                return false;
            bool flag = false;
            factor = toolType.workStatFactors.GetStatValueFromList(stat, 0f);
            if (factor != 0f)
            {
                factor *= val;
                flag = true;
            }
            else
                factor = 1f;
            offset = toolType.workStatOffset.GetStatValueFromList(stat, 0f);
            if (offset != 0f)
            {
                flag = true;
                offset = val - offset;
            }
            return flag;
        }
        public static float GetValue(this ToolComp comp, ToolType toolType, float fallback = 0f)
        {
            if (comp.TryGetValue(toolType, out float val))
                return val;
            return fallback;
        }
        public static float GetValue(this ToolComp comp, JobDef job, ToolType toolType = null, float fallback = 0f)
        {
            if (comp.TryGetValue(job, out float val, toolType))
                return val;
            return fallback;
        }
        public static bool IsForced(this ToolComp comp)
        {
            if (!(comp.HoldingPawn is Pawn pawn) || !pawn.CanUseTools(out var tracker))
                return false;
            return tracker.forcedHandler.ForcedTools.Contains(comp.parent);
        }
        public static bool CanDrop(this ToolComp comp) => !comp.IsForced();
        #endregion
        #region ThingWithComps Extension
        public static bool TryGetToolValue(this ThingWithComps thing, ToolType toolType, out float val)
        {
            val = 0f;
            if (!thing.IsTool(out var comp))
                return false;
            return comp.TryGetValue(toolType, out val);
        }
        public static bool TryGetToolValue(this ThingWithComps thing, JobDef job, out float val, ToolType toolType = null)
        {
            val = 0f;
            if (!thing.IsTool(out var comp))
                return false;
            return comp.TryGetValue(job, out val, toolType);
        }
        public static bool TryGetToolValue(this ThingWithComps thing, ThingDef billGiver, out float val, ToolType toolType = null)
        {
            val = 0f;
            if (!thing.IsTool(out var comp))
                return false;
            return comp.TryGetValue(billGiver, out val, toolType);
        }
        public static bool TryGetToolValue(this ThingWithComps thing, ThingDef billGiver, StatDef stat, out float factor, out float offset, ToolType toolType = null)
        {
            factor = 1f;
            offset = 0f;
            if (!thing.IsTool(out var comp))
                return false;
            return comp.TryGetValue(billGiver, stat, out factor, out offset, toolType);
        }
        public static bool TryGetToolValue(this ThingWithComps thing, JobDef job, StatDef stat, out float factor, out float offset, ToolType toolType = null)
        {
            factor = 1f;
            offset = 0f;
            if (!thing.IsTool(out var comp))
                return false;
            return comp.TryGetValue(job, stat, out factor, out offset, toolType);
        }
        public static bool TryGetToolValue(this ThingWithComps thing, ToolType toolType, StatDef stat, out float factor, out float offset)
        {
            factor = 1f;
            offset = 0f;
            if (!thing.IsTool(out var comp))
                return false;
            return comp.TryGetValue(toolType, stat, out factor, out offset);
        }
        public static float GetToolValue(this ThingWithComps thing, ToolType toolType, float fallback = 0f)
        {
            if (thing.IsTool(out var comp) && comp.TryGetValue(toolType, out float val))
                return val;
            return fallback;
        }
        public static float GetToolValue(this ThingWithComps thing, JobDef job, float fallback = 0f)
        {
            if (thing.IsTool(out var comp) && comp.TryGetValue(job, out float val))
                return val;
            return fallback;
        }
        public static float GetStatValue(this ThingWithComps tool, ToolType toolType, StatDef stat, bool applyPostProcess = true, float fallback = 0f)
        {
            if (tool == null)
                return fallback;
            return stat.Worker.GetValue(tool, toolType, applyPostProcess);
        }
        public static float GetValue(this StatWorker statWorker, ThingWithComps tool, ToolType toolType, bool applyPostProcess = true)
        {
            return statWorker.GetValue(Patch_StatRequest_For.For(tool, toolType), applyPostProcess);
        }
        public static bool IsForced(this ThingWithComps thing)
            => thing.IsTool(out var comp) && comp.IsForced();
        public static bool CanDrop(this ThingWithComps thing)
            => !thing.IsTool(out var comp) || !comp.IsForced();
        #endregion
    }
}
