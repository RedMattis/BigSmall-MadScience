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
        public int maxChain = 1000;
    }

    public class RandyEventHediff : HediffWithComps
    {
        public static RandyEventHediff ChainTriggerer;
        public static int ProcessingChainIdx = 0;
        public ModExtension_RandomEventProps Props => def.GetModExtension<ModExtension_RandomEventProps>();
        protected override void OnStageIndexChanged(int stageIndex)
        {
            ChainTriggerer ??= this;
            if (Props != null)
            {
                var stages = def.stages;
                var resetSeverity = stages[Props.stageToGoToOnTrigger].minSeverity + 0.01f;
                var triggerSeverity = stages[Props.triggerStage].minSeverity;
                float triggerToRestDist = triggerSeverity - resetSeverity;
                //float severityStart = Severity;
                while (ProcessingChainIdx < Props.maxChain && stageIndex >= Props.triggerStage && Severity > 0)
                {
                    ProcessingChainIdx++;
                    var totalChance = Props.eventTables.Sum(x => x.chance);
                    var roll = Rand.Value * totalChance;
                    var currentChance = 0f;
                    foreach (var eventTable in Props.eventTables)
                    {
                        currentChance += eventTable.chance;
                        if (roll <= currentChance)
                        {
                            try
                            {
                                eventTable.eventTable.DoEvent(pawn);
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"Error while executing event table {eventTable.eventTable}: {ex}");
                            }
                            break;
                        }
                    }

                    if (stages == null || Props.stageToGoToOnTrigger > stages.Count)
                    {
                        severityInt = 0;
                        break;
                    }
                    else
                    {
                        if (ProcessingChainIdx >= Props.maxChain)
                        {
                            severityInt = resetSeverity;
                        }
                        else
                        {
                            float newSeverity = Severity - triggerToRestDist;
                            severityInt = newSeverity;
                        }
                    }
                    if (severityInt < triggerSeverity)
                    {
                        break;
                    }
                }
                if (CurStageIndex != stageIndex)
                {
                    OnStageIndexChanged(CurStageIndex);
                }
            }
            if (ChainTriggerer == this)
            {
                ChainTriggerer = null;
                ProcessingChainIdx = 0;
            }
        }
    }
}
