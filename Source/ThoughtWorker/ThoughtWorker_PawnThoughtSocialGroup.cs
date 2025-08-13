using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    internal class ThoughtWorker_PawnThoughtSocialGroup : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
        {
            if (!def.HasModExtension<ModExtension_PawnThoughtSocialGroup>())
                return false;

            if (!p.RaceProps.Humanlike)
                return false;

            if (!RelationsUtility.PawnsKnowEachOther(p, other))
                return false;

            ModExtension_PawnThoughtSocialGroup ext = def.GetModExtension<ModExtension_PawnThoughtSocialGroup>();
           
            FixedPawnDef pawnDef = p.GetFixedPawnDef();
            FixedPawnDef otherDef = other.GetFixedPawnDef();

            if(ext.groupEachOther)
            {
                if (ext.PawnDefs.Contains(pawnDef) && ext.PawnDefs.Contains(otherDef))
                {
                    return true;
                }
            }
            else
            {
                if (ext.PawnDefs.Contains(pawnDef) && ext.OtherDefs.Contains(otherDef))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
