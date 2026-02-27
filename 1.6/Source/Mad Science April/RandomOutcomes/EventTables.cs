using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BigAndSmall
{
    public class RandomEventTable : Def
    {
        public List<EventTrigger> eventTriggers;

        public float chanceToBeAoe = 0f;
        public IntRange triggerRange = new IntRange(1, 2);
        public bool excludeTarget;
        public bool prioritiseHighestValue;
        public bool pPrioritiseLowestValue;

        public bool DoEvent(Pawn target)
        {
            if (target == null) return false;
            float totalChance = eventTriggers.Sum(x => x.chance);
            float roll = Rand.Value * totalChance;
            float currentChance = 0;
            bool success = false;
            var targetPos = target.Position;
            var pawnsOnMap = target.Map?.mapPawns?.AllPawns;
            if (pawnsOnMap == null) return false;
            foreach (var eTrigger in eventTriggers)
            {
                currentChance += eTrigger.chance;
                if (roll <= currentChance)
                {

                    if (Rand.Chance(chanceToBeAoe))
                    {
                        int actualRange = triggerRange.RandomInRange;
                        var pawnsInRange = pawnsOnMap.Where(x => x?.Destroyed == false && x.Map != null  && x.Position.DistanceTo(targetPos) <= actualRange).ToList();
                        if (excludeTarget)
                        {
                            pawnsInRange.Remove(target);
                        }
                        if (prioritiseHighestValue)
                        {
                            pawnsInRange = pawnsInRange.OrderByDescending(x => x.MarketValue).ToList();
                        }
                        else if (pPrioritiseLowestValue)
                        {
                            pawnsInRange = pawnsInRange.OrderBy(x => x.MarketValue).ToList();
                        }
                        foreach (var pawn in pawnsInRange)
                        {
                            success = eTrigger.outcome.DoOutcome(pawn);
                        }
                    }
                    else
                    {
                        success = eTrigger.outcome.DoOutcome(target);
                    }
                    if (success) return true;
                }
            }
            return false;
        }
    }

    public class EventTableChance
    {
        public RandomEventTable eventTable;
        public float chance;

        public void LoadDataFromXmlCustom(System.Xml.XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(eventTable), xmlRoot.Name);
            chance = float.TryParse(xmlRoot.FirstChild.Value, out float result) ? result : 0;
        }
    }
}
