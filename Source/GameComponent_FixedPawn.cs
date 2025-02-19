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

        //加载存档时使用
        private Dictionary<string, FixedPawnDef> pawnDics = new Dictionary<string, FixedPawnDef>();

        //old
        //private Dictionary<Pawn, FixedPawnDef> cachedPawns = new Dictionary<Pawn, FixedPawnDef>();

        private Dictionary<Pawn, FixedPawnDef> spawnedPawns = new Dictionary<Pawn, FixedPawnDef>();

        internal List<FixedPawnDef> spawnedUniquePawns = new List<FixedPawnDef>();

        public GameComponent_FixedPawn(Game game) : base()
        {
            this.game = game;
        }
        public override void LoadedGame()
        {
            base.LoadedGame();

            //兼容旧存档
            if (pawnDics != null && pawnDics.Count > 0 && (!this.spawnedPawns.Any() || !this.spawnedUniquePawns.Any()))
            {
                //Log.Warning("Compatibility with old saves in progress");

                foreach (Map map in Find.Maps)
                {
                    foreach (Pawn pawn in map.mapPawns.AllPawns)
                    {
                        if (pawnDics.TryGetValue(pawn.ThingID, out FixedPawnDef def))
                        {
                            spawnedPawns.AddDistinct(pawn, def);
                            if (def.isUnique)
                                spawnedUniquePawns.AddDistinct(def);
                        }
                    }
                }

                foreach (Pawn pawn in Find.WorldPawns.AllPawnsAliveOrDead)
                {
                    if (pawnDics.TryGetValue(pawn.ThingID, out FixedPawnDef def))
                    {
                        spawnedPawns.AddDistinct(pawn, def);
                        if (def.isUnique && pawn.relations.everSeenByPlayer == true)
                            spawnedUniquePawns.AddDistinct(def);
                    }
                }
            }


        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();

            this.spawnedUniquePawns.AddRange(Find.GameInitData.startingAndOptionalPawns
                    .Select(pawn => pawn.GetFixedPawnDef())
                    .Where(def => def != null && def.isUnique));


            //remove non-unique pawns;
            spawnedPawns.RemoveAll(x => !Find.GameInitData.startingAndOptionalPawns.Contains(x.Key) && !x.Value.isUnique);

            //pass to world
            foreach (var pair in spawnedPawns)
            {
                if (pair.Key.GetPawnPositionState() == PawnPositionState.OTHER)
                {
                    Find.WorldPawns.PassToWorld(pair.Key, RimWorld.Planet.PawnDiscardDecideMode.KeepForever);

                    Faction faction = null;
                    if (pair.Value.faction != null)
                        faction = Find.FactionManager.FirstFactionOfDef(pair.Value.faction);

                    pair.Key.SetFaction(faction);
                }
            }
        }

        /*LoadingVars -> ResolvingCrossRefs -> PostLoadInit*/
        public override void ExposeData()
        {
            base.ExposeData();

            //构建pawnDics
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                pawnDics.Clear();
                foreach (var pair in spawnedPawns)
                {
                    pawnDics.Add(pair.Key.ThingID, pair.Value);
                }
            }

            //pawnDics为加载存档时构建comps时使用
            Scribe_Collections.Look(ref pawnDics, "pawnDics", LookMode.Value, LookMode.Def);
            Scribe_Collections.Look<Pawn, FixedPawnDef>(ref spawnedPawns, "spawnedPawns", LookMode.Reference, LookMode.Def, ref workingPawnList, ref workingDefList, true, true, true);

            Scribe_Collections.Look(ref spawnedUniquePawns, "spawnedUniquePawns", LookMode.Def);

            //null list check
            if (Scribe.mode == LoadSaveMode.LoadingVars && spawnedPawns == null)
            {
                spawnedPawns = new Dictionary<Pawn, FixedPawnDef>();
                spawnedPawns.Clear();
            }
            if (Scribe.mode == LoadSaveMode.LoadingVars && pawnDics == null)
            {
                pawnDics = new Dictionary<string, FixedPawnDef>();
                pawnDics.Clear();
            }
            if (Scribe.mode == LoadSaveMode.LoadingVars && spawnedUniquePawns == null)
            {
                spawnedUniquePawns = new List<FixedPawnDef>();
                spawnedUniquePawns.Clear();
            }
        }

        internal FixedPawnDef GetDef(Pawn pawn)
        {
            if (pawn == null)
            {
                return null;
            }

            FixedPawnDef def = null;

            if (pawnDics.TryGetValue(pawn.ThingID, out def))
            {
                return def;
            }

            if (spawnedPawns.TryGetValue(pawn, out def))
            {
                return def;
            }

            return null;
        }
        internal Pawn GetPawn(FixedPawnDef def)
        {
            return spawnedPawns.FirstOrDefault(x => x.Value == def).Key;
        }

        internal void AddPawn(Pawn pawn, FixedPawnDef def)
        {
            if (!spawnedPawns.ContainsKey(pawn))
            {
                spawnedPawns.Add(pawn, def);
            }
        }

        internal void RemovePawn(Pawn pawn)
        {
            spawnedPawns.Remove(pawn);
        }

        public void LogPawnDics()
        {
            String str = "============Spawned Pawns============\n" +
                $"   {"Name",-30} {"Def",-10} {"Location",-25} {"ThingID",-15}\n";

            Pawn pawn = null;
            FixedPawnDef def = null;
            int count = 0;



            foreach (var pair in spawnedPawns)
            {
                pawn = pair.Key;
                def = pair.Value;

                string location = "Error";

                switch (pawn.GetPawnPositionState())
                {
                    case PawnPositionState.IN_MAP:
                        location = $"Map[{pawn.Map.uniqueID.ToString()}]";
                        break;
                    case PawnPositionState.WORLD_PAWN:
                        location = "World Pawn";
                        break;
                    case PawnPositionState.IN_CONTAINER:
                        location = "In Container Enclosed";
                        break;
                    case PawnPositionState.IN_CORPSE:
                        location = "In Corpse or Unnatural Corpse";
                        break;
                    case PawnPositionState.IN_OTHER_HOLDER:
                        location = "In Unknown Holder";
                        break;
                    case PawnPositionState.IN_CARAVAN:
                        location = "In Caravan";
                        break;
                    case PawnPositionState.OTHER:
                        location = "None";
                        break;
                    case PawnPositionState.ERROR:
                        break;
                    default:
                        break;
                }

                str += $"[{count++}]{pawn.Name,-30}\t{def.defName + (def.isUnique ? "★" : ""),-10}\t{location,-25}\t{pawn.ThingID,-15}\n";
            }

            str += "\n\n============Unique Pawns============\n";
            count = 0;
            foreach (var item in spawnedUniquePawns)
            {
                str += $"[{count++}]{item.defName}:{item.name}, tags:";
                foreach (var tag in item.tags)
                {
                    str += $"{tag},";
                }
                str += "\n";
            }

            Log.Warning(str);
        }

        internal bool IsSpawned(FixedPawnDef def)
        {
            return this.spawnedUniquePawns.Contains(def);
        }

    }

}
