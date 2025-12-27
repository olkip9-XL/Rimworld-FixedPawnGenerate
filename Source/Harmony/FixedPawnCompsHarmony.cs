using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            if (group == ThingRequestGroup.ProjectileInterceptor)
            {
                __result = __result || thing.HasComp<CompProjectileInterceptor>();
            }
        
        }
    }

}
