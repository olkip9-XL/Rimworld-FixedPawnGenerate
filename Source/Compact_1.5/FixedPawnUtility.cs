using FixedPawnGenerate;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static FixedPawnGenerate.FixedPawnDef;

namespace FixedPawnGenerate
{
    [StaticConstructorOnStartup]
    public static class FixedPawnUtility
    {
        //public 
        public static readonly List<string> callerBlackList = new List<string>();
        public static GameComponent_FixedPawn Manager => Current.Game.GetComponent<GameComponent_FixedPawn>();
        public static ModSetting_FixedPawnGenerate Settings => LoadedModManager.GetMod<Mod_FixedPawnGenerate>().GetSettings<ModSetting_FixedPawnGenerate>();

        //private
        private static bool isAlienRaceActive;
        static FixedPawnUtility()
        {
            //add Black List
            //callerBlackList.Add("Faction.TryGenerateNewLeader");
            callerBlackList.Add("<PlayerStartingThings>d__17.MoveNext");
            callerBlackList.Add("GenStep_Monolith.GenerateMonolith");
            callerBlackList.Add("PawnRelationWorker_Sibling.GenerateParent");
            callerBlackList.Add("FixedPawnUtility.GenerateFixedPawnWithDef");
            callerBlackList.Add("PregnancyUtility.ApplyBirthOutcome_NewTemp");
            callerBlackList.Add("GameComponent_PawnDuplicator.Duplicate");

            //fix relations
            foreach (FixedPawnDef def in DefDatabase<FixedPawnDef>.AllDefs.Where(x => x.isUnique))
            {
                foreach (var relationData in def.relations)
                {
                    FixedPawnDef targetDef = relationData.fixedPawn;

                    if (!targetDef.isUnique)
                    {
                        Log.Error($"Trying add relation to {targetDef.defName}, but {targetDef.defName} is NOT unique!");
                        continue;
                    }

                    if (targetDef.relations.Find(x => x.fixedPawn == def) != null)
                    {
                        continue;
                    }

                    targetDef.relations.AddDistinct(new FixedPawnDef.RelationData(GetOppositeRelation(relationData.relation), def));
#if DEBUG
                    Log.Warning($"[Debug]Add relation {GetOppositeRelation(relationData.relation)}({def.defName}) to {targetDef.defName}");
#endif
                }

                //alienrace
                isAlienRaceActive = ModLister.HasActiveModWithName("Humanoid Alien Races 2.0") || ModLister.HasActiveModWithName("Humanoid Alien Races") || ModLister.HasActiveModWithName("Humanoid Alien Races ~ Dev");
            }
        }

        private static PawnRelationDef GetOppositeRelation(PawnRelationDef relation)
        {
            if (relation == PawnRelationDefOf.Parent)
            {
                return PawnRelationDefOf.Child;
            }
            else if (relation == PawnRelationDefOf.Child)
            {
                return PawnRelationDefOf.Parent;
            }
            else if (relation == PawnRelationDefOf.Grandparent)
            {
                return PawnRelationDefOf.Grandchild;
            }
            else if (relation == PawnRelationDefOf.Grandchild)
            {
                return PawnRelationDefOf.Grandparent;
            }
            else if (relation == PawnRelationDefOf.GreatGrandparent)
            {
                return PawnRelationDefOf.GreatGrandchild;
            }
            else if (relation == PawnRelationDefOf.GreatGrandchild)
            {
                return PawnRelationDefOf.GreatGrandparent;
            }
            else if (relation == PawnRelationDefOf.UncleOrAunt)
            {
                return PawnRelationDefOf.NephewOrNiece;
            }
            else if (relation == PawnRelationDefOf.NephewOrNiece)
            {
                return PawnRelationDefOf.UncleOrAunt;
            }
            else if (relation == PawnRelationDefOf.GranduncleOrGrandaunt)
            {
                return PawnRelationDefOf.Grandchild;
            }
            else if (relation == PawnRelationDefOf.ParentBirth)
            {
                return PawnRelationDefOf.Child;
            }
            else if (relation == PawnRelationDefOf.Kin)
            {
                return null;
            }
            else
            {
                return relation;
            }
        }

