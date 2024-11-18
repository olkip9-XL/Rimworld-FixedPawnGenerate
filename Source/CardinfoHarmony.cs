using HarmonyLib;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using static Verse.Dialog_InfoCard;

namespace FixedPawnGenerate
{
    public static class CardinfoHarmony
    {

        [HarmonyPatch(typeof(Dialog_InfoCard), "FillCard")]
        public static class Patch_Dialog_InfoCard
        {
            static List<Dialog_InfoCard.InfoCardTab> allowedTabs = new List<Dialog_InfoCard.InfoCardTab> {InfoCardTab.Stats, InfoCardTab.Character, InfoCardTab.Health, InfoCardTab.Records };

            private static void Prefix(Dialog_InfoCard __instance, Rect cardRect, Thing ___thing, Dialog_InfoCard.InfoCardTab ___tab)
            {
                //info card
                //(x: 18.00, y: 115.00, width: 914.00, height: 589.00)

                if (___thing != null && allowedTabs.Contains(___tab))
                {
                    CompTachie compTachie = ___thing.TryGetComp<CompTachie>();
                    if (compTachie != null)
                    {
                        Texture2D texture = compTachie.texture;
                        Rect rect = new Rect();
                        float minWidth = 447;

                        rect.height = 576f;
                        rect.width = Mathf.Max(rect.height * ((float)texture.width / (float)texture.height), minWidth) ;
                        rect.x = cardRect.x + cardRect.width - rect.width;
                        rect.y = cardRect.y+ cardRect.height - rect.height;

                        //Backgound Debug only
                        //GUI.DrawTexture(rect, new Texture2D(1, 1), ScaleMode.StretchToFill);

                        //image
                        Color originalColor = GUI.color;
                        GUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
                        GUI.DrawTexture(rect, compTachie.texture, ScaleMode.ScaleToFit);
                        GUI.color = originalColor;
                    }
                }
            }
        }

    }
}
