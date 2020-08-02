using HugsLib;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace ToolsFramework
{
    internal class Controller : ModBase
    {
        public override void DefsLoaded()
        {
            foreach (ThingDef tDef in DefDatabase<ThingDef>.AllDefs.Where(t => t.race?.Humanlike == true))
            {
                if (tDef.comps == null)
                    tDef.comps = new List<CompProperties>();
                tDef.comps.Add(new CompProperties(typeof(Pawn_ToolTracker)));
            }
            // Temp
            foreach (var tDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.stuffProps != null))
                tDef.stuffProps.statFactors.Add(new StatModifier() { stat = StatDefOf.Tool_Hardness, value = Random.Range(0.5f,1.5f) });
        }
    }
}
