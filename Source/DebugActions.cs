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

        [DebugAction("FixedPawnGenerate", "Log spawned pawns", false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnAllCharacters()
        {
            FixedPawnUtility.Manager.LogPawnDics();
        }
    }
}
