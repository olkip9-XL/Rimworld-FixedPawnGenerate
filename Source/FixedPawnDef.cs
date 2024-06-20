using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    public class FixedPawnDef : Def
    {
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string s in base.ConfigErrors())
            {
                yield return s;
                //s = null;
            }
            yield break;
        }

        public double generateWeight = 0.0;

        public float generateRate = 0.0f;

        public bool isUnique = false;

        public XenotypeDef xenotype = null;

        public string customXenotype = null;

        public FactionDef faction = null;

        public PawnKindDef pawnKind = null;

        public ThingDef race=null;

        public Gender gender = Gender.None;

        public String firstName=null;

        public String nickName=null;

        public String lastName=null;

        public Name name
        {
            get
            {
                if (firstName == null)
                {
                    return null;
                }

                else if (lastName == null)
                {
                    return new NameSingle(firstName);
                }
                else
                {
                    return new NameTriple(firstName, nickName, lastName);
                }

            }
        }

        public float age = 0f;

        public float chronologicalAge = 0f;

        public BackstoryDef childHood;

        public BackstoryDef adultHood;

        public BeardDef beard;

        public HairDef hair = null;

        public Color hairColor;

        public BodyTypeDef bodyType = null;

        public HeadTypeDef headType = null;

        public Color favoriteColor;

        public TattooDef faceTatoo = null;

        public TattooDef bodyTatoo = null;

        public Color skinColor;

        public class ThingData
        {
            public ThingDef thing = null;
            public int count = 1;
            public ThingDef stuff = null;
        }

        public List<ThingData> equipment = new List<ThingData>();

        public List<ThingData> inventory = new List<ThingData>();

        public List<ThingData> apparel = new List<ThingData>();

        public class SkillData
        {
            public SkillDef skill;
            public int level;
            public Passion passion;
        }

        public List<SkillData> skills = new List<SkillData>();

        public class HediffData
        {
            public HediffDef hediff;
            public BodyPartDef bodyPart = null;
            public float severity=0.5f;
        }

        public List<HediffData> hediffs = new List<HediffData>();
        

        public List<TraitDef> traits = new List<TraitDef>();



    }
}
