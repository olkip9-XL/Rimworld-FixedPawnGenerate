using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FixedPawnGenerate
{
    internal class PawnRenderNode_HairExt : PawnRenderNode_Hair
    {
        public PawnRenderNode_HairExt(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            if (pawn.story?.hairDef == null || pawn.story.hairDef.noGraphic || pawn.DevelopmentalStage.Baby() || pawn.DevelopmentalStage.Newborn())
            {
                return null;
            }

            HairDef hairDef = pawn.story.hairDef;

            ModExtension_HairExt modExt = hairDef.GetModExtension<ModExtension_HairExt>();

            if (modExt == null)
                return null;

            if (modExt.extraHairGraphicPath.NullOrEmpty())
                return null;

            if (hairDef.noGraphic)
            {
                return null;
            }

            return GraphicDatabase.Get<Graphic_Multi>(modExt.extraHairGraphicPath, hairDef.overrideShaderTypeDef?.Shader ?? ShaderDatabase.CutoutHair, Vector2.one, this.ColorFor(pawn));
        }
    }
}
