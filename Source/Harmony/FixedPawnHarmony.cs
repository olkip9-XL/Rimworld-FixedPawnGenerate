﻿using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RuntimeAudioClipLoader;
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
using Verse.AI;

namespace FixedPawnGenerate
{
    public static class FixedPawnHarmony
    {

        public static class FPG_Global
        {
            public static List<CompProperties> compProperties = new List<CompProperties>();
        }

        [HarmonyPatch(typeof(PawnGenerator), "GenerateNewPawnInternal")]
        public static class Patch1
        {
            public static bool Prefix(out string __state, ref Pawn __result, ref PawnGenerationRequest request)
            {
                __state = "None";

                request.CanGeneratePawnRelations = FixedPawnUtility.Settings.allowNaturalRelationGenerate;

                //Black list check
                String caller = FixedPawnUtility.GetCallerMethodName(5);

                if (FixedPawnUtility.callerBlackList.Contains(caller))
                {
#if DEBUG
                    Log.Warning($"[Debug]调用者:{caller}, 生成: Skip");
#endif
                    return true;
                }

                //Randomly Get Def 
                float randValue = Rand.Value;

                bool isStarting = (caller == "StartingPawnUtility.NewGeneratedStartingPawn" ||
                                    caller == "DynamicMethodDefinition.Verse.StartingPawnUtility.NewGeneratedStartingPawn_Patch0");

                float maxRate = (isStarting ? FixedPawnUtility.Settings.maxGenerateRate_Starting : FixedPawnUtility.Settings.maxGenerateRate_Global);


                List<FixedPawnDef> list = GetFixedPawnDefsByRequest(ref request).FindAll(x => randValue < x.generateRate && randValue < maxRate);

                if (isStarting)
                {
                    foreach (var initPawn in Find.GameInitData.startingAndOptionalPawns)
                    {
                        FixedPawnDef fixedPawnDef = FixedPawnUtility.Manager.GetDef(initPawn);

                        if (fixedPawnDef != null && fixedPawnDef.isUnique)
                        {
                            list.Remove(fixedPawnDef);
                        }
                    }
                }

                if (list.Count > 0)
                {
                    FixedPawnDef def = FixedPawnUtility.GetRandomFixedPawnDefByWeight(list);

                    if (def == null)
                        return true;

                    __state = def.defName;
#if DEBUG
                    Log.Warning($"[Debug]调用者:{caller}, 生成:{__state}");
#endif

                    __result = FixedPawnUtility.ModifyRequest(ref request, def);
                    if (__result != null)
                    {
                        __state = "None";
                        return false;
                    }
                }
#if DEBUG
                else
                {
                    Log.Warning($"[Debug]调用者:{caller}, 生成: No Match");
                }
#endif

                return true;
            }
            public static void Postfix(ref Pawn __result, ref PawnGenerationRequest request, string __state)
            {
                Pawn pawn = __result;

                if (pawn != null && __state != "None")
                {
                    FixedPawnDef fixedPawnDef = DefDatabase<FixedPawnDef>.GetNamed(__state);

                    String caller = FixedPawnUtility.GetCallerMethodName(5);
                    bool isStarting = (caller == "StartingPawnUtility.NewGeneratedStartingPawn" ||
                                   caller == "DynamicMethodDefinition.Verse.StartingPawnUtility.NewGeneratedStartingPawn_Patch0");

                    FixedPawnUtility.ModifyPawn(pawn, fixedPawnDef, !isStarting);
                }
            }

            private static List<FixedPawnDef> GetFixedPawnDefsByRequest(ref PawnGenerationRequest request)
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

#if DEBUG

                Log.Warning($"factionDef:{factionDef?.defName} pawnKindDef:{pawnKindDef.defName}, {race.defName}");
#endif


                return DefDatabase<FixedPawnDef>.AllDefsListForReading.FindAll(x => (x.faction == null || x.faction == factionDef) &&
                                                                                (x.race == null || x.race == race) &&
                                                                                (x.pawnKind == null || x.pawnKind == pawnKindDef));
            }
        }

        [HarmonyPatch(typeof(StartingPawnUtility), "RegenerateStartingPawnInPlace")]
        public static class Patch4
        {
            public static bool Prefix(ref int index, ref Pawn __result)
            {
                List<Pawn> startingAndOptionalPawns = Find.GameInitData.startingAndOptionalPawns;

                Pawn pawn = startingAndOptionalPawns[index];
#if DEBUG
                Log.Warning($"Replacing :{pawn.Name}");
#endif

                FixedPawnDef fixedPawnDef = FixedPawnUtility.Manager.GetDef(pawn);
                if (fixedPawnDef != null && fixedPawnDef.isUnique)
                {

                    Pawn pawn2 = StartingPawnUtility.NewGeneratedStartingPawn(index);
                    startingAndOptionalPawns[index] = pawn2;
                    __result = pawn2;

                    return false;
                }

                return true;
            }

            //public static void Postfix(ref int index, ref Pawn __result)
            //{
            //    //Pawn pawn = __result;

            //    //List<DirectPawnRelation> relations = new List<DirectPawnRelation>();

            //    //relations.AddRange(pawn.relations.DirectRelations.FindAll(x=> IsUniquePawn(x.otherPawn)));

