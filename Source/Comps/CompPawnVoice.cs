using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;

namespace FixedPawnGenerate
{
    internal class CompPawnVoice : ThingComp
    {
        public CompProperties_PawnVoice Props => (CompProperties_PawnVoice)this.props;

        public GameComponent_PawnVoice Game => GameComponent_PawnVoice.Instance;

        public Pawn Pawn => this.parent as Pawn;

        public void PlayOneShot(PawnVoiceType type)
        {
            SoundDef def = Props.VoiceOfType(type);
            if (def == null)
            {
                return;
            }

            Game.PlayVoice(Pawn, def);
        }

        public void QueueOneShot(PawnVoiceType type)
        {
            SoundDef def = Props.VoiceOfType(type);
            if (def == null)
            {
                return;
            }

            Game.QueueVoice(Pawn, def);
        }
        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);

            if (totalDamageDealt <= 0)
            {
                return;
            }

            if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                PlayOneShot(PawnVoiceType.Damage);
        }

        public override void Notify_UsedVerb(Pawn pawn, Verb verb)
        {
            if (UnityEngine.Random.value > 0.5f)
                pawn.PlayVoice(PawnVoiceType.Shout);
        }

    }
}
