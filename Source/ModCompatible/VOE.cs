using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Outposts;
using Verse;

namespace FixedPawnGenerate
{
    internal static class VOE
    {
        public static bool IsInVOEOutpost(this Pawn pawn)
        {
            foreach(Outpost outpost in Find.WorldObjects.AllWorldObjects.Where(x => x is Outpost))
            {
                outpost.AllPawns.Contains(pawn);
                return true;
            }

            return false;
        }
    }
}
