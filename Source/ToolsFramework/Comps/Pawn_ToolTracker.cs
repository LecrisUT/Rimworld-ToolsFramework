using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using HarmonyLib;
using UnityEngine;

namespace ToolsFramework
{
    public class Pawn_ToolTracker : ThingComp
    {
        private Pawn Pawn => (Pawn)parent;

        public Tool toolInUse = null;
        public List<Tool> memoryTool = new List<Tool>();
        public ThingWithComps memoryEquipment = null;
        public ThingWithComps memoryEquipmentOffHand = null;

        public int rareTick = 0;
        public bool transfering = false;

        private ToolAssignment toolAssignment;
        public ToolAssignment ToolAssignment
        {
            get
            {
                if (toolAssignment == null)
                    toolAssignment = Current.Game.GetComponent<ToolAssignment_Database>().DefaultAssignment();
                return toolAssignment;
            }
            set => toolAssignment = value;
        }

        public ToolsUsedHandler usedHandler = new ToolsUsedHandler();
        public ToolsForcedHandler forcedHandler = new ToolsForcedHandler();
        public bool dirtyCache_necessaryToolTypes = true;
        private HashSet<ToolType> necessaryToolTypes = new HashSet<ToolType>();
        public IEnumerable<ToolType> NecessaryToolTypes
        {
            get
            {
                if (dirtyCache_necessaryToolTypes)
                    UpdateNecessaryToolTypes();
                return necessaryToolTypes;
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            if (Pawn.workSettings is RimWorld.Pawn_WorkSettings wSet && wSet.EverWork)
                UpdateNecessaryToolTypes();
        }

        public override void CompTickRare()
        {
            if (rareTick++ < 0)
                rareTick = 0;
            if (Settings.optimization && Pawn.workSettings.EverWork && rareTick % Settings.optimizationDelay == 0 &&
                !NecessaryToolTypes.EnumerableNullOrEmpty() && Pawn.jobs is Pawn_JobTracker jobTracker &&
                Pawn.MapHeld?.GetComponent<Map_ToolTracker>() is Map_ToolTracker mapTracker && !mapTracker.StoredTools.EnumerableNullOrEmpty() &&
                jobTracker.curJob.def != JobDefOf.OptimizeTools &&
                !jobTracker.jobQueue.Any(t => t.job.def == JobDefOf.OptimizeTools))
            {
                var queue = new List<Job>();
                var toolsToPick = new List<Tool>();
                foreach (var tool in usedHandler.HeldToolsList.Where(t => !usedHandler.BestTool.ContainsValue(t)))
                {
                    Job job = Pawn.PutAwayTool(tool);
                    if (job != null)
                        queue.Add(job);
                }
                foreach (var toolType in NecessaryToolTypes)
                {
                    if (mapTracker.BestTools.TryGetValue(toolType, out var tool) &&
                        (!usedHandler.BestTool.TryGetValue(toolType, out var currTool) || tool[toolType] > currTool[toolType]))
                    {
                        toolsToPick.Add(tool);
                        if (currTool != null)
                        {
                            Job job = Pawn.PutAwayTool(tool);
                            if (job != null)
                                queue.Add(job);
                        }
                    }
                }
                var optimizeJob = Pawn.TakeTool(toolsToPick);
                if (optimizeJob != null)
                {
                    optimizeJob.def = JobDefOf.OptimizeTools;
                    queue.Add(optimizeJob);
                }
                foreach (var job in queue)
                    jobTracker.jobQueue.EnqueueLast(job);
                rareTick += Rand.Range(0, Mathf.CeilToInt(Settings.optimizationDelay / 1000));
            }
        }
        public void UpdateNecessaryToolTypes()
        {
            necessaryToolTypes = new HashSet<ToolType>();
            foreach (var workGiver in Pawn.workSettings.WorkGiversInOrderNormal.Select(t => t.def))
                if (Utility.ToolWorkGivers.TryGetValue(workGiver, out var toolTypes))
                    foreach (var toolType in toolTypes.Where(t => !necessaryToolTypes.Contains(t)))
                        necessaryToolTypes.Add(toolType);
            dirtyCache_necessaryToolTypes = false;
        }
        public override void PostExposeData()
        {
            Scribe_References.Look(ref toolInUse, "toolInUse");
            Scribe_Collections.Look(ref memoryTool, "memoryTool");
            Scribe_References.Look(ref memoryEquipment, "memoryEquipment");
            Scribe_References.Look(ref memoryEquipmentOffHand, "memoryEquipmentOffHand");
            Scribe_Deep.Look(ref toolAssignment, "toolAssignment", new object[0]);
            Scribe_Values.Look(ref rareTick, "rareTick", 0);
            Scribe_Deep.Look(ref usedHandler, "usedHandler");
            Scribe_Deep.Look(ref forcedHandler, "forecedHandler");
            Scribe_Collections.Look(ref necessaryToolTypes, "necessaryToolTypes", LookMode.Def);
        }
    }
}
