using System;
using RimWorld;

namespace FixedPawnGenerate
{
    [DefOf]
    public static class FixedPawnDefOf
    {
        static FixedPawnDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FixedPawnDefOf));
        }

    }
}