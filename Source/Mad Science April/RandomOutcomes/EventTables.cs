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
        public IntRange triggerRange = new IntRange(1, 4);
        public bool excludeTarget;
        public bool prioritiseHighestValue;
        public bool pPrioritiseLowestValue;

        public bool DoEvent(Pawn target)
        {
            float totalChance = eventTriggers.Sum(x => x.chance);
            float roll = Rand.Value * totalChance;
            float currentChance = 0;
            bool success = false;
            foreach (var eTrigger in eventTriggers)
            {
                currentChance += eTrigger.chance;
                if (roll <= currentChance)
                {

                    if (Rand.Chance(chanceToBeAoe))
                    {
                        int actualRange = triggerRange.RandomInRange;
                        var pawnsInRange = target.Map.mapPawns.AllPawns.Where(x => x.Position.DistanceTo(target.Position) <= actualRange).ToList();
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
