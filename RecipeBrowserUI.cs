using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using RecipeBrowser.UIElements;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Collections.Generic;
using Terraria.ModLoader;
using System.Text;

namespace RecipeBrowser
{
	/*
	 *  ItemSlot            FilterName   CloseButton
	 *                      FilterDesc
	 *                      
	 *  
	 * 
	 * 
	 * 
	 */
	class RecipeBrowserUI : UIModState
	{
		static internal RecipeBrowserUI instance;

		internal UIDragablePanel mainPanel;
		internal UIDragablePanel favoritePanel;
		internal UIQueryItemSlot queryItem;
		internal UIRadioButton TileLookupRadioButton;
		//	internal UICheckbox inventoryFilter;
		internal UIHoverImageButton closeButton;
		internal NewUITextBox itemNameFilter;
		//	internal UIHoverImageButton clearNameFilterButton;
		internal NewUITextBox itemDescriptionFilter;
		internal UIPanel inlaidPanel;
		internal UIGrid recipeGrid;
		internal UIRecipeInfo recipeInfo;
		internal UIRadioButton NearbyIngredientsRadioBitton;
		internal UIRadioButton ItemChecklistRadioButton;
		internal UIRadioButtonGroup RadioButtonGroup;

		internal List<UIRecipeSlot> recipeSlots;
		internal List<UIRecipeSlot> favoritedRecipes;
		// TODO: Idea: automatically remove a Favorited recipe OnCraft?

		internal bool updateNeeded;
		internal int selectedIndex = -1;
		internal bool[] foundItems;
		internal string[] mods;

		private bool showFavoritePanel;
		public bool ShowFavoritePanel
		{
			get { return showFavoritePanel; }
			set
			{
				if (value)
				{
					Append(favoritePanel);
				}
				else
				{
					RemoveChild(favoritePanel);
				}
				showFavoritePanel = value;
			}
		}

		private bool showRecipeBrowser;
		public bool ShowRecipeBrowser
		{
			get { return showRecipeBrowser; }
			set
			{
				if (value)
				{
					Recipe.FindRecipes();
					Append(mainPanel);
				}
				else
				{
					RemoveChild(mainPanel);
				}
				showRecipeBrowser = value;
			}
		}

		public RecipeBrowserUI(UserInterface ui) : base(ui)
		{
			instance = this;
			mods = ModLoader.GetLoadedMods();
		}

