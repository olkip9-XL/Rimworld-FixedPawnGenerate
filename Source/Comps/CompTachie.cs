using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Windows;
using Verse;
using Verse.Noise;
using static AlienRace.CachedData;
using static RimWorld.MechClusterSketch;
using static System.Net.Mime.MediaTypeNames;

namespace FixedPawnGenerate
{

    public enum PawnPortraitStat
    {
        Happy,
        Normal,
        Stress,
        AboutToBreak,
        Break,

        Roaming,

        Dying,

        Sleeping,

        Drafted
    }
    public class CompTachie : ThingComp
    {
        public enum PortraitAnchor
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
        }

        private PawnPortraitStat curPawnStatInt = PawnPortraitStat.Normal;
        private PawnPortraitStat curPawnStat
        {
            get
            {
                return curPawnStatInt;
            }
            set
            {
                if (value != curPawnStatInt &&
                    value != PawnPortraitStat.Normal &&
                    value != PawnPortraitStat.Sleeping)
                {
                    //update
                    string texturePath = Props.texture + "/" + value.ToString();
                    Texture2D stampTexture = ContentFinder<Texture2D>.Get(texturePath, false);

                    Vector2Int stampOffset = Props.GetDiffOffset(value);

                    //Replacement of Break
                    if (value == PawnPortraitStat.Break && stampTexture == null)
                    {
                        stampTexture = ContentFinder<Texture2D>.Get(Props.texture + "/Drafted", false);
                        stampOffset = Props.GetDiffOffset(PawnPortraitStat.Drafted);
                    }

                    //Replacement of AboutToBreak
                    if (value == PawnPortraitStat.AboutToBreak && stampTexture == null)
                    {
                        stampTexture = ContentFinder<Texture2D>.Get(Props.texture + "/Stress", false);
                        stampOffset = Props.GetDiffOffset(PawnPortraitStat.Stress);
                    }

                    if (stampTexture != null)
                    {
                        textureCacheWithStat = MergeTextures(stampTexture, this.textureBase, stampOffset) ?? this.textureBase;
                    }
                    else
                    {
                        textureCacheWithStat = this.textureBase;
                    }
                }
                curPawnStatInt = value;
            }
        }

        public CompProperties_Tachie Props => (CompProperties_Tachie)this.props;

        private Pawn pawn => base.parent as Pawn;

        //Texture caches
        private Texture2D textureCacheWithStat = null;
        private Texture2D textureBase
        {
            get
            {
                if (textureBaseCache == null)
                {
                    string path = Props.texture;

                    if (path != "" && (Props.texture.Contains(":") || Props.texture[0] == '\\'))
                    {
                        textureBaseCache = this.LoadTexture(Props.texture);
                    }
                    else
                    {
                        textureBaseCache = ContentFinder<Texture2D>.Get(Props.texture + "/Base", false);
                        if (textureBaseCache != null)
                        {
                            return textureBaseCache;
                        }

                        textureBaseCache = ContentFinder<Texture2D>.Get(Props.texture, false);
                        if (textureBaseCache == null)
                        {
                            Log.Error($"Texture {Props.texture} not found.");
                            textureBaseCache = ContentFinder<Texture2D>.Get("Empty");
                        }
                    }
                }

                return textureBaseCache;
            }
        }
        private Texture2D textureBaseCache = null;

        private Texture2D textureSleeping
        {
            get
            {
                if (textureSleepingCache == null)
                {
                    Texture2D tex = ContentFinder<Texture2D>.Get(Props.texture + "/Sleeping", false);
                    if (tex != null)
                    {
                        textureSleepingCache = MergeTextures(tex, this.textureBase, Props.GetDiffOffset(PawnPortraitStat.Sleeping));
                    }
                    else
                    {
                        return this.textureBase;
                    }
                }
                return textureSleepingCache;
            }
        }
        private Texture2D textureSleepingCache = null;

        private Texture2D textureDying
        {
            get
            {
                Texture2D texture = ContentFinder<Texture2D>.Get(Props.texture + "/Dying", false);

                if (texture != null)
                {
                    return texture;
                }
                else
                {
                    return this.textureBase;
                }
            }
        }

