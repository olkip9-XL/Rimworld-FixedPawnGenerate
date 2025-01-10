using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    public class CompProperties_Tachie : CompProperties
    {
        public CompProperties_Tachie()
        {
            this.compClass = typeof(CompTachie);
        }

        public String texture;

        public float offsetX = 0f;

        public float offsetY = 0f;

        public float scale = 1f;
    }
}
