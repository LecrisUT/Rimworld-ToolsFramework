using System.Linq;
using ToolsFramework.Harmony;
using Verse;

namespace ToolsFramework
{
    public static class RimworldExtensions
    {
        public static bool IsTool(this BuildableDef def)
            => def is ToolDef || (def is ThingDef tdef && typeof(Tool).IsAssignableFrom(tdef.thingClass));
        public static bool IsTool(this BuildableDef def, out ToolProperties toolProperties)
        {
            toolProperties = null;
            if (def is ToolDef toolDef)
            {
                toolProperties = toolDef.ToolProperties;
                return toolProperties != null;
            }
            if (def is ThingDef thingDef && typeof(Tool).IsAssignableFrom(thingDef.thingClass))
            {
                toolProperties = thingDef.GetModExtension<ToolProperties>();
                return toolProperties != null;
            }
            return false;
        }
        public static bool CanUseTools(this Pawn pawn)
            => pawn.CanUseTools(out _);
        public static bool CanUseTools(this Pawn pawn, out Pawn_ToolTracker tracker)
            => (tracker = pawn.TryGetComp<Pawn_ToolTracker>()) != null;
        public static void EquipTool(this Pawn pawn)
        {
            if (pawn.CanUseTools(out var tracker))
                pawn.EquipTool(tracker.toolInUse);
        }
        public static void EquipTool(this Pawn pawn, Tool tool)
        {
            if (!pawn.CanUseTools(out var tracker) || !tracker.usedHandler.HeldTools.Contains(tool))
                return;
            bool equipTool = true;
            var equipment = pawn.equipment;
            tracker.memoryEquipment = equipment.Primary;
            // tracker.transfering = true;
            if (equipment.TryGetOffHandEquipment(out var offEquipment))
            {
                tracker.memoryEquipmentOffHand = offEquipment;
                if (tool != offEquipment)
                    equipment.GetDirectlyHeldThings().TryTransferToContainer(offEquipment, pawn.inventory.innerContainer, false);
                else
                    equipTool = false;
            }
            if (tracker.memoryEquipment != null)
            {
                if (tool != tracker.memoryEquipment)
                    equipment.GetDirectlyHeldThings().TryTransferToContainer(tracker.memoryEquipment, pawn.inventory.innerContainer, false);
                else
                    equipTool = false;
            }
            if (equipTool)
                pawn.inventory.innerContainer.TryTransferToContainer(tool, equipment.GetDirectlyHeldThings());
            // tracker.transfering = false;
        }
        public static void DeequipTool(this Pawn pawn)
        {
            if (!pawn.CanUseTools(out var tracker))
                return;
            var tool = tracker.toolInUse;
            var equipment = pawn.equipment;
            var mainEquipment = tracker.memoryEquipment;
            var offhandEquipment = tracker.memoryEquipmentOffHand;
            // tracker.transfering = true;
            if (tracker.memoryEquipment != tool && tracker.memoryEquipmentOffHand != tool)
            {
                if (tracker.usedHandler.HeldTools.Contains(tool))
                    equipment.TryTransferEquipmentToContainer(tool, pawn.inventory.innerContainer);
                if (mainEquipment != null && pawn.inventory.Contains(mainEquipment))
                    pawn.inventory.innerContainer.TryTransferToContainer(mainEquipment, equipment.GetDirectlyHeldThings());

            }
            if (offhandEquipment != null)
            {
                if (!equipment.Contains(offhandEquipment))
                {
                    pawn.inventory.innerContainer.TryTransferToContainer(offhandEquipment, equipment.GetDirectlyHeldThings());
                    equipment.AddOffHandEquipment(offhandEquipment);
                }
                else
                {
                    pawn.inventory.innerContainer.TryTransferToContainer(mainEquipment, equipment.GetDirectlyHeldThings());
                    equipment.AddOffHandEquipment(mainEquipment);
                }
            }
            // tracker.transfering = false;
        }
        public static bool HasDamagedTools(this Pawn pawn)
        {
            if (!pawn.CanUseTools(out var tracker))
                return false;
            if (tracker.usedHandler.UsedTools.Any(t => t.LifeSpan <= Settings.alertToolNeedsReplacing_Treshold))
                return true;
            return false;
        }
        public static void DropTool(this Pawn pawn, Tool tool)
        {
            if (pawn.inventory.innerContainer.Contains(tool))
                pawn.inventory.innerContainer.TryDrop(tool, ThingPlaceMode.Direct, out _);
            else if (pawn.equipment.Contains(tool))
                pawn.equipment.TryDropEquipment(tool, out _, pawn.PositionHeld, false);
        }
    }
}
