using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse.AI;

namespace FixedPawnGenerate
{
    internal class JobDriver_OpenGachaComms : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed, false);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn((Toil to) => !((Building_CommsConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseCommsNow);
            Toil openComms = new Toil();
            openComms.initAction = delegate ()
            {
                if (((Building_CommsConsole)openComms.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseCommsNow)
                {
                    //执行抽卡
                    ModExtension_OpenCommsJob modExt = job.def.GetModExtension<ModExtension_OpenCommsJob>();

                    modExt.gachaDef.Worker.DoGacha(openComms.actor);
                }
            };
            yield return openComms;
            yield break;
        }


    }
}
