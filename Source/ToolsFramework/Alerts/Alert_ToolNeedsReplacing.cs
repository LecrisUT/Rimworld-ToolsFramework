using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ToolsFramework
{
    public class Alert_ToolNeedsReplacing : Alert
    {

        public Alert_ToolNeedsReplacing()
        {
            defaultPriority = AlertPriority.Medium;
        }
        private List<Pawn> culpritsResult = new List<Pawn>();

        private List<Pawn> WorkersDamagedTools
        {
            get
            {
                culpritsResult.Clear();
                foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsSpawned.Where(t=>t.HasDamagedTools()))
                    culpritsResult.Add(pawn);
                return culpritsResult;
            }
        }

        public override TaggedString GetExplanation()
        {
            string result = "ToolNeedsReplacingDesc".Translate() + ":\n";
            foreach (Pawn pawn in WorkersDamagedTools)
                result += ("\n    " + pawn.LabelShort);
            return result;
        }

        public override string GetLabel() =>
            "ToolsNeedReplacing".Translate();

        private AlertReport cachedReport = false;
        private int tick = 0;
        private AlertReport Report
        {
            get
            {
                if (++tick < 0)
                    tick = 0;
                if (tick % Settings.alertToolNeedsReplacing_Delay == 0)
                {
                    cachedReport = AlertReport.CulpritsAre(WorkersDamagedTools);
                    tick += Rand.Range(0, Mathf.CeilToInt(Settings.alertToolNeedsReplacing_Delay / 1000));
                }
                return cachedReport;
            }
        }
        public override AlertReport GetReport() =>
            (Settings.degradation && Settings.alertToolNeedsReplacing) ? Report : false;
    }
}
