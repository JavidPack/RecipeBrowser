using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

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
		internal ModHotKey ToggleRecipeBrowserHotKey;
		internal ModHotKey QueryHoveredItemHotKey;

		//internal bool CheatSheetLoaded = false;
		internal RecipeBrowserTool recipeBrowserTool;

		private int lastSeenScreenWidth;
		private int lastSeenScreenHeight;
		internal static bool[] chestContentsAvailable = new bool[1000];

		// TODO, Chinese IME support
		public override void Load()
		{
			// Latest uses UIProgress refactors.
			if (ModLoader.version < new Version(0, 11, 3))
			{
				throw new Exception("\nThis mod uses functionality only present in the latest tModLoader. Please update tModLoader to use this mod\n\n");
			}

			instance = this;

			// Remember, this mod is NOT open source, don't steal these TagHandlers.
			ChatManager.Register<TagHandlers.LinkTagHandler>("l", "link");
			ChatManager.Register<TagHandlers.ImageTagHandler>("image");
			//ChatManager.Register<TagHandlers.URLTagHandler>("u", "url");

			FieldInfo translationsField = typeof(Mod).GetField("translations", BindingFlags.Instance | BindingFlags.NonPublic);
			translations = (Dictionary<string, ModTranslation>)translationsField.GetValue(this);

			itemChecklistInstance = ModLoader.GetMod("ItemChecklist");
			if (itemChecklistInstance != null && itemChecklistInstance.Version < new Version(0, 2, 1))
				itemChecklistInstance = null;

			/*
			Mod cheatSheet = ModLoader.GetMod("CheatSheet");
			if (cheatSheet == null)
			{
			*/
			ToggleRecipeBrowserHotKey = RegisterHotKey("Toggle Recipe Browser", "OemCloseBrackets");
			QueryHoveredItemHotKey = RegisterHotKey("Query Hovered Item", "Mouse3");
			/*
				CheatSheetLoaded = false;
			}
			else
			{
				ToggleRecipeBrowserHotKey = null;
				CheatSheetLoaded = true;
			}
			*/
			if (!Main.dedServ /*&& !CheatSheetLoaded*/)
			{
				recipeBrowserTool = new RecipeBrowserTool();
				UIElements.UIRecipeSlot.favoritedBackgroundTexture = GetTexture("Images/FavoritedOverlay");
				UIElements.UIRecipeSlot.selectedBackgroundTexture = GetTexture("Images/SelectedOverlay");
				UIElements.UIRecipeSlot.ableToCraftBackgroundTexture = GetTexture("Images/CanCraftBackground");
				UIElements.UIMockRecipeSlot.ableToCraftBackgroundTexture = GetTexture("Images/CanCraftBackground");
				UIElements.UICheckbox.checkboxTexture = GetTexture("UIElements/checkBox");
				UIElements.UICheckbox.checkmarkTexture = GetTexture("UIElements/checkMark");
				UIHorizontalGrid.moreLeftTexture = GetTexture("UIElements/MoreLeft");
				UIHorizontalGrid.moreRightTexture = GetTexture("UIElements/MoreRight");
			}
		}

		internal static string RBText(string category, string key)
		{
			return translations[$"Mods.RecipeBrowser.{category}.{key}"].GetTranslation(Language.ActiveCulture);
			// This isn't good until after load....
			// return Language.GetTextValue($"Mods.RecipeBrowser.{category}.{key}");
		}

		public override void PreSaveAndQuit()
		{
			RecipeBrowserUI.instance.CloseButtonClicked(null, null);
			RecipeBrowserUI.instance.ShowRecipeBrowser = false;
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

			UIElements.UIRecipeSlot.favoritedBackgroundTexture = null;
			UIElements.UIRecipeSlot.selectedBackgroundTexture = null;
			UIElements.UIRecipeSlot.ableToCraftBackgroundTexture = null;
			UIElements.UIMockRecipeSlot.ableToCraftBackgroundTexture = null;
			UIElements.UICheckbox.checkboxTexture = null;
			UIElements.UICheckbox.checkmarkTexture = null;
			UIHorizontalGrid.moreLeftTexture = null;
			UIHorizontalGrid.moreRightTexture = null;
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

		public override void UpdateUI(GameTime gameTime)
		{
			// By doing these triggers here, we can use them even if autopaused.
			if (Main.netMode == 0 && (Main.playerInventory || Main.npcChatText != "" || Main.player[Main.myPlayer].sign >= 0 || Main.ingameOptionsWindow || Main.inFancyUI) && Main.autoPause)
				Main.LocalPlayer.GetModPlayer<RecipeBrowserPlayer>().ProcessTriggers(null);
			recipeBrowserTool?.UIUpdate(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			//if (CheatSheetLoaded) return;

			int inventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (inventoryLayerIndex != -1)
			{
				layers.Insert(inventoryLayerIndex, new LegacyGameInterfaceLayer(
					"RecipeBrowser: UI",
					delegate
					{
						//if (!CheatSheetLoaded)
						{
							if (lastSeenScreenWidth != Main.screenWidth || lastSeenScreenHeight != Main.screenHeight)
							{
								recipeBrowserTool.ScreenResolutionChanged();
								lastSeenScreenWidth = Main.screenWidth;
								lastSeenScreenHeight = Main.screenHeight;
							}
							recipeBrowserTool.UIDraw();
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
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
							NetMessage.SendData(32, whoAmI, -1, null, chestIndex, (float)i, 0f, 0f, 0, 0, 0);
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
					if (Main.netMode == 2 && !syncPlayer)
					{
						Main.player[player].GetModPlayer<RecipeBrowserPlayer>().SendFavoritedRecipes(-1, player);
					}
					// We will separately maintain other player favorites. Do not set UIRecipeSlot.favorite 
					if (Main.netMode != 2)
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
		internal List<ModCategory> modFilters = new List<ModCategory>();
		public override object Call(params object[] args)
		{
			/*
			Mod RecipeBrowser = ModLoader.GetMod("RecipeBrowser");
			if (RecipeBrowser != null)
			{
				RecipeBrowser.Call("AddItemCategory", "Example", "Weapons", GetTexture("Items/ExampleItem"), (Predicate<Item>)((Item item) => item.type == ItemType("Mundane")));
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
					RecipeBrowser.instance.Logger.Error("RecipeBrowser Call Error: Unknown Message: " + message);
				}
			}
			catch (Exception e)
			{
				RecipeBrowser.instance.Logger.Error("RecipeBrowser Call Error: " + e.StackTrace + e.Message);
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