        public void DrawPortrait(float x, float y, float height, float minWidth = 0f, float maxWidth = 1E+09f, PortraitAnchor anchor = PortraitAnchor.TopLeft, float transparency = 1.0f, float scale = 1, bool applyProps = true)
        {
            Rect rect = new Rect(x, y, minWidth, height);
            float textureAspect = (float)this.textureBase.width / (float)this.textureBase.height;
            float textureWidth = textureAspect * height;

            rect.width = Mathf.Clamp(textureWidth, minWidth, maxWidth);

            switch (anchor)
            {
                case PortraitAnchor.TopLeft:
                    break;
                case PortraitAnchor.TopRight:
                    rect.x -= rect.width;
                    break;
                case PortraitAnchor.BottomLeft:
                    rect.y -= rect.height;
                    break;
                case PortraitAnchor.BottomRight:
                    rect.x -= rect.width;
                    rect.y -= rect.height;
                    break;
                default:
                    break;
            }

            //ScaleMode scaleMode = ScaleMode.ScaleToFit;
            //if (textureRatio > rect.width / rect.height)// texture too wide
            //    scaleMode = ScaleMode.ScaleAndCrop;

            //Apply props
            if (applyProps)
            {
                rect.y += this.Props.offsetY;
                rect.x += this.Props.offsetX;
                rect.width *= this.Props.scale;
                rect.height *= this.Props.scale;

                rect.width *= scale;
                rect.height *= scale;
            }

            //Draw on CPU
            //Color originalColor = GUI.color;
            //GUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, transparency);
            //GUI.DrawTexture(rect, this.texture, scaleMode);
            //GUI.color = originalColor;

            //Draw on GPU

            textureWidth = rect.height * textureAspect;

            Rect targetRect = new Rect(rect);
            targetRect.x = rect.x + Mathf.Max((rect.width - textureWidth) * 0.5f, 0f);
            targetRect.width = Mathf.Min(textureWidth, rect.width);

            Rect uvRect = new Rect(0f, 0f, 1f, 1f);
            uvRect.x = Mathf.Max((textureWidth - rect.width) * 0.5f / textureWidth, 0f);
            uvRect.width = Mathf.Min(1f, rect.width / textureWidth);

            Material mat = new Material(ShaderDatabase.WorldOverlayTransparent);
            mat.color = new Color(1, 1, 1, transparency);

            Graphics.DrawTexture(targetRect, GetCurrentTexture(), uvRect, 0, 0, 0, 0, mat: mat);
        }

        //从绝对路径读取Texture2D
        Texture2D LoadTexture(string path)
        {
            if (File.Exists(path))
            {
                Texture2D texture = new Texture2D(1, 1);

                byte[] fileData = File.ReadAllBytes(path);

                texture.LoadImage(fileData);

                return texture;
            }

            Log.Error($"File {path} doesn't exsit!!");
            return ContentFinder<Texture2D>.Get("Empty");
        }

        Texture2D MergeTextures_GPU(Texture2D topTex, Texture2D bottomTex, Vector2Int offset)
        {
            int width = bottomTex.width;
            int height = bottomTex.height;

            // 1. 创建 RenderTexture
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

            // 2. 先把 bottomTex 全屏 Blit 到 rt
            Graphics.Blit(bottomTex, rt);

            // 3. 设置目标为 rt，然后用 DrawTexture 绘制 topTex 到指定位置
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, width, height, 0);  // 左上为(0,0)

            Graphics.DrawTexture(
                new Rect(offset.x, height - offset.y - topTex.height, topTex.width, topTex.height), // Unity纹理左下为原点
                topTex
            );
            GL.PopMatrix();
            RenderTexture.active = previous;

