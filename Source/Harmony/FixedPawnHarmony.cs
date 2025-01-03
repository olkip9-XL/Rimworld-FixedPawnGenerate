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

        private static List<CompProperties> compProperties = new List<CompProperties>();

        public static void SetCompProperties(List<CompProperties> list)
        {
            if (compProperties.Count > 0)
                Log.Error("compProperties in FixedPawnHarmony is not null, may be it is been set twice");

            compProperties.Clear();
            compProperties.AddRange(list);
        }

        private static bool CallerInBlackList(string caller)
        {
            foreach(string str in FixedPawnUtility.callerBlackList)
            {
                if (str == caller)
                    return true;
                
                if(caller.Contains(str)) 
                    return true;
            }
            return false;
        }


        [HarmonyPatch(typeof(PawnGenerator), "GenerateNewPawnInternal")]
        public static class Patch_GenerateNewPawnInternal
        {
            public static bool Prefix(out string __state, ref Pawn __result, ref PawnGenerationRequest request)
            {
                __state = "None";

                request.CanGeneratePawnRelations = FixedPawnUtility.Settings.allowNaturalRelationGenerate;

                //Black list check
                String caller = FixedPawnUtility.GetCallerMethodName(5);

                //if (FixedPawnUtility.callerBlackList.Contains(caller))
                if(CallerInBlackList(caller))
                {
#if DEBUG
                    Log.Warning($"[Debug]调用者:{caller}, 生成: Skip");
#endif
                    return true;
                }

                //Randomly Get Def 
                float randValue = Rand.Value;

                //bool isStarting = (caller == "StartingPawnUtility.NewGeneratedStartingPawn" ||
                //                    caller == "DynamicMethodDefinition.Verse.StartingPawnUtility.NewGeneratedStartingPawn_Patch0");

                bool isStarting = caller.Contains("StartingPawnUtility.NewGeneratedStartingPawn");

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
                    Log.Warning($"[Debug]Prefix调用者:{caller}, 生成:{__state}");
#endif

                    __result = FixedPawnUtility.ModifyRequest(ref request, def, !isStarting);
                    if (__result != null)
                    {
                        __state = "None";
                        return false;
                    }
                }
#if DEBUG
                else
                {
                    Log.Warning($"[Debug]Prefix调用者:{caller}, 生成: No Match");
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

                    FixedPawnUtility.ModifyPawn(pawn, fixedPawnDef);
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

        [HarmonyPatch(typeof(WorldPawns), "PassToWorld")]
        public static class Patch_PassToWorld
        {
            public static bool Prefix(Pawn pawn, ref PawnDiscardDecideMode discardMode)
            {
                FixedPawnDef def = pawn.GetFixedPawnDef();
                if (def != null && def.isUnique)
                {
                    discardMode = PawnDiscardDecideMode.KeepForever;

                    //already in world
                    if (pawn.GetPawnPositionState() == PawnPositionState.WORLD_PAWN)
                        return false;
                }

                return true;
            }
        }


        [HarmonyPatch(typeof(ThingWithComps), "InitializeComps")]
        public static class Patch_InitializeComps
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

                        //if (FPG_Global.compProperties.Count > 0)
                        //{
                        //    list.AddRange(FPG_Global.compProperties);
                        //    FPG_Global.compProperties.Clear();
                        //}

                        if (compProperties.Count > 0)
                        {
                            list.AddRange(compProperties);
                            compProperties.Clear();
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
        public static class Patch_ListerThings_Add
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

        //Anomaly Duplicate
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

                    compProperties.AddRange(def.comps);
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



