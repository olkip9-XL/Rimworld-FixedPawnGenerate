using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    public class FPG_AlienraceAddonProps
    {
        [NoTranslate]
        public string addonName;

        public int variantIndex = 0;

        public Color? color;

        public Color? colorTwo;
        public void ApplyToPawn(Pawn pawn)
        {
            AlienraceUtility.SetPawnAddon(pawn, addonName, variantIndex);

            if (color.HasValue || colorTwo.HasValue)
            {
                AlienraceUtility.SetAlienAddonColor(pawn, addonName, color, colorTwo);
            }
        }

    }
}
