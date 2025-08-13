using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    internal class ModExtension_PawnThoughtSocialGroup : DefModExtension
    {
        private List<FixedPawnDef> pawnDefs;
        private List<FixedPawnDef> otherDefs;

        private HashSet<FixedPawnDef> pawnDefsInt = null;
        public HashSet<FixedPawnDef> PawnDefs
        {
            get
            {
                if (pawnDefsInt == null)
                {
                    pawnDefsInt = new HashSet<FixedPawnDef>(pawnDefs);
                }
                return pawnDefsInt;
            }
        }

        private HashSet<FixedPawnDef> otherDefsInt = null;
        public HashSet<FixedPawnDef> OtherDefs
        {
            get
            {
                if (otherDefsInt == null)
                {
                    otherDefsInt = new HashSet<FixedPawnDef>(otherDefs);
                }
                return otherDefsInt;
            }
        }

        public bool groupEachOther = false;

        public override IEnumerable<string> ConfigErrors()
        {
            if (pawnDefs.NullOrEmpty())
            {
                yield return "ModDefExtension PawnThoughtSocialGroup: pawnDefs is null or empty.";
            }
            else if (pawnDefs.Any(x => !x.isUnique))
            {
                yield return "ModDefExtension PawnThoughtSocialGroup: pawnDefs contains non-unique FixedPawnDef(s).";
            }

            if (!groupEachOther)
            {
                if (otherDefs.NullOrEmpty())
                {
                    yield return "ModDefExtension PawnThoughtSocialGroup: otherDefs is null or empty.";
                }
                else if (otherDefs.Any(x => !x.isUnique))
                {
                    yield return "ModDefExtension PawnThoughtSocialGroup: otherDefs contains non-unique FixedPawnDef(s).";
                }
            }


        }

    }
}
