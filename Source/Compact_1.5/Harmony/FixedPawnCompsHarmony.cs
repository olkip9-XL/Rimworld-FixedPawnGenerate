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

    //1.5 only
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

}
