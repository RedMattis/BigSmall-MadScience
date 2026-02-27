using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse.Sound;
using Verse;
using HarmonyLib;
using static RimWorld.PsychicRitualRoleDef;

namespace BigAndSmall
{
    public class EventOutcome : Def
    {
        // System
        public bool failOnNonhumanlike = true;
        public bool failOnNoAnomalyDLC = false;

        // Simple
        public FloatRange? takeRDamage;
        public IntRange? stunDamage;

        // Very Random
        public IntRange? randomEndogenes;
        public IntRange? randomXenogenes;
        public IntRange? randomEndogenesAny;
        public IntRange? randomXenogenesAny;
        public IntRange? randomEndogenesAchite;
        public IntRange? randomXenogenesAchite;
        public bool randomXenotype;
        public bool randomXenotypeArchite;
        public bool randomizeTraits;
        public bool randomizeSkills;
        public bool appendRaceGene;
        public bool replaceRace;
        public bool giveSizeGene;
        // public bool turnIntoRobot; // Use giveXenotypeFromList instead.
        public bool swapGender;
        public Gender? makeGender;
        public FloatRange? randomizeAge;
        public bool makeUndead;
        public bool makeAnimal;
        public bool makeColonyAnimal;
        public bool makeBerserkAnimal;
        public bool makeManhuntingAnimal;
        public bool makeMech;
        public bool makeColonyMech;
        public bool makeSapientAnimal;
        public bool makeHumanlike;
        public bool turnIntoRandomThing;
        public bool turnIntoRanomedMinifiedThing;
        public bool turnIntoRandomResource;
        public bool kill;
        public bool explode;
        public bool giveRandomHediff;
        public IntRange? fuseWithPawns;
        public bool swapApparelStuffables;
        public bool giveRandomMentalBreak;

        public bool doEmpExplosionTiny;
        public bool doEmpExplosionSmall;
        public bool doEmpExplosionLarge;
        public bool makePregnantSane;
        public bool makePregnantInsane;

        public IntRange? deadLifeDustSame;
        public IntRange? deadLifeDustPlayer;
        public IntRange? deadLifeDustHorax;

        public IntRange? spawnAnimalThreat;
        public IntRange? spawnAnimalThreatManhunter;

        public FloatRange? organsBeGone;

        // Controlled Random
        public List<HediffDef> giveARandomHediff;
        public List<GeneDef> giveEndogeneFromList;
        public List<GeneDef> giveXenogeneFromList;
        public List<GeneDef> giveGeneFromList;
        public List<GeneDef> giveAllGenesFromList;
        public List<XenotypeDef> giveXenotypeFromList;
        public List<TraitDef> giveTraitFromList;
        public List<MentalBreakDef> giveMentalBreakFromList;
        public List<ThingDef> turnIntoThingFromList;  // Add Go Juice to this list.
        public List<ThingDef> turnIntoMinifiedThingFromList;

        public List<PawnKindDef> spawnKinds;
        public List<PawnKindDef> spawnKindsManhunter;
        public List<PawnKindDef> spawnKindsAnyMutualHostile;
        

        // Meta Results (triggers other events)
        public List<RandomEventTable> followUpEvents;

