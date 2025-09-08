using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FixedPawnGenerate
{
    public enum PawnVoiceType
    {
        None,

        //skill effect
        Buffed,
        BuffSelf,
        Defense,
        Recovery,

        //ability
        Skill,

        //normal action
        Covered, //x
        Damage,
        Move,
        Attack,
        Retire,
        Shout,
        Selected,
        Lobby//x
    }
    internal class CompProperties_PawnVoice : CompProperties
    {
        public CompProperties_PawnVoice()
        {
            this.compClass = typeof(CompPawnVoice);
        }

        Dictionary<PawnVoiceType, List<SoundDef>> voiceDict = new Dictionary<PawnVoiceType, List<SoundDef>>();

        public string clipsPath;

        public SoundDef VoiceOfType(PawnVoiceType type)
        {
            if (voiceDict == null)
            {
                voiceDict = new Dictionary<PawnVoiceType, List<SoundDef>>();
            }

            if (!voiceDict.ContainsKey(type))
            {
                List<SoundDef> list = new List<SoundDef>();

                string resPath = $"{clipsPath}/{type}";

                AudioGrain_Folder audioGrain = new AudioGrain_Folder();
                audioGrain.clipFolderPath = resPath;

                //manually resolve grains
                IEnumerable<ResolvedGrain> grains = audioGrain.GetResolvedGrains();
                foreach (var grain in grains)
                {
                    SoundDef def = new SoundDef();
                    def.context = SoundContext.MapOnly;
                    def.maxSimultaneous = 1;
                    def.defName = $"PawnVoice_{type}";

                    SubSoundDef subSoundDef = new SubSoundDef();
                    subSoundDef.parentDef = def;
                    //subSoundDef.distRange = new FloatRange(25f, 50f);

                    FieldInfo fieldInfo = typeof(SubSoundDef).GetField("resolvedGrains", BindingFlags.NonPublic | BindingFlags.Instance);
                    List<ResolvedGrain> resolvedGrains = new List<ResolvedGrain>();
                    resolvedGrains.Add(grain);
                    fieldInfo.SetValue(subSoundDef, resolvedGrains);

                    FieldInfo fieldInfo2 = typeof(SubSoundDef).GetField("distinctResolvedGrainsCount", BindingFlags.NonPublic | BindingFlags.Instance);
                    fieldInfo2.SetValue(subSoundDef, 1);

                    def.subSounds = new List<SubSoundDef>() { subSoundDef };

                    list.Add(def);
                }

                if (grains.Any())
                {
                    voiceDict[type] = list;
                }
                else
                {
                    voiceDict[type] = null;
                }
            }

            return voiceDict[type].NullOrEmpty() ? null : voiceDict[type].RandomElement();
        }
    }
}
