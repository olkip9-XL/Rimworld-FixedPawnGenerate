using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace FixedPawnGenerate
{
    internal class GameComponent_PawnVoice : GameComponent
    {
        const int coolDownTicks = 120;

        float soundCoolDown = 0f;

        Queue<Pair<Pawn, SoundDef>> queuedSound = new Queue<Pair<Pawn, SoundDef>>();

        private bool CanPlay
        {
            get
            {
                if (!FixedPawnUtility.Settings.enablePawnVoice)
                {
                    return false;
                }

                if (soundCoolDown <= 0f)
                {
                    return true;
                }
                return false;
            }
        }

        public static GameComponent_PawnVoice Instance { get; private set; }

        public GameComponent_PawnVoice(Game game)
        {
            Instance = this;
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            if (queuedSound.Count > 0 && CanPlay)
            {
                var pair = queuedSound.Dequeue();
                PlayVoice(pair.First, pair.Second);
            }

            if (soundCoolDown > 0f)
            {
                soundCoolDown -= 1f / Find.TickManager.TickRateMultiplier;
            }
        }

        public void PlayVoice(Pawn pawn, SoundDef soundDef)
        {
            if (CanPlay)
            {
                //MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, soundDef.defName, -1);

                SoundInfo sinfo = SoundInfo.InMap(new TargetInfo(pawn.Position, pawn.Map));
                sinfo.volumeFactor = FixedPawnUtility.Settings.voiceVolumeFactor;

                soundDef.PlayOneShot(sinfo);

                soundCoolDown = coolDownTicks + (soundDef.Duration.max * 60f);
            }
        }

        public void QueueVoice(Pawn pawn, SoundDef soundDef)
        {
            queuedSound.Enqueue(new Pair<Pawn, SoundDef>(pawn, soundDef));
        }
    }
}
