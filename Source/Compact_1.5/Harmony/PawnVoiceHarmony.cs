using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using static Verse.Dialog_InfoCard;

namespace FixedPawnGenerate
{

    internal static class PawnVoiceHarmony
    {
        //missing Attack、 Move

        [HarmonyPatch(typeof(Selector), "SelectInternal")]
        internal static class Patch_Selected
        {
            private static void Postfix(object obj)
            {
                Pawn pawn = obj as Pawn;

                if (pawn == null || !pawn.Awake() || pawn.Downed || pawn.Dead || !pawn.Spawned)
                {
                    return;
                }

                pawn.PlayVoice(PawnVoiceType.Selected);
            }
        }

        [HarmonyPatch(typeof(Verb_Shoot), "WarmupComplete")]
        internal static class Patch_Shout
        {
            private static void Postfix(Verb_Shoot __instance)
            {
                Pawn pawn = __instance.CasterPawn;

                //lower the chance to shout
                if(UnityEngine.Random.Range(0f, 1f) > 0.5f)
                    pawn.PlayVoice(PawnVoiceType.Shout);

            }
        }

        [HarmonyPatch(typeof(SanguophageUtility), "TryStartRegenComa")]
        private static class Patch_Retire
        {
            private static void Postfix(Pawn pawn, bool __result)
            {
                if (__result)
                {
                    pawn.PlayVoice(PawnVoiceType.Retire);
                }
            }
        }

    }
}