        public virtual bool DoOutcome(Pawn pawn)
        {
            if (pawn?.Spawned == false) return false;
            try
            {
                DoOutcomeInner(pawn);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
        protected virtual bool DoOutcomeInner(Pawn pawn)
        {
            if (failOnNoAnomalyDLC && !ModLister.HasActiveModWithName("Anomaly"))
            {
                return false;
            }
            if (pawn?.Position == null || pawn.Map == null) return false;
            var pawnPos = pawn.Position;
            var pawnMap = pawn.Map;

            var humanLikeWithGenes = pawn?.genes != null && pawn?.RaceProps.Humanlike == true;
            if (failOnNonhumanlike && !humanLikeWithGenes) return false;
            if (!ValidTarget(pawn)) return true;

            if (true)
            {
                SoundDefOf.Interact_BeatFire.PlayOneShot(new SoundInfo() { volumeFactor = 1f, pitchFactor = 0.7f });
            }

            if (doEmpExplosionLarge)
            {
                GenExplosion.DoExplosion(pawnPos, pawnMap, 4, DamageDefOf.EMP, pawn, damAmount: 50);
            }
            else if (doEmpExplosionSmall)
            {
                GenExplosion.DoExplosion(pawnPos, pawnMap, 2, DamageDefOf.EMP, pawn, damAmount: 10);
            }
            else if (doEmpExplosionTiny)
            {
                GenExplosion.DoExplosion(pawnPos, pawnMap, 1, DamageDefOf.EMP, pawn, damAmount: 5);
            }

            if (!ValidTarget(pawn)) return true;

            if (spawnAnimalThreat != null)
            {
                float threatAmount = spawnAnimalThreat.Value.RandomInRange;
                var animalkinds = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.race?.race.Animal == true && x.combatPower < (threatAmount / 2));
                if (animalkinds.Any())
                {
                    var animalkind = animalkinds.RandomElement();
                    float combatPower = Mathf.Max(animalkind.combatPower, 35); // Avoid excessive spawning of weak animals.
                    int countToSpawn = (int)(threatAmount / combatPower);
                    countToSpawn = Mathf.Min(6, Mathf.Max(1, countToSpawn));
                    for (int i = 0; i < countToSpawn; i++)
                    {
                        var animal = PawnGenerator.GeneratePawn(animalkind);
                        GenSpawn.Spawn(animal, pawnPos, pawnMap);
                    }
                }
            }
            if (spawnKinds != null || spawnKindsAnyMutualHostile != null || spawnKindsManhunter != null)
            {
                if (spawnKinds != null)
                {
                    foreach (var kind in spawnKinds)
                    {
                        var newPawn = PawnGenerator.GeneratePawn(kind);
                        GenSpawn.Spawn(newPawn, pawnPos, pawnMap);
                    }
                }
                if (spawnKindsManhunter != null)
                {
                    foreach (var kind in spawnKindsManhunter)
                    {
                        var newPawn = PawnGenerator.GeneratePawn(kind);
                        newPawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, forced:true);
                        GenSpawn.Spawn(newPawn, pawnPos, pawnMap);
                    }
                }
                if (spawnKindsAnyMutualHostile != null)
                {
                    var mutuallyHostileFactions = Find.FactionManager.AllFactions.Where(x => x.HostileTo(Faction.OfPlayer) && x.HostileTo(pawn.Faction));
                    Faction mutuallyHostileFaction = null;
                    if (mutuallyHostileFactions.Any())
                    {
                        mutuallyHostileFaction = mutuallyHostileFactions.RandomElement();
                    }
                    foreach(var kind in spawnKindsAnyMutualHostile)
                    {
                        var newPawn = PawnGenerator.GeneratePawn(kind);
                        if (mutuallyHostileFaction != null)
                            newPawn.SetFaction(mutuallyHostileFaction);
                        GenSpawn.Spawn(newPawn, pawnPos, pawnMap);
                    }
                }
            }

            if (deadLifeDustSame != null || deadLifeDustPlayer != null || deadLifeDustHorax != null)
            {
                if (deadLifeDustSame != null)
                    GasUtility.AddDeadifeGas(pawnPos, pawnMap, faction: pawn.Faction, deadLifeDustSame.Value.RandomInRange * 255);
                if (deadLifeDustPlayer != null)
                    GasUtility.AddDeadifeGas(pawnPos, pawnMap, faction: Faction.OfPlayer, deadLifeDustSame.Value.RandomInRange * 255);
                if (deadLifeDustHorax != null)
                {
                    if (Find.FactionManager.FirstFactionOfDef(FactionDefOf.HoraxCult) is Faction cult)
                        GasUtility.AddDeadifeGas(pawnPos, pawnMap, faction: cult, deadLifeDustSame.Value.RandomInRange * 255);
                    else
                        GasUtility.AddDeadifeGas(pawnPos, pawnMap, faction: Faction.OfPlayer, deadLifeDustSame.Value.RandomInRange * 255);
                }
            }

            if (makeUndead)
            {
                MakeUndead(pawn);
            }
            if (giveRandomMentalBreak)
            {
                GiveRandomMentalBreak(pawn);
            }
            if (giveARandomHediff != null)
            {
                GiveHediffFromList(pawn, giveARandomHediff);
            }
            if (giveRandomHediff)
            {
                GiveRandomHediff(pawn);
            }
            if (swapApparelStuffables)
            {
                SwapApparelStuffables(pawn);
            }
            if (giveMentalBreakFromList != null)
            {
                foreach (var mentalBreak in giveMentalBreakFromList)
                {
                    pawn.mindState.mentalBreaker.TryDoMentalBreak("", mentalBreak);
                }
            }
            if (explode)
            {
                Explode(pawn);
            }

            // Terminating outcomes
            if (turnIntoRandomThing || turnIntoThingFromList != null)
            {
                TurnIntoRandomThing(pawn, pawnPos, pawnMap, turnIntoThingFromList);
                return true;
            }
            if (turnIntoRanomedMinifiedThing || turnIntoThingFromList != null)
            {
                TurnIntoRandomMinifiedThing(pawn, pawnPos, pawnMap, turnIntoThingFromList);
                return true;
            }
            if (turnIntoRandomResource)
            {
                TurnIntoRandomResource(pawn, pawnPos, pawnMap);
                return true;
            }
            if (makeAnimal || makeColonyAnimal || makeBerserkAnimal || makeMech || makeColonyMech)
            {
                TransformPawn(pawn, pawnPos, pawnMap);
                return true;
            }
            if (kill)
            {
                pawn.Kill(null);
                return true;
            }



            // Credit to Keyz for this horrible idea.


            // In case it changed.
            if (!ValidTarget(pawn)) return true;
            humanLikeWithGenes = pawn?.genes != null && pawn?.RaceProps.Humanlike == true;
            if (humanLikeWithGenes)
            {
                ProcessRandomGenes(pawn, randomEndogenes, false, x => x.biostatArc == 0);
                ProcessRandomGenes(pawn, randomXenogenes, true, x => x.biostatArc == 0);
                ProcessRandomGenes(pawn, randomEndogenesAny, false, x => true);
                ProcessRandomGenes(pawn, randomXenogenesAny, true, x => true);
                ProcessRandomGenes(pawn, randomEndogenesAchite, false, x => x.biostatArc > 0);
                ProcessRandomGenes(pawn, randomXenogenesAchite, true, x => x.biostatArc > 0);

                if (randomXenotype)
                {
                    SetRandomXenotype(pawn, x => !x.Archite);
                }
                if (randomXenotypeArchite)
                {
                    SetRandomXenotype(pawn, x => x.Archite);
                }
                if (randomizeTraits)
                {
                    RandomizeTraits(pawn);
                }
                if (randomizeSkills)
                {
                    RandomizeSkills(pawn);
                }
                
                if (giveSizeGene)
                {
                    GiveSizeGene(pawn);
                }
                if (swapGender)
                {
                    Genderbender.GenderBend(pawn);
                }
                if (makeGender != null)
                {
                    pawn.gender = makeGender.Value;
                    GenderMethods.UpdateBodyHeadAndBeardPostGenderChange(pawn, force: true);
                }
                if (randomizeAge != null)
                {
                    float maxAge = pawn?.RaceProps.lifeExpectancy ?? 500;
                    if (maxAge == 500 && Rand.Chance(0.05f)) maxAge = 5000;//
                    pawn.ageTracker.AgeBiologicalTicks = (long)(maxAge * randomizeAge.Value.RandomInRange * 3600000);
                }
                if (fuseWithPawns != null)
                {
                    FuseWithPawns(pawn);
                }
                if (giveEndogeneFromList != null)
                {
                    GiveGeneFromList(pawn, giveEndogeneFromList, false, false);
                }
                if (giveXenogeneFromList != null)
                {
                    GiveGeneFromList(pawn, giveXenogeneFromList, true, false);
                }
                if (giveGeneFromList != null)
                {
                    GiveGeneFromList(pawn, giveGeneFromList, Rand.Bool, false);
                }
                if (giveAllGenesFromList != null)
                {
                    GiveGeneFromList(pawn, giveAllGenesFromList, true, true);
                }
                if (giveXenotypeFromList != null)
                {
                    GiveXenotypeFromList(pawn, giveXenotypeFromList);
                }
                if (giveTraitFromList != null)
                {
                    RandomizeTraits(pawn);
                    foreach (var trait in giveTraitFromList)
                    {
                        pawn.story.traits.GainTrait(new Trait(trait));
                    }
                }
                if (appendRaceGene)
                {
                    AppendRaceGene(pawn);
                }
                if (replaceRace)
                {
                    ReplaceRaceGene(pawn);
                }
                if (makeSapientAnimal)
                {
                    MakeSapientAnimal(pawn);
                }
            }
            else
            {
                if (makeHumanlike)
                {
                    TransformPawn(pawn, pawnPos, pawnMap);
                    return false;
                }
            }

            if (ValidTarget(pawn) && takeRDamage != null)
            {
                var damageAmount = Mathf.RoundToInt(pawn.HealthScale * takeRDamage.Value.RandomInRange);
                pawn.TakeDamage(new DamageInfo(DamageDefOf.ElectricalBurn, damageAmount));
            }

            if (ValidTarget(pawn) && stunDamage != null)
            {
                var damageAmount = Mathf.RoundToInt(stunDamage.Value.RandomInRange);
                pawn.TakeDamage(new DamageInfo(DamageDefOf.Stun, damageAmount));

                var damageAmountEmp = Mathf.RoundToInt(stunDamage.Value.RandomInRange);
                pawn.TakeDamage(new DamageInfo(DamageDefOf.EMP, damageAmountEmp));
            }

            if (ValidTarget(pawn) && followUpEvents != null)
            {
                for (int idx = followUpEvents.Count - 1; idx >= 0; idx--)
                {
                    var fEvent = followUpEvents[idx];
                    fEvent.DoEvent(pawn);
                }
            }

            return true;

            static bool ValidTarget(Pawn pawn)
            {
                return pawn.Spawned && !pawn.Destroyed && !pawn.Dead;
            }
        }

