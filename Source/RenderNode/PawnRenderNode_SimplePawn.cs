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
            Graphic graphic = BaseContent.BadGraphic;

            Comp_SimplePawn compSimplePawn = pawn.TryGetComp<Comp_SimplePawn>();
            if (compSimplePawn == null)
            {
                Log.Error($"Failed to find Graphic for pawn {pawn.LabelShort}, missing Comp_SimplePawn");
                return graphic;
            }

            try
            {
                graphic = compSimplePawn.GetCurrentGraphicData().GraphicColoredFor(pawn);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to get Graphic for pawn {pawn.LabelShort} in Comp_SimplePawn: {e}");
                return BaseContent.BadGraphic;
            }

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
