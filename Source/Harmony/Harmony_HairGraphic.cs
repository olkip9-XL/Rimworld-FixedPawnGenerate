using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    internal static class Harmony_HairGraphic
    {

        [HarmonyPatch(typeof(PawnRenderNodeWorker), "ScaleFor")]
        static class Patch_Test
        {
            static void Postfix(PawnRenderNode node, PawnDrawParms parms, ref Vector3 __result)
            {
                if (node is PawnRenderNode_Hair hairNode)
                {
                    Pawn pawn = parms.pawn;

                    HairDef hairDef = pawn.story.hairDef;

                    if (hairDef.GetModExtension<ModExtension_HairExt>() is ModExtension_HairExt hairExt)
                    {
                        __result.x *= hairExt.drawSize.x;
                        __result.z *= hairExt.drawSize.y;
                    }
                }
            }
        }


    }
}
