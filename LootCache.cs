using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReLogic.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RecipeBrowser
{
	public class LootCache
	{
		public static LootCache instance;

		// indexed by itemid, value is list of npcs.
		public Dictionary<int, List<int>> lootInfos;

		public LootCache()
		{
			lootInfos = new Dictionary<int, List<int>>();
		}
	}

	internal static class LootCacheManager
	{
		internal static void Setup(Mod recipeBrowserMod)
		{
			LootCache.instance = new LootCache();
			for (int i = -65; i < NPCLoader.NPCCount; i++) {
				// Is npcid 0 a problem?
				// Here we ignore global rules
				List<IItemDropRule> dropRules = Main.ItemDropsDB.GetRulesForNPCID(i, false);

				List<DropRateInfo> list = new List<DropRateInfo>();
				DropRateInfoChainFeed ratesInfo = new DropRateInfoChainFeed(1f);
				foreach (IItemDropRule item in dropRules) {
					item.ReportDroprates(list, ratesInfo);
				}

				foreach (DropRateInfo dropRateInfo in list) {
					List<int> npcThatDropThisItem;
					if (!LootCache.instance.lootInfos.TryGetValue(dropRateInfo.itemId, out npcThatDropThisItem))
						LootCache.instance.lootInfos.Add(dropRateInfo.itemId, npcThatDropThisItem = new List<int>());
					if (!npcThatDropThisItem.Contains(i)) {
						npcThatDropThisItem.Add(i); // Maybe change this to HashSet. 
					}
				}
			}

			// TODO: What should we do with globals? Maybe add an NPC icon indicating any NPC can drop the item?
			// var globals = new GlobalLoot(Main.ItemDropsDB).Get();

			return;
		}
	}
}