        private void ProcessRandomGenes(Pawn pawn, IntRange? geneRange, bool isXeno, Func<GeneDef, bool> predicate)
        {
            if (geneRange != null)
            {
                var allGenes = DefDatabase<GeneDef>.AllDefsListForReading.Where(predicate).Where(x=>!x.defName.ToLower().Contains("morph")).ToList();
                for (int i = 0; i < geneRange.Value.RandomInRange; i++)
                {
                    pawn.genes.AddGene(allGenes.RandomElement(), isXeno);
                }
                Discombobulator.IntegrateGenes(pawn, removeOverriden: true);
            }
        }

        private void SetRandomXenotype(Pawn pawn, Func<XenotypeDef, bool> predicate)
        {
            pawn.genes.SetXenotype(DefDatabase<XenotypeDef>.AllDefsListForReading.Where(predicate).RandomElement());
            Discombobulator.IntegrateGenes(pawn, removeOverriden: true);
        }

        private void RandomizeTraits(Pawn pawn)
        {
            while (pawn.story.traits.allTraits.Count > 0)
            {
                pawn.story.traits.allTraits.Remove(pawn.story.traits.allTraits.RandomElement());
            }
            for (int i = 0; i < Rand.Range(0, 6); i++)
            {
                pawn.story.traits.GainTrait(new Trait(DefDatabase<TraitDef>.AllDefsListForReading.RandomElement()));
            }
        }

