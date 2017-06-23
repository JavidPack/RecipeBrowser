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
		internal UIQueryItemSlot queryItem;
		//	internal UICheckbox inventoryFilter;
		internal UIHoverImageButton closeButton;
		internal NewUITextBox itemNameFilter;
		//	internal UIHoverImageButton clearNameFilterButton;
		internal NewUITextBox itemDescriptionFilter;
		internal UIPanel inlaidPanel;
		internal UIGrid recipeGrid;
		internal UIRecipeInfo recipeInfo;
		internal UIRadioButton NearbyIngredientsRadioBitton;
		internal UIRadioButtonGroup RadioButtonGroup;

		internal bool updateNeeded;
		internal int selectedIndex = -1;
		internal bool[] foundItems;
		internal string[] mods;

		public RecipeBrowserUI(UserInterface ui) : base(ui)
		{
			instance = this;
			mods = ModLoader.GetLoadedMods();
		}

		public override void OnInitialize()
		{
			mainPanel = new UIDragablePanel(true);
			mainPanel.SetPadding(6);
			mainPanel.Left.Set(400f, 0f);
			mainPanel.Top.Set(400f, 0f);
			mainPanel.Width.Set(415f, 0f);
			mainPanel.Height.Set(350, 0f);
			mainPanel.BackgroundColor = new Color(73, 94, 171);
			Append(mainPanel);

			queryItem = new UIQueryItemSlot(new Item());
			queryItem.Top.Set(8, 0f);
			queryItem.Left.Set(2, 0f);
			mainPanel.Append(queryItem);

			var modFilterButton = new UIHoverImageButton(RecipeBrowser.instance.GetTexture("Images/filterMod"), "Mod Filter: All");
			//modFilterButton.Top.Set(37, 0f);
			//modFilterButton.Left.Set(10, 0f);
			//modFilterButton.Left.Set(379, 0f);
			//modFilterButton.Top.Set(30, 0f);
			modFilterButton.Left.Set(194, 0f);
			modFilterButton.Top.Set(-4, 0f);
			modFilterButton.OnClick += ModFilterButton_OnClick;
			modFilterButton.OnRightClick += ModFilterButton_OnRightClick;
			mainPanel.Append(modFilterButton);

			RadioButtonGroup = new UIRadioButtonGroup();
			RadioButtonGroup.Left.Pixels = 45;
			RadioButtonGroup.Width.Set(180, 0f);
			UIRadioButton AllRecipesRadioButton = new UIRadioButton("All Recipes", "");
			NearbyIngredientsRadioBitton = new UIRadioButton("Nearby Chests", "Click to Refresh");
			UIRadioButton ItemChecklistRadioButton = new UIRadioButton("Item Checklist Only", "");
			RadioButtonGroup.Add(AllRecipesRadioButton);
			RadioButtonGroup.Add(NearbyIngredientsRadioBitton);
			RadioButtonGroup.Add(ItemChecklistRadioButton);
			mainPanel.Append(RadioButtonGroup);
			AllRecipesRadioButton.Selected = true;

			NearbyIngredientsRadioBitton.OnSelectedChanged += NearbyIngredientsRadioBitton_OnSelectedChanged;

			// TODO, make sure Oninitialize is called after reload. -- It is
			//var itemChecklist = ModLoader.GetMod("ItemChecklist");
			//if (itemChecklist != null)
			//{
			//	UICheckbox itemChecklistFilter = new UICheckbox("Item Checklist Filter");
			//	itemChecklistFilter.Left.Pixels = 50;
			//	itemChecklistFilter.SelectedChanged += ItemChecklistFilter_SelectedChanged;
			//	mainPanel.Append(itemChecklistFilter);
			//}
			//else
			//{
			//}
			ItemChecklistRadioButton.SetDisabled();

			//inventoryFilter = new UICheckbox("Use Inventory");
			//.Left.Pixels = 50;
			//mainPanel.Append(inventoryFilter);

			itemNameFilter = new NewUITextBox("Filter by Name");
			//itemNameFilter.TextColor = Color.Black;
			//itemNameFilter.SetPadding(0);
			itemNameFilter.OnTextChanged += () => { ValidateItemFilter(); updateNeeded = true; };
			itemNameFilter.Top.Pixels = 0f;
			itemNameFilter.Left.Pixels = 225f;
			//itemNameFilter.Left.Set(text2.GetInnerDimensions().Width, 0f);
			itemNameFilter.Width.Set(150, 0f);
			itemNameFilter.Height.Set(25, 0f);
			//searchFilter.VAlign = 0.5f;
			mainPanel.Append(itemNameFilter);

			Texture2D texture = RecipeBrowser.instance.GetTexture("UIElements/closeButton");
			closeButton = new UIHoverImageButton(texture, "Close");
			closeButton.OnClick += CloseButtonClicked;
			closeButton.Left.Set(-20f, 1f);
			closeButton.Top.Set(6f, 0f);
			mainPanel.Append(closeButton);

			itemDescriptionFilter = new NewUITextBox("Filter by tooltip");
			//itemNameFilter.SetPadding(0);
			itemDescriptionFilter.OnTextChanged += () => { ValidateItemDescription(); updateNeeded = true; };
			itemDescriptionFilter.Top.Pixels = 30f;
			itemDescriptionFilter.Left.Pixels = 225f;
			//itemNameFilter.Left.Set(text2.GetInnerDimensions().Width, 0f);
			itemDescriptionFilter.Width.Set(150, 0f);
			itemDescriptionFilter.Height.Set(25, 0f);
			//searchFilter.VAlign = 0.5f;
			mainPanel.Append(itemDescriptionFilter);

			inlaidPanel = new UIPanel();
			inlaidPanel.SetPadding(6);
			inlaidPanel.Top.Pixels = 60;
			//inlaidPanel.Width.Set(-25f, 1f);
			inlaidPanel.Width.Set(0, 1f);
			//inlaidPanel.Height.Set(155, 0f);
			// Use to be 155, now is 100% minus top minus what is below.
			inlaidPanel.Height.Set(-60 - 121, 1f);
			inlaidPanel.BackgroundColor = Color.DarkBlue;
			mainPanel.Append(inlaidPanel);

			recipeGrid = new UIGrid(9);
			//recipeGrid.Top.Pixels = 60;
			recipeGrid.Width.Set(-25f, 1f);
			//recipeGrid.Height.Set(130, 0f);
			recipeGrid.Height.Set(0, 1f);
			recipeGrid.ListPadding = 2f;
			inlaidPanel.Append(recipeGrid);

			var lootItemsScrollbar = new FixedUIScrollbar(userInterface);
			lootItemsScrollbar.SetView(100f, 1000f);
			//lootItemsScrollbar.Top.Pixels = 60;
			//lootItemsScrollbar.Height.Set(130, 0f);
			lootItemsScrollbar.Height.Set(0, 1f);
			lootItemsScrollbar.Left.Set(-20, 1f);
			inlaidPanel.Append(lootItemsScrollbar);
			recipeGrid.SetScrollbar(lootItemsScrollbar);

			recipeInfo = new UIRecipeInfo();
			recipeInfo.Top.Set(-118, 1f);
			recipeInfo.Width.Set(0, 1f);
			recipeInfo.Height.Set(120, 0f);
			mainPanel.Append(recipeInfo);

			updateNeeded = true;
			modIndex = mods.Length - 1;
		}

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
			UIHoverImageButton button = (evt.Target as UIHoverImageButton);
			button.hoverText = "Mod Filter: " + GetModFilterTooltip(true);
			updateNeeded = true;
		}

		private void ModFilterButton_OnRightClick(UIMouseEvent evt, UIElement listeningElement)
		{
			UIHoverImageButton button = (evt.Target as UIHoverImageButton);
			button.hoverText = "Mod Filter: " + GetModFilterTooltip(false);
			updateNeeded = true;
		}

		private string GetModFilterTooltip(bool increment)
		{
			modIndex = increment ? (modIndex + 1) % mods.Length : (mods.Length + modIndex - 1) % mods.Length;
			return modIndex == mods.Length - 1 ? "All" : mods[modIndex];
		}

		private void ItemChecklistFilter_SelectedChanged(object sender, EventArgs e)
		{
			if ((sender as UICheckbox).Selected)
			{
				object result = ModLoader.GetMod("ItemChecklist").Call("RequestFoundItems");
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
			RecipeBrowser.instance.recipeBrowserTool.visible = !RecipeBrowser.instance.recipeBrowserTool.visible;

			if (queryItem.real && queryItem.item.stack > 0)
			{
				// This causes items to get a new modifier. Oops
				//Main.player[Main.myPlayer].QuickSpawnItem(lookupItemSlot.item.type, lookupItemSlot.item.stack);
				//lookupItemSlot.item.SetDefaults(0);

				Player player = Main.player[Main.myPlayer];
				queryItem.item.position = player.Center;
				Item item = player.GetItem(player.whoAmI, queryItem.item, false, true);
				if (item.stack > 0)
				{
					int num = Item.NewItem((int)player.position.X, (int)player.position.Y, player.width, player.height, item.type, item.stack, false, (int)queryItem.item.prefix, true, false);
					Main.item[num].newAndShiny = false;
					if (Main.netMode == 1)
					{
						NetMessage.SendData(21, -1, -1, null, num, 1f, 0f, 0f, 0, 0, 0);
					}
				}
				queryItem.item = new Item();
			}

			updateNeeded = true;
		}

		internal void SetRecipe(int index)
		{
			selectedIndex = -1;
			recipeInfo.RemoveAllChildren();
			foreach (var item in recipeGrid._items)
			{
				var recipeslot = (item as UIRecipeSlot);
				recipeslot.backgroundTexture = recipeslot.recentlyDiscovered ? UIRecipeSlot.recentlyDiscoveredBackgroundTexture : UIRecipeSlot.defaultBackgroundTexture;
				if (recipeslot.index == index)
				{
					recipeslot.backgroundTexture = UIRecipeSlot.selectedBackgroundTexture;
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

		internal void UpdateGrid()
		{
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

			recipeGrid.Clear();
			for (int i = 0; i < Recipe.numRecipes; i++)
			{
				if (PassRecipeFilters(Main.recipe[i], groups))
				// all the filters
				//if (Main.projName[i].ToLower().IndexOf(searchFilter.Text, StringComparison.OrdinalIgnoreCase) != -1)
				{
					var box = new UIRecipeSlot(i);
					// 
					if (newestItem > 0)
					{
						Recipe recipe = Main.recipe[i];
						if (recipe.requiredItem.Any(x => x.type == newestItem))
						{
							box.recentlyDiscovered = true;
							box.backgroundTexture = UIRecipeSlot.recentlyDiscoveredBackgroundTexture;
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
	}
}
