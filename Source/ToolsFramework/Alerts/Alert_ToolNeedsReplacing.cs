using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ToolsFramework
{
    public class Alert_ToolNeedsReplacing : Alert
    {

        public Alert_ToolNeedsReplacing()
        {
            defaultPriority = AlertPriority.Medium;
            nextAlertTick = Find.TickManager.TicksGame;
        }
        private List<Pawn> culpritsResult = new List<Pawn>();

        private List<Pawn> WorkersDamagedTools
        {
            get
            {
                culpritsResult.Clear();
                foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsSpawned.Where(t => t.HasDamagedTools()))
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
        private int nextAlertTick = 0;
        private AlertReport Report
        {
            get
            {
                if (Find.TickManager.TicksGame > nextAlertTick)
                {
                    cachedReport = AlertReport.CulpritsAre(WorkersDamagedTools);
                    nextAlertTick = Find.TickManager.TicksGame + Settings.alertToolNeedsReplacing_Delay;
                }
                return cachedReport;
            }
        }
        public override AlertReport GetReport() =>
            (Settings.degradation && Settings.alertToolNeedsReplacing) ? Report : false;
    }
}
