using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ToolsFramework
{
    public static class Dictionaries
    {
        public static Dictionary<JobDef, ToolType> jobToolType = new Dictionary<JobDef, ToolType>();
        public static Dictionary<(JobDef job, StatDef stat), ToolType> jobStatToolType = new Dictionary<(JobDef, StatDef), ToolType>();
        public static Dictionary<Map, Map_ToolTracker> MapToolTrackers = new Dictionary<Map, Map_ToolTracker>();
        public static Dictionary<StatDef, float> StatPartSettings = new Dictionary<StatDef, float>();
        public static Dictionary<Pawn, Pawn_ToolTracker> PawnToolTrackers = new Dictionary<Pawn, Pawn_ToolTracker>();
    }
}
