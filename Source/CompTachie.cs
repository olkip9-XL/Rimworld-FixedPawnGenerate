using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    public class CompTachie : ThingComp
    {
        public CompProperties_Tachie Props => (CompProperties_Tachie)this.props;

        public void DrawProtrait(float x, float y, float height, float minWidth = 0f, float maxWidth = 1E+09f, ProtraitAnchor anchor = ProtraitAnchor.TopLeft, float transparency = 1.0f )
        {
            Rect rect = new Rect(x,y,minWidth,height);
            float textureRatio= (float)this.texture.width/(float)this.texture.height;
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
            if(textureRatio > rect.width/rect.height)// texture too wide
                scaleMode = ScaleMode.ScaleAndCrop;

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
                    textureCache = ContentFinder<Texture2D>.Get(Props.texture);

                return textureCache;
            }
        }
        private Texture2D textureCache = null;

    }
}
