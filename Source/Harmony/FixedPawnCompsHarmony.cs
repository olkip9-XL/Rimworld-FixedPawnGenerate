using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace FixedPawnGenerate
{
    [HarmonyPatch(typeof(ListerThings), "GroupIncludes")]
    public static class GroupIncludes_Patch
    {
        public static void Postfix(Thing thing, ThingRequestGroup group, ref bool __result)
        {
            if(thing is Pawn pawn)
            {
                FixedPawnDef fixedPawnDef = FixedPawnUtility.Manager.GetDef(pawn);

                if(fixedPawnDef != null)
                {
                    //Todo: more groups check
                    if (group == ThingRequestGroup.ProjectileInterceptor)
                    {
                        __result = __result || fixedPawnDef.comps.Any(x=> x is CompProperties_ProjectileInterceptor);
                    }
                }
            }
        }
    }

}