        private static bool ReplaceInnercontainer(ThingOwner innercontainer, List<FixedPawnDef.ThingData> list)
        {
            if (list.Count == 0)
            {
                return false;
            }

            List<Thing> removeList = new List<Thing>();
            removeList.AddRange(innercontainer);

            foreach (var item in removeList)
            {
                item.Destroy();
            }

            foreach (var def in list)
            {
                if (def.thing == null)
                    continue;

                ThingDef stuff = def.stuff == null ? GenStuff.DefaultStuffFor(def.thing) : def.stuff;

                Thing thing = ThingMaker.MakeThing(def.thing, stuff);

                if (thing == null)
                {
                    Log.Warning($"Try add {def.thing.defName} with {stuff.defName} Failed");
                    continue;
                }
                thing.stackCount = def.count;

                thing.TryGetComp<CompQuality>()?.SetQuality(def.quality, ArtGenerationContext.Colony);

                if (def.color.a != 0f)
                    thing.TryGetComp<CompColorable>()?.SetColor(def.color);

                innercontainer.TryAdd(thing, thing.stackCount);
            }

            return true;
        }

        private static void SetPawnPersonalInfo(Pawn pawn, FixedPawnDef def)
        {
            if (def.gender == Gender.Male || def.gender == Gender.Female)
            {
                pawn.gender = def.gender;
            }

            if (def.name != null)
            {
                pawn.Name = def.name;
            }
            if (pawn.Name is NameTriple)
            {
                pawn.story.birthLastName = (pawn.Name as NameTriple).Last;
            }
        }

        private static void SetPawnStory(Pawn pawn, FixedPawnDef def)
        {
            if (def.childHood != null && def.childHood.slot == BackstorySlot.Childhood)
            {
                pawn.story.Childhood = def.childHood;
            }

            if (def.adultHood != null && def.adultHood.slot == BackstorySlot.Adulthood)
            {
                pawn.story.Adulthood = def.adultHood;
            }
        }

        private static void SetPawnApparence(Pawn pawn, FixedPawnDef def)
        {
            //hair
            if (def.beard != null)
                pawn.style.beardDef = def.beard;

            if (def.hair != null)
                pawn.story.hairDef = def.hair;

            if (def.hairColor.a != 0f)
                pawn.story.HairColor = def.hairColor;

            //head
            if (def.headType != null)
                pawn.story.headType = def.headType;

            //body
            if (def.skinColor.a != 0f)
            {
                pawn.story.SkinColorBase = def.skinColor;

                //alien race compatible
                if (!isAlienRaceActive)
                {
                    pawn.story.skinColorOverride = def.skinColor;
                }
                else
                {
                    FPG_Alienrace.SetPawnSkinColor(pawn, def.skinColor);
                }
            }

            if (def.bodyType != null)
                pawn.story.bodyType = def.bodyType;

            //Ideology
            if (ModsConfig.IdeologyActive)
            {
                if (def.bodyTattoo != null)
                    pawn.style.BodyTattoo = def.bodyTattoo;

                if (def.faceTattoo != null)
                    pawn.style.FaceTattoo = def.faceTattoo;
            }

            //royalty
            if (ModsConfig.RoyaltyActive)
            {
                if (def.favoriteColor.a != 0f)
                    pawn.story.favoriteColor = def.favoriteColor;
            }
        }

