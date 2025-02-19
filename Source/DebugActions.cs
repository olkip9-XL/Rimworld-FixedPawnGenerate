using LudeonTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    public static class DebugActions
    {

        [DebugAction("FixedPawnGenerate", "FPG: Log spawned pawns", false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void LogSpawnedPawns()
        {
            FixedPawnUtility.Manager.LogPawnDics();
        }
        [DebugAction("FixedPawnGenerate", "FPG: Log world pawns", false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing)]
        private static void LogWorldPawns()
        {
            Find.WorldPawns.LogWorldPawns();
        }


    }
}
