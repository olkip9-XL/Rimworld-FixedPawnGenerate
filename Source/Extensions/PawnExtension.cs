using RimWorld.Planet;
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
        IN_CARAVAN,
        IN_VOE_OUTPOST,
        IN_OTHER_HOLDER,
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

            if (pawn.InContainerEnclosed)
            {
                return PawnPositionState.IN_CONTAINER;
            }

            if (pawn.Corpse != null)
            {
                return PawnPositionState.IN_CORPSE;
            }

            if (pawn.GetCaravan() != null)
            {
                return PawnPositionState.IN_CARAVAN;
            }

            if (Find.WorldPawns.Contains(pawn))
            {
                return PawnPositionState.WORLD_PAWN;
            }


            if (ModLister.HasActiveModWithName("Vanilla Outposts Expanded") && pawn.IsInVOEOutpost())
            {
                return PawnPositionState.IN_VOE_OUTPOST;
            }   

            if (pawn.ParentHolder != null)
            {
                return PawnPositionState.IN_OTHER_HOLDER;
            }

            return PawnPositionState.OTHER;
        }
    
        public static bool IsUniquePawn(this Pawn pawn)
        {
            FixedPawnDef def = pawn.GetFixedPawnDef();

            if(def != null && def.isUnique)
                return true;
            else
                return false;
        }
    
    
    
    }

}
