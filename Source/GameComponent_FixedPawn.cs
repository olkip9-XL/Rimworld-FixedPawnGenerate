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

        private bool isNewGame = true;

        public List<FixedPawnDef> uniqePawns = new List<FixedPawnDef>();
        public GameComponent_FixedPawn(Game game) : base()
        {
            this.game = game;

            if(isNewGame)
            {
                uniqePawns.AddRange(DefDatabase<FixedPawnDef>.AllDefsListForReading.FindAll(x => x.isUnique));
                isNewGame = false;
            }

#if DEBUG          
            Log.Warning("GameComponent_FixedPawn Constructor called");
#endif
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();

            //uniqePawns.AddRange(DefDatabase<FixedPawnDef>.AllDefsListForReading.FindAll(x => x.isUnique));
#if DEBUG
            Log.Warning($"StartedNewGame,unniqePawns count{uniqePawns.Count}");
#endif
        }

        public override void ExposeData()
        {
            base.ExposeData();

#if DEBUG
               Log.Warning($"GameComponent_FixedPawn ExposeData: uniqePawns:{uniqePawns.Count}");
#endif

            if(uniqePawns.Count>0)
                Scribe_Collections.Look(ref uniqePawns, "uniqePawns", LookMode.Def);
        }
    }

}
