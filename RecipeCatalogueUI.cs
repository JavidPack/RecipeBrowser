using Microsoft.Xna.Framework;
using RecipeBrowser.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using Terraria.ID;

namespace RecipeBrowser
{
	internal class RecipeCatalogueUI
	{
		internal static string RBText(string key, string category = "RecipeCatalogueUI") => RecipeBrowser.RBText(category, key);

		internal static RecipeCatalogueUI instance;
		internal static Color color = new Color(73, 94, 171);

		internal UIRecipeCatalogueQueryItemSlot queryItem;
		internal UICheckbox TileLookupRadioButton;

		internal Item queryLootItem; // Last clicked item ingredient/recipe result, will populate lootSourceGrid. 

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
		internal int hoveredIndex = -1;
		internal int newestItem = 0;
		internal List<UIRecipeSlot> recipeSlots;
		internal List<UITileSlot> tileSlots;

		internal List<int> craftingTiles;

		internal bool updateNeeded;
		internal int slowUpdateNeeded;

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
			queryItem.emptyHintText = RBText("EmptyQuerySlotHint");
			//queryItem.OnItemChanged += () => { Main.NewText("Item changed?"); TileLookupRadioButton.SetDisabled(queryItem.item.createTile <= -1); };
			mainPanel.Append(queryItem);

			TileLookupRadioButton = new UICheckbox(RBText("Tile"), "");
			TileLookupRadioButton.Top.Set(42, 0f);
			TileLookupRadioButton.Left.Set(0, 0f);
			TileLookupRadioButton.SetText("  " + RBText("Tile"));
			TileLookupRadioButton.OnSelectedChanged += (s, e) => { ToggleTileChooser(!mainPanel.HasChild(tileChooserPanel)); updateNeeded = true; };
			mainPanel.Append(TileLookupRadioButton);

			RadioButtonGroup = new UIRadioButtonGroup();
			RadioButtonGroup.Left.Pixels = 45;
			RadioButtonGroup.Width.Set(180, 0f);
			UIRadioButton AllRecipesRadioButton = new UIRadioButton(RBText("AllRecipes"), "");
			NearbyIngredientsRadioBitton = new UIRadioButton(RBText("NearbyChests"), RBText("ClickToRefresh"));
			ItemChecklistRadioButton = new UIRadioButton(RBText("ItemChecklistOnly"), "???");
			RadioButtonGroup.Add(AllRecipesRadioButton);
			RadioButtonGroup.Add(NearbyIngredientsRadioBitton);
			RadioButtonGroup.Add(ItemChecklistRadioButton);
			mainPanel.Append(RadioButtonGroup);
			AllRecipesRadioButton.Selected = true;

			NearbyIngredientsRadioBitton.OnSelectedChanged += NearbyIngredientsRadioBitton_OnSelectedChanged;

			if (RecipeBrowser.itemChecklistInstance != null)
			{
				ItemChecklistRadioButton.OnSelectedChanged += ItemChecklistFilter_SelectedChanged;
				ItemChecklistRadioButton.SetHoverText(RBText("OnlyNewItemsMadeFromSeenItems"));
				//ItemChecklistRadioButton.OnRightClick += ItemChecklistRadioButton_OnRightClick;
			}
			else
			{
				ItemChecklistRadioButton.SetDisabled();
				ItemChecklistRadioButton.SetHoverText(RBText("InstallItemChecklistToUse", "Common"));
			}

			itemNameFilter = new NewUITextBox(RBText("FilterByName", "Common"));
			itemNameFilter.OnTextChanged += () => { ValidateItemFilter(); updateNeeded = true; };
			itemNameFilter.OnTabPressed += () => { itemDescriptionFilter.Focus(); };
			itemNameFilter.Top.Pixels = 0f;
			itemNameFilter.Left.Set(-202, 1f);
			itemNameFilter.Width.Set(150, 0f);
			itemNameFilter.Height.Set(25, 0f);
			mainPanel.Append(itemNameFilter);

