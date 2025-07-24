using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RuntimeAudioClipLoader;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FixedPawnGenerate
{
    public static class FixedPawnHarmony
    {

        private static FixedPawnDef curPawnDef = null;

        public static void SetCompProperties(FixedPawnDef def)
        {
            if (curPawnDef == null)
                curPawnDef = def;
        }

        public static FixedPawnDef GetCompProperties()
        {
            FixedPawnDef def = curPawnDef;
            curPawnDef = null; 
            return def;
        }


        private static bool CallerInBlackList(string caller)
        {
            foreach (string str in FixedPawnUtility.callerBlackList)
            {
                if (str == caller)
                    return true;

                if (caller.Contains(str))
                    return true;
            }
            return false;
        }

        private static void AddComp(ThingWithComps thing, CompProperties compProperties)
        {
            ThingComp thingComp = null;
            try
            {
                // Avoid duplicate comp
                if (thing.AllComps.Any(x => x.GetType() == compProperties.compClass))
                {
                    return;
                }

                thingComp = (ThingComp)Activator.CreateInstance(compProperties.compClass);
                thingComp.parent = thing;
                thing.AllComps.Add(thingComp);
                thingComp.Initialize(compProperties);
            }
            catch (Exception arg)
            {
                Log.Error("Could not instantiate or initialize a ThingComp: " + arg);
                thing.AllComps.Remove(thingComp);
            }
        }

        [HarmonyPatch(typeof(PawnGenerator), "GenerateNewPawnInternal")]
        //[HarmonyPatch(typeof(PawnGenerator), "TryGenerateNewPawnInternal")]
        public static class Patch_GenerateNewPawnInternal
        {
            public static bool Prefix(out string __state, ref Pawn __result, ref PawnGenerationRequest request)
            {
                __state = "None";

                request.CanGeneratePawnRelations = FixedPawnUtility.Settings.allowNaturalRelationGenerate;

                //Black list check
                String caller = FixedPawnUtility.GetCallerMethodName(5);

                //if (FixedPawnUtility.callerBlackList.Contains(caller))
                if (CallerInBlackList(caller))
                {
#if DEBUG
                    Log.Warning($"[FixedPawnGenerate] 调用者:{caller}, \n生成: Skip");
#endif
                    return true;
                }

                //Randomly Get Def 
                float randValue = Rand.Value;

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
                    Log.Warning($"[FixedPawnGenerate] Prefix 调用者:{caller}, \n生成:{__state}");
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
                    Log.Warning($"[FixedPawnGenerate] Prefix 调用者:{caller}, \n生成: No Match");
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

                Log.Warning($"[FixedPawnGenerate] Request:\nfactionDef:{factionDef?.defName ?? "null"} \npawnKindDef:{pawnKindDef?.defName ?? "null"}, {race?.defName ?? "null"}");
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
                if (__instance is Pawn pawn)
                {
                    //new Generate
                    FixedPawnDef fixedPawnDef = GetCompProperties();

                    //Load data
                    if(fixedPawnDef == null)
                    {
                        fixedPawnDef = FixedPawnUtility.Manager.GetDef(pawn);
                    }

                    if (fixedPawnDef != null && !fixedPawnDef.comps.NullOrEmpty())
                    {
                        foreach (var comp in fixedPawnDef.comps)
                        {
                            AddComp(__instance, comp);
                        }
                    }

                }
            }
        }

        //Anomaly Duplicate
        [HarmonyPatch(typeof(GameComponent_PawnDuplicator), "Duplicate")]
        public static class PawnDuplicatePatch
        {
            //copy comps
            public static void Prefix(Pawn pawn)
            {

                if (pawn == null)
                {
                    Log.Warning("Pawn is null when Duplicating(Prefix) Pawn");
                    return;
                }

                FixedPawnDef def = pawn.GetFixedPawnDef();
                if (def != null)
                {
#if DEBUG
                    Log.Warning($"[Debug]Pawn复制：{pawn.Name}");
#endif

                    //compProperties.AddRange(def.comps);
                }
            }

            public static void Postfix(Pawn pawn, Pawn __result)
            {
                if (pawn == null || __result == null)
                {
                    Log.Warning("Pawn or __result is null when Duplicating(Postfix) Pawn");

                    //if (compProperties.Any())
                    //{
                    //    Log.Warning("compProperties is not empty when Duplicating Pawn");
                    //    compProperties.Clear();
                    //}
                    return;
                }

                FixedPawnDef def = pawn.GetFixedPawnDef();
                if (def != null && __result.GetFixedPawnDef() == null)
                {
                    FixedPawnUtility.Manager.AddPawn(__result, def);
                }
            }
        }


    }
}



