using Microsoft.Xna.Framework;
using RecipeBrowser.UIElements;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace RecipeBrowser
{
	// Helper class for saving and loading recipes.
	// During load, recipes that no longer match ingredients or tiles will be forgotten.
	internal static class RecipeIO
	{
		// Unused slim TagCompound for just the definition of an item.
		//public static TagCompound SaveItemDefinition(Item item)
		//{
		//	var tag = new TagCompound();
		//	if (item.type <= 0)
		//		return tag;

		//	if (item.modItem == null)
		//	{
		//		tag.Set("mod", "Terraria");
		//		tag.Set("id", item.netID);
		//	}
		//	else
		//	{
		//		tag.Set("mod", item.modItem.mod.Name);
		//		tag.Set("name", item.modItem.Name);
		//		tag.Set("data", item.modItem.Save());
		//	}

		//	return tag;
		//}

		// Necessary? Tile needed?
		public static TagCompound Save(Recipe recipe)
		{
			return new TagCompound
			{
				["createItem"] = ItemIO.Save(recipe.createItem),
				["requiredItem"] = recipe.requiredItem.Where(x => !x.IsAir).Select(ItemIO.Save).ToList(),
			};
		}

		// Returns -1 for missing recipe, index of recipe otherwise
		public static int Load(TagCompound tag)
		{
			Item createItem = ItemIO.Load(tag.Get<TagCompound>("createItem"));
			List<Item> requiredItems = tag.GetList<TagCompound>("requiredItem").Select(ItemIO.Load).ToList();
			for (int i = 0; i < Recipe.numRecipes; i++)
			{
				Recipe recipe = Main.recipe[i];
				if (recipe.createItem.type == createItem.type)
				{
					HashSet<int> tagIngredients = new HashSet<int>(requiredItems.Where(x => !x.IsAir).Select(x => x.type));
					HashSet<int> recipeIngredients = new HashSet<int>(recipe.requiredItem.Where(x => !x.IsAir).Select(x => x.type));
					if (tagIngredients.SetEquals(recipeIngredients))
						return i;
				}
			}
			return -1;
		}
	}

	internal class RecipeBrowserPlayer : ModPlayer
	{
		internal List<int> favoritedRecipes;
		// For now, reset on enter world. Could remember later if needed.
		static internal bool[] seenTiles;
		// TODO: Remember hitNPCs? Implement just like seenTiles and reset each session?

		public override void Initialize()
		{
			favoritedRecipes = new List<int>();
		}

		public override void SaveData(TagCompound tag)
		{
			tag["StarredRecipes"] = favoritedRecipes.Select(x => RecipeIO.Save(Main.recipe[x])).ToList();
		}

		public override void LoadData(TagCompound tag)
		{
			favoritedRecipes = tag.GetList<TagCompound>("StarredRecipes").Select(RecipeIO.Load).Where(x => x > -1).ToList();
		}

		// Only happens on local client/SP
		public override void OnEnterWorld()
		{
			seenTiles = new bool[TileLoader.TileCount];
			Point center = Player.Center.ToTileCoordinates();
			for (int i = center.X - 100; i < center.X + 100; i++)
			{
				for (int j = center.Y - 100; j < center.Y + 100; j++)
				{
					if (WorldGen.InWorld(i, j) && Main.tile[i, j] != null && !seenTiles[Main.tile[i, j].TileType]) {
						int Tile = Main.tile[i, j].TileType;
						List<int> adjTiles = Utilities.PopulateAdjTilesForTile(Tile);
						foreach (var tile in adjTiles) {
							seenTiles[tile] = true;
						}
					}
				}
			}
			// All crafting tile items in inventory also count as "seen"
			foreach (var item in Player.inventory) {
				if (item.active) {
					RecipeBrowserUI.instance.ItemReceived(item);
				}
			}
			RecipeBrowserUI.instance.favoritePanelUpdateNeeded = true;
			RecipeBrowserUI.instance.ShowFavoritePanel = favoritedRecipes.Count > 0 && RecipeBrowserUI.instance.HideUnlessInventoryToggle.CurrentState == 0;
			RecipeCatalogueUI.instance.updateNeeded = true;
			if (RecipeCatalogueUI.instance.recipeSlots.Count > 0)
			{
				RecipeBrowserUI.instance.UpdateFavoritedPanel();
			}
			SharedUI.instance.updateNeeded = true; // Added for creative mode filter hiding.
		}

		// Called on other clients when a player leaves.
		public override void PlayerDisconnect()
		{
			// When a player leaves, trigger an update to get rid of Favorited Recipe entries.
			if(!Main.dedServ)
				RecipeBrowserUI.instance.favoritePanelUpdateNeeded = true;
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			SendFavoritedRecipes(toWho, fromWho, true);
		}

		public override void CopyClientState(ModPlayer clientClone)
		{
			RecipeBrowserPlayer clone = clientClone as RecipeBrowserPlayer;
			clone.favoritedRecipes.Clear();
			clone.favoritedRecipes.AddRange(favoritedRecipes);
		}

		public override void SendClientChanges(ModPlayer clientPlayer)
		{
			RecipeBrowserPlayer clone = clientPlayer as RecipeBrowserPlayer;
			if (!favoritedRecipes.SequenceEqual(clone.favoritedRecipes))
			{
				SendFavoritedRecipes(-1, Player.whoAmI);
			}
		}

		public void SendFavoritedRecipes(int toWho, int fromWho, bool syncPlayer = false)
		{
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)MessageType.SendFavoritedRecipes);
			packet.Write((byte)Player.whoAmI);
			packet.Write((bool)syncPlayer); // prevents duplicate sends when normal syncPlayer is happening.
			packet.Write(favoritedRecipes.Count);
			foreach (var recipeIndex in favoritedRecipes)
			{
				packet.Write(recipeIndex);
			}
			packet.Send(toWho, fromWho);
		}

		// These triggers should work in autopause and aren't related to Player actions, so we can use UpdateUI to call them.
		//public void ProcessTriggersButAlways(TriggersSet triggersSet)
		// 0.6.1.6: hm, seemed to have backfired. Investigate why 0.6.1.5 approach would miss keypresses. 
		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			//if (!RecipeBrowser.instance.CheatSheetLoaded)
			{
				if (RecipeBrowser.instance.ToggleRecipeBrowserHotKey.JustPressed)
				{
					RecipeBrowserUI.instance.ShowRecipeBrowser = !RecipeBrowserUI.instance.ShowRecipeBrowser;
					// Debug assistance, allows for reinitializing RecipeBrowserUI
					//if (!RecipeBrowserUI.instance.ShowRecipeBrowser)
					//{
					//	RecipeBrowserUI.instance.RemoveAllChildren();
					//	var isInitializedFieldInfo = typeof(Terraria.UI.UIElement).GetField("_isInitialized", BindingFlags.Instance | BindingFlags.NonPublic);
					//	isInitializedFieldInfo.SetValue(RecipeBrowserUI.instance, false);
					//	RecipeBrowserUI.instance.Activate();
					//}
				}
				if (RecipeBrowser.instance.QueryHoveredItemHotKey.JustPressed)
				{
					// Debug assistance, manually clear craftPath calculations
					//foreach (var slot in RecipeCatalogueUI.instance.recipeSlots)
					//{
					//	slot.craftPathsNeeded = false;
					//	slot.craftPathsCalculated = false;
					//	slot.craftPaths = null;
					//}
					//RecipeCatalogueUI.instance.updateNeeded = true;
					//RecipeCatalogueUI.instance.InvalidateExtendedCraft();

					if (!Main.HoverItem.IsAir)
					{
						// Query item on "Any Iron Bar", should I uncheck "ignore recipe groups"?, can check _nameoverride to see.
						bool shouldShowRecipeBrowser = true;
						if (RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.RecipeCatalogue)
						{
							if (Main.HoverItem.type == RecipeCatalogueUI.instance.queryItem.item?.type)
								shouldShowRecipeBrowser = !RecipeBrowserUI.instance.ShowRecipeBrowser;
							else
								RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(Main.HoverItem.type);
						}
						else if (RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.Craft)
						{
							if (Main.HoverItem.type == CraftUI.instance.recipeResultItemSlot.item?.type)
								shouldShowRecipeBrowser = !RecipeBrowserUI.instance.ShowRecipeBrowser;
							else
								CraftUI.instance.SetItem(Main.HoverItem.type);
						}
						else if (RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.ItemCatalogue)
						{
							ItemCatalogueUI.instance.itemGrid.Goto(delegate (UIElement element) {
								UIItemCatalogueItemSlot itemSlot = element as UIItemCatalogueItemSlot;
								if (itemSlot != null && itemSlot.itemType == Main.HoverItem.type) {
									ItemCatalogueUI.instance.SetItem(itemSlot);
									return true;
								}
								return false;
							}, true);
						}
						else if (RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.Bestiary)
						{
							if (Main.HoverItem.type == BestiaryUI.instance.queryItem.item?.type)
								shouldShowRecipeBrowser = !RecipeBrowserUI.instance.ShowRecipeBrowser;
							else
								BestiaryUI.instance.queryItem.ReplaceWithFake(Main.HoverItem.type);
						}
						RecipeBrowserUI.instance.ShowRecipeBrowser = shouldShowRecipeBrowser;
					}
				}
				if (RecipeBrowser.instance.ToggleFavoritedPanelHotKey.JustPressed) {
					RecipeBrowserUI.instance.ShowFavoritePanel = !RecipeBrowserUI.instance.ShowFavoritePanel;
					if (!RecipeBrowserUI.instance.ShowFavoritePanel)
						RecipeBrowserUI.instance.ForceHideFavoritePanel = true;
					else {
						RecipeBrowserUI.instance.ForceShowFavoritePanel = true;
					}

					RecipeBrowserUI.instance.favoritePanelUpdateNeeded = true;
				}
			}
		}

		public override void PreUpdateBuffs()
		{
			if (Main.myPlayer == Player.whoAmI && seenTiles != null)
			{
				if (!Main.playerInventory && WorldGen.InWorld((int)Player.position.X / 16, (int)Player.position.Y / 16, 10))
					Main.LocalPlayer.AdjTiles(); 
				for (int i = 0; i < seenTiles.Length; i++)
				{
					if (Player.adjTile[i] && !seenTiles[i]) // could move to Player_AdjTiles_Patcher, nah.
					{
						//Main.NewText("Seen " + Utilities.GetTileName(i));
						seenTiles[i] = true;
						RecipeCatalogueUI.instance.InvalidateExtendedCraft();
					}
				}
			}
		}

		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
			if (drawInfo.drawPlayer == UIArmorSetCatalogueItemSlot.drawPlayer) {
				drawInfo.colorArmorHead = Color.White;
				drawInfo.colorArmorBody = Color.White;
				drawInfo.colorArmorLegs = Color.White;

				//drawInfo.upperArmorColor = Color.White;
				//drawInfo.middleArmorColor = Color.White;
				//drawInfo.lowerArmorColor = Color.White;
			}
		}
	}
}