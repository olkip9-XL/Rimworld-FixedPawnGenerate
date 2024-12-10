using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection.Emit;
using Verse.Noise;

namespace FixedPawnGenerate
{
    public class ModSetting_FixedPawnGenerate : ModSettings
    {
        //字段
        public bool allowNaturalRelationGenerate = true;
        public float maxGenerateRate_Starting = 0.125f;
        public float maxGenerateRate_Global = 1f;

        //立绘
        public int globalProtraitOffsetY = 0;
        public bool allowMainProtrait = true;
        public bool allowInfoProtrait = true;
        public bool showFullProtrait = false;

        public void Reset()
        {
            allowNaturalRelationGenerate = true;
            maxGenerateRate_Starting = 0.125f;
            maxGenerateRate_Global = 1f;

            globalProtraitOffsetY = 0;
            allowMainProtrait = true;
            allowInfoProtrait = true;   
            showFullProtrait = false;
        }

        //ExposeData
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref allowNaturalRelationGenerate, "allowNaturalRelationGenerate", true);
            Scribe_Values.Look(ref maxGenerateRate_Starting, "maxGenerateRate_Starting", 0.125f);
            Scribe_Values.Look(ref maxGenerateRate_Global, "maxGenerateRate_Global", 1f);

            Scribe_Values.Look(ref globalProtraitOffsetY, "globalProtraitOffsetY", 0);
            Scribe_Values.Look(ref allowMainProtrait, "allowMainProtrait", true);
            Scribe_Values.Look(ref allowInfoProtrait, "allowInfoProtrait", true);
            Scribe_Values.Look(ref showFullProtrait, "showFullProtrait", false);
        }
    }

    public class Mod_FixedPawnGenerate : Mod
    {
        private ModSetting_FixedPawnGenerate settings;

        //buffer
        string globalProtraitOffsetYBuffer;

        public Mod_FixedPawnGenerate(ModContentPack content) : base(content)
        {
            settings = GetSettings<ModSetting_FixedPawnGenerate>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();

            Rect rect = inRect;
            rect.width = inRect.width * 0.8f;
            rect.x = inRect.x + (int)(inRect.width * 0.1);

            listingStandard.Begin(rect);

            //Generate
            Text.Font = GameFont.Medium;
            listingStandard.GapLine(6f);
            listingStandard.Gap(6f);
            listingStandard.Label("FPG_Generate".Translate());
            Text.Font = GameFont.Small;

            listingStandard.Gap(6f);
            listingStandard.CheckboxLabeled("allowRelation".Translate(), ref settings.allowNaturalRelationGenerate, "allowRelationDescription".Translate());
            listingStandard.Gap(6f);
            settings.maxGenerateRate_Starting = listingStandard.SliderLabeled("maxGenerateRate_Starting".Translate(settings.maxGenerateRate_Starting.ToStringPercent("F2")), settings.maxGenerateRate_Starting, 0f, 0.9999f);
            listingStandard.Gap(6f);
            settings.maxGenerateRate_Global = listingStandard.SliderLabeled("maxGenerateRate_Global".Translate(settings.maxGenerateRate_Global.ToStringPercent("F2")), settings.maxGenerateRate_Global, 0f, 1f);

            //Protrait
            Text.Font = GameFont.Medium;
            listingStandard.GapLine(6f);
            listingStandard.Gap(6f);
            listingStandard.Label("FPG_Protrait".Translate());
            Text.Font = GameFont.Small;

            listingStandard.Gap(6f);
            listingStandard.CheckboxLabeled("FPG_allowMainProtrait".Translate(), ref settings.allowMainProtrait, "FPG_allowMainProtrait_description".Translate());
            listingStandard.Gap(6f);
            listingStandard.CheckboxLabeled("FPG_allowInfoProtrait".Translate(), ref settings.allowInfoProtrait, "FPG_allowInfoProtrait_description".Translate());
            listingStandard.Gap(6f);
            listingStandard.CheckboxLabeled("FPG_showFullProtrait".Translate(), ref settings.showFullProtrait, "FPG_showFullProtrait_description".Translate());
            listingStandard.Gap(6f);
            listingStandard.TextFieldNumericLabeled<int>("FPG_globalProtraitOffsetY".Translate(), ref settings.globalProtraitOffsetY, ref globalProtraitOffsetYBuffer, 0.7f, "FPG_globalProtraitOffsetY_description".Translate(), -900, 500);

            listingStandard.GapLine(6f);
            if (listingStandard.ButtonTextCenter("Reset".Translate(), widthPct: 0.1f))
            {
                settings.Reset();
                ResetBuffer();
            }

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Fixed Pawn Generate";
        }

        private void ResetBuffer()
        {
            globalProtraitOffsetYBuffer = settings.globalProtraitOffsetY.ToString();
        }
    }
}
