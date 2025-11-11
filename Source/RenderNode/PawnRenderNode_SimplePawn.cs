using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    internal class PawnRenderNode_SimplePawn : PawnRenderNode_AnimalPart
    {

        public PawnRenderNode_SimplePawn(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
       : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            Graphic graphic = pawn.def.GetModExtension<ModExtension_SimplePawnData>()?.graphicData?.GraphicColoredFor(pawn);

            float age = pawn.ageTracker.AgeBiologicalYearsFloat;

            if (age < 13f)
            {
                float drawSizeFactor = 0.5f + (age / 13f) * 0.5f;

                graphic = graphic.GetCopy(graphic.drawSize * drawSizeFactor, null);
            }

            return graphic;
        }
    }
}
