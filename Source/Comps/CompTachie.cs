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

        Pregnant,

        Drafted
    }

    internal class PawnTachieCacher
    {
        private string texturePath;
        private List<PortraitDiffData> statData;


        private Dictionary<PawnPortraitStat, Texture2D> statTextureCache = new Dictionary<PawnPortraitStat, Texture2D>();

        public PawnTachieCacher(AlterTachieData data)
        {
            this.texturePath = data.texturePath;
            this.statData = data.stats;
        }

        public Texture2D Sleeping
        {
            get
            {
                return GetTexture(PawnPortraitStat.Sleeping);
            }
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
        Texture2D MergeTextures(Texture2D topTex, Texture2D bottomTex, Vector2Int offset)
        {
            if (offset.x == 0 && offset.y == 0)
            {
                return topTex;
            }

            Texture2D result = null;

            result = MergeTextures_CommandBuffer(topTex, bottomTex, offset);

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

        Vector2Int GetDiffOffset(PawnPortraitStat stat)
        {
            if (statData.NullOrEmpty())
            {
                return new Vector2Int(0, 0);
            }

            PortraitDiffData diff = statData.FirstOrDefault(x => x.stat == stat);
            if (diff != null)
            {
                return new Vector2Int((int)diff.offsetX, (int)diff.offsetY);
            }
            else
            {
                return new Vector2Int(0, 0);
            }
        }

        Texture2D baseCache = null;
        public Texture2D Base
        {
            get
            {
                if (baseCache == null)
                {
                    string path = texturePath;

                    if (path != "" && (path.Contains(":") || path[0] == '\\'))
                    {
                        baseCache = this.LoadTexture(path);
                    }
                    else
                    {
                        baseCache = ContentFinder<Texture2D>.Get(path + "/Base", false);
                        if (baseCache != null)
                        {
                            return baseCache;
                        }

                        baseCache = ContentFinder<Texture2D>.Get(path, false);
                        if (baseCache == null)
                        {
                            Log.Error($"Texture {path} not found.");
                            baseCache = ContentFinder<Texture2D>.Get("Empty");
                        }
                    }
                }

                return baseCache;
            }
        }

        public Texture2D GetTexture(PawnPortraitStat stat)
        {
            if (statTextureCache == null)
            {
                statTextureCache = new Dictionary<PawnPortraitStat, Texture2D>();
            }

            if (!statTextureCache.ContainsKey(stat))
            {
                Texture2D result = null;

                Texture2D stamp = ContentFinder<Texture2D>.Get(texturePath + "/" + stat.ToString(), false);
                Vector2Int stampOffset = GetDiffOffset(stat);

                if (stamp != null)
                {
                    result = MergeTextures(stamp, this.Base, stampOffset) ?? this.Base;
                }
                else if (stat == PawnPortraitStat.AboutToBreak)
                {
                    //Replacement of AboutToBreak
                    result = GetTexture(PawnPortraitStat.Stress);
                }
                else if (stat == PawnPortraitStat.Break)
                {
                    //Replacement of Break
                    result = GetTexture(PawnPortraitStat.Drafted);
                }
                else
                {
                    result = this.Base;
                }

                statTextureCache[stat] = result;
            }
            return statTextureCache[stat];
        }
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

        private float clickEventOffset = 0f;
        private float clickEventOffsetDelta = 0f;

        public int alterTachieID = -1;
        public Rect currentDrawingRect { get; private set; }

        public PawnPortraitStat curPawnStat { get; private set; } = PawnPortraitStat.Normal;

        public CompProperties_Tachie Props => (CompProperties_Tachie)this.props;

        private Pawn Pawn => base.parent as Pawn;

        //Texture caches
        private PawnTachieCacher cacherInt = null;

        private PawnTachieCacher Cacher {
            get
            {
                if(cacherInt == null)
                {
                    cacherInt = new PawnTachieCacher(Props.alterTachies.FirstOrDefault(x => x.alterTachieID == alterTachieID) ?? Props.DefaultData);
                }
                return cacherInt;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                if (Props.useRandomTachie)
                {
                    this.alterTachieID = Props.alterTachies.RandomElement().alterTachieID;
                }
            }

            cacherInt = new PawnTachieCacher(Props.alterTachies.FirstOrDefault(x => x.alterTachieID == alterTachieID) ?? Props.DefaultData);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref alterTachieID, "alterTachieID", -1);
        }

        public void SetAlterTachie(int _alterTachieID)
        {
            this.alterTachieID = _alterTachieID;

            this.cacherInt = new PawnTachieCacher(Props.alterTachies.FirstOrDefault(x => x.alterTachieID == alterTachieID) ?? Props.DefaultData);
        }

        public void DrawPortrait(float x, float y, float height, float minWidth = 0f, float maxWidth = 1E+09f, PortraitAnchor anchor = PortraitAnchor.TopLeft, float transparency = 1.0f, float scale = 1, bool applyProps = true)
        {
            Rect rect = new Rect(x, y, minWidth, height);

            Texture2D currentTexture = GetCurrentTexture();

            float textureAspect = (float)currentTexture.width / (float)currentTexture.height;
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

                //click event
                rect.y += clickEventOffset;
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

            Graphics.DrawTexture(targetRect, currentTexture, uvRect, 0, 0, 0, 0, mat: mat);
            if (applyProps)
                currentDrawingRect = targetRect;
        }

        public void HandlePortraitClick()
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && currentDrawingRect.Contains(e.mousePosition))
            {
                PortraitClicked();
                e.Use();
            }
        }

        private void PortraitClicked()
        {
            clickEventOffsetDelta = -1f;
            Pawn.PlayVoice(PawnVoiceType.Lobby);
        }

        //Texture2D MergeTextures_GPU(Texture2D topTex, Texture2D bottomTex, Vector2Int offset)
        //{
        //    int width = bottomTex.width;
        //    int height = bottomTex.height;

        //    // 1. 创建 RenderTexture
        //    RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

        //    // 2. 先把 bottomTex 全屏 Blit 到 rt
        //    Graphics.Blit(bottomTex, rt);

        //    // 3. 设置目标为 rt，然后用 DrawTexture 绘制 topTex 到指定位置
        //    RenderTexture previous = RenderTexture.active;
        //    RenderTexture.active = rt;
        //    GL.PushMatrix();
        //    GL.LoadPixelMatrix(0, width, height, 0);  // 左上为(0,0)

        //    Graphics.DrawTexture(
        //        new Rect(offset.x, height - offset.y - topTex.height, topTex.width, topTex.height), // Unity纹理左下为原点
        //        topTex
        //    );
        //    GL.PopMatrix();
        //    RenderTexture.active = previous;

        //    // 4. 从 rt 读取回 Texture2D
        //    Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, true);
        //    result.filterMode = FilterMode.Bilinear;

        //    RenderTexture.active = rt;
        //    result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        //    result.Apply();
        //    RenderTexture.active = previous;
        //    RenderTexture.ReleaseTemporary(rt);

        //    return result;
        //}

        //Texture2D MergeTextures_GPU2(Texture2D topTex, Texture2D bottomTex, Vector2Int offset)
        //{
        //    int width = bottomTex.width;
        //    int height = bottomTex.height;
        //    RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

        //    Graphics.Blit(bottomTex, rt);

        //    RenderTexture previous = RenderTexture.active;
        //    try
        //    {
        //        RenderTexture.active = rt;
        //        GL.PushMatrix();
        //        GL.LoadPixelMatrix(0, width, height, 0);
        //        Graphics.DrawTexture(new Rect(offset.x, height - offset.y - topTex.height, topTex.width, topTex.height), topTex);
        //        GL.PopMatrix();

        //        Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
        //        result.filterMode = FilterMode.Bilinear;
        //        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        //        result.Apply();
        //        return result;
        //    }
        //    finally
        //    {
        //        RenderTexture.active = previous;
        //        RenderTexture.ReleaseTemporary(rt);
        //    }
        //}

        //Texture2D MergeTextures_CPU(Texture2D topTex, Texture2D bottomTex, Vector2Int offset)
        //{
        //    Texture2D result = new Texture2D(bottomTex.width, bottomTex.height, TextureFormat.RGBA32, false);
        //    result.SetPixels(bottomTex.GetPixels());

        //    // 取得表情图像素
        //    Color[] overlayPixels = topTex.GetPixels();
        //    int width = topTex.width;
        //    int height = topTex.height;

        //    // 将表情图贴上去
        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            int destX = offset.x + x;
        //            int destY = offset.y + y;

        //            // 确保不越界
        //            if (destX >= 0 && destX < result.width && destY >= 0 && destY < result.height)
        //            {
        //                Color srcColor = overlayPixels[y * width + x];
        //                Color baseColor = result.GetPixel(destX, destY);

        //                // 简单 alpha 混合
        //                Color blended = Color.Lerp(baseColor, srcColor, srcColor.a);
        //                result.SetPixel(destX, destY, blended);
        //            }
        //        }
        //    }

        //    result.Apply(); // 应用更改
        //    return result;
        //}

        private PawnPortraitStat GetCurrentPawnStat()
        {
            if (Pawn == null || Pawn.Dead || Pawn.Destroyed || Pawn.Map == null)
            {
                return PawnPortraitStat.Normal;
            }

            float currentMood = Pawn.needs.mood.CurLevelPercentage;

            if (Pawn.health.hediffSet.HasHediff(HediffDefOf.PregnantHuman, false))
            {
                return PawnPortraitStat.Pregnant;
            }

            if (Pawn.CurJobDef == JobDefOf.Lovin || Unclothed())
            {
                return PawnPortraitStat.Roaming;
            }
            else if (Pawn.Drafted)
            {
                return PawnPortraitStat.Drafted;
            }
            else if (Pawn.MentalStateDef?.category == MentalStateCategory.Aggro)
            {
                return PawnPortraitStat.Break;
            }
            else if (Pawn.health.summaryHealth.SummaryHealthPercent < 0.5f)
            //else if (pawn.Downed)
            {
                return PawnPortraitStat.Dying;
            }
            else if (Pawn.CurJobDef == JobDefOf.LayDown || Pawn.CurJobDef == JobDefOf.LayDownResting)
            {
                return PawnPortraitStat.Sleeping;
            }

            //mood
            else if (currentMood < Pawn.mindState.mentalBreaker.BreakThresholdExtreme)
            {
                return PawnPortraitStat.AboutToBreak;
            }
            else if (currentMood < Pawn.mindState.mentalBreaker.BreakThresholdMajor)
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

            bool Unclothed()
            {
                if (Pawn.apparel == null)
                {
                    return true;
                }

                foreach (Apparel item in Pawn.apparel.WornApparel)
                {
                    if (item.def.apparel.countsAsClothingForNudity)
                    {
                        return false;
                    }
                }

                return true;
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
                return Cacher.Base;
            }

            if (curPawnStat == PawnPortraitStat.Normal && this.isBlinking)
            {
                return Cacher.Sleeping;
            }
            else
            {
                return Cacher.GetTexture(curPawnStat);
            }
        }

        int ticksBetweenBlinks = 6; //0.1秒
        bool isBlinking = false;

        float blinkCountDown = 0;

        public override void CompTick()
        {
            base.CompTick();

            float multiplier = Find.TickManager.TickRateMultiplier;

            //blinking
            if (blinkCountDown <= 0)
            {
                if (!isBlinking)
                {
                    isBlinking = true;
                    blinkCountDown = ticksBetweenBlinks;
                }
                else
                {
                    isBlinking = false;
                    blinkCountDown = Rand.Range(180, 240); //随机3-4秒后眨一次
                }
            }
            else
            {
                blinkCountDown -= 1f / multiplier;
            }
            //click event

            clickEventOffset += clickEventOffsetDelta / multiplier;

            if (clickEventOffset <= -10f)
            {
                if (clickEventOffsetDelta < 0f)
                    clickEventOffsetDelta = -clickEventOffsetDelta;
            }

            if (clickEventOffset >= 0f)
            {
                clickEventOffset = 0f;
                clickEventOffsetDelta = 0f;
            }
        }

    }
}
