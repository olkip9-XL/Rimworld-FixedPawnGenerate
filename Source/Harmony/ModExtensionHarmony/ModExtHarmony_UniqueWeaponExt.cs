using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    [HarmonyPatch(typeof(CompUniqueWeapon), "PostPostMake")]
    internal static class ModExtHarmony_UniqueWeaponExt
    {
        internal static void Postfix(CompUniqueWeapon __instance)
        {
            if (__instance.parent.def.HasModExtension<ModExtension_UniqueWeaponExt>())
            {
                ModExtension_UniqueWeaponExt ext = __instance.parent.def.GetModExtension<ModExtension_UniqueWeaponExt>();

                if (ext != null)
                {
                    if (ext.forbidRandomName)
                    {
                        FieldInfo fieldInfo = typeof(CompUniqueWeapon).GetField("name", BindingFlags.NonPublic | BindingFlags.Instance);

                        fieldInfo?.SetValue(__instance, __instance.parent.def.label);

                        if (__instance.parent.TryGetComp<CompArt>(out var comp))
                        {
                            comp.Title = __instance.parent.def.label;
                        }
                    }

                    if (ext.useForceColor)
                    {
                        FieldInfo fieldInfo = typeof(CompUniqueWeapon).GetField("color", BindingFlags.NonPublic | BindingFlags.Instance);

                        ColorDef colorDef = new ColorDef() { label = "white" };
                        if (ext.forceColor == null)
                        {
                            colorDef.color = new Color(1f, 1f, 1f);
                        }
                        else
                        {
                            colorDef.color = ext.forceColor;
                        }

                        fieldInfo?.SetValue(__instance, colorDef);
                    }

                    if (!ext.traitsOverride.NullOrEmpty())
                    {
                        __instance.TraitsListForReading.Clear();
                        foreach (WeaponTraitDef trait in ext.traitsOverride)
                        {
                            if (trait != null)
                            {
                                __instance.AddTrait(trait);
                            }
                        }
                    }

                    if (!ext.forceAddTraits.NullOrEmpty())
                    {
                        foreach (WeaponTraitDef trait in ext.forceAddTraits)
                        {
                            if (trait != null)
                            {
                                __instance.TraitsListForReading.RemoveWhere(t => t.Overlaps(trait));

                                __instance.AddTrait(trait);
                            }
                        }
                    }
                }
            }
        }
    }
}
