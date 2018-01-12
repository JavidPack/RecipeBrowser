using Microsoft.Xna.Framework;
using RecipeBrowser.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace RecipeBrowser
{
	internal class RecipeCatalogueUI
	{
		internal static RecipeCatalogueUI instance;
		internal static Color color = new Color(73, 94, 171);

		internal UIRecipeCatalogueQueryItemSlot queryItem;
		internal UICheckbox TileLookupRadioButton;

		internal Item queryLootItem;

		private int tile = -1;
		internal int Tile
		{
			get { return tile; }
			set
			{
				if (tile != value)
					updateNeeded = true;
				tile = value;
				foreach (var tileSlot in tileSlots)
				{
					tileSlot.selected = false;
					if (tileSlot.tile == value)
						tileSlot.selected = true;
				}
			}
		}

		//	internal UICheckbox inventoryFilter;

		internal NewUITextBox itemNameFilter;

		//	internal UIHoverImageButton clearNameFilterButton;
		internal NewUITextBox itemDescriptionFilter;

		internal UIPanel mainPanel;
		internal UIPanel recipeGridPanel;
		internal UIGrid recipeGrid;
		internal UIGrid lootSourceGrid;
		internal UIPanel tileChooserPanel;
		internal UICycleImage uniqueCheckbox;
		internal UIGrid tileChooserGrid;

		internal UIRecipeInfo recipeInfo;
		internal UIRadioButton NearbyIngredientsRadioBitton;
		internal UIRadioButton ItemChecklistRadioButton;
		internal UIRadioButtonGroup RadioButtonGroup;

		internal int selectedIndex = -1;
		internal int newestItem = 0;
		internal List<UIRecipeSlot> recipeSlots;
		internal List<UITileSlot> tileSlots;

		internal bool updateNeeded;

		public RecipeCatalogueUI()
		{
			instance = this;
		}

		internal UIElement CreateRecipeCataloguePanel()
		{
			mainPanel = new UIPanel();
			mainPanel.SetPadding(6);
			//			mainPanel.Left.Set(400f, 0f);
			//			mainPanel.Top.Set(400f, 0f);
			//			mainPanel.Width.Set(475f, 0f); // + 30
			//			mainPanel.MinWidth.Set(415f, 0f);
			//			mainPanel.MaxWidth.Set(784f, 0f);
			//			mainPanel.Height.Set(350, 0f);
			//			mainPanel.MinHeight.Set(243, 0f);
			//			mainPanel.MaxHeight.Set(1000, 0f);
			mainPanel.BackgroundColor = color;
			//Append(mainPanel);

			mainPanel.Top.Set(20, 0f);
			mainPanel.Height.Set(-20, 1f);
			mainPanel.Width.Set(0, 1f);

			queryItem = new UIRecipeCatalogueQueryItemSlot(new Item());
			queryItem.Top.Set(2, 0f);
			queryItem.Left.Set(2, 0f);
			//queryItem.OnItemChanged += () => { Main.NewText("Item changed?"); TileLookupRadioButton.SetDisabled(queryItem.item.createTile <= -1); };
			mainPanel.Append(queryItem);

			TileLookupRadioButton = new UICheckbox("Tile", "");
			TileLookupRadioButton.Top.Set(42, 0f);
			TileLookupRadioButton.Left.Set(0, 0f);
			TileLookupRadioButton.SetText("  Tile");
			TileLookupRadioButton.OnSelectedChanged += (s, e) => { ToggleTileChooser(!mainPanel.HasChild(tileChooserPanel)); updateNeeded = true; };
			mainPanel.Append(TileLookupRadioButton);

			RadioButtonGroup = new UIRadioButtonGroup();
			RadioButtonGroup.Left.Pixels = 45;
			RadioButtonGroup.Width.Set(180, 0f);
			UIRadioButton AllRecipesRadioButton = new UIRadioButton("All Recipes", "");
			NearbyIngredientsRadioBitton = new UIRadioButton("Nearby Chests", "Click to Refresh");
			ItemChecklistRadioButton = new UIRadioButton("Item Checklist Only", "???");
			RadioButtonGroup.Add(AllRecipesRadioButton);
			RadioButtonGroup.Add(NearbyIngredientsRadioBitton);
			RadioButtonGroup.Add(ItemChecklistRadioButton);
			mainPanel.Append(RadioButtonGroup);
			AllRecipesRadioButton.Selected = true;

			NearbyIngredientsRadioBitton.OnSelectedChanged += NearbyIngredientsRadioBitton_OnSelectedChanged;

			if (RecipeBrowser.itemChecklistInstance != null)
			{
				ItemChecklistRadioButton.OnSelectedChanged += ItemChecklistFilter_SelectedChanged;
				ItemChecklistRadioButton.SetHoverText("Only new Items made from Seen Items");
				//ItemChecklistRadioButton.OnRightClick += ItemChecklistRadioButton_OnRightClick;
			}
			else
			{
				ItemChecklistRadioButton.SetDisabled();
				ItemChecklistRadioButton.SetHoverText("Install Item Checklist to use");
			}

			itemNameFilter = new NewUITextBox("Filter by Name");
			itemNameFilter.OnTextChanged += () => { ValidateItemFilter(); updateNeeded = true; };
			itemNameFilter.OnTabPressed += () => { itemDescriptionFilter.Focus(); };
			itemNameFilter.Top.Pixels = 0f;
			itemNameFilter.Left.Set(-208, 1f);
			itemNameFilter.Width.Set(150, 0f);
			itemNameFilter.Height.Set(25, 0f);
			mainPanel.Append(itemNameFilter);

			itemDescriptionFilter = new NewUITextBox("Filter by tooltip");
			itemDescriptionFilter.OnTextChanged += () => { ValidateItemDescription(); updateNeeded = true; };
			itemDescriptionFilter.OnTabPressed += () => { itemNameFilter.Focus(); };
			itemDescriptionFilter.Top.Pixels = 30f;
			itemDescriptionFilter.Left.Set(-208, 1f);
			itemDescriptionFilter.Width.Set(150, 0f);
			itemDescriptionFilter.Height.Set(25, 0f);
			mainPanel.Append(itemDescriptionFilter);

			recipeGridPanel = new UIPanel();
			recipeGridPanel.SetPadding(6);
			recipeGridPanel.Top.Pixels = 60;
			recipeGridPanel.Width.Set(-60, 1f);
			recipeGridPanel.Height.Set(-60 - 121, 1f);
			recipeGridPanel.BackgroundColor = Color.DarkBlue;
			mainPanel.Append(recipeGridPanel);

			recipeGrid = new UIGrid();
			recipeGrid.Width.Set(-20f, 1f);
			recipeGrid.Height.Set(0, 1f);
			recipeGrid.ListPadding = 2f;
			recipeGridPanel.Append(recipeGrid);

			var lootItemsScrollbar = new FixedUIScrollbar(RecipeBrowserUI.instance.userInterface);
			lootItemsScrollbar.SetView(100f, 1000f);
			lootItemsScrollbar.Height.Set(0, 1f);
			lootItemsScrollbar.Left.Set(-20, 1f);
			recipeGridPanel.Append(lootItemsScrollbar);
			recipeGrid.SetScrollbar(lootItemsScrollbar);

			recipeInfo = new UIRecipeInfo();
			recipeInfo.Top.Set(-118, 1f);
			recipeInfo.Width.Set(-50, 1f);
			recipeInfo.Height.Set(120, 0f);
			mainPanel.Append(recipeInfo);

			UIPanel lootSourcePanel = new UIPanel();
			lootSourcePanel.SetPadding(6);
			lootSourcePanel.Top.Pixels = 0;
			lootSourcePanel.Width.Set(50, 0f);
			lootSourcePanel.Left.Set(-50, 1f);
			lootSourcePanel.Height.Set(-16, 1f);
			lootSourcePanel.BackgroundColor = Color.CornflowerBlue;
			mainPanel.Append(lootSourcePanel);

			lootSourceGrid = new UIGrid();
			lootSourceGrid.Width.Set(0, 1f);
			lootSourceGrid.Height.Set(0, 1f);
			lootSourceGrid.ListPadding = 2f;
			lootSourcePanel.Append(lootSourceGrid);

			var lootSourceScrollbar = new InvisibleFixedUIScrollbar(RecipeBrowserUI.instance.userInterface);
			lootSourceScrollbar.SetView(100f, 1000f);
			lootSourceScrollbar.Height.Set(0, 1f);
			lootSourceScrollbar.Left.Set(-20, 1f);
			lootSourcePanel.Append(lootSourceScrollbar);
			lootSourceGrid.SetScrollbar(lootSourceScrollbar);

			// Tile Chooser
			tileChooserPanel = new UIPanel();
			tileChooserPanel.SetPadding(6);
			tileChooserPanel.Top.Pixels = 60;
			tileChooserPanel.Width.Set(50, 0f);
			tileChooserPanel.Height.Set(-60 - 121, 1f);
			tileChooserPanel.BackgroundColor = Color.CornflowerBlue;

			uniqueCheckbox = new UICycleImage(RecipeBrowser.instance.GetTexture("Images/uniqueTile") /* Thanks MiningdiamondsVIII */, 2, new string[] { "Show inherited recipes", "Show unique recipes" }, 36, 20);
			uniqueCheckbox.Top.Set(0, 0f);
			uniqueCheckbox.Left.Set(1, 0f);
			uniqueCheckbox.CurrentState = 1;
			uniqueCheckbox.OnStateChanged += (s, e) => { updateNeeded = true; };
			tileChooserPanel.Append(uniqueCheckbox);

			tileChooserGrid = new UIGrid();
			tileChooserGrid.Width.Set(0, 1f);
			tileChooserGrid.Height.Set(-24, 1f);
			tileChooserGrid.Top.Set(24, 0f);
			tileChooserGrid.ListPadding = 2f;
			tileChooserPanel.Append(tileChooserGrid);

			var tileChooserScrollbar = new InvisibleFixedUIScrollbar(RecipeBrowserUI.instance.userInterface);
			tileChooserScrollbar.SetView(100f, 1000f);
			tileChooserScrollbar.Height.Set(0, 1f);
			tileChooserScrollbar.Left.Set(-20, 1f);
			tileChooserPanel.Append(tileChooserScrollbar);
			tileChooserGrid.SetScrollbar(tileChooserScrollbar);

			recipeSlots = new List<UIRecipeSlot>();
			tileSlots = new List<UITileSlot>();

			updateNeeded = true;

			return mainPanel;
		}

		internal void ToggleTileChooser(bool show = true)
		{
			if (show)
			{
				recipeGridPanel.Width.Set(-113, 1f);
				recipeGridPanel.Left.Set(53, 0f);
				mainPanel.Append(tileChooserPanel);
			}
			else
			{
				recipeGridPanel.Width.Set(-60, 1f);
				recipeGridPanel.Left.Set(0, 0f);
				mainPanel.RemoveChild(tileChooserPanel);
				Tile = -1;
			}
			recipeGridPanel.Recalculate();
		}

		internal void ShowCraftInterface()
		{
			// make smaller? bigger?
			//throw new NotImplementedException();
			Main.NewText("ShowCraftInterface");
			if (Main.rand.NextBool(2))
			{
				recipeGridPanel.Width.Set(-120, 1f);
				recipeGridPanel.Left.Set(60, 0f);
			}
			else
			{
				recipeGridPanel.Width.Set(-60, 1f);
				recipeGridPanel.Left.Set(0, 0f);
			}
			recipeGridPanel.Recalculate();
		}

		internal void CloseButtonClicked()
		{
			// we should have a way for the button itself to be unclicked and notify parent.
			RadioButtonGroup.ButtonClicked(0);

			if (queryItem.real && queryItem.item.stack > 0)
			{
				// This causes items to get a new modifier. Oops
				//Main.player[Main.myPlayer].QuickSpawnItem(lookupItemSlot.item.type, lookupItemSlot.item.stack);
				//lookupItemSlot.item.SetDefaults(0);

				//Player player = Main.player[Main.myPlayer];
				//queryItem.item.position = player.Center;
				//Item item = player.GetItem(player.whoAmI, queryItem.item, false, true);
				//if (item.stack > 0)
				//{
				//	int num = Item.NewItem((int)player.position.X, (int)player.position.Y, player.width, player.height, item.type, item.stack, false, (int)queryItem.item.prefix, true, false);
				//	Main.item[num].newAndShiny = false;
				//	if (Main.netMode == 1)
				//	{
				//		NetMessage.SendData(21, -1, -1, null, num, 1f, 0f, 0f, 0, 0, 0);
				//	}
				//}
				//queryItem.item = new Item();
				queryItem.ReplaceWithFake(0);
			}

			queryLootItem = null;
			updateNeeded = true;
		}

		internal void Update()
		{
			/*if (PlayerInput.Triggers.Current.Hotbar1 && !Main.LocalPlayer.inventory[0].IsAir)
				RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(Main.LocalPlayer.inventory[0].type);
			if (PlayerInput.Triggers.Current.Hotbar2 && !Main.LocalPlayer.inventory[1].IsAir)
				RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(Main.LocalPlayer.inventory[1].type);
			if (PlayerInput.Triggers.Current.Hotbar3 && !Main.LocalPlayer.inventory[2].IsAir)
				RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(Main.LocalPlayer.inventory[2].type);
			*/
			UpdateGrid();
		}

		private void UpdateGrid()
		{
			if (Recipe.numRecipes != recipeSlots.Count)
			{
				recipeSlots.Clear();
				for (int i = 0; i < Recipe.numRecipes; i++)
				{
					recipeSlots.Add(new UIRecipeSlot(i));
				}

				tileChooserGrid.Clear();
				var tileUsageCounts = new Dictionary<int, int>();
				int currentCount;
				for (int i = 0; i < Recipe.numRecipes; i++)
				{
					for (int j = 0; j < 15; j++)
					{
						if (Main.recipe[i].requiredTile[j] == -1)
							break;
						tileUsageCounts.TryGetValue(Main.recipe[i].requiredTile[j], out currentCount);
						tileUsageCounts[Main.recipe[i].requiredTile[j]] = currentCount + 1;
					}
				}
				// sort
				var sorted = tileUsageCounts.OrderBy(kvp => kvp.Value);
				foreach (var tileUsage in sorted)
				{
					var tileSlot = new UITileSlot(tileUsage.Key, tileUsage.Value);
					tileChooserGrid.Add(tileSlot);
					tileSlots.Add(tileSlot);
				}
			}

			if (!updateNeeded) { return; }
			updateNeeded = false;

			List<int> groups = new List<int>();
			if (queryItem.item.stack > 0)
			{
				int type = queryItem.item.type;

				foreach (var group in RecipeGroup.recipeGroups)
				{
					if (group.Value.ValidItems.Contains(type))
					{
						groups.Add(group.Key);
					}
				}
			}

			lootSourceGrid.Clear();
			if (queryLootItem != null)
			{
				//var jsonitem = new JSONItem(queryLootItem.modItem?.mod.Name ?? "Terraria", Lang.GetItemNameValue(queryLootItem.type), queryLootItem.modItem != null ? 0 : queryLootItem.type);
				var jsonitem = new JSONItem(queryLootItem.modItem?.mod.Name ?? "Terraria", queryLootItem.modItem?.Name ?? Lang.GetItemNameValue(queryLootItem.type), queryLootItem.modItem != null ? 0 : queryLootItem.type);
				List<JSONNPC> npcsthatdropme;
				if (LootCache.instance.lootInfos.TryGetValue(jsonitem, out npcsthatdropme))
				{
					foreach (var dropper in npcsthatdropme)
					{
						int id = dropper.GetID();
						if (id == 0) continue;
						/*int id = dropper.id;
						if (id == 0)
						{
							//it's a
							Mod m = ModLoader.GetMod(dropper.mod);
							if (m == null) continue;
							id = m.NPCType(dropper.name);
						}*/
						NPC npc = new NPC();
						npc.SetDefaults(id);
						var slot = new UINPCSlot(npc);
						//lootSourceGrid.Add(slot);
						lootSourceGrid._items.Add(slot);
						lootSourceGrid._innerList.Append(slot);
					}
				}
			}
			lootSourceGrid.UpdateOrder();
			lootSourceGrid._innerList.Recalculate();

			recipeGrid.Clear();
			for (int i = 0; i < Recipe.numRecipes; i++)
			{
				if (PassRecipeFilters(Main.recipe[i], groups))
				// all the filters
				//if (Main.projName[i].ToLower().IndexOf(searchFilter.Text, StringComparison.OrdinalIgnoreCase) != -1)
				{
					var box = recipeSlots[i];
					//
					if (newestItem > 0)
					{
						Recipe recipe = Main.recipe[i];
						box.recentlyDiscovered = false;
						if (recipe.requiredItem.Any(x => x.type == newestItem))
						{
							box.recentlyDiscovered = true;
						}
					}

					recipeGrid._items.Add(box);
					recipeGrid._innerList.Append(box);
				}
			}

			recipeGrid.UpdateOrder();
			recipeGrid._innerList.Recalculate();
		}

		private bool PassRecipeFilters(Recipe recipe, List<int> groups)
		{
			if (RecipeBrowserUI.modIndex != RecipeBrowserUI.instance.mods.Length - 1)
			{
				if (recipe.createItem.modItem == null)
				{
					return false;
				}
				if (recipe.createItem.modItem.mod.Name != RecipeBrowserUI.instance.mods[RecipeBrowserUI.modIndex])
				{
					return false;
				}
			}

			if (NearbyIngredientsRadioBitton.Selected)
			{
				if (!PassNearbyChestFilter(recipe))
				{
					return false;
				}
			}

			// Item Checklist integration
			if (ItemChecklistRadioButton.Selected)
			{
				if (RecipeBrowserUI.instance.foundItems != null)
				{
					for (int i = 0; i < Recipe.maxRequirements; i++)
					{
						if (recipe.requiredItem[i].type > 0)
						{
							if (!RecipeBrowserUI.instance.foundItems[recipe.requiredItem[i].type])
							{
								return false;
							}
						}
					}
					// filter out recipes that make things I've already obtained
					if (RecipeBrowserUI.instance.foundItems[recipe.createItem.type])
					{
						return false;
					}
				}
				else
				{
					Main.NewText("How is this happening??");
				}
			}

			// Filter out recipes that don't use selected Tile
			if (Tile > -1)
			{
				List<int> adjTiles = new List<int>();
				adjTiles.Add(Tile);
				if (uniqueCheckbox.CurrentState == 0)
				{
					Terraria.ModLoader.ModTile modTile = Terraria.ModLoader.TileLoader.GetTile(Tile);
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
				}
				if (!recipe.requiredTile.Any(t => adjTiles.Contains(t)))
				{
					return false;
				}
			}

			if (!queryItem.item.IsAir)
			{
				int type = queryItem.item.type;
				bool inGroup = recipe.acceptedGroups.Intersect(groups).Any();

				inGroup |= recipe.useWood(type, type) || recipe.useSand(type, type) || recipe.useFragment(type, type) || recipe.useIronBar(type, type) || recipe.usePressurePlate(type, type);
				if (!inGroup)
				{
					if (!(recipe.createItem.type == type || recipe.requiredItem.Any(ing => ing.type == type)))
					{
						return false; ;
					}
				}
			}

			if (recipe.createItem.Name.ToLower().IndexOf(itemNameFilter.currentString, StringComparison.OrdinalIgnoreCase) == -1)
				return false;

			if (itemDescriptionFilter.currentString.Length > 0)
			{
				if ((recipe.createItem.ToolTip != null && GetTooltipsAsString(recipe.createItem.ToolTip).IndexOf(itemDescriptionFilter.currentString, StringComparison.OrdinalIgnoreCase) != -1) /*|| (recipe.createItem.toolTip2 != null && recipe.createItem.toolTip2.ToLower().IndexOf(itemDescriptionFilter.Text, StringComparison.OrdinalIgnoreCase) != -1)*/)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			return true;
		}

		// todo, make sure case doesn't matter, conjoined lines don't have results.
		private string GetTooltipsAsString(ItemTooltip toolTip)
		{
			StringBuilder sb = new StringBuilder();
			for (int j = 0; j < toolTip.Lines; j++)
			{
				sb.Append(toolTip.GetLine(j) + "\n");
			}
			return sb.ToString().ToLower();
		}

		// TODO, checkbox for check stack?
		// TODO, # missing allowed slider?
		private bool PassNearbyChestFilter(Recipe recipe)
		{
			// Return true only if all ingredients in recipe are found
			//List<int> needed = new List<int>();
			HashSet<int> needed = new HashSet<int>();
			foreach (var item in recipe.requiredItem)
			{
				needed.Add(item.type);
			}

			//List<Item[]> sources = new List<Item[]>();
			HashSet<int> foundItems = new HashSet<int>();

			for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
			{
				Chest chest = Main.chest[chestIndex];
				if (chest != null && !Chest.isLocked(chest.x, chest.y))
				{
					Vector2 chestPosition = new Vector2((float)(chest.x * 16 + 16), (float)(chest.y * 16 + 16));
					if ((chestPosition - Main.LocalPlayer.Center).Length() < itemSearchRange)
					{
						if (Main.netMode == 0 || RecipeBrowser.chestContentsAvailable[chestIndex])
						{
							foundItems.UnionWith(chest.item.Select(x => x.type));
							//sources.Add(chest.item);
							//Item[] items = chest.item;
							// (int i = 0; i < 40; i++)
							//{
							//	needed.Remove(items[i].type);
							//}
						}
					}
				}
			}
			foundItems.UnionWith(Main.LocalPlayer.bank.item.Select(x => x.type));
			foundItems.UnionWith(Main.LocalPlayer.bank2.item.Select(x => x.type));
			foundItems.UnionWith(Main.LocalPlayer.bank3.item.Select(x => x.type));
			foundItems.UnionWith(Main.LocalPlayer.inventory.Select(x => x.type));
			foundItems.UnionWith(Main.LocalPlayer.armor.Select(x => x.type));
			foundItems.UnionWith(Main.LocalPlayer.dye.Select(x => x.type));
			foundItems.UnionWith(Main.LocalPlayer.miscDyes.Select(x => x.type));
			foundItems.UnionWith(Main.LocalPlayer.miscEquips.Select(x => x.type));
			foundItems.Add(queryItem.item.type);
			//sources.Add(Main.LocalPlayer.bank.item);
			//sources.Add(Main.LocalPlayer.bank2.item);
			//sources.Add(Main.LocalPlayer.bank3.item);
			//sources.Add(Main.LocalPlayer.inventory);
			//sources.Add(Main.LocalPlayer.armor);
			//sources.Add(Main.LocalPlayer.dye);
			//sources.Add(Main.LocalPlayer.miscDyes);
			//sources.Add(Main.LocalPlayer.miscEquips);
			//sources.Add(new Item[] { queryItem.item });

			needed.ExceptWith(foundItems);

			//foreach (var bank in sources)
			//{
			//	needed.RemoveAll(x => bank.Any(i => i.type == x));
			//}

			return needed.Count == 0;
		}

		internal void SetRecipe(int index)
		{
			selectedIndex = -1;
			recipeInfo.RemoveAllChildren();

			foreach (var item in recipeSlots)
			{
				item.selected = false;
			}

			var recipeslot = recipeSlots[index];

			recipeslot.selected = false;
			recipeslot.selected = true;
			selectedIndex = index;

			Recipe recipe = Main.recipe[index];
			for (int i = 0; i < Recipe.maxRequirements; i++)
			{
				if (recipe.requiredItem[i].type > 0)
				{
					UIIngredientSlot ingredient = new UIIngredientSlot(recipe.requiredItem[i].Clone());
					ingredient.Left.Pixels = 200 + (i % 5 * 40);
					ingredient.Top.Pixels = (i / 5 * 40);

					string nameOverride;
					if (recipe.ProcessGroupsForText(recipe.requiredItem[i].type, out nameOverride))
					{
						//Main.toolTip.name = name;
					}
					if (recipe.anyIronBar && recipe.requiredItem[i].type == 22)
					{
						nameOverride = Lang.misc[37].Value + " " + Lang.GetItemNameValue(22);
					}
					else if (recipe.anyWood && recipe.requiredItem[i].type == 9)
					{
						nameOverride = Lang.misc[37].Value + " " + Lang.GetItemNameValue(9);
					}
					else if (recipe.anySand && recipe.requiredItem[i].type == 169)
					{
						nameOverride = Lang.misc[37].Value + " " + Lang.GetItemNameValue(169);
					}
					else if (recipe.anyFragment && recipe.requiredItem[i].type == 3458)
					{
						nameOverride = Lang.misc[37].Value + " " + Lang.misc[51].Value;
					}
					else if (recipe.anyPressurePlate && recipe.requiredItem[i].type == 542)
					{
						nameOverride = Lang.misc[37].Value + " " + Lang.misc[38].Value;
					}
					if (nameOverride != "")
					{
						ingredient.item.SetNameOverride(nameOverride);
					}
					// TODO, stack?

					recipeInfo.Append(ingredient);
				}
			}
		}

		// problem, how to detect that we need to request again?
		private const int itemSearchRange = 60 * 16; // this is in pixels : 400 too low

		private void NearbyIngredientsRadioBitton_OnSelectedChanged(object sender, EventArgs e)
		{
			UIRadioButton button = (UIRadioButton)sender;
			updateNeeded = true;
			if (!button.Selected) return;
			// Reset
			if (Main.netMode == 0) return; // we can skip all this in SP
			RecipeBrowser.chestContentsAvailable = new bool[1000];
			for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
			{
				Chest chest = Main.chest[chestIndex];
				if (chest != null && !Chest.isLocked(chest.x, chest.y))
				{
					Vector2 chestPosition = new Vector2((float)(chest.x * 16 + 16), (float)(chest.y * 16 + 16));
					if ((chestPosition - Main.LocalPlayer.Center).Length() < itemSearchRange)
					{
						//if (chest.item[0] == null)
						{
							var message = RecipeBrowser.instance.GetPacket();
							message.Write((byte)MessageType.SilentRequestChestContents);
							message.Write(chestIndex);
							message.Send();
							//RecipeBrowser.chestContentsAvailable[chestIndex] = true;
							//Main.NewText($"Wait on {chestIndex}");
							continue;
						}
					}
				}
			}
		}

		private void ItemChecklistFilter_SelectedChanged(object sender, EventArgs e)
		{
			if ((sender as UIRadioButton).Selected)
			{
				RecipeBrowserUI.instance.QueryItemChecklist();
			}
			else
			{
				//RecipeBrowserUI.instance.foundItems = null;
			}
			updateNeeded = true;
		}

		/// <summary>
		/// Checks text to verify input is in
		/// </summary>
		private void ValidateItemFilter()
		{
			if (itemNameFilter.currentString.Length > 0)
			{
				bool found = false;
				for (int i = 0; i < Recipe.numRecipes; i++)
				{
					Recipe recipe = Main.recipe[i];
					if (recipe.createItem.Name.ToLower().IndexOf(itemNameFilter.currentString, StringComparison.OrdinalIgnoreCase) != -1)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					itemNameFilter.SetText(itemNameFilter.currentString.Substring(0, itemNameFilter.currentString.Length - 1));
				}
			}
			updateNeeded = true;
		}

		private void ValidateItemDescription()
		{
			//if (itemNameFilter.Text.Length > 0)
			//{
			//	bool found = false;
			//	for (int i = 0; i < Recipe.numRecipes; i++)
			//	{
			//		Recipe recipe = Main.recipe[i];
			//		if (recipe.createItem.name.ToLower().IndexOf(itemNameFilter.Text, StringComparison.OrdinalIgnoreCase) == -1)
			//		{
			//			found = true;
			//			break;
			//		}
			//	}
			//	if (!found)
			//	{
			//		itemNameFilter.SetText(itemNameFilter.Text.Substring(0, itemNameFilter.Text.Length - 1));
			//	}
			//}
			updateNeeded = true;
		}
	}
}