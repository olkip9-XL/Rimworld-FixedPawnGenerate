using FixedPawnGenerate;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

using UnityEngine;
using FacialAnimation;

namespace FixedPawnGenerate
{
    public static class FixedPawnUtility
    {
        public static readonly List<string> callerWhiteList = new List<string>();

        public static readonly List<string> callerBlackList = new List<string>();

        public static int startingPawnCount = 0;

        static FixedPawnUtility()
        {

            callerWhiteList.Add("StartingPawnUtility.NewGeneratedStartingPawn");
            callerWhiteList.Add("WildAnimalSpawner.SpawnRandomWildAnimalAt");
            callerWhiteList.Add("ThingSetMaker_MapGen_AncientPodContents.GenerateAngryAncient");
            callerWhiteList.Add("SymbolResolver_SinglePawn.Resolve");
            callerWhiteList.Add("PawnGroupKindWorker_Normal.GeneratePawns");

            callerBlackList.Add("Faction.TryGenerateNewLeader");
            callerBlackList.Add("<PlayerStartingThings>d__17.MoveNext");
            callerBlackList.Add("GenStep_Monolith.GenerateMonolith");
            callerBlackList.Add("PawnRelationWorker_Sibling.GenerateParent");
            callerBlackList.Add("FixedPawnUtility.GenerateFixedPawnWithDef");
        }


        public static List<FixedPawnDef> GetFixedPawnDefsByRequest(ref PawnGenerationRequest request)
        {
            FactionDef factionDef = null;
            PawnKindDef pawnKindDef = null;
            ThingDef race = null;

            if (request.Faction != null)
                factionDef = request.Faction.def;

            if (request.KindDef != null)
            {
                pawnKindDef = request.KindDef;

                race = request.KindDef.race;
            }

            return DefDatabase<FixedPawnDef>.AllDefsListForReading.FindAll(x => (x.faction == null || x.faction == factionDef) &&
                                                                            (x.race == null || x.race == race) &&
                                                                            (x.pawnKind == null || x.pawnKind == pawnKindDef));
        }

        public static T GetTargetDef<T>(string defName) where T : Def
        {
            defName.Trim();
            return DefDatabase<T>.GetNamed(defName);
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
                innercontainer.TryAdd(thing, thing.stackCount);
            }

            return true;
        }

