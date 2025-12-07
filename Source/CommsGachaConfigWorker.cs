using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace FixedPawnGenerate
{
    public struct GachaResult
    {
        public bool consumedResources;
        public string failReason;
        public FixedPawnDef fixedPawnDef;
        public bool success;
        public MessageTypeDef messageType;

        public GachaResult(bool success, bool consumedResources, string failReason, FixedPawnDef fixedPawnDef)
        {
            this.consumedResources = consumedResources;
            this.failReason = failReason;
            this.fixedPawnDef = fixedPawnDef;
            this.success = success;
            this.messageType = success ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.RejectInput;
        }

        public static GachaResult Failure(bool consumedResources, string failReason)
        {
            return new GachaResult(false, consumedResources, failReason, null);
        }

        public static GachaResult Success(FixedPawnDef fixedPawnDef)
        {
            return new GachaResult(true, true, null, fixedPawnDef);
        }

    }



    public class CommsGachaConfigWorker
    {
        public CommsGachaConfigDef def;

        List<FixedPawnDef> AllPawns => DefDatabase<FixedPawnDef>.AllDefsListForReading.FindAll(x => x.tags.Intersect(def.gachaTags).Any());
        List<FixedPawnDef> SpawnedPawns
        {
            get
            {
                List<FixedPawnDef> list = new List<FixedPawnDef>();

                foreach (var tag in def.gachaTags)
                {
                    list.AddRange(FixedPawnUtility.SpawnedPawnWithTag(tag));
                }

                return list;
            }
        }
        public List<FixedPawnDef> RemainPawns => AllPawns.Except(SpawnedPawns).ToList();

        public virtual string Label
        {
            get
            {
                string costStr = string.Join(", ",
                    def.gachaCost.Select(
                        cc => cc.count + "x" + cc.thingDef.label)
                    );

                return $"{def.label} ({costStr})";
            }

        }

        public virtual void DoGacha(Pawn caller, int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                GachaResult result = GachaAction(caller);

                if (result.failReason != null)
                {
                    Messages.Message(result.failReason, result.messageType);
                }

                if (result.consumedResources)
                {
                    ConsumeResources(caller.Map);
                }

                if (result.success)
                {
                    SuccessAction(caller, result.fixedPawnDef);
                }

                if (!result.success && result.consumedResources)
                {
                    FailAction(caller);
                }
            }
        }

        public virtual GachaResult GachaAction(Pawn caller)
        {
            //资源不足
            if (!HasEnoughResources(caller.Map))
            {
                return GachaResult.Failure(false, def.notEnoughResourcesMessage);
            }

            //剩余角色不足
            if (RemainPawns.NullOrEmpty())
            {
                return GachaResult.Failure(true, def.noMorePawnsMessage);
            }

            //抽卡判定
            if (UnityEngine.Random.value <= def.baseGachaChance)
            {
                FixedPawnDef fixedPawnDef = FixedPawnUtility.GetRandomFixedPawnDefByWeight(RemainPawns);

                return GachaResult.Success(fixedPawnDef);
            }
            else
            {
                return GachaResult.Failure(true, def.failMessage);
            }
        }

        protected virtual void SuccessAction(Pawn caller, FixedPawnDef pawnDef)
        {
            if (pawnDef == null)
            {
                Log.Error("Trying to generate pawn with null FixedPawnDef");
                return;
            }

            if (pawnDef.IsSpawned)
            {
                Log.Error($"Trying to generate unique pawn {pawnDef.defName} again.");
                return;
            }

            Map map = caller.Map;

            if (def.setPawnToPlayerFaction)
            {
                pawnDef.faction = Find.FactionManager.OfPlayer.def;
            }

            Pawn pawn = FixedPawnUtility.GenerateFixedPawnWithDef(pawnDef);
            if (def.setPawnToPlayerFaction && pawn.Faction != Find.FactionManager.OfPlayer)
            {
                pawn.SetFaction(Find.FactionManager.OfPlayer);
            }

            //空降
            PawnsArrivalModeDefOf.CenterDrop.Worker.Arrive(new List<Pawn>
                {
                    pawn
                }, new IncidentParms
                {
                    target = map,
                    spawnCenter = DropCellFinder.TradeDropSpot(map)
                });

            //通知
            SuccessMessage(pawn, map);
        }

        protected virtual bool HasEnoughResources(Map map)
        {
            if (def.gachaCost.NullOrEmpty())
            {
                return true;
            }

            foreach (var cost in def.gachaCost)
            {
                int num = (from t in TradeUtility.AllLaunchableThingsForTrade(map)
                           where t.def == cost.thingDef
                           select t).Sum((Thing t) => t.stackCount);

                if (num < cost.count)
                {
                    return false;
                }
            }

            return true;
        }

        protected void ConsumeResources(Map map)
        {
            if (def.gachaCost.NullOrEmpty())
            {
                return;
            }

            foreach (var cost in def.gachaCost)
            {
                TradeUtility.LaunchThingsOfType(cost.thingDef, cost.count, map, null);
            }
        }

        protected virtual void SuccessMessage(Pawn pawn, Map map)
        {
            Messages.Message(def.successMessage.Formatted(pawn.LabelShort), new LookTargets(pawn.Position, map), MessageTypeDefOf.PositiveEvent);

            ChoiceLetter let = LetterMaker.MakeLetter(def.successLetterLabel.Formatted(pawn.LabelShort), def.successLetterText.Formatted(pawn.LabelShort), LetterDefOf.PositiveEvent, pawn, null, null, null);
            Find.LetterStack.ReceiveLetter(let, null, 0, true);
        }

        protected virtual void FailAction(Pawn caller)
        {
            if (def.gachaFailItems.NullOrEmpty())
            {
                return;
            }

            Map map = caller.Map;

            ThingDefCountClass failItem = def.gachaFailItems.RandomElement();

            Thing thing = ThingMaker.MakeThing(failItem.thingDef);
            thing.stackCount = failItem.count;

            IntVec3 intVec = DropCellFinder.TradeDropSpot(map);
            TradeUtility.SpawnDropPod(intVec, map, thing);

            Messages.Message(def.failMessage, new LookTargets(thing.Position, map), MessageTypeDefOf.PositiveEvent);
        }

    }
}
