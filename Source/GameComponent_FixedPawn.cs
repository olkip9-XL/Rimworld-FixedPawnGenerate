using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    public class GameComponent_FixedPawn : GameComponent
    {
        public Game game;

        public List<FixedPawnDef> uniqePawns = new List<FixedPawnDef>();

        private Dictionary<String, FixedPawnDef> pawnDics = new Dictionary<String, FixedPawnDef>();

        private Dictionary<Pawn, FixedPawnDef> cachedPawns = new Dictionary<Pawn, FixedPawnDef>();

        public GameComponent_FixedPawn(Game game) : base()
        {
            this.game = game;

            uniqePawns.AddRange(DefDatabase<FixedPawnDef>.AllDefsListForReading.FindAll(x => x.isUnique));

#if DEBUG          
            Log.Warning("GameComponent_FixedPawn Constructor called");
#endif
        }
        public override void LoadedGame()
        {
            base.LoadedGame();

            //construct cachedPawns

            foreach (Map map in Find.Maps)
            {
                foreach (Pawn pawn in map.mapPawns.AllPawns)
                {
                    if(pawnDics.TryGetValue(pawn.ThingID, out FixedPawnDef def))
                    {
                        cachedPawns.Add(pawn, def);
                    }
                }
            }

            foreach (Pawn pawn in Find.WorldPawns.AllPawnsAliveOrDead)
            {
                if (pawnDics.TryGetValue(pawn.ThingID, out FixedPawnDef def))
                {
                    cachedPawns.Add(pawn, def);
                }
            }
#if DEBUG
            Log.Warning($"LoadedGame,unniqePawns def count{uniqePawns.Count}");
#endif
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();

            //starting pawns
            for(int i= Find.GameInitData.startingAndOptionalPawns.Count-1; i>=0; i--)
            {
                Pawn pawn = Find.GameInitData.startingAndOptionalPawns[i];
                FixedPawnDef fixedPawnDef = FixedPawnUtility.Manager.GetDef(pawn);

                if (fixedPawnDef != null)
                {
                    uniqePawns.Remove(fixedPawnDef);
                }
            }

#if DEBUG
            Log.Warning($"StartedNewGame,unniqePawns count{uniqePawns.Count}");
#endif
        }
        public override void ExposeData()
        {
            base.ExposeData();

            foreach(var pair in cachedPawns)
            {
                Pawn pawn = pair.Key;

                if(pawn.Map == null && !Find.WorldPawns.Contains(pawn))
                {
                    Find.WorldPawns.PassToWorld(pawn, RimWorld.Planet.PawnDiscardDecideMode.KeepForever );

                    Faction faction = null;
                    if (pair.Value.faction != null)
                        faction = Find.FactionManager.FirstFactionOfDef(pair.Value.faction);

                    pawn.SetFaction(faction);
                }
            }

            Scribe_Collections.Look<FixedPawnDef>(ref uniqePawns, "uniqePawns", LookMode.Def);

            Scribe_Collections.Look(ref pawnDics, "pawnDics", LookMode.Value, LookMode.Def);
#if DEBUG
            Log.Warning($"GameComponent_FixedPawn ExposeData: uniqePawns:{uniqePawns.Count}, pawnDics:{pawnDics.Count}");
#endif
        }

        public FixedPawnDef GetDef(Pawn pawn)
        {
            if(pawn == null)
            {
                return null;
            }

            if(cachedPawns.TryGetValue(pawn, out FixedPawnDef def))
            {
                return def;
            }

            if(pawnDics.TryGetValue(pawn.ThingID, out def))
            {
                return def;
            }

            return null;
        }

        public Pawn GetPawn(FixedPawnDef def)
        {
            Pawn pawn = null;
            pawn = cachedPawns.FirstOrDefault(x => x.Value == def).Key;
            if(pawn != null)
            {
                return pawn;
            }

            String ThingID = pawnDics.FirstOrDefault(x => x.Value == def).Key;

            foreach(Map map in Find.Maps)
            {
                foreach(Pawn p in map.mapPawns.AllPawns)
                {
                    if(p.ThingID == ThingID)
                    {
                       return p;
                    }
                }
            }

            foreach (Pawn p in Find.WorldPawns.AllPawnsAliveOrDead)
            {
                if (p.ThingID == ThingID)
                {
                    return p;
                }
            }

            return null;
        }

        public void AddPawn(Pawn pawn, FixedPawnDef def)
        {
            pawnDics.Add(pawn.ThingID, def);
            cachedPawns.Add(pawn, def);
        }

        public void RemovePawn(Pawn pawn)
        {
            pawnDics.Remove(pawn.ThingID);
            cachedPawns.Remove(pawn);
        }

        public void LogPawnDics()
        {
            String str = "============Pawn Dics============\n";

            str += $"pawnDics count:{pawnDics.Count}\n";
            foreach (var pair in pawnDics)
            {
                str+= $"{pair.Key} : {pair.Value.defName}\n";
            }

            str += $"cached pawnDics count:{cachedPawns.Count}\n";
            foreach (var pair in cachedPawns)
            {
                str += $"{pair.Key.Name}({pair.Key.ThingID}) : {pair.Value.defName}\n";
            }

            Log.Warning(str);
        }

    }

}
