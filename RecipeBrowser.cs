using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using RecipeBrowser.UIElements;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.ID;

namespace RecipeBrowser
{
	// Magic storage: item checklist support?
	// Loot cache manual reset button. Manually trigger recalculation
	// TODO: Auto favorite items needed for starred recipes. And notify?
	// TODO: Save starred recipes. Also, crafting check off starred last time, look into it.
	// TODO: Hide Items, items not interested in crafting. Only show if query item is that item (so you can still know how to craft if needed in craft chain.)
	// TODO: Star Loot
	// TODO: some sort of banner menu?
	// TODO: Craft in UI for Multiple chests/banks/Magic Storage

	// Multistep craft?
	// Craft in GUI?
	internal class RecipeBrowser : Mod
	{
		internal static RecipeBrowser instance;
		internal static Dictionary<string, ModTranslation> translations; // reference to private field.
		internal static Mod itemChecklistInstance;
		internal ModKeybind ToggleRecipeBrowserHotKey;
		internal ModKeybind QueryHoveredItemHotKey;

		//internal bool CheatSheetLoaded = false;
		internal static bool[] chestContentsAvailable = new bool[1000];

		// TODO, Chinese IME support
		public override void Load()
		{
			// Latest uses UIProgress refactors.
			if (BuildInfo.tMLVersion < new Version(0, 11, 5))
			{
				throw new Exception("\nThis mod uses functionality only present in the latest tModLoader. Please update tModLoader to use this mod\n\n");
			}

			instance = this;

			// Remember, this mod is NOT open source, don't steal these TagHandlers.
			ChatManager.Register<TagHandlers.LinkTagHandler>("l", "link");
			ChatManager.Register<TagHandlers.ImageTagHandler>("image");
			ChatManager.Register<TagHandlers.NPCTagHandler>("npc");
			ChatManager.Register<TagHandlers.ItemHoverFixTagHandler>("itemhover");
			//ChatManager.Register<TagHandlers.URLTagHandler>("u", "url");

			FieldInfo translationsField = typeof(LocalizationLoader).GetField("translations", BindingFlags.Static | BindingFlags.NonPublic);
			translations = (Dictionary<string, ModTranslation>)translationsField.GetValue(this);
			
			if (ModLoader.TryGetMod("ItemChecklist", out itemChecklistInstance) && itemChecklistInstance.Version < new Version(0, 2, 1))
				itemChecklistInstance = null;

			/*
			Mod cheatSheet = ModLoader.GetMod("CheatSheet");
			if (cheatSheet == null)
			{
			*/
			ToggleRecipeBrowserHotKey = KeybindLoader.RegisterKeybind(this, "Toggle Recipe Browser", "OemCloseBrackets");
			QueryHoveredItemHotKey = KeybindLoader.RegisterKeybind(this, "Query Hovered Item", "Mouse3");
			/*
				CheatSheetLoaded = false;
			}
			else
			{
				ToggleRecipeBrowserHotKey = null;
				CheatSheetLoaded = true;
			}
			*/

			Patches.Apply();
		}

		internal static string RBText(string category, string key)
		{
			return translations[$"Mods.RecipeBrowser.{category}.{key}"].GetTranslation(Language.ActiveCulture);
			// This isn't good until after load....
			// return Language.GetTextValue($"Mods.RecipeBrowser.{category}.{key}");
		}

		public override void Unload()
		{
			instance = null;
			translations = null;
			itemChecklistInstance = null;
			LootCache.instance = null;
			ToggleRecipeBrowserHotKey = null;
			QueryHoveredItemHotKey = null;
			RecipeBrowserUI.instance = null;
			RecipeCatalogueUI.instance = null;
			ItemCatalogueUI.instance = null;
			BestiaryUI.instance = null;
			CraftUI.instance = null;
			SharedUI.instance = null;
			RecipePath.Refresh(true);
			RecipeBrowserPlayer.seenTiles = null;

			UIElements.UIRecipeSlot.favoritedBackgroundTexture = null;
			UIElements.UIRecipeSlot.selectedBackgroundTexture = null;
			UIElements.UIRecipeSlot.ableToCraftBackgroundTexture = null;
			UIElements.UIRecipeSlot.ableToCraftExtendedBackgroundTexture = null;
			UIElements.UIMockRecipeSlot.ableToCraftBackgroundTexture = null;
			UIElements.UICheckbox.checkboxTexture = null;
			UIElements.UICheckbox.checkmarkTexture = null;
			UIHorizontalGrid.moreLeftTexture = null;
			UIHorizontalGrid.moreRightTexture = null;
			Utilities.tileTextures = null;
			ArmorSetFeatureHelper.Unload();
			UIItemSlot.hoveredItem = null;
		}

		public override void PostSetupContent()
		{
			if (!Main.dedServ)
			{
				if (itemChecklistInstance != null)
				{
					itemChecklistInstance.Call(
						"RegisterForNewItem",
						(Action<int>)ItemChecklistItemFound
					);
				}
			}
		}

		public override void PostAddRecipes()
		{
			if (!Main.dedServ)
			{
				LootCacheManager.Setup(this);
				RecipeBrowserUI.instance.PostSetupContent();
			}
		}

