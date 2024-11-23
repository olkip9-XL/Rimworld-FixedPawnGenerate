using HarmonyLib;
using RimWorld;
using RuntimeAudioClipLoader;
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

        private List<Pawn> workingPawnList;
        private List<FixedPawnDef> workingDefList;

        //old
        //public List<FixedPawnDef> uniqePawns = new List<FixedPawnDef>();

        private Dictionary<String, FixedPawnDef> pawnDics = new Dictionary<String, FixedPawnDef>();

        //old
        //private Dictionary<Pawn, FixedPawnDef> cachedPawns = new Dictionary<Pawn, FixedPawnDef>();

        private Dictionary<Pawn, FixedPawnDef> spawnedPawns = new Dictionary<Pawn, FixedPawnDef>();

        public GameComponent_FixedPawn(Game game) : base()
        {
            this.game = game;
        }
        public override void LoadedGame()
        {
            base.LoadedGame();

            //兼容旧存档
            if (pawnDics != null && pawnDics.Count>0)
            {
                foreach (Map map in Find.Maps)
                {
                    foreach (Pawn pawn in map.mapPawns.AllPawns)
                    {
                        if (pawnDics.TryGetValue(pawn.ThingID, out FixedPawnDef def))
                        {
                            spawnedPawns.Add(pawn, def);
                        }
                    }
                }

                foreach (Pawn pawn in Find.WorldPawns.AllPawnsAliveOrDead)
                {
                    if (pawnDics.TryGetValue(pawn.ThingID, out FixedPawnDef def))
                    {
                        spawnedPawns.Add(pawn, def);
                    }
                }

                //Log.Warning($"old:{pawnDics.Count}, new:{spawnedPawns.Count}");
            }
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();

            //starting pawns
            for(int i= Find.GameInitData.startingAndOptionalPawns.Count-1; i>=0; i--)
            {
                Pawn pawn = Find.GameInitData.startingAndOptionalPawns[i];
                FixedPawnDef fixedPawnDef = this.GetDef(pawn);

                if (fixedPawnDef != null)
                {
                    //uniqePawns.Remove(fixedPawnDef);
                    spawnedPawns.Add(pawn, fixedPawnDef);
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            //Log.Warning($"Scribe mode: {Scribe.mode}");

            Scribe_Collections.Look<Pawn, FixedPawnDef>(ref spawnedPawns, "spawnedPawns", LookMode.Reference, LookMode.Def, ref workingPawnList, ref workingDefList);
            
            if(Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {

            }
            if(Scribe.mode == LoadSaveMode.LoadingVars && spawnedPawns == null)
            {
                spawnedPawns = new Dictionary<Pawn, FixedPawnDef>();
                spawnedPawns.Clear();

                Scribe_Collections.Look(ref pawnDics, "pawnDics", LookMode.Value, LookMode.Def);
            }
            if(Scribe.mode == LoadSaveMode.Saving)
            {
                //无归属的pawn移至世界
                foreach (var pair in spawnedPawns)
                {
                    Pawn pawn = pair.Key;

                    if (pawn.Map == null && !Find.WorldPawns.Contains(pawn) && !pawn.InContainerEnclosed)
                    {
                        Find.WorldPawns.PassToWorld(pawn, RimWorld.Planet.PawnDiscardDecideMode.KeepForever);

                        Faction faction = null;
                        if (pair.Value.faction != null)
                            faction = Find.FactionManager.FirstFactionOfDef(pair.Value.faction);

                        pawn.SetFaction(faction);
                    }
                }
            }

        }

        public FixedPawnDef GetDef(Pawn pawn)
        {
            if(pawn == null)
            {
                return null;
            }

            if(spawnedPawns.TryGetValue(pawn, out FixedPawnDef def))
            {
                return def;
            }

            return null;
        }

        public Pawn GetPawn(FixedPawnDef def)
        {
            return spawnedPawns.FirstOrDefault(x => x.Value == def).Key;
        }

        internal void AddPawn(Pawn pawn, FixedPawnDef def)
        {
            spawnedPawns.Add(pawn,def);
        }

        internal void RemovePawn(Pawn pawn)
        {
            spawnedPawns.Remove(pawn);
        }

        public void LogPawnDics()
        {
            String str = "============Spawned Pawns============\n";

            Pawn pawn = null;
            FixedPawnDef def = null;
            int count = 0;
            foreach(var pair in spawnedPawns)
            {
                pawn = pair.Key;
                def = pair.Value;

                string location = "None";
                if (pawn.Map != null)
                {
                    location = pawn.Map.uniqueID.ToString();
                }
                else if (Find.WorldPawns.Contains(pawn))
                {
                    location = "World Pawn";
                }
                else if (pawn.InContainerEnclosed)
                {
                    location = "In Container Enclosed";
                }

                str += $"[{count++}]Name:{pawn.Name}, Def: {def.defName}-{def.isUnique}, Location:{location}\n";
            }

            Log.Warning(str);
        }

        

    }

}
