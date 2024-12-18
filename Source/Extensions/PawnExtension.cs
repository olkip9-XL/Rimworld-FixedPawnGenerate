using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    public enum PawnPositionState
    {
        IN_MAP,
        WORLD_PAWN,
        IN_CONTAINER,
        IN_CORPSE,
        OTHER,

        ERROR
    }
    public static class PawnExtension
    {
        public static FixedPawnDef GetFixedPawnDef(this Pawn pawn)
        {
            return FixedPawnUtility.Manager.GetDef(pawn);
        }

        public static PawnPositionState GetPawnPositionState(this Pawn pawn)
        {
            if (pawn == null)
            {
                Log.Error("Pawn is null");
                return PawnPositionState.ERROR;
            }

            if (pawn.Map != null)
            {
                return PawnPositionState.IN_MAP;
            }

            if (Find.WorldPawns.Contains(pawn))
            {
                return PawnPositionState.WORLD_PAWN;
            }

            if (pawn.InContainerEnclosed)
            {
                return PawnPositionState.IN_CONTAINER;
            }

            if (pawn.Corpse != null)
            {
                return PawnPositionState.IN_CORPSE;
            }

            return PawnPositionState.OTHER;
        }
    }

}
