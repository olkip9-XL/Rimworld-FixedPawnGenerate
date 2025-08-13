using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    [HarmonyPatch(typeof(FixedPawnUtility), "ModifyPawn")]
    internal static class ModifyPawn_Patch
    {
        private static void Postfix(Pawn pawn, FixedPawnDef def)
        {
            if (def.traits.Count > 0)
            {
                foreach (var traitData in def.traits)
                {
                    int traitDegree;

                    if (traitData.trait.degreeDatas.Count == 1)
                    {
                        traitDegree = 0;
                    }
                    else
                    {
                        if (traitData.trait.degreeDatas.Find(x => x.degree == traitData.degree) != null)
                        {
                            traitDegree = traitData.degree;
                        }
                        else
                        {
                            Log.Warning($"Trait {traitData.trait.defName} does not have degree {traitData.degree}, use First defined degree");
                            traitDegree = traitData.trait.degreeDatas[0].degree;
                        }
                    }

                    //traits skill gain
                    TraitDegreeData traitDegreeData = traitData.trait.degreeDatas.Find(x => x.degree == traitDegree);
                    if (traitDegreeData != null && !traitDegreeData.skillGains.NullOrEmpty())
                    {
                        foreach (var skillGain in traitDegreeData.skillGains)
                        {
                            SkillRecord skill = pawn.skills.GetSkill(skillGain.skill);
                            if (skill != null)
                            {
                                skill.Level = skill.levelInt + skillGain.amount;
                            }
                        }
                    }
                }
            }
        }
    }
}
