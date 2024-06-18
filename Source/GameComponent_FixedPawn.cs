using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    public class GameComponent_FixedPawn : GameComponent
    {

        public Game game;

        public List<FixedPawnDef> uniqePawns = new List<FixedPawnDef>();
        public GameComponent_FixedPawn(Game game) : base()
        {
            this.game = game;
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();

            uniqePawns.AddRange(DefDatabase<FixedPawnDef>.AllDefsListForReading.FindAll(x => x.isUnique));
#if DEBUG
            Log.Warning($"StartedNewGame,unniqePawns count{uniqePawns.Count}");
#endif
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref uniqePawns, "uniqePawns", LookMode.Deep);
        }
    }

}
