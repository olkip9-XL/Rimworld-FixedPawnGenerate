using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    public static class PawnExtension
    {
        public static FixedPawnDef GetFixedPawnDef(this Pawn pawn)
        {
            return FixedPawnUtility.Manager.GetDef(pawn);
        }
    }
}
