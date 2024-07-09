using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    public class CompTachie : ThingComp
    {
        public CompProperties_Tachie Props => (CompProperties_Tachie)this.props;

        private ThingWithComps FirstDrawThing => Find.Selector.SelectedPawns.Find(x => x.HasComp<CompTachie>()) ;

        public override void DrawGUIOverlay()
        {
            if(FirstDrawThing == this.parent)
                DrawTachie();
            base.DrawGUIOverlay();
        }

        private void DrawTachie()
        {
            MainTabWindow_Inspect mainTabWindow_Inspect = Find.WindowStack.WindowOfType<MainTabWindow_Inspect>();

            float num = 0f;
            float num2 = 430f / (float)(2f);
            float num3 = mainTabWindow_Inspect.PaneTopY - 500f - 30f;

            if (this.texture == null)
                this.texture = ContentFinder<Texture2D>.Get(Props.texture);

            if (this.texture != null)
                Widgets.DrawTextureFitted(new Rect(num2 * (float)(num + 1) - 215f + Props.offsetX, num3 + Props.offsetY, 430f, 500f), this.texture, Props.scale);
        }

        private Texture2D texture = null;
    }
}