			itemDescriptionFilter = new NewUITextBox(RBText("FilterByTooltip", "Common"));
			itemDescriptionFilter.OnTextChanged += () => { ValidateItemDescription(); updateNeeded = true; };
			itemDescriptionFilter.OnTabPressed += () => { itemNameFilter.Focus(); };
			itemDescriptionFilter.Top.Pixels = 30f;
			itemDescriptionFilter.Left.Set(-202, 1f);
			itemDescriptionFilter.Width.Set(150, 0f);
			itemDescriptionFilter.Height.Set(25, 0f);
			mainPanel.Append(itemDescriptionFilter);

			recipeGridPanel = new UIPanel();
			recipeGridPanel.SetPadding(6);
			recipeGridPanel.Top.Pixels = 120;
			recipeGridPanel.Width.Set(-52, 1f);
			recipeGridPanel.Height.Set(-50 - 120, 1f);
			recipeGridPanel.BackgroundColor = Color.CornflowerBlue;
			mainPanel.Append(recipeGridPanel);

			recipeGrid = new UIGrid();
			recipeGrid.alternateSort = ItemGridSort;
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

			recipeInfo = new UIRecipeInfo(); // -118, 120....to 50?
			recipeInfo.Top.Set(-48, 1f);
			recipeInfo.Width.Set(-50, 1f);
			recipeInfo.Height.Set(50, 0f);
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
			lootSourceGrid.drawArrows = true;
			lootSourcePanel.Append(lootSourceGrid);

			var lootSourceScrollbar = new InvisibleFixedUIScrollbar(RecipeBrowserUI.instance.userInterface);
			lootSourceScrollbar.SetView(100f, 1000f);
			lootSourceScrollbar.Height.Set(0, 1f);
			lootSourceScrollbar.Left.Set(-20, 1f);
			//lootSourcePanel.Append(lootSourceScrollbar);
			lootSourceGrid.SetScrollbar(lootSourceScrollbar);

			// Tile Chooser
			tileChooserPanel = new UIPanel();
			tileChooserPanel.SetPadding(6);
			tileChooserPanel.Top.Pixels = 120;
			tileChooserPanel.Width.Set(50, 0f);
			tileChooserPanel.Height.Set(-50 - 120, 1f);
			tileChooserPanel.BackgroundColor = Color.CornflowerBlue;

			uniqueCheckbox = new UICycleImage(RecipeBrowser.instance.Assets.Request<Texture2D>("Images/uniqueTile") /* Thanks MiningdiamondsVIII */, 2, new string[] { "Show inherited recipes", "Show unique recipes" }, 36, 20);
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
			tileChooserGrid.drawArrows = true;
			tileChooserPanel.Append(tileChooserGrid);

			var tileChooserScrollbar = new InvisibleFixedUIScrollbar(RecipeBrowserUI.instance.userInterface);
			tileChooserScrollbar.SetView(100f, 1000f);
			tileChooserScrollbar.Height.Set(0, 1f);
			tileChooserScrollbar.Left.Set(-20, 1f);
			// Appending grid after, setting width to 0, or just not appending seem to all work.
			//tileChooserPanel.Append(tileChooserScrollbar);
			tileChooserGrid.SetScrollbar(tileChooserScrollbar);

			// needed? additionalDragTargets.Add(SharedUI.instance.sortsAndFiltersPanel);

			recipeSlots = new List<UIRecipeSlot>();
			tileSlots = new List<UITileSlot>();

			updateNeeded = true;

			return mainPanel;
		}

		internal void ToggleTileChooser(bool show = true)
		{
			if (show)
			{
				recipeGridPanel.Width.Set(-104, 1f);
				recipeGridPanel.Left.Set(52, 0f);
				mainPanel.Append(tileChooserPanel);
			}
			else
			{
				recipeGridPanel.Width.Set(-52, 1f);
				recipeGridPanel.Left.Set(0, 0f);
				mainPanel.RemoveChild(tileChooserPanel);
				Tile = -1;
			}
			recipeGridPanel.Recalculate();
		}

