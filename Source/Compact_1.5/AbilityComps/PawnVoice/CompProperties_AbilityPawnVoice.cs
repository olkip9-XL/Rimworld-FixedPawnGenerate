using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixedPawnGenerate
{
    internal class CompProperties_AbilityPawnVoice : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityPawnVoice()
        {
            this.compClass = typeof(CompAbilityEffect_PawnVoice);
        }

        public PawnVoiceType selfVoice = PawnVoiceType.None;

        public PawnVoiceType targetVoice = PawnVoiceType.None;

        public float targetRadius = 0f;
    }
}
