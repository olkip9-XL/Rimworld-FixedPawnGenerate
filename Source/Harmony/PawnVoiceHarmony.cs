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

        [HarmonyPatch(typeof(FloatMenuOptionProvider_DraftedMove), "PawnGotoAction")]
        internal static class Patch_Move
        {
            private static void Postfix(Pawn pawn)
            {
                pawn.PlayVoice(PawnVoiceType.Move);
            }
        }

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

        [HarmonyPatch(typeof(FloatMenuOptionProvider_DraftedAttack), "GetRangedAttackAction")]
        private static class Patch_Attack
        {
            private static void Postfix(Pawn pawn)
            {
                pawn.PlayVoice(PawnVoiceType.Attack);
            }
        }

        [HarmonyPatch(typeof(SanguophageUtility), "TryStartRegenComa")]
        private static class Patch_Retire
        {
            private static void Postfix(Pawn pawn, bool __result)
            {
                if (__result)
                    pawn.PlayVoice(PawnVoiceType.Retire);
            }
        }

    }
}