		private void ItemChecklistItemFound(int type)
		{
			//Main.NewText("RB: new item to add to list " + type);
			RecipeBrowserUI.instance.NewItemFound(type);
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			MessageType msgType = (MessageType)reader.ReadByte();
			switch (msgType)
			{
				case MessageType.SilentRequestChestContents:
					int chestIndex = reader.ReadInt32();
					if (chestIndex > -1)
					{
						for (int i = 0; i < 40; i++)
						{
							NetMessage.SendData(MessageID.SyncChestItem, whoAmI, -1, null, chestIndex, (float)i, 0f, 0f, 0, 0, 0);
						}
						var message = GetPacket();
						message.Write((byte)MessageType.SilentSendChestContentsComplete);
						message.Write(chestIndex);
						message.Send(whoAmI);
					}
					break;

				case MessageType.SilentSendChestContentsComplete:
					int completedChestindex = reader.ReadInt32();
					chestContentsAvailable[completedChestindex] = true;
					RecipeCatalogueUI.instance.updateNeeded = true;
					ItemCatalogueUI.instance.updateNeeded = true;
					//Main.NewText($"Complete on {completedChestindex}");
					break;

				case MessageType.SendFavoritedRecipes:
					byte player = reader.ReadByte();
					bool syncPlayer = reader.ReadBoolean();
					var r = Main.player[player].GetModPlayer<RecipeBrowserPlayer>().favoritedRecipes;
					r.Clear();
					int count = reader.ReadInt32();
					for (int i = 0; i < count; i++)
					{
						r.Add(reader.ReadInt32());
					}
					//Main.NewText($"Player {player} now has: " + string.Join(",", r.ToArray()));
					//Console.WriteLine($"Player {player} now has: " + string.Join(",", r.ToArray()));
					if (Main.netMode == NetmodeID.Server && !syncPlayer)
					{
						Main.player[player].GetModPlayer<RecipeBrowserPlayer>().SendFavoritedRecipes(-1, player);
					}
					// We will separately maintain other player favorites. Do not set UIRecipeSlot.favorite 
					if (Main.netMode != NetmodeID.Server)
					{
						RecipeBrowserUI.instance.favoritePanelUpdateNeeded = true;
					}
					break;

				default:
					//DebugText("Unknown Message type: " + msgType);
					break;
			}
		}

		// Messages:
		// string:"AddItemCategory" - string:SortName - string:Parent - Texture2D:Icon - Predicate<Item>:belongs
		internal List<ModCategory> modCategories = new List<ModCategory>();
		internal List<ModCategory> modFilters = new List<ModCategory>(); // TODO: BannerBonanza tmod bug and unload this.
		public override object Call(params object[] args)
		{
			/*
			if (ModLoader.TryGetMod("RecipeBrowser", out Mod recipeBrowser))
			{
				recipeBrowser.Call("AddItemCategory", "Example", "Weapons", GetTexture("Items/ExampleItem"), (Predicate<Item>)((Item item) => item.type == ItemType("Mundane")));
			}
			*/
			try
			{
				string message = args[0] as string;
				if (message == "AddItemCategory")
				{
					string sortName = args[1] as string;
					string parentName = args[2] as string;
					Texture2D icon = args[3] as Texture2D;
					Predicate<Item> belongs = args[4] as Predicate<Item>;
					if (!Main.dedServ)
						modCategories.Add(new ModCategory(sortName, parentName, icon, belongs));
					//modCategories.Add(new ModCategory(sortName, parentName, icon, (Item item) => item.type == ItemType("Mundane")));
					return "Success";
				}
				else if (message == "AddItemFilter")
				{
					string sortName = args[1] as string;
					string parentName = args[2] as string;
					Texture2D icon = args[3] as Texture2D;
					Predicate<Item> belongs = args[4] as Predicate<Item>;
					if (!Main.dedServ)
						modFilters.Add(new ModCategory(sortName, parentName, icon, belongs));
					//modCategories.Add(new ModCategory(sortName, parentName, icon, (Item item) => item.type == ItemType("Mundane")));
					return "Success";
				}
				else
				{
					Logger.Error("Call Error: Unknown Message: " + message);
				}
			}
			catch (Exception e)
			{
				Logger.Error("Call Error: " + e.StackTrace + e.Message);
			}
			return "Failure";
		}

		public override void AddRecipes()
		{
			// Test crafting station display
			//var recipe = new ModRecipe(this);
			//recipe.AddIngredient(Terraria.ID.ItemID.BlueBerries, 20);
			//recipe.AddTile(Terraria.ID.TileID.WorkBenches);
			//recipe.AddTile(Terraria.ID.TileID.Chairs);
			//recipe.needWater = true;
			//recipe.SetResult(Terraria.ID.ItemID.PumpkinPie, 2);
			//recipe.AddRecipe();
		}
	}

	//static class Extensions
	//{
	//	public static void MultiplyColorsByAlpha(this Texture2D texture)
	//	{
	//		Color[] data = new Color[texture.Width * texture.Height];
	//		texture.GetData(data);
	//		for (int i = 0; i < data.Length; i++)
	//		{
	//			Vector4 we = data[i].ToVector4();
	//			data[i] = new Color(we.X * we.W, we.Y * we.W, we.Z * we.W, we.W);
	//		}
	//		texture.SetData(data);
	//	}
	//}

	internal enum MessageType : byte
	{
		/// <summary>
		/// Vanilla client sends 31 to server, getting 32s, 33, and 80 in response, also claiming the chest open.
		/// We don't want that, so we'll do an alternate version of that
		/// 32 for each item -- Want
		/// We don't want 33 -- Syncs name, make noise.
		/// We don't want 80 -- informs others that the chest is open
		/// </summary>
		SilentRequestChestContents,

		/// <summary>
		/// Once the 40 items are sent, send this packet so we don't have to wait anymore
		/// </summary>
		SilentSendChestContentsComplete,

		/// <summary>
		/// Sends player.whoami, count, and list of recipeindexs. Sent in SyncPlayer and SendClientChanges
		/// </summary>
		SendFavoritedRecipes
	}
}