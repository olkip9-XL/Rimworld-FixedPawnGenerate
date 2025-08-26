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

        public String texture;

        public float offsetX = 0f;

        public float offsetY = 0f;

        public float scale = 1f;

        public List<PortraitDiffData> stats = new List<PortraitDiffData>();

        public Vector2Int GetDiffOffset(PawnPortraitStat stat)
        {
            if (stats == null || stats.Count == 0)
            {
                return new Vector2Int(0,0);
            }

            PortraitDiffData diff = stats.FirstOrDefault(x => x.stat == stat);
            if (diff != null)
            {
                return new Vector2Int((int)diff.offsetX, (int)diff.offsetY);
            }
            else
            {
                return new Vector2Int(0,0);
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
}
