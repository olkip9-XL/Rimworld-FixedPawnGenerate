using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using System.Xml;
using System.Xml.Linq;


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

        public TattooDef faceTattoo = null;

        public TattooDef bodyTattoo = null;

        public Color skinColor;

        public class ThingData
        {
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                if(xmlRoot.Name == "li")
                {
                    foreach (XmlNode xmlNode in xmlRoot.ChildNodes)
                    {
                        switch(xmlNode.Name)
                        {
                            case "thing":
                                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thing", xmlNode.InnerText, null, null, null);
                                break;
                            case "count":
                                this.count = ParseHelper.FromString<int>(xmlNode.InnerText);
                                break;
                            case "stuff":
                                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "stuff", xmlNode.InnerText, null, null, null);
                                break;
                            case "quality":
                                this.quality = (QualityCategory)Enum.Parse(typeof(QualityCategory), xmlNode.InnerText);
                                break;
                            case "color":
                                this.color = ParseHelper.FromString<Color>(xmlNode.InnerText);
                                break;
                            default:
                                Log.Error($"Unknown node {xmlNode.Name} in {xmlRoot.Name}");
                                break;
                        }
                    }

                    return;
                }

                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thing", xmlRoot.Name, null, null, null);


                foreach (XmlAttribute xmlAttribute in xmlRoot.Attributes)
                {
                    switch (xmlAttribute.Name)
                    {
                        case "quality":
                            this.quality = (QualityCategory)Enum.Parse(typeof(QualityCategory), xmlAttribute.Value);
                            break;
                        case "color":
                            this.color = ParseHelper.FromString<Color>(xmlAttribute.Value);
                            break;
                        case "stuff":
                            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "stuff", xmlAttribute.Value, null, null, null);
                            break;
                        default:
                            Log.Error($"Unknown attribute {xmlAttribute.Name} in {xmlRoot.Name}");
                            break;
                    }
                }

                if (xmlRoot.HasChildNodes)
                {
                    if (!int.TryParse(xmlRoot.InnerText, out this.count))
                    {
                        this.count = 1;
                        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "stuff", xmlRoot.InnerText, null, null, null);
                    }
                }
            }

            public ThingDef thing = null;
            public int count = 1;
            public ThingDef stuff = null;

            public QualityCategory quality = QualityCategory.Normal;

            public Color color;
        }

        public List<ThingData> equipment = new List<ThingData>();

        public List<ThingData> inventory = new List<ThingData>();

        public List<ThingData> apparel = new List<ThingData>();

        public class SkillData
        {
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                if (xmlRoot.Name == "li")
                {
                    foreach (XmlNode xmlNode in xmlRoot.ChildNodes)
                    {
                        switch (xmlNode.Name)
                        {
                            case "skill":
                                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skill", xmlNode.InnerText, null, null, null);
                                break;
                            case "level":
                                this.level = ParseHelper.FromString<int>(xmlNode.InnerText);
                                break;
                            case "passion":
                                this.passion = (Passion)Enum.Parse(typeof(Passion), xmlNode.InnerText);
                                replacePassion = true;
                                break;
                            default:
                                Log.Error($"Unknown node {xmlNode.Name} in {xmlRoot.Name}");
                                break;
                        }
                    }
                    return;
                }

                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skill", xmlRoot.Name, null, null, null);
                if (xmlRoot.InnerText.Contains('('))
                {
                    string valueText = xmlRoot.InnerText.Trim('(', ')');
                    string[] strArray = valueText.Split(',');

                    this.level = ParseHelper.FromString<int>(strArray[0]);
                    this.passion = (Passion)Enum.Parse(typeof(Passion), strArray[1]);

                    replacePassion = true;
                }
                else
                {
                    this.level = ParseHelper.FromString<int>(xmlRoot.InnerText);
                }


            }

            public bool replacePassion = false;

            public SkillDef skill;
            public int level;
            public Passion passion;
        }

        public List<SkillData> skills = new List<SkillData>();

        public class HediffData
        {
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                if (xmlRoot.Name == "li")
                {
                    foreach (XmlNode xmlNode in xmlRoot.ChildNodes)
                    {
                        switch (xmlNode.Name)
                        {
                            case "hediff":
                                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "hediff", xmlNode.InnerText, null, null, null);
                                break;
                            case "bodyPart":
                                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "bodyPart", xmlNode.InnerText, null, null, null);
                                break;
                            case "severity":
                                this.severity = ParseHelper.FromString<float>(xmlNode.InnerText);
                                break;
                            default:
                                Log.Error($"Unknown node {xmlNode.Name} in {xmlRoot.Name}");
                                break;
                        }
                    }
                    return;
                }

                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "hediff", xmlRoot.Name, null, null, null);
                if (xmlRoot.InnerText.Contains('(')){
                    string valueText = xmlRoot.InnerText.Trim('(', ')');
                    string[] strArray = valueText.Split(',');

                    this.severity = ParseHelper.FromString<float>(strArray[0]);
                    DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "bodyPart", strArray[1], null, null, null);
                }
                else
                {
                   this.severity = ParseHelper.FromString<float>(xmlRoot.InnerText);
                }
            }

            public HediffDef hediff;
            public BodyPartDef bodyPart = null;
            public float severity=0.5f;
        }

        public List<HediffData> hediffs = new List<HediffData>();
        
        public class TraitData
        {
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                if (xmlRoot.Name == "li")
                {
                    foreach (XmlNode xmlNode in xmlRoot.ChildNodes)
                    {
                        switch (xmlNode.Name)
                        {
                            case "trait":
                                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "trait", xmlNode.InnerText, null, null, null);
                                break;
                            case "degree":
                                this.degree = ParseHelper.FromString<int>(xmlNode.InnerText);
                                break;
                            default:
                                Log.Error($"Unknown node {xmlNode.Name} in {xmlRoot.Name}");
                                break;
                        }
                    }
                    return;
                }

                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "trait", xmlRoot.Name, null, null, null);
                this.degree = 0;
                if (xmlRoot.HasChildNodes)
                {
                    this.degree = ParseHelper.FromString<int>(xmlRoot.InnerText);
                }
            }

            public TraitDef trait;
            public int degree = 0;
        }

        public List<TraitData> traits = new List<TraitData>();

        public List<CompProperties> comps = new List<CompProperties>();

        public List<AbilityDef> abilities = new List<AbilityDef>();

        //Facial Animation
        [MayRequire("Nals.FacialAnimation")]
        public FPG_FacialAnimationProps facialAnimationProps = null;

        public List<String> tags = new List<String>();

        public class RelationData
        {
            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                if (xmlRoot.Name == "li")
                {
                    foreach (XmlNode xmlNode in xmlRoot.ChildNodes)
                    {
                        switch (xmlNode.Name)
                        {
                            case "relation":
                                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "relation", xmlNode.InnerText, null, null, null);
                                break;
                            case "fixedPawn":
                                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "fixedPawn", xmlNode.InnerText, "LotusLand.FixedPawnGenerate", null, null);
                                break;
                            default:
                                Log.Error($"Unknown node {xmlNode.Name} in {xmlRoot.Name}");
                                break;
                        }
                    }
                    return;
                }

                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "relation", xmlRoot.Name, null, null, null);
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "fixedPawn", xmlRoot.InnerText, "LotusLand.FixedPawnGenerate", null, null);
            }

            public RelationData(PawnRelationDef relationDef, FixedPawnDef fixedPawnDef )
            {
                this.relation = relationDef;
                this.fixedPawn = fixedPawnDef;
            }
            public RelationData()
            {
                this.relation = null;
                this.fixedPawn = null;
            }

            public PawnRelationDef relation;
            public FixedPawnDef fixedPawn;
        }

        public List<RelationData> relations = new List<RelationData>();

        /**********************************/
        public bool IsSpawned => this.isUnique && FixedPawnUtility.Manager.GetPawn(this) != null;

        public Pawn GetPawn()
        {
            if (this.isUnique)
            {
                return FixedPawnUtility.Manager.GetPawn(this);
            }
            return null;
        }
    }
}
