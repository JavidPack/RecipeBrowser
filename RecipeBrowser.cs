using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace RecipeBrowser
{
	internal class RecipeBrowser : Mod
	{
		internal static RecipeBrowser instance;
		internal static Mod itemChecklistInstance;
		internal ModHotKey ToggleRecipeBrowserHotKey;

		//internal bool CheatSheetLoaded = false;
		internal RecipeBrowserTool recipeBrowserTool;

		private int lastSeenScreenWidth;
		private int lastSeenScreenHeight;
		internal static bool[] chestContentsAvailable = new bool[1000];

		// TODO, Chinese IME support
		public override void Load()
		{
			// Too many people are downloading 0.10 versions on 0.9....
			if (ModLoader.version < new Version(0, 10))
			{
				throw new Exception("\nThis mod uses functionality only present in the latest tModLoader. Please update tModLoader to use this mod\n\n");
			}

			instance = this;
			itemChecklistInstance = ModLoader.GetMod("ItemChecklist");
			if (itemChecklistInstance != null && itemChecklistInstance.Version < new Version(0, 2, 1))
				itemChecklistInstance = null;

			/*
			Mod cheatSheet = ModLoader.GetMod("CheatSheet");
			if (cheatSheet == null)
			{
			*/
			ToggleRecipeBrowserHotKey = RegisterHotKey("Toggle Recipe Browser", "OemCloseBrackets");
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
			}
		}

		public override void Unload()
		{
			instance = null;
			itemChecklistInstance = null;
			LootCache.instance = null;
			ToggleRecipeBrowserHotKey = null;
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
							recipeBrowserTool.UIUpdate();
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
					//Main.NewText($"Complete on {completedChestindex}");
					break;

				default:
					//DebugText("Unknown Message type: " + msgType);
					break;
			}
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
	}
}