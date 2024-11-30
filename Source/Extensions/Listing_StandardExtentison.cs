using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;

namespace FixedPawnGenerate
{
    public static class Listing_StandardExtentison
    {
        public static void TextFieldNumericLabeled<T>(this Listing_Standard listing, string label, ref T value, ref string buffer, float labelPct = 0.5f, string tooltip = null, float min = 0, float max = 1E+09f) where T : struct
        {
            Rect rect = listing.GetRect(Text.LineHeight);

            Rect leftRect = rect.LeftPart(labelPct);
            Rect rightRect = rect.RightPart(1 - labelPct);

            if (!listing.BoundingRectCached.HasValue || rect.Overlaps(listing.BoundingRectCached.Value))
            {
                if (!tooltip.NullOrEmpty())
                {
                    if (Mouse.IsOver(leftRect))
                    {
                        Widgets.DrawHighlight(leftRect);
                    }

                    TooltipHandler.TipRegion(leftRect, tooltip);
                }

                Widgets.Label(leftRect, label);
                Widgets.TextFieldNumeric(rightRect, ref value, ref buffer, min, max);
            }

            listing.Gap(listing.verticalSpacing);
        }

        public static bool ButtonTextCenter(this Listing_Standard listing, string label, string highlightTag = null, float widthPct = 1f)
        {
            Rect rect = listing.GetRect(30f);

            Rect centerRect = rect;
            centerRect.width = rect.width * widthPct;
            centerRect = centerRect.CenteredOnXIn(rect);

            bool result = false;
            if (!listing.BoundingRectCached.HasValue || rect.Overlaps(listing.BoundingRectCached.Value))
            {
                result = Widgets.ButtonText(centerRect, label);
                if (highlightTag != null)
                {
                    UIHighlighter.HighlightOpportunity(centerRect, highlightTag);
                }
            }

            listing.Gap(listing.verticalSpacing);
            return result;
        }
    }
}
