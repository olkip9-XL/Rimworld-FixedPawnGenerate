using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static UnityEngine.Scripting.GarbageCollector;

namespace FixedPawnGenerate
{
    public class PawnRenderNode_ApparelExt : PawnRenderNode_Apparel
    {
        public PawnRenderNode_ApparelExt(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        public PawnRenderNode_ApparelExt(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel)
            : base(pawn, props, tree, apparel)
        {
        }

        public PawnRenderNode_ApparelExt(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel, bool useHeadMesh)
            : base(pawn, props, tree, apparel, useHeadMesh)
        {
        }

        protected override IEnumerable<Graphic> GraphicsFor(Pawn pawn)
        {
            ApparelGraphicRecord apparelGraphicRecord;

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (TryGetGraphicApparelExt(apparel, pawn.story.bodyType, out apparelGraphicRecord))
                {
                    this.apparel = apparel;
                    yield return apparelGraphicRecord.graphic;
                    break;
                }
            }

            yield break;
        }

        private bool TryGetGraphicApparelExt(Apparel apparel, BodyTypeDef bodyType, out ApparelGraphicRecord rec)
        {
            if (bodyType == null)
            {
                Log.Error("Getting apparel graphic with undefined body type.");
                bodyType = BodyTypeDefOf.Male;
            }

            if (apparel.WornGraphicPath.NullOrEmpty())
            {
                rec = new ApparelGraphicRecord(null, null);
                return false;
            }

            ModExtension_ApparelExt modExt = apparel.def.GetModExtension<ModExtension_ApparelExt>();
            if (modExt == null)
            {
                rec = new ApparelGraphicRecord(null, null);
                return false;
            }

            Shader shader = ShaderDatabase.Cutout;
            if (apparel.StyleDef?.graphicData.shaderType != null)
            {
                shader = apparel.StyleDef.graphicData.shaderType.Shader;
            }
            else if ((apparel.StyleDef == null && apparel.def.apparel.useWornGraphicMask) || (apparel.StyleDef != null && apparel.StyleDef.UseWornGraphicMask))
            {
                shader = ShaderDatabase.CutoutComplex;
            }

            Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(modExt.extraWornGraphicPath, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
            rec = new ApparelGraphicRecord(graphic, apparel);

            return true;
        }


    }
}