        public static void ModifyPawn(Pawn pawn, FixedPawnDef def)
        {
            if (def == null)
            {
                return;
            }

            //Personal info
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

            //inventory
            ReplaceInnercontainer(pawn.equipment.GetDirectlyHeldThings(), def.equipment);

            ReplaceInnercontainer(pawn.inventory.GetDirectlyHeldThings(), def.inventory);

            ReplaceInnercontainer(pawn.apparel.GetDirectlyHeldThings(), def.apparel);

            //story
            if (def.childHood != null && def.childHood.slot == BackstorySlot.Childhood)
            {
                pawn.story.Childhood = def.childHood;
            }

            if (def.adultHood != null && def.adultHood.slot == BackstorySlot.Adulthood)
            {
                pawn.story.Adulthood = def.adultHood;
            }

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
                pawn.story.skinColorOverride = def.skinColor;

            if (def.bodyType != null)
                pawn.story.bodyType = def.bodyType;

            //Ideology
            if (ModsConfig.IdeologyActive)
            {
                if (def.bodyTatoo != null)
                    pawn.style.BodyTattoo = def.bodyTatoo;

                if (def.faceTatoo != null)
                    pawn.style.FaceTattoo = def.faceTatoo;
            }

            //royalty
            if (ModsConfig.RoyaltyActive)
            {
                if (def.favoriteColor.a != 0f)
                    pawn.story.favoriteColor = def.favoriteColor;
            }

            //skills
            foreach (var skillData in def.skills)
            {
                SkillRecord skill = pawn.skills.GetSkill(skillData.skill);
                if (skill != null)
                {
                    skill.Level = skillData.level;
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

                    if(traitData.trait.degreeDatas.Count == 1)
                    {
                        traitDegree = 0;
                    }
                    else
                    {
                        if(traitData.trait.degreeDatas.Find(x=>x.degree== traitData.degree) != null)
                        {
                            traitDegree = traitData.degree;
                        }
                        else
                        {
                            Log.Warning($"Trait {traitData.trait.defName} does not have degree {traitData.degree}, use First defined degree");
                            traitDegree = traitData.trait.degreeDatas[0].degree;
                        }
                    }

                    pawn.story.traits.GainTrait(new Trait(traitData.trait, degree:traitDegree));
                }
            }

            //backstory traits and items ??

            //if (def.childHood != null)
            //{
            //    foreach (var trait in def.childHood.forcedTraits)
            //    {
            //        pawn.story.traits.GainTrait(new Trait(trait.def, trait.degree));
            //    }
            //}
            //if(def.adultHood != null && def.age>=13)
            //{
            //    foreach (var trait in def.adultHood.forcedTraits)
            //    {
            //        pawn.story.traits.GainTrait(new Trait(trait.def, trait.degree));
            //    }
            //}
            

            //health
            if (def.hediffs.Count > 0)
            {
                pawn.health.Reset();
                foreach (var hediffData in def.hediffs)
                {
                    BodyPartRecord pawnBodyPart;
                    if (hediffData.bodyPart == null)
                        pawnBodyPart = null;
                    else
                        pawnBodyPart = pawn.RaceProps.body.AllParts.Find(x => x.def == hediffData.bodyPart);

                    Hediff hediff = HediffMaker.MakeHediff(hediffData.hediff, pawn, pawnBodyPart);
                    hediff.Severity = hediffData.severity;
                    pawn.health.AddHediff(hediff);
                }
            }

            //comps
            if (def.comps.Count > 0)
            {
                foreach (var compProp in def.comps)
                {
                    ThingComp thingComp = null;
                    try
                    {
                        thingComp = (ThingComp)Activator.CreateInstance(compProp.compClass);
                        thingComp.parent = pawn;
                        pawn.AllComps.Add(thingComp);
                        thingComp.Initialize(compProp);
                    }
                    catch (Exception arg)
                    {
                        Log.Error("Could not instantiate or initialize a ThingComp: " + arg);
                        pawn.AllComps.Remove(thingComp);
                    }

                    if (thingComp != null)
                    {
                        thingComp.PostPostMake();
                    }
                }
            }

            //abilities
            foreach (var ability in def.abilities)
            {
                pawn.abilities.GainAbility(ability);
            }

            //FacialAnimation
            if (ModLister.HasActiveModWithName("[NL] Facial Animation - WIP"))
            {
                if(def.facialAnimationProps.head != null)
                {
                    pawn.GetComp<HeadControllerComp>().FaceType = def.facialAnimationProps.head;
                }

                if(def.facialAnimationProps.brow != null)
                {
                    pawn.GetComp<BrowControllerComp>().FaceType = def.facialAnimationProps.brow;
                }

                if(def.facialAnimationProps.lid != null)
                {
                    pawn.GetComp<LidControllerComp>().FaceType = def.facialAnimationProps.lid;
                }

                if(def.facialAnimationProps.eye != null)
                {
                    pawn.GetComp<EyeballControllerComp>().FaceType = def.facialAnimationProps.eye;
                }
                if(def.facialAnimationProps.leftEyeColor.a != 0f)
                {
                    pawn.GetComp<EyeballControllerComp>().FaceSecondColor = def.facialAnimationProps.leftEyeColor;
                }
                if(def.facialAnimationProps.rightEyeColor.a != 0f)
                {
                    pawn.GetComp<EyeballControllerComp>().FaceColor = def.facialAnimationProps.rightEyeColor;
                }

                if(def.facialAnimationProps.mouth != null)
                {
                    pawn.GetComp<MouthControllerComp>().FaceType = def.facialAnimationProps.mouth;
                }

                if(def.facialAnimationProps.skin != null)
                {
                    pawn.GetComp<SkinControllerComp>().FaceType = def.facialAnimationProps.skin;
                }
            }


            //relation Todo
            //pawn.relations.everSeenByPlayer = true;

        }

        // Generate a random defName based on weights
        public static FixedPawnDef GetRandomFixedPawnDefByWeight(List<FixedPawnDef> list)
        {
            double totalWeight = 0;

            list.RemoveAll(x => x.isUnique && !Manager.uniqePawns.Contains(x));

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

        public static GameComponent_FixedPawn Manager
        {
            get
            {
                return Current.Game.GetComponent<GameComponent_FixedPawn>();
            }
        }

        public static void ModifyRequest(ref PawnGenerationRequest request, FixedPawnDef def)
        {
            if (def == null)
            {
                return;
            }

            if (def.age > 0)
            {
                request.FixedBiologicalAge = def.age;
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
                request.SetFixedBirthName(def.firstName);
            if (def.lastName != null)
                request.SetFixedLastName(def.lastName);
            if (def.bodyType != null)
                request.ForceBodyType = def.bodyType;
        }

        public static Pawn GenerateFixedPawnWithDef(FixedPawnDef def)
        {
            if (def == null || def.pawnKind == null)
            {
                return null;
            }

            Faction faction = null;
            if (def.faction != null)
                faction = Find.FactionManager.FirstFactionOfDef(def.faction);

            PawnGenerationRequest request = new PawnGenerationRequest(def.pawnKind, faction);
            ModifyRequest(ref request, def);

            Pawn result = PawnGenerator.GeneratePawn(request);
            ModifyPawn(result, def);

            return result;
        }

    }



}