		public override void OnInitialize()
		{
			mainPanel = new UIDragablePanel(true, true, true);
			mainPanel.SetPadding(6);
			mainPanel.Left.Set(400f, 0f);
			mainPanel.Top.Set(400f, 0f);
			mainPanel.Width.Set(415f, 0f);
			mainPanel.MinWidth.Set(415f, 0f);
			mainPanel.MaxWidth.Set(784f, 0f);
			mainPanel.Height.Set(350, 0f);
			mainPanel.MinHeight.Set(243, 0f);
			mainPanel.MaxHeight.Set(1000, 0f);
			mainPanel.BackgroundColor = new Color(73, 94, 171);
			//Append(mainPanel);

			queryItem = new UIQueryItemSlot(new Item());
			queryItem.Top.Set(2, 0f);
			queryItem.Left.Set(2, 0f);
			queryItem.OnItemChanged += () => { TileLookupRadioButton.SetDisabled(queryItem.item.createTile <= -1); };
			mainPanel.Append(queryItem);

			TileLookupRadioButton = new UIRadioButton("Tile", "");
			TileLookupRadioButton.Top.Set(42, 0f);
			TileLookupRadioButton.Left.Set(0, 0f);
			TileLookupRadioButton.SetText("  Tile");
			TileLookupRadioButton.OnSelectedChanged += (s, e) => { updateNeeded = true; };
			TileLookupRadioButton.SetDisabled(true);
			mainPanel.Append(TileLookupRadioButton);

			var modFilterButton = new UIHoverImageButtonMod(RecipeBrowser.instance.GetTexture("Images/filterMod"), "Mod Filter: All");
			modFilterButton.Left.Set(-208, 1f);
			modFilterButton.Top.Set(-4, 0f);
			modFilterButton.OnClick += ModFilterButton_OnClick;
			modFilterButton.OnRightClick += ModFilterButton_OnRightClick;
			mainPanel.Append(modFilterButton);

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
			itemNameFilter.Left.Set(-178, 1f);
			itemNameFilter.Width.Set(150, 0f);
			itemNameFilter.Height.Set(25, 0f);
			mainPanel.Append(itemNameFilter);

			Texture2D texture = RecipeBrowser.instance.GetTexture("UIElements/closeButton");
			closeButton = new UIHoverImageButton(texture, "Close");
			closeButton.OnClick += CloseButtonClicked;
			closeButton.Left.Set(-20f, 1f);
			closeButton.Top.Set(6f, 0f);
			mainPanel.Append(closeButton);

			itemDescriptionFilter = new NewUITextBox("Filter by tooltip");
			itemDescriptionFilter.OnTextChanged += () => { ValidateItemDescription(); updateNeeded = true; };
			itemDescriptionFilter.OnTabPressed += () => { itemNameFilter.Focus(); };
			itemDescriptionFilter.Top.Pixels = 30f;
			itemDescriptionFilter.Left.Set(-178, 1f);
			itemDescriptionFilter.Width.Set(150, 0f);
			itemDescriptionFilter.Height.Set(25, 0f);
			mainPanel.Append(itemDescriptionFilter);

			inlaidPanel = new UIPanel();
			inlaidPanel.SetPadding(6);
			inlaidPanel.Top.Pixels = 60;
			inlaidPanel.Width.Set(0, 1f);
			inlaidPanel.Height.Set(-60 - 121, 1f);
			inlaidPanel.BackgroundColor = Color.DarkBlue;
			mainPanel.Append(inlaidPanel);

			recipeGrid = new UIGrid();
			recipeGrid.Width.Set(-20f, 1f);
			recipeGrid.Height.Set(0, 1f);
			recipeGrid.ListPadding = 2f;
			inlaidPanel.Append(recipeGrid);

			var lootItemsScrollbar = new FixedUIScrollbar(userInterface);
			lootItemsScrollbar.SetView(100f, 1000f);
			lootItemsScrollbar.Height.Set(0, 1f);
			lootItemsScrollbar.Left.Set(-20, 1f);
			inlaidPanel.Append(lootItemsScrollbar);
			recipeGrid.SetScrollbar(lootItemsScrollbar);

			recipeInfo = new UIRecipeInfo();
			recipeInfo.Top.Set(-118, 1f);
			recipeInfo.Width.Set(0, 1f);
			recipeInfo.Height.Set(120, 0f);
			mainPanel.Append(recipeInfo);

			mainPanel.AddDragTarget(recipeInfo);
			mainPanel.AddDragTarget(RadioButtonGroup);

			favoritedRecipes = new List<UIRecipeSlot>();
			recipeSlots = new List<UIRecipeSlot>();

			favoritePanel = new UIDragablePanel();
			favoritePanel.SetPadding(6);
			favoritePanel.Left.Set(-310f, 0f);
			favoritePanel.HAlign = 1f;
			favoritePanel.Top.Set(90f, 0f);
			favoritePanel.Width.Set(415f, 0f);
			favoritePanel.MinWidth.Set(50f, 0f);
			favoritePanel.MaxWidth.Set(500f, 0f);
			favoritePanel.Height.Set(350, 0f);
			favoritePanel.MinHeight.Set(50, 0f);
			favoritePanel.MaxHeight.Set(300, 0f);
			//favoritePanel.BackgroundColor = new Color(73, 94, 171);
			favoritePanel.BackgroundColor = Color.Transparent;
			//Append(favoritePanel);

			updateNeeded = true;
			modIndex = mods.Length - 1;
		}

		//private void ItemChecklistRadioButton_OnRightClick(UIMouseEvent evt, UIElement listeningElement)
		//{
		//	// Switch modes.
		//	ItemChecklistRadioButton.SetHoverText("Mode: Only All Results");
		//}

		// problem, how to detect that we need to request again?
		const int itemSearchRange = 60 * 16; // this is in pixels : 400 too low
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

		// Vanilla ModLoader mod will act as "all"
		private static int modIndex;
		private void ModFilterButton_OnClick(UIMouseEvent evt, UIElement listeningElement)
		{
			UIHoverImageButtonMod button = (evt.Target as UIHoverImageButtonMod);
			button.hoverText = "Mod Filter: " + GetModFilterTooltip(true);
			UpdateModHoverImage(button);
			updateNeeded = true;
		}

		private void ModFilterButton_OnRightClick(UIMouseEvent evt, UIElement listeningElement)
		{
			UIHoverImageButtonMod button = (evt.Target as UIHoverImageButtonMod);
			button.hoverText = "Mod Filter: " + GetModFilterTooltip(false);
			UpdateModHoverImage(button);
			updateNeeded = true;
		}

		private void UpdateModHoverImage(UIHoverImageButtonMod button)
		{
			button.texture = null;
			Mod otherMod = ModLoader.GetMod(mods[modIndex]);
			if(otherMod != null && otherMod.TextureExists("icon"))
			{
				button.texture = otherMod.GetTexture("icon");
			}
		}

		private string GetModFilterTooltip(bool increment)
		{
			modIndex = increment ? (modIndex + 1) % mods.Length : (mods.Length + modIndex - 1) % mods.Length;
			return modIndex == mods.Length - 1 ? "All" : mods[modIndex];
		}

