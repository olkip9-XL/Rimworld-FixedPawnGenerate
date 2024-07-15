using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using UnityEngine;

namespace FixedPawnGenerate
{
    public class ModSetting_FixedPawnGenerate : ModSettings
    {
        //字段
        public bool allowNaturalRelationGenerate = true;
        public float maxGenerateRate_Starting = 0.125f;
        public float maxGenerateRate_Global = 1f;
        public void Reset()
        {
            allowNaturalRelationGenerate = true;
            maxGenerateRate_Starting = 0.125f;
            maxGenerateRate_Global = 1f;
        }

        //ExposeData
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref allowNaturalRelationGenerate, "allowNaturalRelationGenerate", true);
            Scribe_Values.Look(ref maxGenerateRate_Starting, "maxGenerateRate_Starting", 0.125f);
            Scribe_Values.Look(ref maxGenerateRate_Global, "maxGenerateRate_Global", 1f);
        }
    }

    public class Mod_FixedPawnGenerate : Mod
    {
        private ModSetting_FixedPawnGenerate settings;

        public Mod_FixedPawnGenerate(ModContentPack content) : base(content)
        {
            settings = GetSettings<ModSetting_FixedPawnGenerate>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.GapLine();

            listingStandard.Gap();
            listingStandard.CheckboxLabeled("allowRelation".Translate(), ref settings.allowNaturalRelationGenerate, "allowRelationDescription".Translate());

            listingStandard.Gap();
            listingStandard.Label("maxGenerateRate_Starting".Translate(settings.maxGenerateRate_Starting.ToStringPercent("F2")));
            settings.maxGenerateRate_Starting = listingStandard.Slider(settings.maxGenerateRate_Starting, 0f, 0.9999f);

            listingStandard.Gap();
            listingStandard.Label("maxGenerateRate_Global".Translate(settings.maxGenerateRate_Global.ToStringPercent("F2")));
            settings.maxGenerateRate_Global = listingStandard.Slider(settings.maxGenerateRate_Global, 0f, 1f);
            
            listingStandard.Gap();
            if (listingStandard.ButtonText("Reset".Translate()))
            {
                settings.Reset(); 
            }

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Fixed Pawn Generate";
        }

    }
}