		// Previous approach showing Craft in Recipe Catalog menu. Might bring this back as an option later?
		//internal void ToggleCraftPanel(bool show = true)
		//{
		//	if (show)
		//	{
		//		//recipeGridPanel.Width.Set(-113, 1f);
		//		//recipeGridPanel.Left.Set(53, 0f);
		//		recipeGridPanel.Width.Set(0, 0.5f);
		//		mainPanel.Append(craftPanel);
		//	}
		//	else
		//	{
		//		//recipeGridPanel.Width.Set(-60, 1f);
		//		//recipeGridPanel.Left.Set(0, 0f);
		//		recipeGridPanel.Width.Set(-56, 1f);
		//		mainPanel.RemoveChild(craftPanel);
		//	}

		//	recipeGridPanel.Recalculate();

		//	RecipeCatalogueUI.instance.recipeGrid.Goto(delegate (UIElement element)
		//	{
		//		UIRecipeSlot itemSlot = element as UIRecipeSlot;
		//		return itemSlot.index == selectedIndex;
		//	}, true, true);
		//}

		//internal void ShowCraftInterface()
		//{
		//	// make smaller? bigger?
		//	//throw new NotImplementedException();
		//	Main.NewText("ShowCraftInterface");
		//	if (Main.rand.NextBool(2))
		//	{
		//		recipeGridPanel.Width.Set(-120, 1f);
		//		recipeGridPanel.Left.Set(60, 0f);
		//	}
		//	else
		//	{
		//		recipeGridPanel.Width.Set(-60, 1f);
		//		recipeGridPanel.Left.Set(0, 0f);
		//	}
		//	recipeGridPanel.Recalculate();
		//}

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
			hoveredIndex = -1;
			UIElements.UIItemSlot.hoveredItem = null;
			/*if (PlayerInput.Triggers.Current.Hotbar1 && !Main.LocalPlayer.inventory[0].IsAir)
				RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(Main.LocalPlayer.inventory[0].type);
			if (PlayerInput.Triggers.Current.Hotbar2 && !Main.LocalPlayer.inventory[1].IsAir)
				RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(Main.LocalPlayer.inventory[1].type);
			if (PlayerInput.Triggers.Current.Hotbar3 && !Main.LocalPlayer.inventory[2].IsAir)
				RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(Main.LocalPlayer.inventory[2].type);
			*/
			// Automatically populate query slot option:
			//if (RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.RecipeCatalogue)
			//{
			//	if ((!queryItem.real || queryItem.item.IsAir) && !Main.mouseItem.IsAir)
			//	{
			//		queryItem.ReplaceWithFake(Main.mouseItem.type);
			//	}
			//}
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
					foreach (int type in Main.recipe[i].requiredTile)
					{
						tileUsageCounts.TryGetValue(type, out currentCount);
						tileUsageCounts[type] = currentCount + 1;
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
				craftingTiles = tileUsageCounts.Select(x => x.Key).ToList();

				RecipeBrowserUI.instance.UpdateFavoritedPanel();
			}

			if (!RecipeBrowserUI.instance.ShowRecipeBrowser || RecipeBrowserUI.instance.CurrentPanel != RecipeBrowserUI.RecipeCatalogue) {
				return;
			}

			if (slowUpdateNeeded > 0) {
				slowUpdateNeeded--;
				if (slowUpdateNeeded == 0)
					updateNeeded = true;
			}

