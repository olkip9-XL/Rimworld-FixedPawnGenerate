using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace FixedPawnGenerate
{
    internal static class Harmony_CommsMenu
    {

        [HarmonyPatch(typeof(Building_CommsConsole), "GetFloatMenuOptions")]
        static class Patch_Building_CommsConsole_GetFloatMenuOptions
        {
            private static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result, Building_CommsConsole __instance, Pawn myPawn)
            {
                List<FloatMenuOption> list = Enumerable.ToList<FloatMenuOption>(__result);
                foreach (FloatMenuOption floatMenuOption in __result)
                {
                    yield return floatMenuOption;
                }
                if (Enumerable.Count<FloatMenuOption>(list) == 1 && list[0].action == null)
                {
                    yield break;
                }

                //gacha ui
                foreach (var def in DefDatabase<CommsGachaConfigDef>.AllDefs)
                {
                    if (!def.DisplayMenu())
                        continue;

                    if (!def.openCommsJobDef.HasModExtension<ModExtension_OpenCommsJob>())
                    {
                        Log.Warning($"Adding modExtension to {def.defName}");

                        if (def.openCommsJobDef.modExtensions == null)
                        {
                            def.openCommsJobDef.modExtensions = new List<DefModExtension>();
                        }

                        def.openCommsJobDef.modExtensions.Add(new ModExtension_OpenCommsJob() { gachaDef = def });
                    }

                    FloatMenuOption option = new FloatMenuOption(def.Worker.Label, delegate ()
                    {
                        Job job = JobMaker.MakeJob(def.openCommsJobDef, __instance);

                        myPawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
                    }, MenuOptionPriority.InitiateSocial, null, null, 0f, null, null, true, 0);
                    yield return FloatMenuUtility.DecoratePrioritizedTask(option, myPawn, __instance, "ReservedBy", null);
                }
                yield break;
            }
        }
    }
}
