using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BigAndSmall
{
    public class ModExtension_RandomEventProps : DefModExtension
    {
        public List<EventTableChance> eventTables;
        public int triggerStage = 1;
        public int stageToGoToOnTrigger = 0;
    }

    public class RandyEventHediff : HediffWithComps
    {
        public ModExtension_RandomEventProps Props => def.GetModExtension<ModExtension_RandomEventProps>();
        protected override void OnStageIndexChanged(int stageIndex)
        {
            if (Props != null && stageIndex >= Props.triggerStage && Severity > 0)
            {
                var totalChance = Props.eventTables.Sum(x => x.chance);
                var roll = Rand.Value * totalChance;
                var currentChance = 0f;
                foreach (var eventTable in Props.eventTables)
                {
                    currentChance += eventTable.chance;
                    if (roll <= currentChance)
                    {
                        eventTable.eventTable.DoEvent(pawn);
                        break;
                    }
                }
                var stages = def.stages;
                if (stages == null || Props.stageToGoToOnTrigger > stages.Count)
                {
                    Severity = 0;
                }
                else
                {
                    Severity = stages[Props.stageToGoToOnTrigger].minSeverity + 0.02f;
                }
            }
        }
    }
}
