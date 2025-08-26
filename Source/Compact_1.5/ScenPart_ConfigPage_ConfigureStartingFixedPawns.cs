using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    public class ScenPart_ConfigPage_ConfigureStartingFixedPawns : ScenPart_ConfigPage_ConfigureStartingPawns
    {
        public List<string> pawnTags = new List<string>();
    }
}
