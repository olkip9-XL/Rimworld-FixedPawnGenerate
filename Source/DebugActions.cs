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

        //[DebugAction("FixedPawnGenerate", "FPG: Export Texture", false, false, false, false, allowedGameStates = AllowedGameStates.Playing)]
        //private static void ExportTexture()
        //{
        //    List<string> texturePaths = new List<string>()
        //    {

        //        "Things/Mote/CombatCommandMask",
        //        "Things/Mote/WorkCommandLinkLine"
        //    };

        //    foreach (string path in texturePaths)
        //    {
        //        var sourceTex = ContentFinder<Texture2D>.Get(path, false);
        //        if (sourceTex != null)
        //        {
        //            RenderTexture rt = RenderTexture.GetTemporary(sourceTex.width, sourceTex.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        //            Graphics.Blit(sourceTex, rt);

        //            RenderTexture prev = RenderTexture.active;
        //            RenderTexture.active = rt;

        //            Texture2D readableTex = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.RGBA32, false);
        //            readableTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        //            readableTex.Apply();

        //            RenderTexture.active = prev;
        //            RenderTexture.ReleaseTemporary(rt);

        //            byte[] pngData = readableTex.EncodeToPNG();

        //            string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        //            string filePath = Path.Combine(desktopPath, path+".png");
        //            Directory.CreateDirectory(Path.GetDirectoryName(filePath)); 
        //            File.WriteAllBytes(filePath, pngData);
        //        }
        //        else
        //        {
        //            Log.Error($"Texture not found: {path}");
        //        }
        //    }

        //}
    }
}
