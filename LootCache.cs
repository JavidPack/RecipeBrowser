using Newtonsoft.Json;
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
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RecipeBrowser
{
	public class LootCache
	{
		[JsonIgnore]
		public static LootCache instance;

		public Version recipeBrowserVersion;

		//public int iterations;
		public long lastUpdateTime;

		public bool calculationCancelled;

		//public List<Tuple<string, Version>> cachedMods; // Dictionary better?
		public Dictionary<string, Version> cachedMods;

		public Dictionary<JSONItem, List<JSONNPC>> lootInfos;

		public LootCache()
		{
			//cachedMods = new List<Tuple<string, Version>>();
			cachedMods = new Dictionary<string, Version>();
			lootInfos = new Dictionary<JSONItem, List<JSONNPC>>();
		}
	}

	//public class ModRecord
	//{
	//	public string modname;
	//	public Version version;
	//}

	//public class LootInfo
	//{
	//	public List<JSONNPC> npcsthatdropme;
	//}

	[DebuggerDisplay("Mod = {mod}, Name = {name}, ID = {id}")]
	public class JSONNPC
	{
		public string mod;
		public string name;
		public int id;

		public JSONNPC(string mod, string name, int id)
		{
			this.mod = mod;
			this.name = name;
			this.id = id;
			if (id != 0)
			{
				this.mod = "Terraria";
			}
		}

		// We only want to serialize id when name is null, meaning it's a vanilla npc. ModNPC have a guaranteed (ModName, Name) uniqueness. Name for vanilla is just convenience for editing json manually.
		public bool ShouldSerializeid()
		{
			return mod == "Terraria";
		}

		internal int GetID()
		{
			if (id != 0) return id;

			if (ModLoader.TryGetMod(this.mod, out Mod mod))
				return mod.GetContent<ModNPC>().FirstOrDefault(npc => npc.Name == this.name)?.Type ?? 0;

			return 0;
		}
	}

	/*
	[TypeConverter(typeof(JSONItemConverter))]
	public class JSONItem2
	{
		public string mod;
		public string name;

		public JSONItem2(string mod, string name)
		{
			this.mod = mod;
			this.name = name;
		}

		public JSONItem2(string mod, int id)
		{
			this.mod = mod;
			if (mod == "Terraria")
				name = ItemID.Search.GetName(id);
			else
			{
				name = NPCLoader.GetNPC(id).Name;
			}
		}


		public JSONItem2(int id)
		{
			if (id < ItemID.Count)
			{
				mod = "Terraria";
				name = ItemID.Search.GetName(id);
			}
			else
			{
				mod = NPCLoader.GetNPC(id).Mod.Name;
				name = NPCLoader.GetNPC(id).Name;
			}
		}

		public override bool Equals(object obj)
		{
			JSONItem p = obj as JSONItem;
			if (p == null)
			{
				return false;
			}
			return (mod == p.mod) && (name == p.name);
		}

		public override int GetHashCode()
		{
			return new { mod, name }.GetHashCode();
		}

		internal bool IsAvailable()
		{
			return true;
		}

		internal int GetID()
		{
			//IdDictionary Search = IdDictionary.Create<NPCID, short>();

			if (mod == "Terraria")
			{
				if (ItemID.Search.ContainsName(name))
					return ItemID.Search.GetId(name);
				return 0;
			}

			if (ModLoader.TryGetMod(this.mod, out Mod mod))
				return mod.GetContent<ModItem>().FirstOrDefault(item => item.Name == this.name)?.Type ?? 0;

			return 0;
		}
	}
	*/

	[TypeConverter(typeof(JSONItemConverter))]
	public class JSONItem
	{
		public string mod;
		public string name;
		public int id;

		public JSONItem(string mod, string name, int id)
		{
			this.mod = mod;
			this.name = name;
			this.id = id;
			if (id != 0)
			{
				this.mod = "Terraria";
			}
		}

		// We only want to serialize id when name is null, meaning it's a vanilla npc. ModNPC have a guaranteed (ModName, Name) uniqueness. Name for vanilla is just convinience for editing json manually.
		public bool ShouldSerializeid()
		{
			return mod == "Terraria";
		}

		public override bool Equals(object obj)
		{
			JSONItem p = obj as JSONItem;
			if (p == null)
			{
				return false;
			}
			return (mod == p.mod) && (name == p.name);
		}

		public override int GetHashCode()
		{
			return new { mod, name }.GetHashCode();
		}

		internal int GetID()
		{
			if (id != 0) return id;

			if (ModLoader.TryGetMod(this.mod, out Mod mod))
				return mod.GetContent<ModItem>().FirstOrDefault(item => item.Name == this.name)?.Type ?? 0;

			return 0;
		}
	}

	internal class JSONItemConverter : TypeConverter
	{
		// Overrides the CanConvertFrom method of TypeConverter.
		// The ITypeDescriptorContext interface provides the context for the
		// conversion. Typically, this interface is used at design time to
		// provide information about the design-time container.
		public override bool CanConvertFrom(ITypeDescriptorContext context,
		   Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		// Overrides the ConvertFrom method of TypeConverter.
		public override object ConvertFrom(ITypeDescriptorContext context,
		   CultureInfo culture, object value)
		{
			if (value is string)
			{
				string[] v = ((string)value).Split('\t');
				if (v[0] == "Terraria")
				{
					return new JSONItem(v[0], v[1], int.Parse(v[2]));
				}
				return new JSONItem(v[0], v[1], 0);
			}
			return base.ConvertFrom(context, culture, value);
		}

		// Overrides the ConvertTo method of TypeConverter.
		public override object ConvertTo(ITypeDescriptorContext context,
		   CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				JSONItem item = (JSONItem)value;
				if (item.mod == "Terraria")
				{
					return $"{item.mod}\t{item.name}\t{item.id}";
				}
				return $"{item.mod}\t{item.name}";
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	internal static class LootCacheManager
	{
		internal static bool LootCacheManagerActive;

		internal static void Setup(Mod recipeBrowserMod)
		{
			// Save format:
			/*
			 * .. hmm, vanilla item drops...do I have to recalculate each time? New Set of mods => complete refresh?
			 *
			 * RecipeBrowserVersion // So if we add more features we can ignore this file
			 * [ of Mod Info
			 * -> ModName
			 * -> Version
			 * -> [ of npc drop info? or item info?
			 * -->  Tuple<JSONNPC, LootItem[]> or
			 * -->  JSONNPC<T>
			 * -->
			 * ---> OR
			 * --> Item Info
			 * ---> ["NPCInfo" with droprate]
			 * -->
			 * -> ]
			 * ]
			 *
			 * hmm, instead of mod info, just a list of npc? just a list of Item?
			 *
			 * <Mod, Version>[] --> For knowing which have updated....for now, any update or unaccounted mod, recalculate allll
			 * ItemInfo[] JSONNPC and array of JSONNPC that drop it. (forget droprates for now?)
			 *
			 *
			 * item to npc or npc to item....
			 */
			string json;
			string filename = "LootCache.json";
			string folder = Path.Combine(Main.SavePath, "Mods", "Cache");
			string path = Path.Combine(folder, filename);
			LootCache li = null;
			bool needsRecalculate = true;
			LootCacheManagerActive = true;
			List<string> modsThatNeedRecalculate = new List<string>();
			if (File.Exists(path))
			{
				using (StreamReader r = new StreamReader(path))
				{
					json = r.ReadToEnd();
					try {
						li = JsonConvert.DeserializeObject<LootCache>(json, new JsonSerializerSettings { Converters = { new Newtonsoft.Json.Converters.VersionConverter() } });
						needsRecalculate = false;
					}
					catch (Exception e) {
						RecipeBrowser.instance.Logger.Error("Deserialize LootCache.json failed");
					}
				}
			}
			if (li == null) // Investigate why some people get LootCache.json with only 0s in it.
				li = new LootCache();
			LootCache.instance = li;
			return; // TODO: investigate newtonsoft dictionary regression

			// New Recipe Browser version, assume total reset needed (adjust this logic next update.)
			if (li.recipeBrowserVersion != recipeBrowserMod.Version || li.calculationCancelled)
			{
				li.lootInfos.Clear();
				li.cachedMods.Clear();
				li.recipeBrowserVersion = recipeBrowserMod.Version;
				needsRecalculate = true;
			}

			// If we aren't up to date on each mod...
			foreach (var m in ModLoader.Mods)
			{
				if (/*mod == "ModLoader" || */m.Name == "RecipeBrowser")
					continue;
				string modName = m.Name == "ModLoader" ? "Terraria" : m.Name;
				if (!li.cachedMods.Any(x => x.Key == modName && x.Value == m.Version)) // if this mod is either updated or doesn't exist yet
																					   //if (li.cachedMods.ContainsKey(modName) && li.cachedMods[modName] == m.Version)
				{
					needsRecalculate = true;
					// Remove mod from list
					li.cachedMods.Remove(modName);
					// Remove items from this mod
					var toRemove = li.lootInfos.Where(pair => pair.Key.mod == modName) // Can't detect if a vanilla npc dropping a vanilla is because of mod. It's fine
					 .Select(pair => pair.Key)
					 .ToList();
					foreach (var key in toRemove)
					{
						li.lootInfos.Remove(key);
					}
					// Remove npc from items.
					foreach (var itemToNPCs in li.lootInfos)
					{
						itemToNPCs.Value.RemoveAll(x => x.mod == modName);
					}
					modsThatNeedRecalculate.Add(modName);
					li.cachedMods[modName] = m.Version; // (new Tuple<string, Version>(modName, m.Version));
				}
			}
			//li.cachedMods.Add(new Tuple<string, Version>(m.Name, m.Version));

			if (needsRecalculate)
			{
				// Temp variables
				float soundVolume = Main.soundVolume;
				Main.soundVolume = 0f;
				if (!Main.dedServ)
				{
					try {
						Reflect();
					}
					catch {
					}
				}
				setLoadProgressText?.Invoke("Recipe Browser: Rebuilding Loot Cache (Hold shift to skip if stuck)");
				setLoadProgressProgress?.Invoke(0f);

				// expert drops?
				for (int playernum = 0; playernum < 256; playernum++)
				{
					Main.player[playernum] = new Player();
				}
				//Main.player[0].active = true;

				// Fix Terraria Overhaul bug
				if(Main.maxTilesY < 600 || Main.maxTilesX < 2100)
				{
					Main.maxTilesX = 8400;
					Main.maxTilesY = 2400;
					Main.tile = new Tile[Main.maxTilesX + 1, Main.maxTilesY + 1];
				}

				int oldMx = Main.maxTilesX;
				Main.maxTilesX = 2100;
				int oldMy = Main.maxTilesY;
				Main.maxTilesY = 600;
				for (int x = 0; x < Main.maxTilesX; x++)
				{
					for (int y = 0; y < Main.maxTilesY; y++)
					{
						Main.tile[x, y] = new Tile();
						Main.tile[x, y].type = 0;
						if (y > Main.maxTilesY * 0.3f)
							Main.tile[x, y].IsActive = true;
					}
				}
				Main.worldSurface = 200;
				//Main.netMode = 1; // hope this doesn't do anything weird
				NPC npc = new NPC();
				Item item = new Item();
				loots = new HashSet<int>();
				string lastMod = "";
				var watch = Stopwatch.StartNew();
				var oldRand = Main.rand;
				if (Main.rand == null)
					Main.rand = new Terraria.Utilities.UnifiedRandom();

				bool cancelled = false;
				for (int i = 1; i < NPCLoader.NPCCount; i++) // for every npc...
				{
					
					npc.SetDefaults(i);
					npc.value = 0; // Causes some drops to be missed, why is this here?
					string currentMod = npc.ModNPC?.Mod.Name ?? "Terraria";
					if (!modsThatNeedRecalculate.Contains(currentMod))
						continue;
					if (lastMod != currentMod)
					{
						lastMod = currentMod;
						setLoadSubProgressText?.Invoke(lastMod);
					}
					setLoadProgressProgress?.Invoke((float)i / NPCLoader.NPCCount);
					JSONNPC jsonNPC = new JSONNPC(npc.ModNPC?.Mod.Name ?? "Terraria", npc.ModNPC?.Name ?? npc.TypeName, npc.ModNPC != null ? 0 : i);

					loots.Clear();
					cancelled = CalculateLoot(npc);  // ...calculate drops

					foreach (var loot in loots)
					{
						if (ignoreItemIDS.Contains(loot))
							continue;

						item.SetDefaults(loot, true);

						//JSONItem jsonitem = new JSONItem(item.modItem?.mod.Name ?? "Terraria", Lang.GetItemNameValue(loot), item.modItem != null ? 0 : loot);
						JSONItem jsonitem = new JSONItem(item.ModItem?.Mod.Name ?? "Terraria", item.ModItem?.Name ?? Lang.GetItemNameValue(loot), item.ModItem != null ? 0 : loot);
						List<JSONNPC> npcsthatdropme;
						if (!li.lootInfos.TryGetValue(jsonitem, out npcsthatdropme))
							li.lootInfos.Add(jsonitem, npcsthatdropme = new List<JSONNPC>());
						npcsthatdropme.Add(jsonNPC);
					}
					if (cancelled)
						break;
				}
				loots.Clear();
				// Reset temp values
				Main.rand = oldRand; // value 8 seconds.  // don't value to 0 and ignore.contains: 5 seconds.
									 // value to 0, contains, 4 seconds.   .6 seconds without contains.
				Main.maxTilesX = oldMx;
				Main.maxTilesY = oldMy;
				Main.soundVolume = soundVolume;

				// Save json:
				watch.Stop();
				var elapsedMs = watch.ElapsedMilliseconds;
				//li.iterations = MaxNumberLootExperiments;
				li.lastUpdateTime = elapsedMs;
				li.calculationCancelled = cancelled;
				Directory.CreateDirectory(folder);
				json = JsonConvert.SerializeObject(li, Formatting.Indented, new JsonSerializerSettings { Converters = { new Newtonsoft.Json.Converters.VersionConverter() } });
				File.WriteAllText(path, json);

				// Reset Load Mods Progress bar
				setLoadSubProgressText?.Invoke("");
				setLoadProgressText?.Invoke("Adding Recipes");
				setLoadProgressProgress?.Invoke(0f);
			}
			LootCacheManagerActive = false;
			LootCache.instance = li;
		}

		private static Action<string> setLoadProgressText;
		private static Action<float> setLoadProgressProgress;
		private static Action<string> setLoadSubProgressText;

		private static void Reflect()
		{
			Assembly assembly = Assembly.GetAssembly(typeof(Mod));
			// TODO, return to old float and string.
			
			var type = assembly.GetType("Terraria.ModLoader.UI.Interface");
			FieldInfo loadModsField = type.GetField("loadMods", BindingFlags.Static | BindingFlags.NonPublic);
			var loadModsValue = loadModsField.GetValue(null);

			Type UILoadModsType = assembly.GetType("Terraria.ModLoader.UI.UILoadMods");

			MethodInfo SetLoadStageMethod = UILoadModsType.GetMethod("SetLoadStage", BindingFlags.Instance | BindingFlags.Public);
			PropertyInfo ProgressProperty = UILoadModsType.GetProperty("Progress", BindingFlags.Instance | BindingFlags.Public);
			PropertyInfo SubProgressTextProperty = UILoadModsType.GetProperty("SubProgressText", BindingFlags.Instance | BindingFlags.Public);

			setLoadProgressText = (string s) => SetLoadStageMethod.Invoke(loadModsValue, new object[] { s, -1 });
			setLoadProgressProgress = (float f) => ProgressProperty.SetValue(loadModsValue, f );
			setLoadSubProgressText = (string s) => SubProgressTextProperty.SetValue(loadModsValue, s);
		}

		private static int[] ignoreItemIDS = { ItemID.Heart, 1734, 1867, 184, 1735, 1868, ItemID.CopperCoin, ItemID.CopperCoin, ItemID.SilverCoin, ItemID.GoldCoin, ItemID.PlatinumCoin };

		public const int MaxNumberLootExperiments = 5000;
		internal static HashSet<int> loots;

		internal static bool CalculateLoot(NPC npc)
		{
			bool cancelled = false;
			if (npc.type == NPCID.WallofFlesh) Main.hardMode = true;//return;
																	// Hmmmmmm, start hardmode code might overwrite world....
			npc.Center = new Microsoft.Xna.Framework.Vector2(1000, 1000);
			int iterationsWithNoChange = 0;

			var realRandom = Main.rand;
			var fakeRandom = new LootUnifiedRandom();
			fakeRandom.realRandom = realRandom;

			for (int i = 0; i < MaxNumberLootExperiments; i++)
			{
				if (Main.keyState.PressingShift()) {
					RecipeBrowser.instance.Logger.Error($"LootCache calculation cancelled. NPCID {npc.type}/{NPCLoader.NPCCount}, Source Mod: {npc.ModNPC?.Mod.Name ?? "Terraria"}, Name: {Lang.GetNPCNameValue(npc.type)}, Step: {i}/{MaxNumberLootExperiments}");
					cancelled = true;
					break;
				}
				if (i == 0)
					Main.rand = fakeRandom;
				if (i == 50)
					Main.rand = realRandom;
				try
				{
					LootUnifiedRandom.loop = i;
					npc.NPCLoot();
				}
				catch
				{
				}
				npc.active = false;
				bool anyNew = false;
				//if(Main.item[400].active || Main.item[399].active)
				//{
				//	Console.WriteLine();
				//}
				foreach (var item in Main.item)
				{
					if (item.active)
					{
						//if (ignoreItemIDS.Contains(item.type))  // npc.value = 0;
						//	continue;
						loots.Add(item.type); // hmm, Item.NewItem reverseLookup?
						item.active = false;
						anyNew = true;
						//if (iterationsWithNoChange > 150)
						//	Debug.WriteLine($"{i}: {iterationsWithNoChange} {item.Name}");
					}
					else
					{
						break;
					}
				}
				if (anyNew)
					iterationsWithNoChange = 0;
				else
					iterationsWithNoChange++;

				if (iterationsWithNoChange > 250)
					break;
			}
			//}
			Main.hardMode = false;
			return cancelled;
		}
	}

	public class RecipeBrowserGlobalNPC : GlobalNPC
	{
		public override bool PreKill(NPC npc)
		{
			if (LootCacheManager.LootCacheManagerActive)
				((List<int>)(NPCLoader.blockLoot)).AddRange(LootCacheManager.loots);
			return true;
		}
	}
}
