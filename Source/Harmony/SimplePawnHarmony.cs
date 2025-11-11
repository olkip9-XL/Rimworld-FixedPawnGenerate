using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{

    [HarmonyPatch(typeof(PawnRenderNode), "AddChildren")]
    internal static class SimplePawnHarmony
    {
        static void Postfix(ref PawnRenderNode[] ___children, ref PawnRenderTree ___tree)
        {
            if (___children.Any(x => x is PawnRenderNode_SimplePawn))
            {
                List<PawnRenderNode> list = ___children.ToList();
                list.RemoveAll(x => x.GetType().Name.Contains("NLFacialAnimation"));
                ___children = list.ToArray();
            }
        }
    }
}
