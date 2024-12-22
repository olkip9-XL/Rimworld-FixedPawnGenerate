using RimWorld;
using System;
using System.IO;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    public class CompTachie : ThingComp
    {
        public CompProperties_Tachie Props => (CompProperties_Tachie)this.props;

        public void DrawProtrait(float x, float y, float height, float minWidth = 0f, float maxWidth = 1E+09f, ProtraitAnchor anchor = ProtraitAnchor.TopLeft, float transparency = 1.0f)
        {
            Rect rect = new Rect(x, y, minWidth, height);
            float textureRatio = (float)this.texture.width / (float)this.texture.height;
            float textureWidth = textureRatio * height;

            rect.width = Mathf.Clamp(textureWidth, minWidth, maxWidth);

            switch (anchor)
            {
                case ProtraitAnchor.TopLeft:
                    break;
                case ProtraitAnchor.TopRight:
                    rect.x -= rect.width;
                    break;
                case ProtraitAnchor.BottomLeft:
                    rect.y -= rect.height;
                    break;
                case ProtraitAnchor.BottomRight:
                    rect.x -= rect.width;
                    rect.y -= rect.height;
                    break;
                default:
                    break;
            }

            ScaleMode scaleMode = ScaleMode.ScaleToFit;
            if (textureRatio > rect.width / rect.height)// texture too wide
                scaleMode = ScaleMode.ScaleAndCrop;

            //Apply props
            rect.y += this.Props.offsetY;
            rect.x += this.Props.offsetX;
            rect.width *= this.Props.scale;
            rect.height *= this.Props.scale;

            //debug
            //GUI.DrawTexture(rect, new Texture2D(1, 1), ScaleMode.StretchToFill);

            Color originalColor = GUI.color;
            GUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, transparency);
            GUI.DrawTexture(rect, this.texture, scaleMode);
            GUI.color = originalColor;
        }

        public enum ProtraitAnchor
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
        }
        private Texture2D texture
        {
            get
            {
                if (textureCache == null)
                {
                    string path = Props.texture;

                    if (path != "" && (Props.texture.Contains(":") || Props.texture[0] == '\\'))
                    {
                        textureCache = this.LoadTexture(Props.texture);
                    }
                    else
                    {
                        textureCache = ContentFinder<Texture2D>.Get(Props.texture);
                    }
                }

                return textureCache;
            }
        }
        private Texture2D textureCache = null;

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

    } 
}
