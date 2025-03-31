using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse.Sound;
using Verse;

namespace BigAndSmall
{
    public class EventOutcome : Def
    {
        // System
        public bool failOnNonhumanlike = true;

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
        public bool turnIntoRandomResource;
        public bool kill;
        public bool explode;
        public bool giveRandomHediff;
        public bool fuseWithNearestPawn;
        public IntRange? fuseWithMultiplePawns;
        public bool swapApparelStuffables;
        public bool giveRandomMentalBreak;

        public bool doEmpExplosionTiny;
        public bool doEmpExplosionSmall;
        public bool doEmpExplosionLarge;
        public bool makePregnantSane;
        public bool makePregnantInsane;

        // Controlled Random
        public List<HediffDef> giveARandomHediff;
        public List<GeneDef> giveEndogeneFromList;
        public List<GeneDef> giveXenogeneFromList;
        public List<GeneDef> giveGeneFromList;
        public List<XenotypeDef> giveXenotypeFromList;
        public List<TraitDef> giveTraitFromList;
        public List<MentalBreakDef> giveMentalBreakFromList;
        public List<ThingDef> turnIntoThingFromList;  // Add Go Juice to this list.

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
            if (pawn?.Position == null || pawn.Map == null) return false;
            var pawnPos = pawn.Position;
            var pawnMap = pawn.Map;

            var humanLikeWithGenes = pawn?.genes != null && pawn?.RaceProps.Humanlike == true;
            if (failOnNonhumanlike && !humanLikeWithGenes) return false;

            if (true)
            {
                SoundDefOf.Interact_BeatFire.PlayOneShot(new SoundInfo() { volumeFactor = 1f, pitchFactor = 0.7f });
            }

            if (doEmpExplosionLarge)
            {
                GenExplosion.DoExplosion(pawn.Position, pawn.Map, 4, DamageDefOf.EMP, pawn, damAmount: 50);
            }
            else if (doEmpExplosionSmall)
            {
                GenExplosion.DoExplosion(pawn.Position, pawn.Map, 2, DamageDefOf.EMP, pawn, damAmount: 10);
            }
            else if (doEmpExplosionTiny)
            {
                GenExplosion.DoExplosion(pawn.Position, pawn.Map, 1, DamageDefOf.EMP, pawn, damAmount: 5);
            }

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
                if (appendRaceGene)
                {
                    AppendRaceGene(pawn);
                }
                if (replaceRace)
                {
                    ReplaceRaceGene(pawn);
                }
                if (giveSizeGene)
                {
                    GiveSizeGene(pawn);
                }
                if (swapGender)
                {
                    Genderbender.GenderBend(pawn);
                }
                if (fuseWithMultiplePawns != null || fuseWithNearestPawn)
                {
                    FuseWithPawns(pawn);
                }
                if (giveEndogeneFromList != null)
                {
                    GiveGeneFromList(pawn, giveEndogeneFromList, false);
                }
                if (giveXenogeneFromList != null)
                {
                    GiveGeneFromList(pawn, giveXenogeneFromList, true);
                }
                if (giveGeneFromList != null)
                {
                    GiveGeneFromList(pawn, giveGeneFromList, Rand.Bool);
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

            if (turnIntoRandomThing)
            {
                TurnIntoRandomThing(pawn, pawnPos, pawnMap);
            }
            if (turnIntoRandomResource)
            {
                TurnIntoRandomResource(pawn, pawnPos, pawnMap);
            }
            if (makeAnimal || makeColonyAnimal || makeBerserkAnimal || makeMech || makeColonyMech)
            {
                TransformPawn(pawn, pawnPos, pawnMap);
            }
            if (makeUndead)
            {
                MakeUndead(pawn);
            }
            if (kill)
            {
                pawn.Kill(null);
            }
            if (explode)
            {
                Explode(pawn);
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

            // Credit to Keyz for this horrible idea.
            if (makePregnantSane || makePregnantInsane)
            {
                if (makePregnantInsane || pawn.gender == Gender.Female)
                {
                    if (humanLikeWithGenes || makePregnantInsane && Rand.Chance(0.5f))
                    {
                        var pregnantHediff = HediffDefOf.PregnantHuman;
                        var preg = pawn.health.AddHediff(pregnantHediff);
                        preg.Severity = Rand.Range(0.25f, 1.2f);
                    }
                    else if (pawn.RaceProps.Animal || makePregnantInsane)
                    {
                        var pregnantHediff = HediffDefOf.Pregnant;
                        var preg = pawn.health.AddHediff(pregnantHediff);
                        preg.Severity = Rand.Range(0.25f, 1.2f);
                    }
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
                var allGenes = DefDatabase<GeneDef>.AllDefsListForReading.Where(predicate).ToList();
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
                    newPawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
                }
            }
            if (makeMech || makeColonyMech)
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
            if (makeHumanlike)
            {
                newPawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist);
                newPawn.SetFaction(faction);
                pawn = newPawn;
                if (Rand.Chance(0.25f))
                {
                    SetRandomXenotype(pawn, x => true);
                }
                if (Rand.Chance(0.95f))
                {
                    SetRandomXenotype(pawn, x => !x.Archite);
                }
            }
            if (newPawn != null)
            {
                newPawn.Name = pawnName;
                newPawn.ageTracker.AgeBiologicalTicks = age;
            }
            GenSpawn.Spawn(newPawn, pawnPos, pawnMap);
        }

