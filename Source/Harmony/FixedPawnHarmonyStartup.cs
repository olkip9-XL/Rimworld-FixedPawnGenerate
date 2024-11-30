using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

using HarmonyLib;

namespace FixedPawnGenerate
{
    [StaticConstructorOnStartup]
    public static class FixedPawnHarmonyPatchStartup
    {
        static FixedPawnHarmonyPatchStartup()
        {
            var harmony = new HarmonyLib.Harmony("Lotus.FixedPawnGenerate");
            harmony.PatchAll(typeof(FixedPawnHarmony).Assembly);
            harmony.PatchAll(typeof(FPGProtraitHarmony).Assembly);
        }
    }
}
