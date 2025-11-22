using LudeonTK;
using RimWorld;
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



        [DebugAction("FixedPawnGenerate", "FPG: Export Pawn texture", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]

        private static void ExportPawnTex()
        {

            Pawn pawn = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).OfType<Pawn>().FirstOrDefault();
            if (pawn != null)
            {
                List<Rot4> rot4s = new List<Rot4>() { Rot4.North, Rot4.East, Rot4.South, Rot4.West };

                foreach (var rot in rot4s)
                {
                    RenderTexture rt = RenderTexture.GetTemporary(512, 512, 0, RenderTextureFormat.ARGB32);

                    CreatePawnTex(ref rt, pawn, new Vector2(0f, 0f), rot, 1f, 0f);
                    Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                    tex.Apply();
                    RenderTexture.active = null;

                    //create dir
                    string folderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "PawnTex", pawn.LabelShort);

                    System.IO.Directory.CreateDirectory(folderPath);

                    byte[] bytes = tex.EncodeToPNG();
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(folderPath, $"{rot}.png"), bytes);
                }

                Messages.Message($"Exported {pawn.LabelShort}'s portrait to Desktop", MessageTypeDefOf.NeutralEvent);
            }
        }

        private static RenderTexture CreatePawnTex(ref RenderTexture pawnTex, Pawn pawn, Vector2 offset, Rot4 rot, float zoom, float angle)
        {
            Vector3 cameraOffset = Vector3.left * offset.x + Vector3.forward * offset.y;

            Find.PawnCacheRenderer.RenderPawn(pawn, pawnTex, cameraOffset, zoom, angle, rot);
            return pawnTex;
        }

    }
}
