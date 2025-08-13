using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using UnityEngine;

namespace FixedPawnGenerate
{
    public class ModExtension_UniqueWeaponExt : DefModExtension
    {
        public bool forbidRandomName = false;

        public bool useForceColor = false;
        public Color forceColor;

        public List<WeaponTraitDef> forceAddTraits = new List<WeaponTraitDef>();
        public List<WeaponTraitDef> traitsOverride = new List<WeaponTraitDef>();
    }
}

