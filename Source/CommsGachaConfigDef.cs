using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;

namespace FixedPawnGenerate
{
    public class CommsGachaConfigDef : Def
    {
        //参数
        private Type workerClass = typeof(CommsGachaConfigWorker);
        private CommsGachaConfigWorker workerInt;
        public CommsGachaConfigWorker Worker
        {
            get
            {
                if (workerInt == null)
                {
                    workerInt = (CommsGachaConfigWorker)Activator.CreateInstance(workerClass);
                    workerInt.def = this;
                }
                return workerInt;
            }
        }

        public List<ThingDefCountClass> gachaCost;

        public List<ThingDefCountClass> gachaFailItems;

        public List<string> gachaTags = new List<string>();

        public float baseGachaChance = 0.5f;

        public bool setPawnToPlayerFaction = false;

        public string successMessage = "";

        public string failMessage = "";

        public string successLetterLabel = "";

        public string successLetterText = "";

        public string noMorePawnsMessage = "";

        public string notEnoughResourcesMessage = "";

        public bool disableIfNoMorePawns = true;

        public JobDef openCommsJobDef;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors())
            {
                yield return error;
            }

            if (gachaTags.NullOrEmpty())
            {
                yield return "gachaTags is null or empty";
            }

            if (baseGachaChance <= 0f || baseGachaChance > 1f)
            {
                yield return "baseGachaChance must between 0~1";
            }

            if (openCommsJobDef == null)
            {
                yield return "openCommsJobDef is null";
            }

            if (openCommsJobDef != null && openCommsJobDef.driverClass != typeof(JobDriver_OpenGachaComms))
            {
                yield return $"openCommsJobDef's driverClass must be {typeof(JobDriver_OpenGachaComms)}";
            }
        }

        public override void PostLoad()
        {
            base.PostLoad();
            if (openCommsJobDef != null)
            {
                openCommsJobDef.modExtensions.Add(new ModExtension_OpenCommsJob() { gachaDef = this });
            }
        }

        public bool DisplayMenu()
        {
            if (Worker.RemainPawns.NullOrEmpty() && disableIfNoMorePawns)
            {
                return false;
            }
            return true;
        }


    }
}