        internal static void ModifyPawn(Pawn pawn, FixedPawnDef def)
        {
            if (def == null || pawn == null)
            {
                return;
            }

            try
            {
                //Personal info
                SetPawnPersonalInfo(pawn, def);

                //inventory
                ReplaceInnercontainer(pawn.equipment.GetDirectlyHeldThings(), def.equipment);

                ReplaceInnercontainer(pawn.inventory.GetDirectlyHeldThings(), def.inventory);

                ReplaceInnercontainer(pawn.apparel.GetDirectlyHeldThings(), def.apparel);

                //story
                SetPawnStory(pawn, def);

                //apparence
                SetPawnApparence(pawn, def);

                //skills
                foreach (var skillData in def.skills)
                {
                    SkillRecord skill = pawn.skills.GetSkill(skillData.skill);
                    if (skill != null)
                    {
                        skill.Level = skillData.level;
                        if (skillData.replacePassion)
                            skill.passion = skillData.passion;
                    }
                }

                //traits
                if (def.traits.Count > 0)
                {
                    pawn.story.traits.allTraits.RemoveAll(x => x.sourceGene == null);
                    foreach (var traitData in def.traits)
                    {
                        int traitDegree;

                        if (traitData.trait.degreeDatas.Count == 1)
                        {
                            traitDegree = 0;
                        }
                        else
                        {
                            if (traitData.trait.degreeDatas.Find(x => x.degree == traitData.degree) != null)
                            {
                                traitDegree = traitData.degree;
                            }
                            else
                            {
                                Log.Warning($"Trait {traitData.trait.defName} does not have degree {traitData.degree}, use First defined degree");
                                traitDegree = traitData.trait.degreeDatas[0].degree;
                            }
                        }

                        //traits skill gain
                        TraitDegreeData traitDegreeData = traitData.trait.degreeDatas.Find(x => x.degree == traitDegree);
                        if (traitDegreeData != null && !traitDegreeData.skillGains.NullOrEmpty())
                        {
                            foreach (var skillGain in traitDegreeData.skillGains)
                            {
                                SkillRecord skill = pawn.skills.GetSkill(skillGain.skill);
                                if (skill != null)
                                {
                                    skill.Level = skill.levelInt + skillGain.amount;
                                }
                            }
                        }

                        pawn.story.traits.GainTrait(new Trait(traitData.trait, degree: traitDegree));
                    }
                }

                //health
                if (def.hediffs.Count > 0)
                {
                    pawn.health.Reset();
                    foreach (var hediffData in def.hediffs)
                    {
                        BodyPartRecord pawnBodyPart = null;
                        if (hediffData.bodyPart != null)
                        {
                            List<BodyPartRecord> parts = pawn.RaceProps.body.AllParts.FindAll(x => x.def == hediffData.bodyPart);

                            if (hediffData.index < parts.Count)
                            {
                                pawnBodyPart = parts[hediffData.index];
                            }
                        }

                        Hediff hediff = HediffMaker.MakeHediff(hediffData.hediff, pawn, pawnBodyPart);
                        hediff.Severity = hediffData.severity;
                        pawn.health.AddHediff(hediff);
                    }
                }


                //abilities
                foreach (var ability in def.abilities)
                {
                    pawn.abilities.GainAbility(ability);
                }

                //FacialAnimation
                if (ModLister.HasActiveModWithName("[NL] Facial Animation - WIP") && def.facialAnimationProps != null)
                {
                    def.facialAnimationProps.SetPawn(pawn);
                }

                //relation
                Manager.AddPawn(pawn, def);

                if (def.isUnique)
                {
                    GenerateRelations(pawn);
                }

            }
            catch (Exception e)
            {
                Log.Error($"[Fixed Pawn Generate] ModifyPawn {def?.defName ?? "null"} Error: {e.Message}");
            }
        }