			if (!updateNeeded) { return; }
			updateNeeded = false;
			slowUpdateNeeded = 0;

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
				int queryLootItemType = queryLootItem.type;
				List<int> npcsthatdropme;
				if (LootCache.instance.lootInfos.TryGetValue(queryLootItemType, out npcsthatdropme))
				{
					foreach (var dropper in npcsthatdropme)
					{
						int id = dropper;
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

			//if (SharedUI.instance.ObtainableFilter.button.selected)
			//{
			//	Main.NewText("Warning, Extended Craftable Filter will cause lag.");
			//}

			recipeGrid.Clear();
			//int craftPathsCalculatedCount = 0;
			for (int i = 0; i < Recipe.numRecipes; i++)
			{
				//if (recipeSlots[i].craftPathsCalculated)
				//	craftPathsCalculatedCount++;
				if (PassRecipeFilters(recipeSlots[i], Main.recipe[i], groups))
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
			//Main.NewText($"craftPathsCalculated: {craftPathsCalculatedCount}/{Recipe.numRecipes}");

			recipeGrid.UpdateOrder();
			recipeGrid._innerList.Recalculate();
		}

		private int ItemGridSort(UIElement x, UIElement y)
		{
			if (x is UIPanel)
				return -1;
			if (y is UIPanel)
				return 1;
			UIRecipeSlot a = x as UIRecipeSlot;
			UIRecipeSlot b = y as UIRecipeSlot;
			if (a.CompareToIgnoreIndex(b) == 0 && SharedUI.instance.SelectedSort != null)
				return SharedUI.instance.SelectedSort.sort(a.item, b.item);
			return a.CompareTo(b);
		}

		private bool PassRecipeFilters(UIRecipeSlot recipeSlot, Recipe recipe, List<int> groups)
		{
			// TODO: Option to filter by source of Recipe rather than by createItem maybe?
			if (RecipeBrowserUI.modIndex != 0)
			{
				if (recipe.createItem.ModItem == null)
				{
					return false;
				}
				if (recipe.createItem.ModItem.Mod.Name != RecipeBrowserUI.instance.mods[RecipeBrowserUI.modIndex])
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
					foreach (Item item in recipe.requiredItem)
					{
						if (!RecipeBrowserUI.instance.foundItems[item.type])
						{
							return false;
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
						adjTiles.AddRange(modTile.AdjTiles);
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
				bool inGroup = recipe.acceptedGroups.Intersect(groups).Any(); // Lesion item bug, they have the Wood group but don't have any wood in them
				
				if (!inGroup)
				{
					if (!(recipe.createItem.type == type || recipe.requiredItem.Any(ing => ing.type == type)))
					{
						return false;
					}
				}
			}

			var SelectedCategory = SharedUI.instance.SelectedCategory;
			if (SelectedCategory != null)
			{
				if (!SelectedCategory.belongs(recipe.createItem) && !SelectedCategory.subCategories.Any(x => x.belongs(recipe.createItem)))
					return false;
			}
			var availableFilters = SharedUI.instance.availableFilters;
			if (availableFilters != null)
				foreach (var filter in SharedUI.instance.availableFilters)
				{
					if(!filter.button.selected && filter == SharedUI.instance.DisabledFilter) {
						if(recipe.Disabled)
							return false;
					}
					if (filter.button.selected)
					{
						// Extended craft problem.
						if (!filter.belongs(recipe.createItem))
							return false;
						if (filter == SharedUI.instance.ObtainableFilter)
						{
							recipeSlot.CraftPathNeeded();
							if (!((recipeSlot.craftPathCalculated || recipeSlot.craftPathsCalculated) && recipeSlot.craftPaths.Count > 0))
								return false;
						}
						if (filter == SharedUI.instance.CraftableFilter)
						{
							int index = recipeSlot.index;
							bool ableToCraft = false;
							for (int n = 0; n < Main.numAvailableRecipes; n++)
							{
								if (index == Main.availableRecipe[n])
								{
									ableToCraft = true;
									break;
								}
							}
							if (!ableToCraft)
								return false;
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
			// TODO: Inefficient to calculate this repeatedly for each recipe.
			HashSet<int> foundItems = new HashSet<int>();

			for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
			{
				Chest chest = Main.chest[chestIndex];
				if (chest != null && !Chest.IsLocked(chest.x, chest.y))
				{
					Vector2 chestPosition = new Vector2((float)(chest.x * 16 + 16), (float)(chest.y * 16 + 16));
					if ((chestPosition - Main.LocalPlayer.Center).Length() < itemSearchRange)
					{
						if (Main.netMode == NetmodeID.SinglePlayer || RecipeBrowser.chestContentsAvailable[chestIndex])
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
			recipeInfo.craftingIngredientsGrid.Clear();

			foreach (var item in recipeSlots)
			{
				item.selected = false;
			}

			var recipeslot = recipeSlots[index];

			recipeslot.selected = false;
			recipeslot.selected = true;
			selectedIndex = index;

			// TODO: Should these just be TrackIngredientSlots? It might be nice to see what you have, or have in nearby chests
			List<UIIngredientSlot> ingredients = new List<UIIngredientSlot>();
			Recipe recipe = Main.recipe[index];
			for (int i = 0; i < recipe.requiredItem.Count; i++)
			{
				UIIngredientSlot ingredient = new UIIngredientSlot(recipe.requiredItem[i].Clone(), i);
				//ingredient.Left.Pixels = 200 + (i % 5 * 40);
				//ingredient.Top.Pixels = (i / 5 * 40);

				ingredients.Add(ingredient);

				OverrideForGroups(recipe, ingredient.item);
				// TODO, stack?

				//recipeInfo.Append(ingredient);
			}

			recipeInfo.craftingIngredientsGrid.AddRange(ingredients);
			CraftUI.instance.SetRecipe(index);

			//for (int i = 0; i < Recipe.maxRequirements; i++)
			//{
			//	if (recipe.requiredTile[i] > 0)
			//	{
			//		var tileSlot = new UITileNoSlot(recipe.requiredTile[i], i, 0.75f);
			//		tileSlot.Left.Pixels = 10 + (i * 40);
			//		tileSlot.Top.Pixels = 0;
			//		recipeInfo.Append(tileSlot);
			//		recipeInfo.tileList.Add(tileSlot);
			//	}
			//}

		}

		public static void OverrideForGroups(Recipe recipe, Item item)
		{
			if (recipe.ProcessGroupsForText(item.type, out string nameOverride))
			{
				//Main.toolTip.name = name;
			}

			if (nameOverride != "")
			{
				item.SetNameOverride(nameOverride);
			}
		}

		public static string OverrideForGroups(Recipe recipe, int item)
		{
			if (recipe.ProcessGroupsForText(item, out string nameOverride))
			{
				//Main.toolTip.name = name;
			}

			return nameOverride;
		}

		// problem, how to detect that we need to request again?
		private const int itemSearchRange = 60 * 16; // this is in pixels : 400 too low

		private void NearbyIngredientsRadioBitton_OnSelectedChanged(object sender, EventArgs e)
		{
			UIRadioButton button = (UIRadioButton)sender;
			updateNeeded = true;
			if (!button.Selected) return;
			// Reset
			if (Main.netMode == NetmodeID.SinglePlayer) return; // we can skip all this in SP
			RecipeBrowser.chestContentsAvailable = new bool[1000];
			for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
			{
				Chest chest = Main.chest[chestIndex];
				if (chest != null && !Chest.IsLocked(chest.x, chest.y))
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

		// Prefix of FindRecipes calls this. Ignores calls from AdjTiles.
		internal void InvalidateExtendedCraft()
		{
			// Invalidate: Change any settings. Pickup any items, Move to new tiles?, 
			// manually flag as dirty.
			foreach (var slot in recipeSlots)
			{
				slot.craftPathNeeded = false;
				slot.craftPathCalculated = false;
				slot.craftPathsCalculated = false;
				if (slot.craftPathCalculationBegun)
					slot.craftPathCancellationTokenSource?.Cancel(); // won't cancel if not already begun, but TryDequeue will.
				slot.craftPathCalculationBegun = false;
				slot.craftPaths = null;
			}
			while (RecipeBrowser.instance.concurrentTasks.TryDequeue(out var _)) ;
			updateNeeded = true;
			//CraftUI.instance.craftPathList.Clear();
			CraftUI.instance.craftPathsUpToDate = false;

			if (SharedUI.instance.ObtainableFilter?.button.selected == true)
				SharedUI.instance.ObtainableFilter.button.LeftClick(new UIMouseEvent(null, Vector2.Zero));
		}
	}
}