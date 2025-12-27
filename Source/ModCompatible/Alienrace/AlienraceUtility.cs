using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    public static class AlienraceUtility
    {
        //color channel: base, hair, skin, skinBase, tattoo, favorite, ideo, mech
        public static Action<Pawn, string, Color?, Color?> SetAlienChannelColorFunc = null;

        public static Action<Pawn, string, Color?, Color?> SetAlienAddonColorFunc = null;

        public static Action<Pawn, string, int> SetPawnAddonFunc = null;

        internal static void SetPawnSkinColor(Pawn pawn, Color color)
        {
            SetAlienChannelColorFunc?.Invoke(pawn, "skin", color, null);
        }

        internal static void SetPawnChannelColor(Pawn pawn, string channel, Color? color, Color? colorTwo)
        {
            SetAlienChannelColorFunc?.Invoke(pawn, channel, color, colorTwo);
        }

        internal static void SetAlienAddonColor(Pawn pawn, string addonName, Color? color, Color? colorTwo)
        {
            SetAlienAddonColorFunc?.Invoke(pawn, addonName, color, colorTwo);
        }

        internal static void SetPawnAddon(Pawn pawn, string addonName, int variantIndex)
        {
            SetPawnAddonFunc?.Invoke(pawn, addonName, variantIndex);
        }


    }
}
