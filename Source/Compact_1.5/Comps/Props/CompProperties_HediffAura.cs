using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    internal class CompProperties_HediffAura : CompProperties
    {
        public CompProperties_HediffAura()
        {
            this.compClass = typeof(CompHediffAura);
        }

        public float radius = 10f;

        public HediffDef hediffDef;

        public string requiredFixedPawnTag;

        public bool affectSelf = false;

        public bool draftedOnly = true;

        public int ticksToDisappear;

        public float severity = -1f;

        public ThingDef mote;
    }
}