        private void MakeSapientAnimal(Pawn pawn)
        {
            if (!HumanlikeAnimalGenerator.humanlikeAnimals.Any()) return;
            var anyHumanlikeAnimal = HumanlikeAnimalGenerator.humanlikeAnimals.RandomElement();
            RaceMorpher.SwapThingDef(pawn, anyHumanlikeAnimal.Key, true, targetPriority: 999999, force: true);
        }

        private void TurnIntoRandomThing(Pawn pawn, IntVec3 pawnPos, Map pawnMap)
        {
            pawn.Destroy(DestroyMode.KillFinalize);
            var randomThing = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(x => x.category == ThingCategory.Item && x.BaseMarketValue < 10000 && x.BaseMarketValue > 150).RandomElement();
            GenSpawn.Spawn(randomThing, pawnPos, pawnMap);
        }

        private void TurnIntoRandomResource(Pawn pawn, IntVec3 pawnPos, Map pawnMap)
        {
            float butcherAmount = pawn.GetStatValue(StatDefOf.MeatAmount);
            pawn.Destroy(DestroyMode.KillFinalize);
            var randomThing = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.stackLimit > 50 && x.BaseMarketValue < 100).RandomElement();
            var newThing = GenSpawn.Spawn(randomThing, pawnPos, pawnMap);
            newThing.stackCount = Mathf.RoundToInt(butcherAmount);
        }

        private void Explode(Pawn pawn)
        {
            DamageDef[] damageDefs = { DamageDefOf.Bomb, DamageDefOf.EMP, DamageDefOf.AcidBurn, DamageDefOf.Flame };
            GenExplosion.DoExplosion(pawn.Position, pawn.Map, Rand.Range(2, 6), damageDefs.RandomElement(), pawn, damAmount: Rand.Range(4, 100));
        }

        private void GiveRandomHediff(Pawn pawn)
        {
            var rngHediff = DefDatabase<HediffDef>.AllDefsListForReading.RandomElement();
            AddHediff(pawn, rngHediff);
        }

        private void FuseWithPawns(Pawn pawn)
        {
            if (pawn.RaceProps.Humanlike == false) return;
            var countToFuse = fuseWithMultiplePawns?.RandomInRange ?? 1;
            var nearbyPawns = pawn.Map.mapPawns.AllPawns.Where(x => x.genes != null && x.Position.DistanceTo(pawn.Position) < 10).ToList();
            var maxDistance = Rand.Range(1, 10);
            if (nearbyPawns.Count > 0)
            {
                var firstPawn = pawn;
                var nearestPawns = nearbyPawns.OrderBy(x => x.Position.DistanceTo(pawn.Position)).Take(countToFuse).ToList();
                nearestPawns = nearestPawns.Where(x => x.Position.DistanceTo(pawn.Position) < maxDistance).ToList();
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
                }
            }
        }

        private void GiveRandomMentalBreak(Pawn pawn)
        {
            var mentalBreak = DefDatabase<MentalBreakDef>.AllDefsListForReading.RandomElement();
            pawn.mindState.mentalBreaker.TryDoMentalBreak("", mentalBreak);
        }

        private void GiveHediffFromList(Pawn pawn, List<HediffDef> hediffList)
        {
            var rngHediff = hediffList.RandomElement();
            AddHediff(pawn, rngHediff);
        }

        private void GiveGeneFromList(Pawn pawn, List<GeneDef> geneList, bool isXeno)
        {
            var rngGene = geneList.RandomElement();
            pawn.genes.AddGene(rngGene, isXeno);
            Discombobulator.IntegrateGenes(pawn, removeOverriden: true);
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
            if (recipeDef?.appliedOnFixedBodyParts is List<BodyPartDef> validParts)
            {
                var getFirstValidPart = pawn.health.hediffSet.GetNotMissingParts().Where(x => validParts.Contains(x.def)).RandomElement();
                if (getFirstValidPart != null)
                {
                    pawn.health.AddHediff(rngHediff, getFirstValidPart);
                }
            }
            else if (recipeDef?.appliedOnFixedBodyPartGroups is List<BodyPartGroupDef> validGroups)
            {
                var getFirstValidPart = pawn.health.hediffSet.GetNotMissingParts().Where(x => validGroups.Intersect(x.groups).Any()).RandomElement();
                if (getFirstValidPart != null)
                {
                    pawn.health.AddHediff(rngHediff, getFirstValidPart);
                }
            }
            else
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
