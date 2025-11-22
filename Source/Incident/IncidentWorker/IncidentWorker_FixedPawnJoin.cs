using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace FixedPawnGenerate
{
    internal class IncidentWorker_FixedPawnJoin : IncidentWorker_WandererJoin
    {
        ModExtension_FixedPawnIncident ModExt => this.def.GetModExtension<ModExtension_FixedPawnIncident>();

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            bool result = base.TryExecuteWorker(parms);
            if (!result)
                return false;

            int ticksDelay = 0;
            Map map = Find.AnyPlayerHomeMap;


            for (int i = 0; i < ModExt.raidCount; i++)
            {
                IncidentParms raidParms = new IncidentParms();
                raidParms.target = map;
                raidParms.target = Find.CurrentMap;
                raidParms.forced = true;
                raidParms.points = StorytellerUtility.DefaultThreatPointsNow(map);
                raidParms.faction = Find.FactionManager.RandomRaidableEnemyFaction();

                List<RaidStrategyDef> raidStrategyDefs = DefDatabase<RaidStrategyDef>.AllDefsListForReading.
                    Where(x => x.Worker.CanUseWith(raidParms, PawnGroupKindDefOf.Combat)).ToList();

                if (raidStrategyDefs.Any())
                    raidParms.raidStrategy = raidStrategyDefs.RandomElement();
                else
                    raidParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;

                ticksDelay += ModExt.raidIntervalTicks.RandomInRange;
                Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame + ticksDelay, raidParms, 0);
            }

            return true;
        }

        public override bool CanSpawnJoiner(Map map)
        {
            if (ModExt.SatisfiedPawns.NullOrEmpty())
                return false;

            return base.CanSpawnJoiner(map);
        }

        public override Pawn GeneratePawn()
        {
            if (ModExt?.SatisfiedPawns == null || ModExt.SatisfiedPawns.Count == 0)
                return base.GeneratePawn();

            FixedPawnDef def = ModExt.SatisfiedPawns.RandomElement();
            try
            {
                Pawn pawn = FixedPawnUtility.GenerateFixedPawnWithDef(def);
                ModExt.SatisfiedPawns.Remove(def);
                return pawn;
            }
            catch (Exception e)
            {
                Log.Error($"Error while generating fixed pawn in incident: {e}");
                return base.GeneratePawn();
            }
        }
    }
}
