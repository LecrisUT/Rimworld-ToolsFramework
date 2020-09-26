﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using ToolsFramework.Harmony;
using UnityEngine;
using Verse;

namespace ToolsFramework
{
    public static class ToolExtensions
    {
        public static float GetStatValue(this Tool tool, ToolType toolType, StatDef stat, bool applyPostProcess = true, float fallback = 0f)
        {
            if (tool == null)
                return fallback;
            return stat.Worker.GetValue(tool, toolType, applyPostProcess);
        }
        public static float GetValue(this StatWorker statWorker, Tool tool, ToolType toolType, bool applyPostProcess = true)
        {
            return statWorker.GetValue(Patch_StatRequest_For.For(tool, toolType), applyPostProcess);
        }
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
        public static bool TryGetValue(this ToolProperties toolProp, ToolType toolType, out float val, ThingDef stuffDef = null)
        {
            val = 0f;
            if (!toolProp.toolTypesValue.ToolTypeListContains(toolType, out var modifier))
                return false;
            val = modifier.value * toolType.GetStuffEfficiency(stuffDef);
            return true;
        }
        public static float GetValue(this ToolProperties toolProp, ToolType toolType, float fallback = 0f, ThingDef stuffDef = null)
        {
            if (toolProp.TryGetValue(toolType, out float val, stuffDef))
                return val;
            return fallback;
        }
        public static bool TryGetValue(this ToolDef toolDef, ToolType toolType, out float val, ThingDef stuffDef = null)
        {
            val = 0f;
            if (!toolDef.ToolProperties.toolTypesValue.ToolTypeListContains(toolType, out var modifier))
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
        #region Tool extension
        public static bool TryGetValue(this Tool tool, ToolType toolType, out float val)
        {
            val = 0f;
            if (tool == null || !tool.ToolTypes.Contains(toolType))
                return false;
            val = tool.GetStatValue(toolType, StatDefOf.ToolEffectivenessFactor);
            return true;
        }
        public static bool TryGetValue(this Tool tool, JobDef job, out float val, ToolType toolType = null)
        {
            val = 0f;
            if (tool == null)
                return false;
            if (toolType == null && !Dictionaries.jobToolType.TryGetValue(job, out toolType))
                return false;
            if (!tool.TryGetValue(toolType, out val))
                return false;
            var toolProp = tool.ToolProperties;
            val *= toolProp.jobBonus.GetJobValueFromList(job, 1f);
            return true;
        }
        public static bool TryGetValue(this Tool tool, ThingDef billGiver, out float val, ToolType toolType = null)
        {
            val = 0f;
            if (tool == null)
                return false;
            if (toolType == null && !Dictionaries.billGiverToolType.TryGetValue(billGiver, out toolType))
                return false;
            if (!tool.TryGetValue(toolType, out val))
                return false;
            var toolProp = tool.ToolProperties;
            val *= toolProp.billGiverBonus.GetBillGiverValueFromList(billGiver, 1f);
            return true;
        }
        public static bool TryGetValue(this Tool tool, ThingDef billGiver, StatDef stat, out float factor, out float offset, ToolType toolType = null)
        {
            factor = 1f;
            offset = 0f;
            if (tool == null)
                return false;
            if (toolType == null && !Dictionaries.billGiverToolType.TryGetValue(billGiver, out toolType))
                return false;
            if (!tool.TryGetValue(billGiver, out var val, toolType))
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
        public static bool TryGetValue(this Tool tool, JobDef job, StatDef stat, out float factor, out float offset, ToolType toolType = null)
        {
            factor = 1f;
            offset = 0f;
            if (tool == null)
                return false;
            if (toolType == null && !Dictionaries.jobToolType.TryGetValue(job, out toolType))
                return false;
            if (!tool.TryGetValue(job, out var val, toolType))
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
        public static bool TryGetValue(this Tool tool, ToolType toolType, StatDef stat, out float factor, out float offset)
        {
            factor = 1f;
            offset = 0f;
            if (tool == null || !tool.TryGetValue(toolType, out var val))
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
        public static float GetValue(this Tool tool, ToolType toolType, float fallback = 0f)
        {
            if (tool.TryGetValue(toolType, out float val))
                return val;
            return fallback;
        }
        public static float GetValue(this Tool tool, JobDef job, float fallback = 0f)
        {
            if (tool.TryGetValue(job, out float val))
                return val;
            return fallback;
        }
        public static bool IsForced(this Tool tool)
        {
            if (!(tool.HoldingPawn is Pawn pawn) || !pawn.CanUseTools(out var tracker))
                return false;
            return tracker.forcedHandler.ForcedTools.Contains(tool);
        }
        public static bool CanDrop(this Tool tool) => !tool.IsForced();
        #endregion
    }
}
