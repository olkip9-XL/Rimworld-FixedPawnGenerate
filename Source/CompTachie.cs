using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    public class CompTachie : ThingComp
    {
        public CompProperties_Tachie Props => (CompProperties_Tachie)this.props;

        private ThingWithComps FirstDrawThing => Find.Selector.SelectedPawns.Find(x => x.HasComp<CompTachie>());

        public override void DrawGUIOverlay()
        {
            if (FirstDrawThing == this.parent)
                DrawTachie();

            base.DrawGUIOverlay();
        }

        private void DrawTachie()
        {
            //mainTabWindow_Inspect
            //(x:0.00, y:664.00, width:432.00, height:165.00)
            MainTabWindow_Inspect mainTabWindow_Inspect = Find.WindowStack.WindowOfType<MainTabWindow_Inspect>();
            if (mainTabWindow_Inspect != null)
            {
                float minWidth = 360f;
                Rect rect = new Rect();

                rect.height = 500f;
                rect.width = Mathf.Max(rect.height * ((float)texture.width / (float)texture.height), minWidth);
                rect.x = 0f;
                rect.y = mainTabWindow_Inspect.windowRect.y - 300f;


                if (rect.width > mainTabWindow_Inspect.windowRect.width)
                {
                    rect.width = mainTabWindow_Inspect.windowRect.width;
                    GUI.DrawTexture(rect, this.texture, ScaleMode.ScaleAndCrop);
                }
                else
                {
                    GUI.DrawTexture(rect, this.texture, ScaleMode.ScaleToFit);
                }
            }
        }

        public Texture2D texture
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
