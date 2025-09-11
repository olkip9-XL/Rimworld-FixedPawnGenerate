using LudeonTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    public static class DebugActions
    {

        [DebugAction("FixedPawnGenerate", "FPG: Log spawned pawns", false, false, false, false, allowedGameStates = AllowedGameStates.Playing)]
        private static void LogSpawnedPawns()
        {
            FixedPawnUtility.Manager.LogPawnDics();
        }
        [DebugAction("FixedPawnGenerate", "FPG: Log world pawns", false, false, false, false, allowedGameStates = AllowedGameStates.Playing)]
        private static void LogWorldPawns()
        {
            Find.WorldPawns.LogWorldPawns();
        }

        [DebugAction("FixedPawnGenerate", "FPG: Spawn fixed pawn", false, false, false, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> SpawnFixedPawn()
        {
            List<DebugActionNode> list = new List<DebugActionNode>();

            foreach (var def in DefDatabase<FixedPawnDef>.AllDefs)
            {
                DebugActionNode node = new DebugActionNode(def.isUnique ? $"{def.defName} ★" : def.defName, DebugActionType.ToolMap, delegate
                {
                    Pawn pawn = FixedPawnUtility.GenerateFixedPawnWithDef(def);
                    if (pawn != null)
                    {
                        GenPlace.TryPlaceThing(pawn, UI.MouseCell(), Find.CurrentMap, ThingPlaceMode.Near);
                    }
                });

                node.category = def.isUnique ? "Unique Pawns" : "Regular Pawns";

                list.Add(node);
            }

            list = list.OrderBy((DebugActionNode n) => n.label).ToList();

            return list;
        }

    }
}
