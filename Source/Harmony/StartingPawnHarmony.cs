using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Verse.Dialog_InfoCard;
using UnityEngine;
using Verse;
using RimWorld;

namespace FixedPawnGenerate
{
    public static class StartingPawnHarmony
    {
        private static List<Pawn> StartingAndOptionalPawns => Find.GameInitData.startingAndOptionalPawns;


        [HarmonyPatch(typeof(StartingPawnUtility), "GeneratePossessions")]
        public static class Patch_GeneratePossessions
        {
            private static Dictionary<Pawn, List<ThingDefCount>> StartingPossessions => Find.GameInitData.startingPossessions;
            private static bool Prefix(Pawn pawn)
            {
                FixedPawnDef def = null;
                if((def = pawn.GetFixedPawnDef())!= null)
                {
                    if (!StartingPossessions.ContainsKey(pawn))
                    {
                        StartingPossessions.Add(pawn, new List<ThingDefCount>());
                    }
                    else
                    {
                        StartingPossessions[pawn].Clear();
                    }

                    foreach (var item in def.inventory)
                    {
                        StartingPossessions[pawn].Add(new ThingDefCount(item.thing, item.count));
                    }

                    return false;
                }
                return true;    
            }
        }

        [HarmonyPatch(typeof(StartingPawnUtility), "NewGeneratedStartingPawn")]
        public static class Patch_NewGeneratedStartingPawn
        {
            private static ScenPart_ConfigPage_ConfigureStartingFixedPawns FixedPawnPart
            {
                get
                {
                    IEnumerable<ScenPart> allParts = Find.Scenario.AllParts;

                    if (allParts.OfType<ScenPart_ConfigPage_ConfigureStartingFixedPawns>().Any())
                        return (ScenPart_ConfigPage_ConfigureStartingFixedPawns)allParts.First(x=>x is ScenPart_ConfigPage_ConfigureStartingFixedPawns);

                    return null;
                }
            }

            private static List<Pawn> StartingPawns => Find.GameInitData.startingAndOptionalPawns;
            private static List<FixedPawnDef> StartingDefs
            { 
                get
                {
                    List<FixedPawnDef> list = new List<FixedPawnDef>();
                    foreach(Pawn pawn in StartingPawns)
                    {
                        if(pawn.GetFixedPawnDef() != null)
                        {
                            list.Add(pawn.GetFixedPawnDef());
                        }
                    }

                    return list;
                }
            }
          
            private static Pawn GenerateNewPawn(string tag, int index)
            {
                if(tag == "")
                    return null;

                IEnumerable<FixedPawnDef> list = DefDatabase<FixedPawnDef>.AllDefsListForReading.FindAll(x => x.tags.Contains(tag)).Except(StartingDefs);

                if (list.Any())
                {
                    FixedPawnDef def = FixedPawnUtility.GetRandomFixedPawnDefByWeight(list);

                    Pawn pawn = FixedPawnUtility.GenerateFixedPawnWithDef(def);

                    StartingPawnUtility.GeneratePossessions(pawn);

                    return pawn;
                }
                else
                {
                    //replace with current pawn
                    if(StartingPawns.Count> index)
                    {
                        Pawn pawn = StartingPawns[index];
                        if (pawn.IsUniquePawn())
                        {
                            StartingPawnUtility.GeneratePossessions(pawn);
                            return pawn;
                        }
                    }

                    return null;
                }

            }

            public static bool Prefix(int index, ref Pawn __result)
            {
                if (FixedPawnPart == null)
                    return true;

                List<string> pawnTags = FixedPawnPart.pawnTags;

                if (index < -1 || index >= pawnTags.Count)
                    return true;

                //index
                int currentIndex = index;
                if (index == -1)
                    currentIndex = StartingPawns.Count;

                string tag = "";
                if (currentIndex < pawnTags.Count)
                    tag = pawnTags[currentIndex];

                __result = GenerateNewPawn(tag, currentIndex);

                return __result == null;

            }
        }

        [HarmonyPatch(typeof(StartingPawnUtility), "RegenerateStartingPawnInPlace")]
        public static class Patch_RegenerateStartingPawnInPlace
        {
            //skip destroy family of unique pawn
            public static bool Prefix(int index, ref Pawn __result)
            {
                List<Pawn> startingAndOptionalPawns = Find.GameInitData.startingAndOptionalPawns;

                Pawn pawn = startingAndOptionalPawns[index];
#if DEBUG
                Log.Warning($"Replacing :{pawn.Name}");
#endif

                FixedPawnDef fixedPawnDef = FixedPawnUtility.Manager.GetDef(pawn);
                if (fixedPawnDef != null && fixedPawnDef.isUnique)
                {

                    Pawn pawn2 = StartingPawnUtility.NewGeneratedStartingPawn(index);
                    startingAndOptionalPawns[index] = pawn2;
                    __result = pawn2;

                    return false;
                }

                return true;
            }

        }

#if DEBUG

        [HarmonyPatch(typeof(PawnUtility), "DestroyStartingColonistFamily")]
        public static class Patch_Debug_DestroyStartingColonistFamily
        {
            public static void Prefix(Pawn pawn)
            {
                String str=$"Destroying StartingColonistFamily of {pawn.Name}\n";
                foreach (Pawn pawn2 in Enumerable.ToList<Pawn>(pawn.relations.RelatedPawns))
                {
                    if (!Find.GameInitData.startingAndOptionalPawns.Contains(pawn2))
                    {
                        WorldPawnSituation situation = Find.WorldPawns.GetSituation(pawn2);
                        if (situation == WorldPawnSituation.Free || situation == WorldPawnSituation.Dead)
                        {
                            str += $"Destroying {pawn2.Name}\n";
                        }
                    }
                }
                Log.Warning(str);
            }
        }

#endif

    }
}
