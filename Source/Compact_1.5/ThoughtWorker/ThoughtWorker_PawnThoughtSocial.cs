using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    internal class ThoughtWorker_PawnThoughtSocial : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
        {
            if (!def.HasModExtension<ModExtension_PawnThoughtSocial>())
                return false;

            if (!p.RaceProps.Humanlike)
                return false;

            if (!RelationsUtility.PawnsKnowEachOther(p, other))
                return false;

            bool useTrait = def.GetModExtension<ModExtension_PawnThoughtSocial>().useTrait;
            if (useTrait)
            {
                if(p.story.traits.HasTrait(def.GetModExtension<ModExtension_PawnThoughtSocial>().pawnTrait) &&
                    other.story.traits.HasTrait(def.GetModExtension<ModExtension_PawnThoughtSocial>().otherTrait))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if(p.GetFixedPawnDef() == def.GetModExtension<ModExtension_PawnThoughtSocial>().pawnDef &&
                    other.GetFixedPawnDef() == def.GetModExtension<ModExtension_PawnThoughtSocial>().otherDef)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


    }
}