        private static void GenerateRelations(Pawn pawn)
        {
#if DEBUG
            Log.Warning($"GenerateRelations:{pawn.Name}");
#endif

            if (pawn == null)
            {
                return;
            }
            List<FixedPawnDef.RelationData> relations = Manager.GetDef(pawn)?.relations;

            if (relations == null)
            {
                return;
            }

            foreach (var relationData in relations)
            {
                if (relationData.fixedPawn.isUnique && relationData.fixedPawn.GetPawn() != null)
                {

                    Pawn relationPawn = relationData.fixedPawn.GetPawn();

                    if (pawn.relations.RelatedPawns.Contains(relationPawn))
                    {
                        continue;
                    }

                    //这个会让未被招募的角色显示在人物社交面板里面
                    //relationPawn.relations.everSeenByPlayer = true;
                    //pawn.relations.everSeenByPlayer = true;
                    PawnGenerationRequest request = new PawnGenerationRequest();
                    relationData.relation.Worker.CreateRelation(pawn, relationPawn, ref request);
                }
                else
                {
                    Pawn relationPawn = GenerateFixedPawnWithDef(relationData.fixedPawn, addToManager: false);

                    //pass to world
                    if (relationPawn.GetPawnPositionState() == PawnPositionState.OTHER)
                    {
#if DEBUG
                        Log.Warning($"[Debug]:Pass to world:{relationPawn.Name}");
#endif
                        Find.WorldPawns.PassToWorld(relationPawn, RimWorld.Planet.PawnDiscardDecideMode.KeepForever);

                        Faction faction = null;
                        if (relationData.fixedPawn.faction != null)
                            faction = Find.FactionManager.FirstFactionOfDef(relationData.fixedPawn.faction);

                        relationPawn.SetFaction(faction);
                    }

                }
            }

        }


        // Get a random def based on weights
        public static FixedPawnDef GetRandomFixedPawnDefByWeight(List<FixedPawnDef> list, bool ExceptSpawned = true)
        {
            double totalWeight = 0;

            //list.RemoveAll(x => x.isUnique && !Manager.uniqePawns.Contains(x));
            if (ExceptSpawned)
                //list.RemoveAll(x => x.isUnique &&  Manager.GetPawn(x) != null);
                list.RemoveAll(x => x.IsSpawned);

            foreach (var def in list)
            {
                totalWeight += def.generateWeight;
            }

            double randomValue = Rand.Value * totalWeight;
            double cumulativeWeight = 0;

            foreach (var def in list)
            {
                cumulativeWeight += def.generateWeight;
                if (randomValue <= cumulativeWeight)
                {
                    return def;
                }
            }

            return null; // Return null if no defName is found
        }
        public static FixedPawnDef GetRandomFixedPawnDefByWeight(IEnumerable<FixedPawnDef> list, bool ExceptSpawned = true)
        {
            return FixedPawnUtility.GetRandomFixedPawnDefByWeight(new List<FixedPawnDef>(list), ExceptSpawned);
        }


        //get caller of PawnGenerator:GeneratePawn(), 5 at least
        public static string GetCallerMethodName(int index = 5)
        {
            StackTrace stackTrace = new StackTrace();

            StackFrame[] stackFrames = stackTrace.GetFrames();

            if (stackFrames.Length > index)
            {
                StackFrame callerFrame = stackFrames[index];
                MethodBase callerMethod = callerFrame.GetMethod();
                string callerClassName = callerMethod.ReflectedType.Name;
                string callerMethodName = callerMethod.Name;

                if (callerClassName == "PawnGenerator" && callerMethodName == "GeneratePawn")
                {
                    callerFrame = stackFrames[index + 1];
                    callerMethod = callerFrame.GetMethod();
                    callerClassName = callerMethod.ReflectedType.Name;
                    callerMethodName = callerMethod.Name;
                }

                return callerClassName + "." + callerMethodName;
            }

            return null;
        }

