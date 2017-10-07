using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Utilities;
using Terraria.ModLoader;
using Terraria;

namespace RecipeBrowser
{
    internal class LootInfo
    {
        internal int minValue;
        internal int maxValue;
        internal int value;
        internal Dictionary<int, Item> items = new Dictionary<int, Item>();
        internal LootInfo(int minValue, int maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.value = minValue;
        }
        internal void SetItem(Item item)
        {
            if (!items.ContainsKey(value))
            {
                items.Add(value, item);
            }
            else if (items[value].type != item.type)
            {
                //アイテムが異なるケースがある場合、対処が必要
                //If there are cases where items are different, action is required
            }
        }
    }

    internal class LootInfoList
    {
        internal List<LootInfo> list = new List<LootInfo>();
        internal LootInfo this[int index] { get { return list[index]; } }
        internal int Count { get { return list.Count; } }
        internal LootInfo Add(int minValue, int maxValue)
        {
            LootInfo result = new LootInfo(minValue, maxValue);
            list.Add(result);
            return result;
        }
        internal void Increment()
        {
            for (int i = list.Count - 1; 0 <= i; i--)
            {
                var lootInfo = list[i];
                if (lootInfo.value < lootInfo.maxValue)
                {
                    ++lootInfo.value;
                    break;
                }
                else
                {
                    lootInfo.value = lootInfo.minValue;
                }
            }
        }
    }

    internal class LootUnifiedRandom : UnifiedRandom
    {
        internal static List<LootInfoList> list = new List<LootInfoList>();
        internal static LootInfo lastLootInfo;
        internal static int count;
        internal static int index;
        internal static int indexSub;
        private static bool isSetItem;

        internal static void NextLoop(int val)
        {
            if (val == 0)
            {
                list.Clear();
                lastLootInfo = null;
                isSetItem = false;
            }
            count = val;
            index = 0;
            indexSub = 0;
        }

        public int Set(int minValue, int maxValue)
        {
            int result = -1;
            if (count == 0 || (0 < count && (list.Count <= index)))
            {
                LootInfoList listItem;
                if (isSetItem || lastLootInfo == null)
                {
                    listItem = new LootInfoList();
                    list.Add(listItem);
                    isSetItem = false;
                }
                else
                {
                    listItem = list[index];
                }
                lastLootInfo = listItem.Add(minValue, maxValue);
                index = list.Count - 1;
                indexSub = list[index].Count - 1;
                result = minValue;
            }
            else
            {
                for (; index < list.Count; index++)
                {
                    lastLootInfo = list[index][indexSub];
                    if (lastLootInfo.minValue == minValue && lastLootInfo.maxValue == maxValue)
                    {
                        if (indexSub == 0)
                        {
                            list[index].Increment();
                        }
                        result = lastLootInfo.value;

                        ++indexSub;
                        if (list[index].Count <= indexSub)
                        {
                            indexSub = 0;
                            ++index;
                        }
                        break;
                    }
                }
                if (result < 0)
                {
                    var listItem = new LootInfoList();
                    list.Add(listItem);
                    index = list.Count - 1;
                    indexSub = 0;
                    lastLootInfo = list[index].Add(minValue, maxValue);
                    result = 0;
                }
            }
            return result;
        }

        public override int Next(int maxValue)
        {
            //var st = new System.Diagnostics.StackTrace(new System.Diagnostics.StackFrame(1, false));
            //if (!st.GetFrame(0).GetMethod().Name.Equals("NPCLoot"))
            //    return base.Next();
            return Set(0, maxValue);
        }
        public override int Next(int minValue, int maxValue)
        {
            //var st = new System.Diagnostics.StackTrace(new System.Diagnostics.StackFrame(1, false));
            //if (!st.GetFrame(0).GetMethod().Name.Equals("NPCLoot"))
            //    return base.Next(minValue, maxValue);
            return Set(minValue, maxValue);
        }

        public static void SetItem(Item item)
        {
            if (LootUnifiedRandom.lastLootInfo != null)
            {
                LootUnifiedRandom.lastLootInfo.SetItem(item);
            }
            LootUnifiedRandom.isSetItem = true;
        }
    }

    internal class LootGlobalItem : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            if (Main.rand is LootUnifiedRandom)
            {
                LootUnifiedRandom.SetItem(item);
            }
        }
    }
}