        private void RandomizeSkills(Pawn pawn)
        {
            foreach (var skill in pawn.skills.skills)
            {
                if (Rand.Chance(0.5f))
                    skill.Level = Rand.Range(0, 5);
                else if (Rand.Chance(0.5f))
                    skill.Level = Rand.Range(0, 10);
                else
                    skill.Level = Rand.Range(0, 20);
            }
        }

        private void AppendRaceGene(Pawn pawn)
        {
            pawn.genes.AddGene(DefDatabase<GeneDef>.AllDefsListForReading
                .Where(IsRaceSwap).RandomElement(), Rand.Bool);
        }

        private void ReplaceRaceGene(Pawn pawn)
        {
            while (pawn.genes.GenesListForReading.Any(x => IsRaceSwap(x.def)))
            {
                var gene = pawn.genes.GenesListForReading.First(x => IsRaceSwap(x.def));
                pawn.genes.RemoveGene(gene);
            }
            pawn.genes.AddGene(DefDatabase<GeneDef>.AllDefsListForReading
                .Where(IsRaceSwap).RandomElement(), Rand.Bool);
        }

        private void GiveSizeGene(Pawn pawn)
        {
            while (pawn.genes.GenesListForReading.Any(x => x.def.exclusionTags.Intersect(sizeTags).Any()))
            {
                var gene = pawn.genes.GenesListForReading.First(x => x.def.exclusionTags.Intersect(sizeTags).Any());
                pawn.genes.RemoveGene(gene);
            }
            pawn.genes.AddGene(DefDatabase<GeneDef>.AllDefsListForReading
                .Where(x => x.exclusionTags.Intersect(sizeTags).Any()).RandomElement(), Rand.Bool);
        }

