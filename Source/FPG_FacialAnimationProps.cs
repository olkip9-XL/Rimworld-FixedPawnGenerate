using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FacialAnimation;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    public class FPG_FacialAnimationProps
    {
        public string head;
        public string brow;
        public string lid;
        public string eye;
        public Color leftEyeColor;
        public Color rightEyeColor;
        public string mouth;
        public string skin;
        public void SetPawn(Pawn pawn)
        {
            if(pawn == null)
            {
                return;
            }

            if (head != null)
            {
                FacialAnimation.HeadTypeDef headTypeDef = DefDatabase<FacialAnimation.HeadTypeDef>.GetNamed(head);

                pawn.GetComp<HeadControllerComp>().FaceType = headTypeDef;

                //pawn.GetComp<HeadControllerComp>().FaceType = head;
            }

            if (brow != null)
            {
                BrowTypeDef browTypeDef = DefDatabase<BrowTypeDef>.GetNamed(brow);

                pawn.GetComp<BrowControllerComp>().FaceType = browTypeDef;
            }

            if (lid != null)
            {
                LidTypeDef lidTypeDef = DefDatabase<LidTypeDef>.GetNamed(lid);

                pawn.GetComp<LidControllerComp>().FaceType = lidTypeDef;
            }

            if (eye != null)
            {
                EyeballTypeDef eyeballTypeDef = DefDatabase<EyeballTypeDef>.GetNamed(eye);

                pawn.GetComp<EyeballControllerComp>().FaceType = eyeballTypeDef;
            }
            if (leftEyeColor.a != 0f)
            {
                pawn.GetComp<EyeballControllerComp>().FaceSecondColor = leftEyeColor;
            }
            if (rightEyeColor.a != 0f)
            {
                pawn.GetComp<EyeballControllerComp>().FaceColor = rightEyeColor;
            }

            if (mouth != null)
            {
                MouthTypeDef mouthTypeDef = DefDatabase<MouthTypeDef>.GetNamed(mouth);

                pawn.GetComp<MouthControllerComp>().FaceType = mouthTypeDef;
            }

            if (skin != null)
            {
                SkinTypeDef skinTypeDef = DefDatabase<SkinTypeDef>.GetNamed(skin);

                pawn.GetComp<SkinControllerComp>().FaceType = skinTypeDef;
            }
        }

    }
}
