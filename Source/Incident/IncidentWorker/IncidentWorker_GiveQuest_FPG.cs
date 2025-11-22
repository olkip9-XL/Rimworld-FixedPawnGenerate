using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixedPawnGenerate
{
    internal class IncidentWorker_GiveQuest_FPG : IncidentWorker_GiveQuest
    {
        ModExtension_FixedPawnIncident ModExt => this.def.GetModExtension<ModExtension_FixedPawnIncident>();

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if(ModExt?.SatisfiedPawns == null || ModExt.SatisfiedPawns.Count == 0)
            {
                return false;
            }

            return base.CanFireNowSub(parms);
        }

     

    }
}