		private void ItemChecklistFilter_SelectedChanged(object sender, EventArgs e)
		{
			if ((sender as UIRadioButton).Selected)
			{
				object result = RecipeBrowser.itemChecklistInstance.Call("RequestFoundItems");
				if (result is string)
				{
					Main.NewText("Error, ItemChecklist said: " + result);
				}
				else if (result is bool[])
				{
					foundItems = (result as bool[]);
				}
			}
			else
			{
				foundItems = null;
			}
			updateNeeded = true;
		}

		int newestItem = 0;
		public void NewItemFound(int type)
		{
			newestItem = type;
			updateNeeded = true;
		}

		private void CloseButtonClicked(UIMouseEvent evt, UIElement listeningElement)
		{
			// we should have a way for the button itself to be unclicked and notify parent.
			RadioButtonGroup.ButtonClicked(0);
			RecipeBrowserUI.instance.ShowRecipeBrowser = !RecipeBrowserUI.instance.ShowRecipeBrowser;

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

			updateNeeded = true;
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

		internal void FavoriteChange(UIRecipeSlot slot)
		{
			if (favoritedRecipes.Contains(slot))
			{
				favoritedRecipes.Remove(slot);
			}
			if (slot.favorited)
			{
				favoritedRecipes.Add(slot);
			}
			UpdateFavoritedPanel();
		}

		internal void UpdateFavoritedPanel()
		{
			ShowFavoritePanel = favoritedRecipes.Count > 0;
			favoritePanel.RemoveAllChildren();

			UIGrid list = new UIGrid();
			list.Width.Set(0, 1f);
			list.Height.Set(0, 1f);
			list.ListPadding = 5f;
			favoritePanel.Append(list);
			favoritePanel.AddDragTarget(list);
			favoritePanel.AddDragTarget(list._innerList);
			int width = 1;
			int height = 0;
			int order = 1;
			foreach (var item in favoritedRecipes)
			{
				Recipe r = Main.recipe[item.index];
				UIRecipeProgress s = new UIRecipeProgress(item.index, r, order);
				order++;
				s.Recalculate();
				var a = s.GetInnerDimensions();
				s.Width.Precent = 1;
				list.Add(s);
				height += (int)(a.Height + list.ListPadding);
				width = Math.Max(width, (int)a.Width);
				favoritePanel.AddDragTarget(s);
			}
			favoritePanel.Height.Pixels = height + favoritePanel.PaddingBottom + favoritePanel.PaddingTop - list.ListPadding;
			favoritePanel.Width.Pixels = width;
			favoritePanel.Recalculate();

			var scrollbar = new InvisibleFixedUIScrollbar(userInterface);
			scrollbar.SetView(100f, 1000f);
			scrollbar.Height.Set(0, 1f);
			scrollbar.Left.Set(-20, 1f);
			favoritePanel.Append(scrollbar);
			list.SetScrollbar(scrollbar);

			Recipe.FindRecipes();
		}

		internal void UpdateGrid()
		{
			if (Recipe.numRecipes != recipeSlots.Count)
			{
				recipeSlots.Clear();
				for (int i = 0; i < Recipe.numRecipes; i++)
				{
					recipeSlots.Add(new UIRecipeSlot(i));
				}
			}

			if (!updateNeeded) { return; }
			updateNeeded = false;

			List<int> groups = new List<int>();
			if (queryItem.item.stack > 0 && !TileLookupRadioButton.Selected)
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
			if (modIndex != mods.Length - 1)
			{
				if (recipe.createItem.modItem == null)
				{
					return false;
				}
				if (recipe.createItem.modItem.mod.Name != mods[modIndex])
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
			if (foundItems != null)
			{
				for (int i = 0; i < Recipe.maxRequirements; i++)
				{
					if (recipe.requiredItem[i].type > 0)
					{
						if (!foundItems[recipe.requiredItem[i].type])
						{
							return false;
						}
					}
				}
				// filter out recipes that make things I've already obtained
				if (foundItems[recipe.createItem.type])
				{
					return false;
				}
			}
			//if (inventoryFilter.Selected)
			//{

			//}
			if (!queryItem.item.IsAir)
			{
				if (TileLookupRadioButton.Selected)
				{
					int type = queryItem.item.createTile;
					if (!recipe.requiredTile.Any(ing => ing == type))
					{
						return false;
					}
				}
				else
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

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			// additional updates.
			UpdateGrid();
		}

		internal void ItemReceived(Item item)
		{
			var removes = favoritedRecipes.Where(x => x.item.type == item.type && x.item.maxStack == 1).ToList();

			foreach (var recipe in removes)
			{
				recipe.favorited = false;
				FavoriteChange(recipe);
			}
		}
	}
}
