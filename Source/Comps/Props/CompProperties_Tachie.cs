using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    

    public class CompProperties_Tachie : CompProperties
    {
        public CompProperties_Tachie()
        {
            this.compClass = typeof(CompTachie);
        }

        //props
        public string texture;

        public float offsetX = 0f;

        public float offsetY = 0f;

        public float scale = 1f;

        public List<PortraitDiffData> stats = new List<PortraitDiffData>();

        internal List<AlterTachieData> alterTachies = new List<AlterTachieData>();

        public bool useRandomTachie = false;

        // private
        private AlterTachieData defaultDataInt = null;
        internal AlterTachieData DefaultData
        {
            get
            {
                if (defaultDataInt == null)
                {
                    defaultDataInt = new AlterTachieData();
                    defaultDataInt.alterTachieID = -1;
                    defaultDataInt.texturePath = texture;
                    defaultDataInt.stats = stats;

                    if (!alterTachies.Any(x => x.alterTachieID == -1))
                    {
                        alterTachies.Add(defaultDataInt);
                    }
                }
                return defaultDataInt;
            }
        }
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }

            foreach (var alter in alterTachies)
            {
                if (alter.alterTachieID < 0)
                {
                    yield return "AlterTachieData has invalid alterTachieID: " + alter.alterTachieID + ", it must be non-negative.";
                }
            }
        }
    }

    public class PortraitDiffData
    {
        public PawnPortraitStat stat;
        public int offsetX = 0;
        public int offsetY = 0;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            //normal
            if (xmlRoot.Name == "li")
            {
                foreach (XmlNode item in xmlRoot.ChildNodes)
                {
                    switch (item.Name)
                    {
                        case "stat":
                            stat = (PawnPortraitStat)Enum.Parse(typeof(PawnPortraitStat), item.InnerText);
                            break;
                        case "offsetX":
                            offsetX = int.Parse(item.InnerText);
                            break;
                        case "offsetY":
                            offsetY = int.Parse(item.InnerText);
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                //<PawnPortraitStat>(0,0)</PawnPortraitStat>
                stat = (PawnPortraitStat)Enum.Parse(typeof(PawnPortraitStat), xmlRoot.Name);

                if (xmlRoot.InnerText != null && xmlRoot.InnerText.Length > 0)
                {
                    string valueText = xmlRoot.InnerText.Trim('(', ')');
                    string[] strArray = valueText.Split(',');

                    if (strArray.Length == 2)
                    {
                        offsetX = int.Parse(strArray[0]);
                        offsetY = int.Parse(strArray[1]);
                    }
                }

            }

        }
    }

    class AlterTachieData
    {
        public int alterTachieID = -1;
        public string texturePath;
        public List<PortraitDiffData> stats = new List<PortraitDiffData>();
    }
}
