using HarmonyLib;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using static Verse.Dialog_InfoCard;
using RimWorld;

namespace FixedPawnGenerate
{
    public static class FPGPortraitHarmony
    {

        [HarmonyPatch(typeof(Dialog_InfoCard), "FillCard")]
        public static class Patch_Dialog_InfoCard
        {
            static List<Dialog_InfoCard.InfoCardTab> allowedTabs = new List<Dialog_InfoCard.InfoCardTab> {InfoCardTab.Stats, InfoCardTab.Character, InfoCardTab.Health, InfoCardTab.Records };

            private static void Prefix(Dialog_InfoCard __instance, Rect cardRect, Thing ___thing, Dialog_InfoCard.InfoCardTab ___tab)
            {

                if(!FixedPawnUtility.Settings.allowInfoPortrait) 
                    return;

                //info card
                //(x: 18.00, y: 115.00, width: 914.00, height: 589.00)

                if (___thing != null && allowedTabs.Contains(___tab))
                {
                    CompTachie compTachie = ___thing.TryGetComp<CompTachie>();
                    if (compTachie != null)
                    {
                        compTachie.DrawPortrait(cardRect.x + cardRect.width, cardRect.y, 576f, minWidth: 447f, anchor: CompTachie.PortraitAnchor.TopRight, transparency: 0.5f, applyProps:false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MapInterface), "MapInterfaceOnGUI_BeforeMainTabs")]
        public static class Patch_MainTabWindow_Inspect
        {
            private static void Prefix()
            {
                ModSetting_FixedPawnGenerate settings = FixedPawnUtility.Settings;
                if (!settings.allowMainPortrait)
                    return;

                MainTabWindow_Inspect mainTabWindow_Inspect = Find.WindowStack.WindowOfType<MainTabWindow_Inspect>();
                if (mainTabWindow_Inspect == null)
                    return;

                Rect inRect = mainTabWindow_Inspect.windowRect;

                ThingWithComps FirstDrawThing = Find.Selector.SelectedPawns.Find(x => x.HasComp<CompTachie>());

                CompTachie compTachie = FirstDrawThing.TryGetComp<CompTachie>();
                if (compTachie != null)
                {
                    //mainTabWindow_Inspect
                    //(x:0.00, y:664.00, width:432.00, height:165.00)
                    float rectY = inRect.y - 300f;
                    float rectX = 0;

                    //apply settings
                    if (settings.showFullPortrait)
                    {
                        rectY = rectY - 200 - 30;
                    }
                    rectY += settings.globalPortraitOffsetY;
                    rectX += settings.globalPortraitOffsetX;

                    compTachie.DrawPortrait(rectX, rectY, 500f, minWidth: 360f, maxWidth: inRect.width, scale:settings.globalPortraitScale);
                }
            }
        }

    }
}