        private void MakeUndead(Pawn pawn)
        {
            pawn.health.AddHediff(DefDatabase<HediffDef>.AllDefsListForReading.Where(x => x.defName == "BS_Skeletal").RandomElement());
        }

        private void TransformPawn(Pawn pawn, IntVec3 pawnPos, Map pawnMap)
        {
            var pawnName = pawn.Name;
            var faction = pawn.Faction;
            var age = pawn.ageTracker.AgeBiologicalTicks;
            pawn.Destroy();
            Pawn newPawn = null;
            if (makeAnimal || makeColonyAnimal || makeBerserkAnimal)
            {
                var randomAnimal = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race?.race.Animal == true).RandomElement();
                if (makeColonyAnimal)
                {
                    newPawn = PawnGenerator.GeneratePawn(randomAnimal, Faction.OfPlayer);
                }
                else
                {
                    newPawn = PawnGenerator.GeneratePawn(randomAnimal);
                }
                if (makeBerserkAnimal)
                {
                    newPawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, forced: true);
                }
            }
            else if (makeMech || makeColonyMech)
            {
                var randomMech = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race?.race.IsMechanoid == true).RandomElement();
                if (makeColonyMech)
                {
                    newPawn = PawnGenerator.GeneratePawn(randomMech, Faction.OfPlayer);
                }
                else
                {
                    newPawn = PawnGenerator.GeneratePawn(randomMech);
                    var factionOfMechanoids = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Mechanoid);
                    if (factionOfMechanoids != null)
                    {
                        newPawn.SetFaction(factionOfMechanoids);
                    }
                }
            }
            else if (makeHumanlike)
            {
                newPawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist);
                newPawn.SetFaction(faction);
                if (Rand.Chance(0.25f))
                {
                    SetRandomXenotype(newPawn, x => true);
                }
                if (Rand.Chance(0.95f))
                {
                    SetRandomXenotype(newPawn, x => !x.Archite);
                }
            }
            if (newPawn != null)
            {
                newPawn.Name = pawnName;
                newPawn.ageTracker.AgeBiologicalTicks = age;
                GenSpawn.Spawn(newPawn, pawnPos, pawnMap);
                newPawn.RecacheStatsForThing();
            }
            
        }

        private void MakeSapientAnimal(Pawn pawn)
        {
            if (!HumanlikeAnimalGenerator.humanlikeAnimals.Any()) return;
            var anyHumanlikeAnimal = HumanlikeAnimalGenerator.humanlikeAnimals.RandomElement();
            RaceMorpher.SwapThingDef(pawn, anyHumanlikeAnimal.Key, true, targetPriority: 999999, force: true);
            pawn.RecacheStatsForThing();
        }

        private void TurnIntoRandomThing(Pawn pawn, IntVec3 pawnPos, Map pawnMap, List<ThingDef> specificThings)
        {
            pawn.Destroy(DestroyMode.KillFinalize);
            var randomThing = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(x => x.category == ThingCategory.Item && x.BaseMarketValue < 10000 && x.BaseMarketValue > 150).RandomElement();
            if (specificThings != null && specificThings.Any())
            {
                randomThing = specificThings.RandomElement();
            }
            var thing = GenSpawn.Spawn(randomThing, pawnPos, pawnMap);
            int maxMarketValue = Rand.Range(50, 1200);
            float mValue = thing.MarketValue;
            if (mValue < 150)
            {
                int stackTarget = maxMarketValue / Mathf.RoundToInt(mValue);
                thing.stackCount = stackTarget;
            }
            else
            {
                thing.stackCount = 1;
            }
        }

        private void TurnIntoRandomMinifiedThing(Pawn pawn, IntVec3 pawnPos, Map pawnMap, List<ThingDef> specificThings)
        {
            pawn.Destroy(DestroyMode.KillFinalize);
            var minifiedThingList = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(x => x.category == ThingCategory.Item && x.BaseMarketValue < 10000 && x.BaseMarketValue > 150)
                .Select(x => x.minifiedDef).Where(x => x != null);
            if (specificThings != null && specificThings.Any())
            {
                minifiedThingList = specificThings.Select(x => x.minifiedDef).Where(x => x != null);
            }
            if (minifiedThingList.Any())
            {
                var randomMinifiableThing = minifiedThingList.RandomElement();
                SpawnMinifiedThing(pawnPos, pawnMap, randomMinifiableThing);
            }
        }

        private static void SpawnMinifiedThing(IntVec3 pawnPos, Map pawnMap, ThingDef randomMinifiableThing)
        {
            var minifiedThing = GenSpawn.Spawn(randomMinifiableThing.minifiedDef, pawnPos, pawnMap) as MinifiedThing;
            minifiedThing.InnerThing = ThingMaker.MakeThing(randomMinifiableThing);
        }

        private void TurnIntoRandomResource(Pawn pawn, IntVec3 pawnPos, Map pawnMap)
        {
            float butcherAmount = pawn.GetStatValue(StatDefOf.MeatAmount);
            pawn.Destroy(DestroyMode.KillFinalize);
            var randomThing = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.stackLimit > 50 && x.BaseMarketValue < 100).RandomElement();
            var newThing = GenSpawn.Spawn(randomThing, pawnPos, pawnMap);
            newThing.stackCount = Mathf.Max(1, Mathf.RoundToInt(butcherAmount));
        }

        private void Explode(Pawn pawn)
        {
            DamageDef[] damageDefs = { DamageDefOf.Bomb, DamageDefOf.EMP, DamageDefOf.AcidBurn, DamageDefOf.Flame };
            GenExplosion.DoExplosion(pawn.Position, pawn.Map, Rand.Range(2, 6), damageDefs.RandomElement(), pawn, damAmount: Rand.Range(4, 100));
        }


        private bool IsExcludedHediff(HediffDef hediff)
        {
            if (hediff.defName.ToLower().Contains("preg"))
                return true;
            return false;
        }
        private void GiveRandomHediff(Pawn pawn)
        {
            var rngHediffs = DefDatabase<HediffDef>.AllDefsListForReading.Where(x => x.countsAsAddedPartOrImplant || x.makesSickThought || x.tendable || x.isBad || x.defaultInstallPart != null);
            rngHediffs = rngHediffs.Where(x => !IsExcludedHediff(x)).ToList();
            if (rngHediffs.Any())
            {
                var rngHediff = rngHediffs.RandomElement();
                AddHediff(pawn, rngHediff);
            }
        }

        private void FuseWithPawns(Pawn pawn)
        {
            if (pawn.RaceProps.Humanlike == false) return;
            var countToFuse = fuseWithPawns?.RandomInRange ?? 1;
            var nearbyPawns = pawn.Map.mapPawns.AllPawns.Where(x => x.genes != null && x.Position.DistanceTo(pawn.Position) < 3).ToList();
            var maxDistance = Rand.Range(1, 3);
            if (nearbyPawns.Count > 0)
            {
                var firstPawn = pawn;
                var nearestPawns = nearbyPawns.OrderBy(x => x.Position.DistanceTo(pawn.Position)).Take(countToFuse).ToList();
                nearestPawns = nearestPawns.Where(x => x.Position.DistanceTo(pawn.Position) < maxDistance && x?.RaceProps?.Humanlike == true).ToList();
                if (!pawn.IsColonist)
                {
                    var colonist = nearestPawns.FirstOrDefault(x => x.IsColonist);
                    if (colonist != null)
                    {
                        nearestPawns.Add(pawn);
                        nearestPawns.Remove(colonist);
                        firstPawn = colonist;
                    }
                }
                FusePawns(firstPawn, nearestPawns);
                firstPawn.RecacheStatsForThing();
            }
        }

        private void SwapApparelStuffables(Pawn pawn)
        {
            var apparel = pawn.apparel?.WornApparel;
            if (apparel == null) return;
            foreach (var wornApparel in apparel)
            {
                var stuffables = wornApparel.def.MadeFromStuff ? GenStuff.AllowedStuffsFor(wornApparel.def) : null;
                if (stuffables != null)
                {
                    wornApparel.SetStuffDirect(stuffables.RandomElement());
                    wornApparel.RecacheStatsForThing();
                }
            }

            var weapons = pawn?.equipment?.AllEquipmentListForReading;
            if (weapons == null) return;
            foreach (var weapon in weapons)
            {
                var stuffables = weapon.def.MadeFromStuff ? GenStuff.AllowedStuffsFor(weapon.def) : null;
                if (stuffables != null)
                {
                    weapon.SetStuffDirect(stuffables.RandomElement());
                    weapon.RecacheStatsForThing();
                }
            }
        }

        

        private void GiveRandomMentalBreak(Pawn pawn)
        {
            var mentalBreak = DefDatabase<MentalBreakDef>.AllDefsListForReading.RandomElement();
            if (!pawn.mindState.mentalBreaker.TryDoMentalBreak("", mentalBreak))
            {
                pawn.mindState.mentalBreaker.TryDoMentalBreak("", MentalBreakDefOf.BerserkShort);
            }
            //mentalBreak.Worker.TryStart(pawn, null, false);
        }

        private void GiveHediffFromList(Pawn pawn, List<HediffDef> hediffList)
        {
            var rngHediff = hediffList.RandomElement();
            AddHediff(pawn, rngHediff);
        }

        private void GiveGeneFromList(Pawn pawn, List<GeneDef> geneList, bool isXeno, bool all)
        {
            Discombobulator.IntegrateGenes(pawn, removeOverriden: true);
            if (all)
            {
                foreach (var gene in geneList)
                {
                    pawn.genes.AddGene(gene, isXeno);
                }
                Discombobulator.IntegrateGenes(pawn, removeOverriden: true);
                return;
            }
            else
            {
                var rngGene = geneList.RandomElement();
                pawn.genes.AddGene(rngGene, isXeno);
            }
            if (!isXeno) { Discombobulator.IntegrateGenes(pawn, removeOverriden: true); }
        }

        private void GiveXenotypeFromList(Pawn pawn, List<XenotypeDef> xenotypeList)
        {
            var rngXeno = xenotypeList.RandomElement();
            pawn.genes.SetXenotype(rngXeno);
            Discombobulator.IntegrateGenes(pawn, removeOverriden: true);
        }

        private static void AddHediff(Pawn pawn, HediffDef rngHediff)
        {
            // Find associated RecipeDef, if any.
            var recipeDef = DefDatabase<RecipeDef>.AllDefsListForReading.FirstOrDefault(x => x.addsHediff == rngHediff);
            bool? isInjury = rngHediff.hediffClass?.SameOrSubclassOf(typeof(Hediff_Injury));
            isInjury ??= rngHediff.hediffClass?.SameOrSubclassOf(typeof(MissingPart));



            if (recipeDef?.appliedOnFixedBodyParts is List<BodyPartDef> validParts)
            {
                var parts = pawn.health.hediffSet.GetNotMissingParts().Where(x => validParts.Contains(x.def));
                if (parts.Any())
                {
                    pawn.health.AddHediff(rngHediff, parts.RandomElement());
                }
            }
            else if (recipeDef?.appliedOnFixedBodyPartGroups is List<BodyPartGroupDef> validGroups)
            {
                var parts = pawn.health.hediffSet.GetNotMissingParts().Where(x => validGroups.Intersect(x.groups).Any());
                if (parts.Any())
                {
                    pawn.health.AddHediff(rngHediff, parts.RandomElement());
                }
            }
            else if (isInjury == true)
            {
                var parts = pawn.health.hediffSet.GetNotMissingParts();
                if (parts.Any())
                {
                    pawn.health.AddHediff(rngHediff, parts.RandomElement());
                }
            }
            else if (rngHediff.addedPartProps == null)
            {
                pawn.health.AddHediff(rngHediff);
            }
            
        }

        static string[] sizeTags = ["BodySize", "Body_Size_Multiplier"];
        static bool IsRaceSwap(GeneDef x)
        {
            return x.GetAllPawnExtensionsOnGene().Any(y => y.thingDefSwap != null);
        }
        public static void FusePawns(Pawn mainPawn, List<Pawn> pawns)
        {
            if (pawns.Count == 0) return;
            int pawnCount = pawns.Count + 1;
            Dictionary<SkillDef, int> skills = [];
            Dictionary<TraitDef, int> pawnTraits = [];
            List<GeneDef> endoGenes = [];
            List<GeneDef> xenoGenes = [];
            List<PawnRelationDef> reflexsiveNonImpliedRelations = [];
            List<Pawn> allPawns = [mainPawn, .. pawns];

            foreach (var pawn in allPawns)
            {
                foreach (var skill in pawn.skills.skills)
                {
                    if (skills.ContainsKey(skill.def))
                    {
                        skills[skill.def] += skill.Level;
                    }
                    else
                    {
                        skills[skill.def] = skill.Level;
                    }
                }
                if (pawn != mainPawn)
                {
                    foreach (var trait in pawn.story.traits.allTraits)
                    {
                        if (pawnTraits.ContainsKey(trait.def))
                        {
                            pawnTraits[trait.def]++;
                        }
                        else
                        {
                            pawnTraits[trait.def] = 1;
                        }
                    }
                    foreach (var gene in pawn.genes.Endogenes)
                    {
                        endoGenes.Add(gene.def);
                    }
                    foreach (var gene in pawn.genes.Xenogenes)
                    {
                        xenoGenes.Add(gene.def);
                    }
                    foreach (var relation in pawn.relations.DirectRelations)
                    {
                        if (relation.def.reflexive && !relation.def.implied)
                        {
                            reflexsiveNonImpliedRelations.Add(relation.def);
                        }
                    }
                }
            }

            foreach (var skill in mainPawn.skills.skills)
            {
                var averageSkill = skills[skill.def] / pawnCount;
                var minimumSkill = Mathf.Min(skills[skill.def]);
                var maximumSkill = Mathf.Max(skills[skill.def]);
                if (Rand.Chance(0.4f))
                {
                    skill.Level = Rand.Range(minimumSkill, averageSkill);
                }
                else
                {
                    skill.Level = Rand.Range(averageSkill, maximumSkill);
                }
            }
            foreach (var trait in pawnTraits)
            {
                int currentTraitCount = pawnTraits.Count;
                // Chance of removing a random trait first increases with the number of traits.
                if (Rand.Chance(-0.4f + (currentTraitCount * 0.1f)))
                {
                    mainPawn.story.traits.allTraits.Remove(mainPawn.story.traits.allTraits.RandomElement());
                }
                // Reduce chance based on how many traits the pawn has.
                if (currentTraitCount < 7 && Rand.Chance(4f / pawnTraits.Count))
                {
                    mainPawn.story.traits.GainTrait(new Trait(trait.Key));
                }
            }
            foreach (var gene in endoGenes)
            {
                mainPawn.genes.AddGene(gene, false);
            }
            foreach (var gene in xenoGenes)
            {
                mainPawn.genes.AddGene(gene, true);
            }
            Discombobulator.IntegrateGenes(mainPawn, removeOverriden: true);
            foreach (var relation in reflexsiveNonImpliedRelations)
            {
                mainPawn.relations.AddDirectRelation(relation, mainPawn);
            }
            foreach (var pawn in pawns)
            {
                pawn.Destroy(DestroyMode.KillFinalize);
            }
        }
    }

    

    

    
}
