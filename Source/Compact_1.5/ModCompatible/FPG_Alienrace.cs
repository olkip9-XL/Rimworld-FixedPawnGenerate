using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    internal static class FPG_Alienrace
    {
        internal static void SetPawnSkinColor(Pawn pawn, Color color)
        {
            AlienRace.AlienPartGenerator.AlienComp alienComp = pawn.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>();
            if (alienComp != null)
            {
                //color channel: base, hair, skin, skinBase, tattoo, favorite, ideo, mech
                alienComp.OverwriteColorChannel("skin", color, color);
            }
        }
    }
}
