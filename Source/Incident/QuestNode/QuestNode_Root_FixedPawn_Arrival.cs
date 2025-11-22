using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace FixedPawnGenerate
{
    internal class QuestNode_Root_FixedPawnJoin_WalkIn : QuestNode_Root_WandererJoin_WalkIn
    {
        public IncidentDef sourceIncident;

        public bool overrideLetterMessage = false;
        public string letterTitleFormat = "";
        public string letterTextFormat = "";

        public ModExtension_FixedPawnIncident ModExt => sourceIncident?.GetModExtension<ModExtension_FixedPawnIncident>();


        public override Pawn GeneratePawn()
        {
            if (ModExt?.SatisfiedPawns == null || ModExt.SatisfiedPawns.Count == 0)
                return base.GeneratePawn();

            FixedPawnDef def = ModExt.SatisfiedPawns.RandomElement();

            try
            {
                Pawn pawn = FixedPawnUtility.GenerateFixedPawnWithDef(def);
                ModExt.SatisfiedPawns.Remove(def);
                return pawn;
            }
            catch (Exception e)
            {
                Log.Error($"Error while generating fixed pawn for quest: {e}");
                return base.GeneratePawn();
            }
        }

        public override void SendLetter_NewTemp(Quest quest, Pawn pawn, Map map)
        {
            if (!overrideLetterMessage)
            {
                base.SendLetter_NewTemp(quest, pawn, map);
                return;
            }

            FieldInfo fieldInfo = typeof(QuestNode_Root_WandererJoin_WalkIn).GetField("signalAccept", BindingFlags.NonPublic | BindingFlags.Instance);
            string signalAccept = (string)fieldInfo.GetValue(this);

            fieldInfo = typeof(QuestNode_Root_WandererJoin_WalkIn).GetField("signalReject", BindingFlags.NonPublic | BindingFlags.Instance);
            string signalReject = (string)fieldInfo.GetValue(this);

            ChoiceLetter_AcceptJoiner choiceLetter_AcceptJoiner = (ChoiceLetter_AcceptJoiner)LetterMaker.MakeLetter(
                GetLetterTitle(pawn),
                GetLetterText(pawn),
                LetterDefOf.AcceptJoiner);

            choiceLetter_AcceptJoiner.signalAccept = signalAccept;
            choiceLetter_AcceptJoiner.signalReject = signalReject;
            choiceLetter_AcceptJoiner.quest = quest;
            choiceLetter_AcceptJoiner.overrideMap = map;
            choiceLetter_AcceptJoiner.StartTimeout(60000);
            Find.LetterStack.ReceiveLetter(choiceLetter_AcceptJoiner);
        }
        public virtual string GetLetterTitle(Pawn pawn)
        {
            return letterTitleFormat.Formatted(pawn.NameShortColored);
        }
        public virtual string GetLetterText(Pawn pawn)
        {
            return letterTextFormat.Formatted(pawn.NameShortColored);
        }


    }
}
