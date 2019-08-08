using Microsoft.Xna.Framework;
using RecipeBrowser.UIElements;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
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

		public override TagCompound Save()
		{
			return new TagCompound
			{
				["StarredRecipes"] = favoritedRecipes.Select(x => RecipeIO.Save(Main.recipe[x])).ToList(),
			};
		}

		public override void Load(TagCompound tag)
		{
			favoritedRecipes = tag.GetList<TagCompound>("StarredRecipes").Select(RecipeIO.Load).Where(x => x > -1).ToList();
		}

		// Only happens on local client/SP
		public override void OnEnterWorld(Player player)
		{
			seenTiles = new bool[TileLoader.TileCount];
			Point center = player.Center.ToTileCoordinates();
			for (int i = center.X - 100; i < center.X + 100; i++)
			{
				for (int j = center.Y - 100; j < center.Y + 100; j++)
				{
					if (WorldGen.InWorld(i, j) && Main.tile[i, j] != null && !seenTiles[Main.tile[i, j].type])
					{
						int Tile = Main.tile[i, j].type;
						List<int> adjTiles = new List<int>();
						adjTiles.Add(Tile);

						ModTile modTile = TileLoader.GetTile(Tile);
						if (modTile != null)
						{
							adjTiles.AddRange(modTile.adjTiles);
						}
						if (Tile == 302)
							adjTiles.Add(17);
						if (Tile == 77)
							adjTiles.Add(17);
						if (Tile == 133)
						{
							adjTiles.Add(17);
							adjTiles.Add(77);
						}
						if (Tile == 134)
							adjTiles.Add(16);
						if (Tile == 354)
							adjTiles.Add(14);
						if (Tile == 469)
							adjTiles.Add(14);
						if (Tile == 355)
						{
							adjTiles.Add(13);
							adjTiles.Add(14);
						}
						// TODO: GlobalTile.AdjTiles support (no player object, reflection needed since private)
						foreach (var tile in adjTiles)
						{
							seenTiles[tile] = true;
						}
					}
				}
			}
			RecipeBrowserUI.instance.favoritePanelUpdateNeeded = true;
			RecipeCatalogueUI.instance.updateNeeded = true;
			if (RecipeCatalogueUI.instance.recipeSlots.Count > 0)
			{
				RecipeBrowserUI.instance.UpdateFavoritedPanel();
			}
		}

		// Called on other clients when a player leaves.
		public override void PlayerDisconnect(Player player)
		{
			// When a player leaves, trigger an update to get rid of Starred Recipe entries.
			RecipeBrowserUI.instance.favoritePanelUpdateNeeded = true;
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			SendFavoritedRecipes(toWho, fromWho, true);
		}

		public override void clientClone(ModPlayer clientClone)
		{
			RecipeBrowserPlayer clone = clientClone as RecipeBrowserPlayer;
			clone.favoritedRecipes.AddRange(favoritedRecipes);
		}

		public override void SendClientChanges(ModPlayer clientPlayer)
		{
			RecipeBrowserPlayer clone = clientPlayer as RecipeBrowserPlayer;
			if (!favoritedRecipes.SequenceEqual(clone.favoritedRecipes))
			{
				SendFavoritedRecipes(-1, player.whoAmI);
			}
		}

		public void SendFavoritedRecipes(int toWho, int fromWho, bool syncPlayer = false)
		{
			ModPacket packet = mod.GetPacket();
			packet.Write((byte)MessageType.SendFavoritedRecipes);
			packet.Write((byte)player.whoAmI);
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
						RecipeBrowserUI.instance.ShowRecipeBrowser = true;
						if (RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.RecipeCatalogue)
						{
							RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(Main.HoverItem.type);
						}
						else if (RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.Craft)
						{
							CraftUI.instance.SetItem(Main.HoverItem.type);
						}
						else if (RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.ItemCatalogue)
						{
							ItemCatalogueUI.instance.itemGrid.Goto(delegate (UIElement element)
							{
								UIItemCatalogueItemSlot itemSlot = element as UIItemCatalogueItemSlot;
								if (itemSlot != null && itemSlot.itemType == Main.HoverItem.type)
								{
									ItemCatalogueUI.instance.SetItem(itemSlot);
									return true;
								}
								return false;
							}, true);
						}
						else if (RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.Bestiary)
						{
							BestiaryUI.instance.queryItem.ReplaceWithFake(Main.HoverItem.type);
						}
					}
				}
			}
		}

		public override void PreUpdateBuffs()
		{
			if (Main.myPlayer == player.whoAmI && seenTiles != null)
			{
				if (!Main.playerInventory)
					Main.LocalPlayer.AdjTiles(); 
				for (int i = 0; i < seenTiles.Length; i++)
				{
					if (player.adjTile[i] && !seenTiles[i]) // could move to Player_AdjTiles_Patcher, nah.
					{
						//Main.NewText("Seen " + Utilities.GetTileName(i));
						seenTiles[i] = true;
						RecipeCatalogueUI.instance.InvalidateExtendedCraft();
					}
				}
			}
		}
	}
}