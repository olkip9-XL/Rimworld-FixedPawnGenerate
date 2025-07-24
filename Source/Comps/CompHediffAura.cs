using FixedPawnGenerate;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace FixedPawnGenerate
{
    internal class CompHediffAura : ThingComp
    {
        public CompHediffAura() { }

        public CompProperties_HediffAura Props => (CompProperties_HediffAura)this.props;

        public Pawn parentPawn => this.parent as Pawn;

        private Mote mote;

        public override void CompTick()
        {
            base.CompTick();

            //draw mote
            if (parentPawn.Drafted)
            {
                if (this.Props.mote != null && (this.mote == null || this.mote.Destroyed))
                {
                    this.mote = MoteMaker.MakeAttachedOverlay(this.parentPawn, this.Props.mote, Vector3.zero);
                }
                if (this.mote != null)
                {
                    this.mote.Maintain();
                }
            }

            //1 fresh per second
            if (GenTicks.TicksGame % 60 != 0)
            {
                return;
            }

            //check pawn
            if (!this.parentPawn.Awake() || this.parentPawn.health == null || this.parentPawn.health.InPainShock || !this.parentPawn.Spawned || this.parentPawn.DeadOrDowned)
            {
                return;
            }

            if (Props.draftedOnly && !this.parentPawn.Drafted)
            {
                return;
            }

            //add hediff to pawns in range
            List<Pawn> pawnsInRange = new List<Pawn>(GenRadial.RadialDistinctThingsAround(this.parent.Position, this.parent.Map, this.Props.radius, Props.affectSelf).OfType<Pawn>());

            if (!Props.requiredFixedPawnTag.NullOrEmpty())
            {
                pawnsInRange = pawnsInRange
                    .Where(p => p.HasFixedPawnTag(Props.requiredFixedPawnTag))
                    .ToList();
            }

            foreach (var pawn in pawnsInRange)
            {
                if (!pawn.health.hediffSet.HasHediff(this.Props.hediffDef))
                {
                    Hediff hediff = HediffMaker.MakeHediff(this.Props.hediffDef, pawn);

                    HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
                    if(hediffComp_Disappears == null)
                    {
                        Log.Error($"Hediff {Props.hediffDef} is missing HediffComp_Disappears");
                        return;
                    }
                    hediffComp_Disappears.ticksToDisappear = 600;

                    HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
                    if(hediffComp_Link != null)
                    {
                        hediffComp_Link.drawConnection = true;
                        hediffComp_Link.other = this.parentPawn;
                    }

                    if (Props.severity >= 0f)
                    {
                        hediff.Severity = Props.severity;
                    }

                    pawn.health.AddHediff(hediff);
                }
                else
                {
                    //refresh hediff
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(this.Props.hediffDef);

                    HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();

                    hediffComp_Disappears.ticksToDisappear = Props.ticksToDisappear;
                }

            }
          
        }
     
    }
}
