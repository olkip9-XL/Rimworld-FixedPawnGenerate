using Fixed_Pawn_Generate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    static class Example
    {
        /*
         * 这是一个手动生成固定角色的例子。
         * 手动生成固定角色的方法，需要先定义一些FixedPawnDef，并将generateRate设置为1。
         * 
         * A Example of how to generate a fixed pawn manually.
         * Before using this method, you need to define some FixedPawnDef, and set the generateRate to 1.
         */
        public static Pawn ExampleGeneratePawnFromPool()
        {
            //构建生成池（卡池）
            //construct pool
            List<FixedPawnDef> list = new List<FixedPawnDef>();

            list.AddRange(DefDatabase<FixedPawnDef>.AllDefsListForReading.FindAll(x => x.defName == "aaa" || x.defName == "bbb"));

            //从池中随机选取一个，包含唯一性检查
            //pick a random one from pool, contain the uniqueness check 
            FixedPawnDef def = FixedPawnUtility.GetRandomFixedPawnDefByWeight(list);

            //生成角色
            //generate pawn
            return FixedPawnUtility.GenerateFixedPawnWithDef(def);
        }

    }
}
