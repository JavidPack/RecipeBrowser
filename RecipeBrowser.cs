using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;

namespace RecipeBrowser
{
	class RecipeBrowser : Mod
	{
		internal static RecipeBrowser instance;
		internal ModHotKey ToggleRecipeBrowserHotKey;
		//internal bool CheatSheetLoaded = false;
		internal RecipeBrowserTool recipeBrowserTool;
		int lastSeenScreenWidth;
		int lastSeenScreenHeight;
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

			/*
			Mod cheatSheet = ModLoader.GetMod("CheatSheet");
			if (cheatSheet == null)
			{
				ToggleRecipeBrowserHotKey = RegisterHotKey("Toggle Recipe Browser", "OemCloseBrackets");
				CheatSheetLoaded = false;
			}
			else
			{
				ToggleRecipeBrowserHotKey = null;
				CheatSheetLoaded = true;
			}

			if (!Main.dedServ && !CheatSheetLoaded)
			{
				recipeBrowserTool = new RecipeBrowserTool();
			}
			*/
		}

		public override void PostSetupContent()
		{
			if (!Main.dedServ)
			{
				//var itemChecklist = ModLoader.GetMod("ItemChecklist");
				//if (itemChecklist != null)
				//{
				//	itemChecklist.Call(
				//		"RegisterForNewItem",
				//		(Action<int>)ItemChecklistItemFound
				//	);
				//}
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
					RecipeBrowserUI.instance.updateNeeded = true;
					//Main.NewText($"Complete on {completedChestindex}");
					break;
				default:
					//DebugText("Unknown Message type: " + msgType);
					break;
			}
		}
	}

	enum MessageType : byte
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
