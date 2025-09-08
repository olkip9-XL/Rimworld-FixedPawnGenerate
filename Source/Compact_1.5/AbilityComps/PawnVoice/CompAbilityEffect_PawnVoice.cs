using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    internal class CompAbilityEffect_PawnVoice : CompAbilityEffect
    {
        protected Pawn Caster => this.parent.pawn;

        public new CompProperties_AbilityPawnVoice Props => (CompProperties_AbilityPawnVoice)this.props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (Props.selfVoice != PawnVoiceType.None)
            {
                Caster.PlayVoice(Props.selfVoice);
            }

            if (Props.targetVoice != PawnVoiceType.None && target.Pawn != null && target.Pawn != Caster)
            {
                target.Pawn.QueueVoice(Props.targetVoice);
            }

            if (Props.targetVoice != PawnVoiceType.None && Props.targetRadius > 0f)
            {
                Pawn pawn = GenRadial.RadialDistinctThingsAround(target.Cell, Caster.Map, Props.targetRadius, true).OfType<Pawn>()
                    .Where(x => x != Caster && x.Faction.AllyOrNeutralTo(Caster.Faction) && x.HasComp<CompPawnVoice>()).RandomElement();

                if (pawn != null)
                {
                    pawn.QueueVoice(Props.targetVoice);
                }
            }
        }
    }
}
