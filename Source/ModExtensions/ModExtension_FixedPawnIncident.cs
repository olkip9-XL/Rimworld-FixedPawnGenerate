using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static UnityEngine.Scripting.GarbageCollector;

namespace FixedPawnGenerate
{
    internal class ModExtension_FixedPawnIncident : DefModExtension
    {
        public List<string> pawnTags;
        public bool uniqueOnly = false;

        public int raidCount = 0;
        public IntRange raidIntervalTicks = new IntRange(300, 600);

        private GameComponent_FixedPawn gameComponent => Current.Game.GetComponent<GameComponent_FixedPawn>();
        public List<FixedPawnDef> SatisfiedPawns
        {
            get
            {
                List<FixedPawnDef> list = DefDatabase<FixedPawnDef>.AllDefs.Where(def =>
                {
                    if (uniqueOnly && !def.isUnique)
                        return false;

                    if (!def.tags.Intersect(pawnTags).Any())
                        return false;

                    if (gameComponent.spawnedUniquePawns.Contains(def))
                        return false;

                    return true;
                }).ToList();

                return list;
            }
        }

    }
}
