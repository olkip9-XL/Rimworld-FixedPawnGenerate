﻿using Fixed_Pawn_Generate;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
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

        [HarmonyPatch(typeof(PawnGenerator), "GenerateNewPawnInternal")]
        public static class Patch1
        {
            public static void Prefix(out string __state, ref PawnGenerationRequest request)
            {
                __state = "None";

                String caller = FixedPawnUtility.GetCallerMethodName(5);

                //if(!FixedPawnUtility.Manager.callerWhiteList.Contains(caller))
                //{
                //    return;
                //}

                if (FixedPawnUtility.callerBlackList.Contains(caller))
                {
                    return;
                }

                float randValue = Rand.Value;

                List<FixedPawnDef> list = FixedPawnUtility.GetFixedPawnDefsByCRequest(ref request).FindAll(x => randValue < x.generateRate);

                if (list.Count > 0)
                {
                    FixedPawnDef def = FixedPawnUtility.GetRandomFixedPawnDef(list);

                    if (def == null)
                        return;

                    if (def.isUnique)
                    {
                        FixedPawnUtility.Manager.uniqePawns.Remove(def);
                    }

                    __state = def.defName;

                    request.CanGeneratePawnRelations = false;

                    if(def.fileName!=null)
                        request.SetFixedBirthName(def.fileName);
                    if(def.lastName != null)
                        request.SetFixedLastName(def.lastName);
                    if(def.bodyType!=null)
                        request.ForceBodyType = def.bodyType;

                }

            }
            public static void Postfix(ref Pawn __result, ref PawnGenerationRequest request, string __state)
            {
                Pawn pawn = __result;

                String caller = FixedPawnUtility.GetCallerMethodName(5);

                if (pawn != null && __state != "None")
                {
                    FixedPawnDef fixedPawnDef = DefDatabase<FixedPawnDef>.GetNamed(__state);

                    FixedPawnUtility.ModifyPawn(pawn, fixedPawnDef);
                }
#if DEBUG
                Log.Warning($"[Debug]调用者:{caller}, 生成:{__state}");
#endif

            }
        }
    }
}



