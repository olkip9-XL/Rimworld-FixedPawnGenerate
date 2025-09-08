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
        public bool allowMainPortrait = true;
        public bool allowInfoPortrait = true;
        public bool showFullPortrait = false;

        public int globalPortraitOffsetY = 0;
        public int globalPortraitOffsetX = 0;
        public float globalPortraitScale = 1;

        //角色语音
        public bool enablePawnVoice = true;
        public float voiceVolumeFactor = 1f;

        public void Reset()
        {
            allowNaturalRelationGenerate = true;
            maxGenerateRate_Starting = 0.125f;
            maxGenerateRate_Global = 1f;

            allowMainPortrait = true;
            allowInfoPortrait = true;   
            showFullPortrait = false;

            globalPortraitOffsetY = 0;
            globalPortraitOffsetX = 0;
            globalPortraitScale = 1f;

            enablePawnVoice = true;
            voiceVolumeFactor = 1f;
        }

        //ExposeData
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref allowNaturalRelationGenerate, "allowNaturalRelationGenerate", true);
            Scribe_Values.Look(ref maxGenerateRate_Starting, "maxGenerateRate_Starting", 0.125f);
            Scribe_Values.Look(ref maxGenerateRate_Global, "maxGenerateRate_Global", 1f);

            Scribe_Values.Look(ref allowMainPortrait, "allowMainPortrait", true);
            Scribe_Values.Look(ref allowInfoPortrait, "allowInfoPortrait", true);
            Scribe_Values.Look(ref showFullPortrait, "showFullPortrait", false);

            Scribe_Values.Look(ref globalPortraitOffsetY, "globalPortraitOffsetY", 0);
            Scribe_Values.Look(ref globalPortraitOffsetX, "globalPortraitOffsetX", 0);
            Scribe_Values.Look(ref globalPortraitScale, "globalPortraitScale", 1f);

            Scribe_Values.Look(ref enablePawnVoice, "enablePawnVoice", true);
            Scribe_Values.Look(ref voiceVolumeFactor, "voiceVolumeFactor", 1f);
        }
    }

    public class Mod_FixedPawnGenerate : Mod
    {
        private ModSetting_FixedPawnGenerate settings;

        //buffer
        string globalPortraitOffsetYBuffer;
        string globalPortraitOffsetXBuffer;
        string globalPortraitScaleBuffer;

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

            //Portrait
            Text.Font = GameFont.Medium;
            listingStandard.GapLine(6f);
            listingStandard.Gap(6f);
            listingStandard.Label("FPG_Portrait".Translate());
            Text.Font = GameFont.Small;

            listingStandard.Gap(6f);
            listingStandard.CheckboxLabeled("FPG_allowMainPortrait".Translate(), ref settings.allowMainPortrait, "FPG_allowMainPortrait_description".Translate());
            listingStandard.Gap(6f);
            listingStandard.CheckboxLabeled("FPG_allowInfoPortrait".Translate(), ref settings.allowInfoPortrait, "FPG_allowInfoPortrait_description".Translate());
            listingStandard.Gap(6f);
            listingStandard.CheckboxLabeled("FPG_showFullPortrait".Translate(), ref settings.showFullPortrait, "FPG_showFullPortrait_description".Translate());
            listingStandard.Gap(6f);
            listingStandard.TextFieldNumericLabeled<int>("FPG_globalPortraitOffsetY".Translate(), ref settings.globalPortraitOffsetY, ref globalPortraitOffsetYBuffer, 0.7f, "FPG_globalPortraitOffsetY_description".Translate(), -900, 500);
            listingStandard.Gap(6f);
            listingStandard.TextFieldNumericLabeled<int>("FPG_globalPortraitOffsetX".Translate(), ref settings.globalPortraitOffsetX, ref globalPortraitOffsetXBuffer, 0.7f, "FPG_globalPortraitOffsetX_description".Translate());
            listingStandard.Gap(6f);
            listingStandard.TextFieldNumericLabeled<float>("FPG_globalPortraitScale".Translate(), ref settings.globalPortraitScale, ref globalPortraitScaleBuffer, 0.7f, "FPG_globalPortraitScale_description".Translate());

            //Voice
            listingStandard.Gap(6f);
            listingStandard.CheckboxLabeled("FPG_enablePawnVoice".Translate(), ref settings.enablePawnVoice, "FPG_enablePawnVoice_description".Translate());
            listingStandard.Gap(6f);
            settings.voiceVolumeFactor = listingStandard.SliderLabeled("FPG_voiceVolumeFactor".Translate(settings.voiceVolumeFactor.ToStringPercent("F2")), settings.voiceVolumeFactor, 0f, 1f, tooltip: "FPG_voiceVolumeFactor_description".Translate());

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
            globalPortraitOffsetYBuffer = settings.globalPortraitOffsetY.ToString();
            globalPortraitOffsetXBuffer = settings.globalPortraitOffsetX.ToString();
            globalPortraitScaleBuffer = settings.globalPortraitScale.ToString();
        }
    }
}
