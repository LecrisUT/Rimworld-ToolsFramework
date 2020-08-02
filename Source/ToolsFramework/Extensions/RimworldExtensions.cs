using System.Linq;
using ToolsFramework.Harmony;
using Verse;

namespace ToolsFramework
{
    public static class RimworldExtensions
    {
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
            if (!pawn.CanUseTools(out var tracker))
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
            // tracker.transfering = true;
            if (tracker.memoryEquipment != tool && tracker.memoryEquipmentOffHand != tool)
            {
                equipment.TryTransferEquipmentToContainer(tool, pawn.inventory.innerContainer);
                if (tracker.memoryEquipment != null)
                    pawn.inventory.innerContainer.TryTransferToContainer(tracker.memoryEquipment, equipment.GetDirectlyHeldThings());

            }
            if (tracker.memoryEquipmentOffHand != null)
            {
                if (!equipment.Contains(tracker.memoryEquipmentOffHand))
                    equipment.AddOffHandEquipment(tracker.memoryEquipmentOffHand);
                else
                    equipment.AddOffHandEquipment(tracker.memoryEquipment);
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
    }
}
