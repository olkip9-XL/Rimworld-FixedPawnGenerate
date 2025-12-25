using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    public class CompProperties_SimplePawn : CompProperties
    {
        protected class AlterSimplePawnGraphicData
        {
            [NoTranslate]
            public string tag;

            public GraphicData graphicData;
        }

        public CompProperties_SimplePawn()
        {
            this.compClass = typeof(Comp_SimplePawn);
        }

        //props
        public GraphicData graphicData;

        protected List<AlterSimplePawnGraphicData> alterGraphics = new List<AlterSimplePawnGraphicData>();

        public EffecterDef switchEffecter;

        public GraphicData GraphicOfTag(string tag)
        {
            if (tag == "default")
            {
                return graphicData;
            }

            foreach (AlterSimplePawnGraphicData alter in alterGraphics)
            {
                if (alter.tag == tag)
                {
                    return alter.graphicData;
                }
            }
            return graphicData;
        }

    }
}
