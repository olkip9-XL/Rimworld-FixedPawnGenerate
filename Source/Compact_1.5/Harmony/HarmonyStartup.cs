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
    public static class HarmonyStartup
    {
        static HarmonyStartup()
        {
            new Harmony("LotusLand.FixedPawnGenerate").PatchAll();
        }
    }
}
