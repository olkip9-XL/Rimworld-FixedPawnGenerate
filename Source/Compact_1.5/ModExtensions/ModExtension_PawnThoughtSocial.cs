using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    internal class ModExtension_PawnThoughtSocial : DefModExtension
    {
        public bool useTrait = false;

        public TraitDef pawnTrait;
        public TraitDef otherTrait;

        public FixedPawnDef pawnDef;
        public FixedPawnDef otherDef;

        public override IEnumerable<string> ConfigErrors()
        {
            if (useTrait)
            {
                if(pawnTrait == null)
                {
                    yield return "ModDefExtension PawnTraitSocial: use trait but pawnTrait is null.";
                }
                if(otherTrait == null)
                {
                    yield return "ModDefExtension PawnTraitSocial: use trait but otherTrait is null.";
                }
            }
            else
            {
                if(pawnDef == null)
                {
                    yield return "ModDefExtension PawnTraitSocial: use FixedPawnDef but pawnDef is null.";
                }
                else if(!pawnDef.isUnique)
                {
                    yield return "ModDefExtension PawnTraitSocial: use FixedPawnDef but pawnDef is not unique.";
                }

                if(otherDef == null)
                {
                    yield return "ModDefExtension PawnTraitSocial: use FixedPawnDef but otherDef is null.";
                }
                else if(!otherDef.isUnique)
                {
                    yield return "ModDefExtension PawnTraitSocial: use FixedPawnDef but otherDef is not unique.";
                }
            }
        }
    }
}