            //    //foreach (var relation in relations)
            //    //{
            //    //    pawn.relations.RemoveDirectRelation(relation);
            //    //}   

            //}
          
        }

#if DEBUG

        [HarmonyPatch(typeof(PawnUtility), "DestroyStartingColonistFamily")]
        public static class Patch_DestroyStartingColonistFamily
        {
            public static void Prefix(Pawn pawn)
            {
                String str=$"Destroying StartingColonistFamily of {pawn.Name}\n";
                foreach (Pawn pawn2 in Enumerable.ToList<Pawn>(pawn.relations.RelatedPawns))
                {
                    if (!Find.GameInitData.startingAndOptionalPawns.Contains(pawn2))
                    {
                        WorldPawnSituation situation = Find.WorldPawns.GetSituation(pawn2);
                        if (situation == WorldPawnSituation.Free || situation == WorldPawnSituation.Dead)
                        {
                            str += $"Destroying {pawn2.Name}\n";
                        }
                    }
                }
                Log.Warning(str);
            }
        }

#endif
        [HarmonyPatch(typeof(Pawn), "Destroy")]
        public static class Patch2
        {
            public static void Prefix(Pawn __instance)
            {
#if DEBUG
                Log.Warning($"Pawn Destroyed:{__instance.Name}");
#endif

                //FixedPawnUtility.Manager.RemovePawn(__instance);
            }
        }




        [HarmonyPatch(typeof(ThingWithComps), "InitializeComps")]
        public static class Patch3
        {
            public static void Postfix(ThingWithComps __instance)
            {

                //test
                if (__instance is Pawn pawn)
                {

                    if (pawn.AllComps.Count > 0)
                    {
                        List<CompProperties> list = new List<CompProperties>();
                        FixedPawnDef fixedPawnDef = FixedPawnUtility.Manager.GetDef(pawn);
                        if (fixedPawnDef != null)
                        {
                            list.AddRange(fixedPawnDef.comps);
                        }

                        if (FPG_Global.compProperties.Count > 0)
                        {
                            list.AddRange(FPG_Global.compProperties);
                            FPG_Global.compProperties.Clear();
                        }

                        foreach (var item in list)
                        {
                            ThingComp thingComp = null;
                            try
                            {
                                thingComp = (ThingComp)Activator.CreateInstance(item.compClass);
                                thingComp.parent = __instance;
                                __instance.AllComps.Add(thingComp);
                                thingComp.Initialize(item);
                            }
                            catch (Exception arg)
                            {
                                Log.Error("Could not instantiate or initialize a ThingComp: " + arg);
                                __instance.AllComps.Remove(thingComp);
                            }
                        }
                    }
                }
            }
        }

        //Todo more Comps needed
        [HarmonyPatch(typeof(ListerThings), "Add")]
        public static class Patch5
        {
            private static bool ContainComp(Thing t, Type CompClass)
            {
                if (t is Pawn)
                {
                    Pawn pawn = t as Pawn;

                    FixedPawnDef fixedPawnDef = FixedPawnUtility.Manager.GetDef(pawn);

                    if (fixedPawnDef == null)
                        return false;

                    foreach (var comp in fixedPawnDef.comps)
                    {
                        if (comp.compClass == CompClass)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            public static void Postfix(ref ListerThings __instance, ref Thing t)
            {
                FieldInfo fieldInfo = typeof(ListerThings).GetField("listsByGroup", BindingFlags.NonPublic | BindingFlags.Instance);

                List<Thing>[] listsByGroup = (List<Thing>[])fieldInfo.GetValue(__instance);

                FieldInfo fieldInfo2 = typeof(ListerThings).GetField("stateHashByGroup", BindingFlags.NonPublic | BindingFlags.Instance);

                int[] stateHashByGroup = (int[])fieldInfo2.GetValue(__instance);

                if (ContainComp(t, typeof(CompProjectileInterceptor)))
                {
                    List<Thing> list = listsByGroup[(int)ThingRequestGroup.ProjectileInterceptor];
                    if (list == null)
                    {
                        list = new List<Thing>();
                        listsByGroup[(int)ThingRequestGroup.ProjectileInterceptor] = list;
                        stateHashByGroup[(int)ThingRequestGroup.ProjectileInterceptor] = 0;
                    }
                    list.Add(t);
                    stateHashByGroup[(int)ThingRequestGroup.ProjectileInterceptor]++;
                }
            }
        }

        //Anomaly
        [HarmonyPatch(typeof(GameComponent_PawnDuplicator), "Duplicate")]
        public static class PawnDuplicatePatch
        {
            //copy comps
            public static void Prefix(Pawn pawn, Pawn __result)
            {
                FixedPawnDef def = pawn.GetFixedPawnDef();
                if(def != null)
                {
#if DEBUG
                    Log.Warning($"[Debug]Pawn复制：{pawn.Name}");
#endif

                    FPG_Global.compProperties.AddRange(def.comps);
                }
            }

            public static void Postfix(Pawn pawn, Pawn __result)
            {
                FixedPawnDef def = pawn.GetFixedPawnDef();
                if (def != null && __result.GetFixedPawnDef() == null)
                {
                    FixedPawnUtility.Manager.AddPawn(__result, def);
                }
            }
        }


    }
}



