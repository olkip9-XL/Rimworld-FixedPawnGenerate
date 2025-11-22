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
        public string emotion;
        public void SetPawn(Pawn pawn)
        {
            if (pawn == null)
            {
                return;
            }

            if (head != null)
            {
                FacialAnimation.HeadTypeDef headTypeDef = DefDatabase<FacialAnimation.HeadTypeDef>.GetNamed(head);
                if (headTypeDef != null)
                    pawn.GetComp<HeadControllerComp>().FaceType = headTypeDef;
            }

            if (brow != null)
            {
                BrowTypeDef browTypeDef = DefDatabase<BrowTypeDef>.GetNamed(brow);
                if (browTypeDef != null)
                    pawn.GetComp<BrowControllerComp>().FaceType = browTypeDef;
            }

            if (lid != null)
            {
                LidTypeDef lidTypeDef = DefDatabase<LidTypeDef>.GetNamed(lid);
                if (lidTypeDef != null)
                    pawn.GetComp<LidControllerComp>().FaceType = lidTypeDef;
            }

            if (eye != null)
            {
                EyeballTypeDef eyeballTypeDef = DefDatabase<EyeballTypeDef>.GetNamed(eye);
                if (eyeballTypeDef != null)
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
                if (mouthTypeDef != null)
                    pawn.GetComp<MouthControllerComp>().FaceType = mouthTypeDef;
            }

            if (skin != null)
            {
                SkinTypeDef skinTypeDef = DefDatabase<SkinTypeDef>.GetNamed(skin);
                if (skinTypeDef != null)
                    pawn.GetComp<SkinControllerComp>().FaceType = skinTypeDef;
            }

            if (emotion != null)
            {
                EmotionTypeDef emotionTypeDef = DefDatabase<EmotionTypeDef>.GetNamed(emotion);
                if (emotionTypeDef != null)
                    pawn.GetComp<EmotionControllerComp>().FaceType = emotionTypeDef;
            }
        }
    }
}
