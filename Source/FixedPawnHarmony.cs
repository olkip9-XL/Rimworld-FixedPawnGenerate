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

                if (FixedPawnUtility.callerBlackList.Contains(caller))
                {
                    return;
                }

                float randValue = Rand.Value;

                float maxRate = (caller == "StartingPawnUtility.NewGeneratedStartingPawn" ? 0.125f : 1f);

                List<FixedPawnDef> list = FixedPawnUtility.GetFixedPawnDefsByRequest(ref request).FindAll(x => randValue < x.generateRate && randValue<maxRate);

                if (list.Count > 0)
                {
                    FixedPawnDef def = FixedPawnUtility.GetRandomFixedPawnDefByWeight(list);

                    if (def == null)
                        return;

                    if (def.isUnique && caller != "StartingPawnUtility.NewGeneratedStartingPawn")
                    {
                        FixedPawnUtility.Manager.uniqePawns.Remove(def);
                    }
                    __state = def.defName;

                    request.CanGeneratePawnRelations = false;

                    //request.ForcedXenotype= DefDatabase<XenotypeDef>.GetNamed("Highmate");
 
                    if(def.xenotype!=null)
                        request.ForcedXenotype = def.xenotype;

                    if (def.customXenotype!=null)
                    {
                        CustomXenotype customXenotype= CharacterCardUtility.CustomXenotypesForReading.Find(x => x.name == def.customXenotype);
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

                    if(def.gender!= Gender.None)
                        request.FixedGender = def.gender;

                    if(def.firstName!=null)
                        request.SetFixedBirthName(def.firstName);
                    if(def.lastName != null)
                        request.SetFixedLastName(def.lastName);
                    if(def.bodyType!=null)
                        request.ForceBodyType = def.bodyType;

                }

            }
            public static void Postfix(ref Pawn __result, ref PawnGenerationRequest request, string __state)
            {
                Pawn pawn = __result;

                if (pawn != null && __state != "None")
                {
                    FixedPawnDef fixedPawnDef = DefDatabase<FixedPawnDef>.GetNamed(__state);

                    FixedPawnUtility.ModifyPawn(pawn, fixedPawnDef);
                }
#if DEBUG
                String caller = FixedPawnUtility.GetCallerMethodName(5);
                Log.Warning($"[Debug]调用者:{caller}, 生成:{__state}");
#endif

            }
        }
    }
}