        internal static Pawn ModifyRequest(ref PawnGenerationRequest request, FixedPawnDef def, bool addToManager = true)
        {
            if (def == null)
            {
                return null;
            }

            //Locate pawn if it is unique
            Pawn pawn = null;
            if (def.isUnique)
            {
                if (addToManager)
                    Manager.spawnedUniquePawns.AddDistinct(def);

                pawn = Manager.GetPawn(def);

                if (pawn != null)
                {
                    return pawn;
                }
            }

            request.CanGeneratePawnRelations = false;

            if (request.Faction != null && request.Faction.ideos != null && request.Faction.ideos.PrimaryIdeo != null)
            {
                request.FixedIdeo = request.Faction.ideos.PrimaryIdeo;
            }

            if (def.age > 0)
            {
                if (ModsConfig.BiotechActive)
                {
                    request.FixedBiologicalAge = def.age;
                }
                else
                {
                    request.FixedBiologicalAge = Mathf.Max(def.age, 13f);
                }
            }

            if (def.xenotype != null)
            {
                request.ForcedXenotype = def.xenotype;
            }

            if (def.customXenotype != null)
            {
                CustomXenotype customXenotype = CharacterCardUtility.CustomXenotypesForReading.Find(x => x.name == def.customXenotype);
#if DEBUG
                foreach (var item in CharacterCardUtility.CustomXenotypesForReading)
                {
                    Log.Warning($"customXenotype:{item.name}");
                }
#endif
                if (customXenotype != null)
                {
                    request.ForcedXenotype = null;
                    request.ForcedCustomXenotype = customXenotype;
                }
                else
                {
                    Log.Warning($"customXenotype:{def.customXenotype} not found");
                }
            }

            if (def.gender != Gender.None)
                request.FixedGender = def.gender;

            if (def.firstName != null)
                request.FixedBirthName = def.firstName;

            if (def.lastName != null)
                request.FixedLastName = def.lastName;

            if (def.bodyType != null)
                request.ForceBodyType = def.bodyType;

            //comps properties
            FixedPawnHarmony.SetCompProperties(def);

            return null;
        }

        public static Pawn GenerateFixedPawnWithDef(FixedPawnDef def, bool addToManager = true)
        {
            if (def == null)
            {
                return null;
            }

            Faction faction = null;
            if (def.faction != null)
                faction = Find.FactionManager.FirstFactionOfDef(def.faction);

            if (def.factionType != FactionType.None)
            {
                switch (def.factionType)
                {
                    case FactionType.Player:
                        faction = Faction.OfPlayer;
                        break;
                    case FactionType.Mechanoids:
                        faction = Faction.OfMechanoids;
                        break;
                    case FactionType.Insects:
                        faction = Faction.OfInsects;
                        break;
                    case FactionType.Ancients:
                        faction = Faction.OfAncients;
                        break;
                    case FactionType.AncientsHostile:
                        faction = Faction.OfAncientsHostile;
                        break;
                    case FactionType.Empire:
                        faction = Faction.OfEmpire;
                        break;
                    case FactionType.Pirates:
                        faction = Faction.OfPirates;
                        break;
                    case FactionType.HoraxCult:
                        faction = Faction.OfHoraxCult;
                        break;
                    case FactionType.Entities:
                        faction = Faction.OfEntities;
                        break;
                    default:
                        break;
                }
            }

            PawnKindDef pawnKind = def.pawnKind;
            if (pawnKind == null && faction != null)
                pawnKind = faction.RandomPawnKind();

            if (pawnKind == null)
            {
                Log.Error($"[Fixed Pawn Generate] {def.defName} has no pawnKind or faction");
                return null;
            }

            PawnGenerationRequest request = new PawnGenerationRequest(pawnKind, faction);

            Pawn result = null;
            if ((result = ModifyRequest(ref request, def, addToManager)) != null)
            {
                if (result.Faction != faction)
                {
                    result.SetFaction(faction);
                }
                return result;
            }

            result = PawnGenerator.GeneratePawn(request);

            ModifyPawn(result, def);

            if (result.Faction != faction)
            {
                result.SetFaction(faction);
            }
            return result;
        }

        public static IEnumerable<FixedPawnDef> SpawnedPawnWithTag(string tag)
        {
            foreach (var def in Manager.spawnedUniquePawns)
            {
                if (def.tags.Contains(tag))
                    yield return def;
            }

            yield break;
        }


    }


}
