using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    public static class FPGPawnTextureUtility
    {
        public static void ExportPawnTexture(List<FixedPawnDef> defs, int width = 512, int height = 512)
        {
            foreach (var def in defs)
            {
                Pawn pawn = FixedPawnUtility.GenerateFixedPawnWithDef(def);

                List<Rot4> rot4s = new List<Rot4>() { Rot4.North, Rot4.East, Rot4.South, Rot4.West };

                foreach (var rot in rot4s)
                {
                    RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

                    CreatePawnTex(ref rt, pawn, new Vector2(0f, 0f), rot, 1f, 0f);
                    Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                    tex.Apply();
                    RenderTexture.active = null;

                    //create dir
                    string folderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "PawnTex", def.defName);

                    System.IO.Directory.CreateDirectory(folderPath);

                    byte[] bytes = tex.EncodeToPNG();
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(folderPath, $"{rot}.png"), bytes);
                }
            }

            RenderTexture CreatePawnTex(ref RenderTexture pawnTex, Pawn pawn, Vector2 offset, Rot4 rot, float zoom, float angle)
            {
                Vector3 cameraOffset = Vector3.left * offset.x + Vector3.forward * offset.y;

                Find.PawnCacheRenderer.RenderPawn(pawn, pawnTex, cameraOffset, zoom, angle, rot);
                return pawnTex;
            }
        }


    }
}
