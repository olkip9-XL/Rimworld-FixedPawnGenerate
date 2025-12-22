using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FixedPawnGenerate
{
    public class Comp_SimplePawn : ThingComp
    {
        private string curTag = "default";

        protected Pawn pawn => this.parent as Pawn;

        public string CurTag
        {
            get => curTag;
            set {

                //effecter
                DebugActionsUtility.DustPuffFrom(pawn);
                if(Props.switchEffecter != null)
                {
                    Effecter effecter = Props.switchEffecter.Spawn();
                    effecter.Trigger(new TargetInfo(pawn.Position, pawn.Map), new TargetInfo(pawn.Position, pawn.Map));
                    effecter.Cleanup();
                }

                curTag = value;

                //refresh graphics
                pawn.Drawer.renderer.SetAllGraphicsDirty();

                Notify_TagSwitched(curTag);
            }
        }

        public CompProperties_SimplePawn Props => this.props as CompProperties_SimplePawn;

        public virtual GraphicData GetCurrentGraphicData()
        {
            return Props.GraphicOfTag(curTag);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref curTag, "curTag", "default");
        }
       
        
        public virtual void Notify_TagSwitched(string tag)
        {
        }

    }
}