            // 4. 从 rt 读取回 Texture2D
            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, true);
            result.filterMode = FilterMode.Bilinear;

            RenderTexture.active = rt;
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }

        Texture2D MergeTextures_GPU2(Texture2D topTex, Texture2D bottomTex, Vector2Int offset)
        {
            int width = bottomTex.width;
            int height = bottomTex.height;
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

            Graphics.Blit(bottomTex, rt);

            RenderTexture previous = RenderTexture.active;
            try
            {
                RenderTexture.active = rt;
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, width, height, 0);
                Graphics.DrawTexture(new Rect(offset.x, height - offset.y - topTex.height, topTex.width, topTex.height), topTex);
                GL.PopMatrix();

                Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
                result.filterMode = FilterMode.Bilinear;
                result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                result.Apply();
                return result;
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        Texture2D MergeTextures_CPU(Texture2D topTex, Texture2D bottomTex, Vector2Int offset)
        {
            Texture2D result = new Texture2D(bottomTex.width, bottomTex.height, TextureFormat.RGBA32, false);
            result.SetPixels(bottomTex.GetPixels());

            // 取得表情图像素
            Color[] overlayPixels = topTex.GetPixels();
            int width = topTex.width;
            int height = topTex.height;

            // 将表情图贴上去
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int destX = offset.x + x;
                    int destY = offset.y + y;

                    // 确保不越界
                    if (destX >= 0 && destX < result.width && destY >= 0 && destY < result.height)
                    {
                        Color srcColor = overlayPixels[y * width + x];
                        Color baseColor = result.GetPixel(destX, destY);

                        // 简单 alpha 混合
                        Color blended = Color.Lerp(baseColor, srcColor, srcColor.a);
                        result.SetPixel(destX, destY, blended);
                    }
                }
            }

            result.Apply(); // 应用更改
            return result;
        }

        Texture2D MergeTextures_CommandBuffer(Texture2D topTex, Texture2D bottomTex, Vector2Int offset)
        {
            int width = bottomTex.width;
            int height = bottomTex.height;

            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

            //底图
            CommandBuffer cb = new CommandBuffer();
            cb.name = "Merge texture";
            cb.Blit(bottomTex, rt);
            cb.SetRenderTarget(rt);

            //顶图
            cb.SetViewport(new Rect(offset.x, offset.y, topTex.width, topTex.height));

            Material mat = new Material(ShaderDatabase.WorldOverlayTransparent);
            mat.mainTexture = topTex;
            cb.Blit(topTex, BuiltinRenderTextureType.CurrentActive, mat);

            Graphics.ExecuteCommandBuffer(cb);

            //读取
            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, true);
            result.filterMode = FilterMode.Bilinear;
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            RenderTexture.active = previous;

            // 释放资源
            cb.Release();
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }

        Texture2D MergeTextures(Texture2D topTex, Texture2D bottomTex, Vector2Int offset)
        {
            if (offset.x == 0 && offset.y == 0)
            {
                return topTex;
            }

            //Stopwatch sw = Stopwatch.StartNew();
            Texture2D result = null;

            //CPU
            //result = MergeTextures_CPU(topTex, bottomTex, offset);

            //GPU
            //result = MergeTextures_GPU2(topTex, bottomTex, offset);

            //CommandBuffer(GPU)
            result = MergeTextures_CommandBuffer(topTex, bottomTex, offset);

            //sw.Stop();
            //Log.Warning($"MergeTextures took {sw.ElapsedMilliseconds} ms");

            return result;
        }

        private PawnPortraitStat GetCurrentPawnStat()
        {
            if (pawn == null || pawn.Dead || pawn.Destroyed)
            {
                return PawnPortraitStat.Normal;
            }

            float currentMood = pawn.needs.mood.CurLevelPercentage;

            if (pawn.CurJobDef == JobDefOf.Lovin)
            {
                return PawnPortraitStat.Roaming;
            }
            else if (pawn.Drafted)
            {
                return PawnPortraitStat.Drafted;
            }
            else if (pawn.MentalStateDef?.category == MentalStateCategory.Aggro)
            {
                return PawnPortraitStat.Break;
            }
            else if (pawn.health.summaryHealth.SummaryHealthPercent < 0.5f)
            //else if (pawn.Downed)
            {
                Log.Warning("return dying");
                return PawnPortraitStat.Dying;
            }
            else if (pawn.CurJobDef == JobDefOf.LayDown || pawn.CurJobDef == JobDefOf.LayDownResting)
            {
                return PawnPortraitStat.Sleeping;
            }

            //mood
            else if (currentMood < pawn.mindState.mentalBreaker.BreakThresholdExtreme)
            {
                return PawnPortraitStat.AboutToBreak;
            }
            else if (currentMood < pawn.mindState.mentalBreaker.BreakThresholdMajor)
            {
                return PawnPortraitStat.Stress;
            }
            else if (currentMood < 0.9f)
            {
                return PawnPortraitStat.Normal;
            }
            else
            {
                return PawnPortraitStat.Happy;
            }

        }

        private Texture2D GetCurrentTexture()
        {
            try
            {
                curPawnStat = GetCurrentPawnStat();
            }
            catch (Exception e)
            {
                Log.Error("Error in GetCurrentPawnStat, returning Normal stat. e:" + e.Message);
                return this.textureBase;
            }

            //Log.Warning($"curStat : {stat.ToString()}");
            Texture2D result = this.textureBase;

            switch (curPawnStat)
            {
                case PawnPortraitStat.Normal:
                    if (this.isBlinking)
                    {
                        result = textureSleeping;
                    }
                    break;
                case PawnPortraitStat.Sleeping:
                    result = textureSleeping;
                    break;
                default:
                    result = textureCacheWithStat;
                    break;
            }

            return result;
        }

        int nextBlinkTick = 0;
        int ticksBetweenBlinks = 6; //0.1秒
        bool isBlinking = false;

        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);

            int currentTick = Find.TickManager.TicksGame;

            if (currentTick >= nextBlinkTick)
            {
                if (!isBlinking)
                {
                    isBlinking = true;
                    nextBlinkTick = currentTick + ticksBetweenBlinks;
                }
                else
                {
                    isBlinking = false;
                    int nextTicks = Rand.Range(180, 240); //随机3-4秒后眨一次
                    nextBlinkTick = currentTick + nextTicks;
                }
            }
        }


    }
}
