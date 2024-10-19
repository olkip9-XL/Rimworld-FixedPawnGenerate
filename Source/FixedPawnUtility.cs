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
using System.Security.Cryptography;
using HarmonyLib;

namespace FixedPawnGenerate
{
    [StaticConstructorOnStartup]
    public static class FixedPawnUtility
    {
        public static readonly List<string> callerBlackList = new List<string>();

        static FixedPawnUtility()
        {
            callerBlackList.Add("Faction.TryGenerateNewLeader");
            callerBlackList.Add("<PlayerStartingThings>d__17.MoveNext");
            callerBlackList.Add("GenStep_Monolith.GenerateMonolith");
            callerBlackList.Add("PawnRelationWorker_Sibling.GenerateParent");
            callerBlackList.Add("FixedPawnUtility.GenerateFixedPawnWithDef");

            //fix relations
            foreach (FixedPawnDef def in DefDatabase<FixedPawnDef>.AllDefs.Where(x => x.isUnique))
            {
                foreach (var relationData in def.relations)
                {
                    FixedPawnDef targetDef = relationData.fixedPawn;

                    if (targetDef.relations.Find(x => x.fixedPawn == def) != null)
                    {
                        continue;
                    }

                    targetDef.relations.AddDistinct(new FixedPawnDef.RelationData(GetOppositeRelation(relationData.relation), def));
#if DEBUG
                    Log.Warning($"[Debug]Add relation {GetOppositeRelation(relationData.relation)}({def.defName}) to {targetDef.defName}");
#endif
                }
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
            else if(relation == PawnRelationDefOf.ParentBirth)
            {
                return PawnRelationDefOf.Child;
            }
            else if(relation == PawnRelationDefOf.Kin)
            {
                return null;
            }
            else
            {
                return relation;
            }
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

                thing.TryGetComp<CompQuality>()?.SetQuality(def.quality, ArtGenerationContext.Colony);

                if(def.color.a != 0f)
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
                pawn.story.skinColorOverride = def.skinColor;

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
                    if(skillData.replacePassion)
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

                    pawn.story.traits.GainTrait(new Trait(traitData.trait, degree: traitDegree));
                }
            }

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
                    

            //abilities
            foreach (var ability in def.abilities)
            {
                pawn.abilities.GainAbility(ability);
            }

            //FacialAnimation
            if (ModLister.HasActiveModWithName("[NL] Facial Animation - WIP") && def.facialAnimationProps!=null)
            {
                def.facialAnimationProps.SetPawn(pawn);
            }

            //relation
            Manager.AddPawn(pawn, def);

            GenerateRelations(pawn);
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

            if(relations == null)
            {
                return;
            }
            
            foreach (var relationData in relations)
            {
                Pawn relationPawn = GenerateFixedPawnWithDef(relationData.fixedPawn, false);
                if (relationPawn != null)
                {
#if DEBUG
                    Log.Warning($"GenerateRelations:{pawn.Name} {relationData.relation.defName} {relationPawn.Name}");
#endif
                    bool flag = false;

                    if (relationData.relation.implied)
                    {
                        flag = true;
                        relationData.relation.implied = false;
                    }

                    if (pawn.relations.RelatedPawns.Contains(relationPawn))
                    {
                        continue;
                    }

                    pawn.relations.AddDirectRelation(relationData.relation, relationPawn);
                    if(flag)
                    {
                        relationData.relation.implied = true;
                    }
                  
                }
            }

        }
           

        // Get a random def based on weights
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

        internal static Pawn ModifyRequest(ref PawnGenerationRequest request, FixedPawnDef def, bool removeUnique = true)
        {
            if (def == null)
            {
                return null;
            }

            //Locate pawn if it is unique
            Pawn pawn = null;
            if (def.isUnique)
            {
                if (removeUnique)
                    Manager.uniqePawns.Remove(def);

                pawn = Manager.GetPawn(def);

                if (pawn != null)
                {
                    return pawn;
                }
            }

            request.CanGeneratePawnRelations = false;

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

            //comps properties
            FixedPawnHarmony.Global.compProperties.Clear();
            FixedPawnHarmony.Global.compProperties.AddRange(def.comps);

            return null;
        }

        public static Pawn GenerateFixedPawnWithDef(FixedPawnDef def, bool removeUnique = true)
        {
            if (def == null || def.pawnKind == null)
            {
                return null;
            }

            //Pawn result = null;
            //if (def.isUnique)
            //{
            //    if (removeUnique)
            //    {
            //        Manager.uniqePawns.Remove(def);
            //    }

            //    result = Manager.GetPawn(def);

            //    if(result != null)
            //    {
            //       return result;
            //    }
            //}

            Faction faction = null;
            if (def.faction != null)
                faction = Find.FactionManager.FirstFactionOfDef(def.faction);

            PawnGenerationRequest request = new PawnGenerationRequest(def.pawnKind, faction);

            Pawn result = null;
            if((result = ModifyRequest(ref request, def, removeUnique))!=null)
            {
                return result;
            }

            result = PawnGenerator.GeneratePawn(request);

            ModifyPawn(result, def);

            return result;
        }


        public static ModSetting_FixedPawnGenerate Settings => LoadedModManager.GetMod<Mod_FixedPawnGenerate>().GetSettings<ModSetting_FixedPawnGenerate>();

    }
}
