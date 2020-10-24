using System.Collections.Generic;
using System.Linq;
using ToolsFramework.Harmony;
using RimWorld;
using Verse;
using Verse.AI;

namespace ToolsFramework
{
    public static class RimworldExtensions
    {
        public static Pawn HoldingPawn(this IThingHolder holder, bool includeCarryTracker = false)
        {
            switch (holder)
            {
                case Pawn_EquipmentTracker eq:
                    return eq.pawn;
                case Pawn_InventoryTracker inv:
                    return inv.pawn;
                case Pawn_ApparelTracker ap:
                    return ap.pawn;
                case Pawn_CarryTracker carry:
                    return includeCarryTracker ? carry.pawn : null;
                default:
                    return null;
            }
        }
        public static bool IsTool(this BuildableDef def)
            => def is ThingDef tdef && tdef.IsTool();
        public static bool IsTool(this ThingDef def)
            => typeof(ITool).IsAssignableFrom(def.thingClass) || (!Settings.AllITool && def.HasComp(typeof(ToolComp)));
        public static bool IsTool(this ThingDef def, out CompProperties_Tool compProp)
        {
            compProp = def.GetCompProperties<CompProperties_Tool>();
            return compProp != null;
        }
        public static bool IsTool(this Thing thing, bool addToDictionary = false)
            => thing is ThingWithComps twc && twc.IsTool(addToDictionary);
        public static bool IsTool(this Thing thing, out ToolComp comp, bool addToDictionary = false)
        {
            if (!(thing is ThingWithComps twc))
            {
                comp = null;
                return false;
            }
            return twc.IsTool(out comp, addToDictionary);
        }
        public static bool IsTool(this ThingWithComps thing, bool addToDictionary = false)
            => thing is ITool || (!Settings.AllITool && thing.IsTool(out _, addToDictionary));
        public static bool IsTool(this ThingWithComps thing, out ToolComp comp, bool addToDictionary = true)
        {
            if (thing is ITool tool)
                comp = tool.ToolComp;
            else if (Settings.AllITool)
            {
                comp = null;
                return false;
            }
            else if (!Dictionaries.ThingTools.TryGetValue(thing, out comp))
            {
                comp = thing.TryGetComp<ToolComp>();
                if (comp != null || addToDictionary)
                {
                    if (Dictionaries.ThingTools.Count == Settings.DictionaryCache)
                        Dictionaries.ThingTools.Clear();
                    Dictionaries.ThingTools.Add(thing, comp);
                }
            }
            return comp != null;
        }
        public static bool ToolIsForbidden(this ThingWithComps tool, Pawn pawn, ThingFilter filter)
            => tool.IsForbidden(pawn) || !filter.Allows(tool);
        public static bool ToolIsForbidden(this ThingWithComps tool, Pawn pawn, ReservationManager reservation, Faction faction = null)
            => tool.IsForbidden(pawn) || reservation.IsReservedByAnyoneOf(tool, faction ?? pawn.Faction);
        public static bool ToolIsForbidden(this ThingWithComps tool, Pawn pawn, ThingFilter filter, ReservationManager reservation, Faction faction = null)
            => tool.IsForbidden(pawn) || !filter.Allows(tool) || reservation.IsReservedByAnyoneOf(tool, faction ?? pawn.Faction);
        public static CompProperties_Tool ToolCompProp(this ThingDef tdef)
            => tdef.GetCompProperties<CompProperties_Tool>();
        public static CompProperties_Tool ToolCompProp(this ThingWithComps thing, bool addToDictionary = true)
            => thing.ToolComp(addToDictionary)?.CompProp;
        public static IEnumerable<ToolType> ToolTypes(this ThingWithComps thing, bool addToDictionary = true)
            => thing.ToolComp(addToDictionary)?.CompProp.ToolTypes;
        public static ToolComp ToolComp(this ThingWithComps thing, bool addToDictionary = true)
        {
            if (thing is ITool tool)
                return tool.ToolComp;
            if (Settings.AllITool)
                return null;
            if (thing.IsTool(out var comp, addToDictionary))
                return comp;
            return null;
        }
        public static bool TryGetMapToolTracker(this Map map, out Map_ToolTracker tracker)
        {
            tracker = null;
            if (map == null)
                return false;
            tracker = map.GetMapToolTracker();
            return tracker != null;
        }
        public static Map_ToolTracker GetMapToolTracker(this Map map)
        {
            if (!Dictionaries.MapToolTrackers.TryGetValue(map, out var tracker))
            {
                tracker = map.GetComponent<Map_ToolTracker>();
                Dictionaries.MapToolTrackers.Add(map, tracker);
            }
            return tracker;
        }
        //public static bool CanUseTools(this Pawn pawn, bool addToDictionary = true)
        //    => pawn.CanUseTools(out _, addToDictionary);
        public static bool CanUseTools(this Pawn pawn, out Pawn_ToolTracker tracker, bool addToDictionary = true)
        {
            if (pawn == null || !Dictionaries.PawnCanUseTools.Contains(pawn.kindDef.race))
            {
                tracker = null;
                return false;
            }
            if (!Dictionaries.PawnToolTrackers.TryGetValue(pawn, out tracker))
            {
                tracker = pawn.GetComp<Pawn_ToolTracker>();
                if (tracker != null || addToDictionary)
                {
                    if (Dictionaries.PawnToolTrackers.Count == Settings.DictionaryCache)
                        Dictionaries.PawnToolTrackers.Clear();
                    Dictionaries.PawnToolTrackers.Add(pawn, tracker);
                }
            }
            return tracker != null;
        }
        public static void EquipTool(this Pawn pawn, Pawn_ToolTracker tracker)
        {
            if (tracker == null && !pawn.CanUseTools(out tracker))
                return;
            pawn.EquipTool(tracker.toolInUse, tracker);
        }
        public static void EquipTool(this Pawn pawn, ThingWithComps tool, Pawn_ToolTracker tracker = null)
        {
            if (tracker == null && !pawn.CanUseTools(out tracker))
                return;
            if (!tracker.UsedHandler.HeldTools.Contains(tool))
                return;
            if (tool is Apparel ap)
            {
                var apparel = pawn.apparel;
                if (apparel.Contains(ap))
                    tracker.memoryEquipment.Add(ap);
                else
                {
                    var body = pawn.RaceProps.body;
                    var apThingOwner = apparel.GetDirectlyHeldThings();
                    foreach (var otherAp in apparel.WornApparel.ToList())
                        if (!ApparelUtility.CanWearTogether(ap.def, otherAp.def, body))
                        {
                            tracker.memoryEquipment.Add(otherAp);
                            apThingOwner.TryTransferToContainer(otherAp, pawn.inventory.innerContainer);
                        }
                    pawn.inventory.innerContainer.TryTransferToContainer(ap, apThingOwner);
                }
            }
            else
            {
                bool equipTool = true;
                var equipment = pawn.equipment;
                var primary = equipment.Primary;
                if (primary != null)
                {
                    tracker.memoryEquipment.Add(primary);
                    if (equipment.TryGetOffHandEquipment(out var offEquipment))
                    {
                        tracker.memoryEquipment.Add(offEquipment);
                        if (tool != offEquipment)
                            equipment.GetDirectlyHeldThings().TryTransferToContainer(offEquipment, pawn.inventory.innerContainer, false);
                        else
                            equipTool = false;
                    }
                    if (primary != tool)
                        equipment.GetDirectlyHeldThings().TryTransferToContainer(primary, pawn.inventory.innerContainer, false);
                    else
                        equipTool = false;
                }
                if (equipTool)
                    pawn.inventory.innerContainer.TryTransferToContainer(tool, equipment.GetDirectlyHeldThings());
            }
        }
        public static void DeequipTool(this Pawn pawn, Pawn_ToolTracker tracker = null)
        {
            if (tracker == null && !pawn.CanUseTools(out tracker))
                return;
            var tool = tracker.toolInUse;
            if (tool is Apparel ap)
            {
                var apparel = pawn.apparel;
                var apThingOwner = apparel.GetDirectlyHeldThings();
                if (!ap.Destroyed && !tracker.memoryEquipment.Contains(ap))
                    apThingOwner.TryTransferToContainer(ap, pawn.inventory.innerContainer);
                foreach (var eq in tracker.memoryEquipment)
                    if (!apparel.Contains(eq))
                        pawn.inventory.innerContainer.TryTransferToContainer(eq, apThingOwner);
            }
            else
            {
                var equipment = pawn.equipment;
                if (!tool.DestroyedOrNull() && !tracker.memoryEquipment.Contains(tool))
                    equipment.TryTransferEquipmentToContainer(tool, pawn.inventory.innerContainer);
                foreach (var eq in tracker.memoryEquipment)
                {
                    if (eq == equipment.Primary || eq == tool)
                        continue;
                    bool flag = equipment.Primary != null;
                    pawn.inventory.innerContainer.TryTransferToContainer(eq, equipment.GetDirectlyHeldThings());
                    if (flag)
                        equipment.AddOffHandEquipment(eq);
                }
            }
        }
        public static bool HasDamagedTools(this Pawn pawn)
        {
            if (pawn == null || !pawn.CanUseTools(out var tracker))
                return false;
            if (tracker.UsedHandler.UsedToolInfos?.Any(t => t.comp.LifeSpan <= Settings.alertToolNeedsReplacing_Treshold) ?? false)
                return true;
            return false;
        }
        public static void DropTool(this Pawn pawn, ThingWithComps tool)
        {
            if (pawn.inventory.innerContainer.Contains(tool))
                pawn.inventory.innerContainer.TryDrop(tool, ThingPlaceMode.Direct, out _);
            else if (pawn.equipment.Contains(tool))
                pawn.equipment.TryDropEquipment(tool, out _, pawn.PositionHeld, false);
        }
    }
}
