using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RecipeBrowser.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace RecipeBrowser
{
	internal class ItemCatalogueUI
	{
		internal static string RBText(string key, string category = "ItemCatalogueUI") => RecipeBrowser.RBText(category, key);

		internal static ItemCatalogueUI instance;
		internal static Color color = Color.DarkGreen;

		internal UIPanel mainPanel;
		internal UIPanel sortsAndFiltersPanel;
		internal UIHorizontalGrid categoriesGrid;
		internal UIHorizontalGrid subCategorySortsFiltersGrid;
		internal InvisibleFixedUIHorizontalScrollbar lootGridScrollbar2;
		internal UIPanel itemGridPanel;
		internal UIGrid itemGrid;
		internal bool updateNeeded;
		internal NewUITextBox itemNameFilter;
		internal NewUITextBox itemDescriptionFilter;
		internal List<UIItemCatalogueItemSlot> itemSlots;
		internal bool[] craftResults;
		internal bool[] isLoot;
		internal List<UIElement> additionalDragTargets;
		internal UICheckbox CraftedRadioButton;
		internal UICheckbox LootRadioButton;
		internal UICheckbox UnobtainedRadioButton;

		private Sort selectedSort;
		internal Sort SelectedSort
		{
			get { return selectedSort; }
			set
			{
				if (selectedSort != value)
					updateNeeded = true;
				selectedSort = value;
			}
		}

		private Category selectedCategory;
		internal Category SelectedCategory
		{
			get { return selectedCategory; }
			set
			{
				if (selectedCategory != value)
					updateNeeded = true;
				selectedCategory = value;
				if (selectedCategory != null && selectedCategory.sorts.Count > 0)
					SelectedSort = selectedCategory.sorts[0];
				else if (selectedCategory != null && selectedCategory.parent != null && selectedCategory.parent.sorts.Count > 0)
					SelectedSort = selectedCategory.parent.sorts[0];
			}
		}

		public ItemCatalogueUI()
		{
			instance = this;
			itemSlots = new List<UIItemCatalogueItemSlot>();
			additionalDragTargets = new List<UIElement>();
		}

		internal UIElement CreateItemCataloguePanel()
		{
			mainPanel = new ItemCatalogueUIPanel();
			mainPanel.SetPadding(6);
			mainPanel.BackgroundColor = color;

			mainPanel.Top.Set(20, 0f);
			mainPanel.Height.Set(-20, 1f);
			mainPanel.Width.Set(0, 1f);

			/*var inlaidPanel = new UIPanel();
			inlaidPanel.SetPadding(6);
			inlaidPanel.Width.Set(0, .8f);
			inlaidPanel.Height.Set(65, 0f);
			inlaidPanel.HAlign = 0.5f;
			inlaidPanel.VAlign = 0.5f;
			inlaidPanel.BackgroundColor = Color.DarkBlue;
			mainPanel.Append(inlaidPanel);

			var text = new UIText("Coming Soon", 1.8f);
			text.HAlign = 0.5f;
			text.VAlign = 0.5f;
			inlaidPanel.Append(text);*/

			itemNameFilter = new NewUITextBox(RBText("FilterByName", "Common"));
			itemNameFilter.OnTextChanged += () => { ValidateItemFilter(); updateNeeded = true; };
			itemNameFilter.OnTabPressed += () => { itemDescriptionFilter.Focus(); };
			itemNameFilter.Top.Pixels = 0f;
			itemNameFilter.Left.Set(-152, 1f);
			itemNameFilter.Width.Set(150, 0f);
			itemNameFilter.Height.Set(25, 0f);
			mainPanel.Append(itemNameFilter);

			itemDescriptionFilter = new NewUITextBox(RBText("FilterByTooltip", "Common"));
			itemDescriptionFilter.OnTextChanged += () => { ValidateItemDescription(); updateNeeded = true; };
			itemDescriptionFilter.OnTabPressed += () => { itemNameFilter.Focus(); };
			itemDescriptionFilter.Top.Pixels = 30f;
			itemDescriptionFilter.Left.Set(-152, 1f);
			itemDescriptionFilter.Width.Set(150, 0f);
			itemDescriptionFilter.Height.Set(25, 0f);
			mainPanel.Append(itemDescriptionFilter);

			// Sorts
			// Filters: Categories?
			// Craft and Loot Badges as well!
			// Hide with alt click?
			// show hidden toggle
			// Favorite: Only affects sort order?

			sortsAndFiltersPanel = new UIPanel();
			sortsAndFiltersPanel.SetPadding(6);
			sortsAndFiltersPanel.Top.Set(0, 0f);
			sortsAndFiltersPanel.Width.Set(-275, 1);
			sortsAndFiltersPanel.Height.Set(60, 0f);
			sortsAndFiltersPanel.BackgroundColor = Color.LightSeaGreen;
			//sortsAndFiltersPanel.SetPadding(4);
			mainPanel.Append(sortsAndFiltersPanel);
			additionalDragTargets.Add(sortsAndFiltersPanel);
			//SetupSortsAndCategories();

			//PopulateSortsAndFiltersPanel();

			CraftedRadioButton = new UICheckbox(RBText("Crafted"), RBText("OnlyShowCraftedItems"));
			CraftedRadioButton.Top.Set(0, 0f);
			CraftedRadioButton.Left.Set(-270, 1f);
			CraftedRadioButton.OnSelectedChanged += (a, b) => updateNeeded = true;
			mainPanel.Append(CraftedRadioButton);

			LootRadioButton = new UICheckbox(RBText("Loot"), RBText("ShowOnlyLootItems"));
			LootRadioButton.Top.Set(20, 0f);
			LootRadioButton.Left.Set(-270, 1f);
			LootRadioButton.OnSelectedChanged += (a, b) => updateNeeded = true;
			mainPanel.Append(LootRadioButton);

			UnobtainedRadioButton = new UICheckbox(RBText("Unobtained"), "???");
			UnobtainedRadioButton.Top.Set(40, 0f);
			UnobtainedRadioButton.Left.Set(-270, 1f);
			UnobtainedRadioButton.OnSelectedChanged += (a, b) => { updateNeeded = true; /*HasLootRadioButton.Selected = true;*/ };
			mainPanel.Append(UnobtainedRadioButton);

			if (RecipeBrowser.itemChecklistInstance != null)
			{
				UnobtainedRadioButton.OnSelectedChanged += UnobtainedRadioButton_OnSelectedChanged;
				UnobtainedRadioButton.SetHoverText(RBText("OnlyUnobtainedItems"));
			}
			else
			{
				UnobtainedRadioButton.SetDisabled();
				UnobtainedRadioButton.SetHoverText(RBText("InstallItemChecklistToUse", "Common"));
			}

			//updateNeeded = true;

			itemGridPanel = new UIPanel();
			itemGridPanel.SetPadding(6);
			itemGridPanel.Top.Pixels = 60;
			itemGridPanel.Width.Set(0, 1f);
			itemGridPanel.Left.Set(0, 0f);
			itemGridPanel.Height.Set(-76, 1f);
			itemGridPanel.BackgroundColor = Color.CornflowerBlue;
			mainPanel.Append(itemGridPanel);

			itemGrid = new UIGrid();
			itemGrid.alternateSort = ItemGridSort;
			itemGrid.Width.Set(-20, 1f);
			itemGrid.Height.Set(0, 1f);
			itemGrid.ListPadding = 2f;
			itemGrid.OnScrollWheel += RecipeBrowserUI.OnScrollWheel_FixHotbarScroll;
			itemGridPanel.Append(itemGrid);

			var itemGridScrollbar = new FixedUIScrollbar(RecipeBrowserUI.instance.userInterface);
			itemGridScrollbar.SetView(100f, 1000f);
			itemGridScrollbar.Height.Set(0, 1f);
			itemGridScrollbar.Left.Set(-20, 1f);
			itemGridPanel.Append(itemGridScrollbar);
			itemGrid.SetScrollbar(itemGridScrollbar);

			//"2x LMB: View Recipes  ---  2x RMB: See dropping NPCs"
			UIText text = new UIText(RBText("BottomInstructions"), 0.85f);
			text.Top.Set(-14, 1f);
			text.HAlign = 0.5f;
			mainPanel.Append(text);
			additionalDragTargets.Add(text);

			return mainPanel;
		}

		List<Filter> availableFilters;
		private void PopulateSortsAndFiltersPanel()
		{
			var availableSorts = new List<Sort>(sorts);
			availableFilters = new List<Filter>(filters);
			//sortsAndFiltersPanel.RemoveAllChildren();
			if (subCategorySortsFiltersGrid != null)
			{
				sortsAndFiltersPanel.RemoveChild(subCategorySortsFiltersGrid);
				sortsAndFiltersPanel.RemoveChild(lootGridScrollbar2);
			}

			bool doTopRow = false;
			if (categoriesGrid == null)
			{
				doTopRow = true;

				categoriesGrid = new UIHorizontalGrid();
				categoriesGrid.Width.Set(0, 1f);
				categoriesGrid.Height.Set(26, 0f);
				categoriesGrid.ListPadding = 2f;
				categoriesGrid.OnScrollWheel += RecipeBrowserUI.OnScrollWheel_FixHotbarScroll;
				sortsAndFiltersPanel.Append(categoriesGrid);
				categoriesGrid.drawArrows = true;

				var lootGridScrollbar = new InvisibleFixedUIHorizontalScrollbar(RecipeBrowserUI.instance.userInterface);
				lootGridScrollbar.SetView(100f, 1000f);
				lootGridScrollbar.Width.Set(0, 1f);
				lootGridScrollbar.Top.Set(0, 0f);
				sortsAndFiltersPanel.Append(lootGridScrollbar);
				categoriesGrid.SetScrollbar(lootGridScrollbar);
			}

			subCategorySortsFiltersGrid = new UIHorizontalGrid();
			subCategorySortsFiltersGrid.Width.Set(0, 1f);
			subCategorySortsFiltersGrid.Top.Set(26, 0f);
			subCategorySortsFiltersGrid.Height.Set(26, 0f);
			subCategorySortsFiltersGrid.ListPadding = 2f;
			subCategorySortsFiltersGrid.OnScrollWheel += RecipeBrowserUI.OnScrollWheel_FixHotbarScroll;
			sortsAndFiltersPanel.Append(subCategorySortsFiltersGrid);
			subCategorySortsFiltersGrid.drawArrows = true;

			lootGridScrollbar2 = new InvisibleFixedUIHorizontalScrollbar(RecipeBrowserUI.instance.userInterface);
			lootGridScrollbar2.SetView(100f, 1000f);
			lootGridScrollbar2.Width.Set(0, 1f);
			lootGridScrollbar2.Top.Set(28, 0f);
			sortsAndFiltersPanel.Append(lootGridScrollbar2);
			subCategorySortsFiltersGrid.SetScrollbar(lootGridScrollbar2);

			//sortsAndFiltersPanelGrid = new UIGrid();
			//sortsAndFiltersPanelGrid.Width.Set(0, 1);
			//sortsAndFiltersPanelGrid.Height.Set(0, 1);
			//sortsAndFiltersPanel.Append(sortsAndFiltersPanelGrid);

			//sortsAndFiltersPanelGrid2 = new UIGrid();
			//sortsAndFiltersPanelGrid2.Width.Set(0, 1);
			//sortsAndFiltersPanelGrid2.Height.Set(0, 1);
			//sortsAndFiltersPanel.Append(sortsAndFiltersPanelGrid2);

			int count = 0;

			var visibleCategories = new List<Category>();
			var visibleSubCategories = new List<Category>();
			int left = 0;
			foreach (var category in categories)
			{
				category.button.selected = false;
				visibleCategories.Add(category);
				bool meOrChildSelected = SelectedCategory == category;
				foreach (var subcategory in category.subCategories)
				{
					subcategory.button.selected = false;
					meOrChildSelected |= subcategory == SelectedCategory;
				}
				if (meOrChildSelected)
				{
					visibleSubCategories.AddRange(category.subCategories);
					category.button.selected = true;
				}
			}

			if (doTopRow)
				foreach (var category in visibleCategories)
				{
					var container = new UISortableElement(++count);
					container.Width.Set(24, 0);
					container.Height.Set(24, 0);
					//category.button.Left.Pixels = left;
					//if (category.parent != null)
					//	container.OrderIndex
					//	category.button.Top.Pixels = 12;
					//sortsAndFiltersPanel.Append(category.button);
					container.Append(category.button);
					categoriesGrid.Add(container);
					left += 26;
				}

			//UISortableElement spacer = new UISortableElement(++count);
			//spacer.Width.Set(0, 1);
			//sortsAndFiltersPanelGrid2.Add(spacer);

			foreach (var category in visibleSubCategories)
			{
				var container = new UISortableElement(++count);
				container.Width.Set(24, 0);
				container.Height.Set(24, 0);
				container.Append(category.button);
				subCategorySortsFiltersGrid.Add(container);
				left += 26;
			}

			if (visibleSubCategories.Count > 0)
			{
				var container2 = new UISortableElement(++count);
				container2.Width.Set(24, 0);
				container2.Height.Set(24, 0);
				var image = new UIImage(RecipeBrowser.instance.GetTexture("Images/spacer"));
				//image.Left.Set(6, 0);
				image.HAlign = 0.5f;
				container2.Append(image);
				subCategorySortsFiltersGrid.Add(container2);
			}

			// add to sorts here
			if (SelectedCategory != null)
			{
				SelectedCategory.button.selected = true;
				SelectedCategory.ParentAddToSorts(availableSorts);
			}

			left = 0;
			foreach (var sort in availableSorts)
			{
				sort.button.selected = false;
				if (SelectedSort == sort) // TODO: SelectedSort no longwe valid
					sort.button.selected = true;
				//sort.button.Left.Pixels = left;
				//sort.button.Top.Pixels = 24;
				//sort.button.Width
				//grid.Add(sort.button);
				var container = new UISortableElement(++count);
				container.Width.Set(24, 0);
				container.Height.Set(24, 0);
				container.Append(sort.button);
				subCategorySortsFiltersGrid.Add(container);
				//sortsAndFiltersPanel.Append(sort.button);
				left += 26;
			}
			if (!availableSorts.Contains(SelectedSort))
			{
				availableSorts[0].button.selected = true;
				SelectedSort = availableSorts[0];
				updateNeeded = false;
			}

			if (filters.Count > 0)
			{
				var container2 = new UISortableElement(++count);
				container2.Width.Set(24, 0);
				container2.Height.Set(24, 0);
				var image = new UIImage(RecipeBrowser.instance.GetTexture("Images/spacer"));
				image.HAlign = 0.5f;
				container2.Append(image);
				subCategorySortsFiltersGrid.Add(container2);

				foreach (var item in filters)
				{
					var container = new UISortableElement(++count);
					container.Width.Set(24, 0);
					container.Height.Set(24, 0);
					container.Append(item.button);
					subCategorySortsFiltersGrid.Add(container);
				}
			}
		}

		private int ItemGridSort(UIElement x, UIElement y)
		{
			UIItemCatalogueItemSlot a = x as UIItemCatalogueItemSlot;
			UIItemCatalogueItemSlot b = y as UIItemCatalogueItemSlot;
			if (SelectedSort != null)
				return SelectedSort.sort(a.item, b.item);
			return a.itemType.CompareTo(b.itemType);
		}

		private void UnobtainedRadioButton_OnSelectedChanged(object sender, EventArgs e)
		{
			if ((sender as UICheckbox).Selected)
			{
				RecipeBrowserUI.instance.QueryItemChecklist();
			}
			else
			{
				//RecipeBrowserUI.instance.foundItems = null;
			}
			updateNeeded = true;
		}

		private void ValidateItemFilter()
		{
			if (itemNameFilter.currentString.Length > 0)
			{
				bool found = false;
				foreach (var itemSlot in itemSlots)
				{
					if (itemSlot.item.Name.IndexOf(itemNameFilter.currentString, StringComparison.OrdinalIgnoreCase) != -1)
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
			updateNeeded = true;
		}

		internal void Update()
		{
			if (!updateNeeded) { return; }
			updateNeeded = false;

			if (itemSlots.Count == 0)
			{
				// should only happen once
				craftResults = new bool[ItemLoader.ItemCount];
				isLoot = new bool[ItemLoader.ItemCount];
				itemSlots.Clear();
				for (int type = 1; type < ItemLoader.ItemCount; type++)
				{
					Item item = new Item();
					item.SetDefaults(type, false); // 300 ms vs 30 ms
					if (item.type == 0)
						continue;
					var slot = new UIItemCatalogueItemSlot(item);
					itemSlots.Add(slot);
				}

				for (int i = 0; i < Recipe.numRecipes; i++)
				{
					Recipe recipe = Main.recipe[i];
					craftResults[recipe.createItem.type] = true;
				}

				foreach (var kvp in LootCache.instance.lootInfos)
				{
					//if (kvp.Key.id == 0 && kvp.Key.mod == "Terraria")
					//	Console.WriteLine();
					int id = kvp.Key.GetID();
					if (id > 0)
						isLoot[id] = true;
				}
			}

			// Delay this so we can integrate mod categories.
			if (sorts == null)
			{
				SetupSortsAndCategories();
			}

			PopulateSortsAndFiltersPanel();

			itemGrid.Clear();
			foreach (var slot in itemSlots)
			{
				if (PassItemFilters(slot))
				{
					itemGrid._items.Add(slot);
					itemGrid._innerList.Append(slot);
				}
			}
			itemGrid.UpdateOrder();
			itemGrid._innerList.Recalculate();
		}

		internal void SetItem(UIItemCatalogueItemSlot slot)
		{
			foreach (var item in itemSlots)
			{
				item.selected = false;
			}
			slot.selected = true;
		}

		private bool PassItemFilters(UIItemCatalogueItemSlot slot)
		{
			if (RecipeBrowserUI.modIndex != RecipeBrowserUI.instance.mods.Length - 1)
			{
				if (slot.item.modItem == null)
				{
					return false;
				}
				if (slot.item.modItem.mod.Name != RecipeBrowserUI.instance.mods[RecipeBrowserUI.modIndex])
				{
					return false;
				}
			}

			if (CraftedRadioButton.Selected)
			{
				if (!craftResults[slot.item.type])
					return false;
			}

			if (LootRadioButton.Selected)
			{
				if (!isLoot[slot.item.type])
					return false;
			}

			if (UnobtainedRadioButton.Selected && RecipeBrowserUI.instance.foundItems != null)
			{
				if (RecipeBrowserUI.instance.foundItems[slot.item.type])
					return false;
			}

			if (SelectedCategory != null)
			{
				if (!SelectedCategory.belongs(slot.item) && !SelectedCategory.subCategories.Any(x => x.belongs(slot.item)))
					return false;
			}


			foreach (var filter in availableFilters)
			{
				if (filter.button.selected)
					if (!filter.belongs(slot.item))
						return false;
			}

			if (slot.item.Name.IndexOf(itemNameFilter.currentString, StringComparison.OrdinalIgnoreCase) == -1)
				return false;

			if (itemDescriptionFilter.currentString.Length > 0)
			{
				if ((slot.item.ToolTip != null && GetTooltipsAsString(slot.item.ToolTip).IndexOf(itemDescriptionFilter.currentString, StringComparison.OrdinalIgnoreCase) != -1) /*|| (recipe.createItem.toolTip2 != null && recipe.createItem.toolTip2.ToLower().IndexOf(itemDescriptionFilter.Text, StringComparison.OrdinalIgnoreCase) != -1)*/)
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

		private string GetTooltipsAsString(ItemTooltip toolTip)
		{
			StringBuilder sb = new StringBuilder();
			for (int j = 0; j < toolTip.Lines; j++)
			{
				sb.Append(toolTip.GetLine(j) + "\n");
			}
			return sb.ToString().ToLower();
		}

		List<Category> categories;
		List<Filter> filters;
		List<Sort> sorts;
		private void SetupSortsAndCategories()
		{
			sorts = new List<Sort>()
			{
				new Sort("ItemID", "Images/sortItemID", (x,y)=>x.type.CompareTo(y.type)),
				new Sort("Value", "Images/sortValue", (x,y)=>x.value.CompareTo(y.value)),
				new Sort("Alphabetical", "Images/sortAZ", (x,y)=>x.Name.CompareTo(y.Name)),
			};

			Texture2D materialsIcon = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.SpellTome] }, 24, 24);
			filters = new List<Filter>()
			{
				new Filter("Materials", x=>x.material, materialsIcon),
			};

			// TODOS: Vanity armor, grapple, cart, potions buffs
			// 24x24 pixels

			List<int> yoyos = new List<int>();
			for (int i = 0; i < ItemID.Sets.Yoyo.Length; ++i)
			{
				if (ItemID.Sets.Yoyo[i])
				{
					yoyos.Add(i);
				}
			}

			Texture2D smallMelee = ResizeImage(Main.itemTexture[ItemID.GoldBroadsword], 24, 24);
			Texture2D smallYoyo = ResizeImage(Main.itemTexture[Main.rand.Next(yoyos)], 24, 24); //Main.rand.Next(ItemID.Sets.Yoyo) ItemID.Yelets
			Texture2D smallMagic = ResizeImage(Main.itemTexture[ItemID.GoldenShower], 24, 24);
			Texture2D smallRanged = ResizeImage(Main.itemTexture[ItemID.FlintlockPistol], 24, 24);
			Texture2D smallThrown = ResizeImage(Main.itemTexture[ItemID.Shuriken], 24, 24);
			Texture2D smallSummon = ResizeImage(Main.itemTexture[ItemID.SlimeStaff], 24, 24);
			Texture2D smallSentry = ResizeImage(Main.itemTexture[ItemID.DD2LightningAuraT1Popper], 24, 24);
			Texture2D smallHead = ResizeImage(Main.itemTexture[ItemID.SilverHelmet], 24, 24);
			Texture2D smallBody = ResizeImage(Main.itemTexture[ItemID.SilverChainmail], 24, 24);
			Texture2D smallLegs = ResizeImage(Main.itemTexture[ItemID.SilverGreaves], 24, 24);
			Texture2D smallTiles = ResizeImage(Main.itemTexture[ItemID.Sign], 24, 24);
			Texture2D smallCraftingStation = ResizeImage(Main.itemTexture[ItemID.IronAnvil], 24, 24);
			Texture2D smallWalls = ResizeImage(Main.itemTexture[ItemID.PearlstoneBrickWall], 24, 24);
			Texture2D smallExpert = ResizeImage(Main.itemTexture[ItemID.EoCShield], 24, 24);
			Texture2D smallPets = ResizeImage(Main.itemTexture[ItemID.ZephyrFish], 24, 24);
			Texture2D smallLightPets = ResizeImage(Main.itemTexture[ItemID.FairyBell], 24, 24);
			Texture2D smallBossSummon = ResizeImage(Main.itemTexture[ItemID.MechanicalSkull], 24, 24);
			Texture2D smallMounts = ResizeImage(Main.itemTexture[ItemID.SlimySaddle], 24, 24);
			Texture2D smallDyes = ResizeImage(Main.itemTexture[ItemID.OrangeDye], 24, 24);
			Texture2D smallHairDye = ResizeImage(Main.itemTexture[ItemID.BiomeHairDye], 24, 24);
			Texture2D smallQuestFish = ResizeImage(Main.itemTexture[ItemID.FallenStarfish], 24, 24);
			Texture2D smallAccessories = ResizeImage(Main.itemTexture[ItemID.HermesBoots], 24, 24);
			Texture2D smallWings = ResizeImage(Main.itemTexture[ItemID.LeafWings], 24, 24);
			Texture2D smallCarts = ResizeImage(Main.itemTexture[ItemID.Minecart], 24, 24);
			Texture2D smallHealth = ResizeImage(Main.itemTexture[ItemID.HealingPotion], 24, 24);
			Texture2D smallMana = ResizeImage(Main.itemTexture[ItemID.ManaPotion], 24, 24);
			Texture2D smallBuff = ResizeImage(Main.itemTexture[ItemID.RagePotion], 24, 24);
			Texture2D smallAll = ResizeImage(Main.itemTexture[ItemID.AlphabetStatueA], 24, 24);
			Texture2D smallContainer = ResizeImage(Main.itemTexture[ItemID.GoldChest], 24, 24);
			Texture2D smallPaintings = ResizeImage(Main.itemTexture[ItemID.PaintingMartiaLisa], 24, 24);
			Texture2D smallStatue = ResizeImage(Main.itemTexture[ItemID.HeartStatue], 24, 24);
			Texture2D smallWiring = ResizeImage(Main.itemTexture[ItemID.Wire], 24, 24);
			Texture2D smallConsumables = ResizeImage(Main.itemTexture[ItemID.PurificationPowder], 24, 24);
			Texture2D smallExtractinator = ResizeImage(Main.itemTexture[ItemID.Extractinator], 24, 24);
			Texture2D smallOther = ResizeImage(Main.itemTexture[ItemID.UnicornonaStick], 24, 24);

			Texture2D smallArmor = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.SilverHelmet], Main.itemTexture[ItemID.SilverChainmail], Main.itemTexture[ItemID.SilverGreaves] }, 24, 24);
			Texture2D smallPetsLightPets = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.ZephyrFish], Main.itemTexture[ItemID.FairyBell] }, 24, 24);
			Texture2D smallPlaceables = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.Sign], Main.itemTexture[ItemID.PearlstoneBrickWall] }, 24, 24);
			Texture2D smallWeapons = StackResizeImage(new Texture2D[] { smallMelee, smallMagic, smallThrown }, 24, 24);
			Texture2D smallTools = StackResizeImage(new Texture2D[] { RecipeBrowser.instance.GetTexture("Images/sortPick"), RecipeBrowser.instance.GetTexture("Images/sortAxe"), RecipeBrowser.instance.GetTexture("Images/sortHammer") }, 24, 24);
			Texture2D smallFishing = StackResizeImage(new Texture2D[] { RecipeBrowser.instance.GetTexture("Images/sortFish"), RecipeBrowser.instance.GetTexture("Images/sortBait"), Main.itemTexture[ItemID.FallenStarfish] }, 24, 24);
			Texture2D smallPotions = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.HealingPotion], Main.itemTexture[ItemID.ManaPotion], Main.itemTexture[ItemID.RagePotion] }, 24, 24);
			Texture2D smallBothDyes = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.OrangeDye], Main.itemTexture[ItemID.BiomeHairDye] }, 24, 24);
			Texture2D smallSortTiles = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.Candelabra], Main.itemTexture[ItemID.GrandfatherClock] }, 24, 24);

			// Potions, other?
			// should inherit children?
			// should have other category?
			if (WorldGen.statueList == null)
				WorldGen.SetupStatueList();

			categories = new List<Category>() {
				new Category("All", x=> true, smallAll),
				new Category("Weapons"/*, x=>x.damage>0*/, x=> false, smallWeapons) { //"Images/sortDamage"
					subCategories = new List<Category>() {
						new Category("Melee", x=>x.melee, smallMelee),
						new Category("Yoyo", x=>ItemID.Sets.Yoyo[x.type], smallYoyo),
						new Category("Magic", x=>x.magic, smallMagic),
						new Category("Ranged", x=>x.ranged && x.ammo == 0, smallRanged) // TODO and ammo no
						{
							sorts = new List<Sort>() { new Sort("Use Ammo Type", "Images/sortAmmo", (x,y)=>x.useAmmo.CompareTo(y.useAmmo)), }
						},
						new Category("Throwing", x=>x.thrown, smallThrown),
						new Category("Summon", x=>x.summon && !x.sentry, smallSummon),
						new Category("Sentry", x=>x.summon && x.sentry, smallSentry),
					},
					sorts = new List<Sort>() { new Sort("Damage", "Images/sortDamage", (x,y)=>x.damage.CompareTo(y.damage)), }
				},
				new Category("Tools"/*,x=>x.pick>0||x.axe>0||x.hammer>0*/, x=>false, smallTools) {
					subCategories = new List<Category>() {
						new Category("Pickaxes", x=>x.pick>0, "Images/sortPick") { sorts = new List<Sort>() { new Sort("Pick Power", "Images/sortPick", (x,y)=>x.pick.CompareTo(y.pick)), } },
						new Category("Axes", x=>x.axe>0, "Images/sortAxe"){ sorts = new List<Sort>() { new Sort("Axe Power", "Images/sortAxe", (x,y)=>x.axe.CompareTo(y.axe)), } },
						new Category("Hammers", x=>x.hammer>0, "Images/sortHammer"){ sorts = new List<Sort>() { new Sort("Hammer Power", "Images/sortHammer", (x,y)=>x.hammer.CompareTo(y.hammer)), } },
					},
				},
				new Category("Armor"/*,  x=>x.headSlot!=-1||x.bodySlot!=-1||x.legSlot!=-1*/, x => false, smallArmor) {
					subCategories = new List<Category>() {
						new Category("Head", x=>x.headSlot!=-1, smallHead),
						new Category("Body", x=>x.bodySlot!=-1, smallBody),
						new Category("Legs", x=>x.legSlot!=-1, smallLegs),
					},
					sorts = new List<Sort>() { new Sort("Defense", "Images/sortDefense", (x,y)=>x.defense.CompareTo(y.defense)), }
				},
				new Category("Tiles", x=>x.createTile!=-1, smallTiles)
				{
					subCategories = new List<Category>()
					{
						new Category("Crafting Stations", x=>RecipeCatalogueUI.instance.craftingTiles.Contains(x.createTile), smallCraftingStation),
						new Category("Containers", x=>x.createTile!=-1 && Main.tileContainer[x.createTile], smallContainer),
						new Category("Wiring", x=>ItemID.Sets.SortingPriorityWiring[x.type] > -1, smallWiring),
						new Category("Statues", x=>WorldGen.statueList.Any(point => point.X == x.createTile && point.Y == x.placeStyle), smallStatue), 
						//new Category("Paintings", x=>ItemID.Sets.SortingPriorityPainting[x.type] > -1, smallPaintings), // oops, this is painting tools not painting tiles
						//new Category("5x4", x=>{
						//	if(x.createTile!=-1)
						//	{
						//		var tod = Terraria.ObjectData.TileObjectData.GetTileData(x.createTile, x.placeStyle);
						//		return tod != null && tod.Width == 5 && tod.Height == 4;
						//	}
						//	return false;
						//} , smallContainer),
					},
					// wires

					// Banners
					sorts = new List<Sort>() {
						new Sort("Place Tile", smallSortTiles, (x,y)=> x.createTile == y.createTile ? x.placeStyle.CompareTo(y.placeStyle) : x.createTile.CompareTo(y.createTile)),
					}
				},
				new Category("Walls", x=>x.createWall!=-1, smallWalls),
				new Category("Accessories", x=>x.accessory, smallAccessories)
				{
					subCategories = new List<Category>()
					{
						new Category("Wings", x=>x.wingSlot > 0, smallWings)
					}
				},
				new Category("Ammo", x=>x.ammo!=0, RecipeBrowser.instance.GetTexture("Images/sortAmmo"))
				{
					sorts = new List<Sort>() { new Sort("Ammo Type", "Images/sortAmmo", (x,y)=>x.ammo.CompareTo(y.ammo)), }
					// TODO: Filters/Subcategories for all ammo types?
				},
				new Category("Potions", x=>(x.UseSound != null && x.UseSound.Style == 3), smallPotions)
				{
					subCategories = new List<Category>() {
						new Category("Health Potions", x=>x.healLife > 0, smallHealth) { sorts = new List<Sort>() { new Sort("Heal Life", smallHealth, (x,y)=>x.healLife.CompareTo(y.healLife)), } },
						new Category("Mana Potions", x=>x.healMana > 0, smallMana) { sorts = new List<Sort>() { new Sort("Heal Mana", smallMana, (x,y)=>x.healMana.CompareTo(y.healMana)),   }},
						new Category("Buff Potions", x=>(x.UseSound != null && x.UseSound.Style == 3) && x.buffType > 0, smallBuff),
						// Todo: Automatic other category?
					}
				},
				new Category("Expert", x=>x.expert, smallExpert),
				new Category("Pets"/*, x=> x.buffType > 0 && (Main.vanityPet[x.buffType] || Main.lightPet[x.buffType])*/, x=>false, smallPetsLightPets){
					subCategories = new List<Category>() {
						new Category("Pets", x=>Main.vanityPet[x.buffType], smallPets),
						new Category("Light Pets", x=>Main.lightPet[x.buffType], smallLightPets),
					}
				},
				new Category("Mounts", x=>x.mountType != -1, smallMounts)
				{
					subCategories = new List<Category>()
					{
						new Category("Carts", x=>x.mountType != -1 && MountID.Sets.Cart[x.mountType], smallCarts) // TODO: need mountType check? inherited parent logic or parent unions children?
					}
				},
				new Category("Dyes", x=>false, smallBothDyes)
				{
					subCategories = new List<Category>()
					{
						new Category("Dyes", x=>x.dye != 0, smallDyes),
						new Category("Hair Dyes", x=>x.hairDye != -1, smallHairDye),
					}
				},
				new Category("Boss Summons", x=>ItemID.Sets.SortingPriorityBossSpawns[x.type] != -1 && x.type != ItemID.LifeCrystal && x.type != ItemID.ManaCrystal && x.type != ItemID.CellPhone && x.type != ItemID.IceMirror && x.type != ItemID.MagicMirror && x.type != ItemID.LifeFruit && x.netID != ItemID.TreasureMap || x.netID == ItemID.PirateMap, smallBossSummon) { // vanilla bug.
					sorts = new List<Sort>() { new Sort("Progression Order", "Images/sortDamage", (x,y)=>ItemID.Sets.SortingPriorityBossSpawns[x.type].CompareTo(ItemID.Sets.SortingPriorityBossSpawns[y.type])), }
				},
				new Category("Consumables", x=>x.consumable, smallConsumables),
				new Category("Fishing"/*, x=> x.fishingPole > 0 || x.bait>0|| x.questItem*/, x=>false, smallFishing){
					subCategories = new List<Category>() {
						new Category("Poles", x=>x.fishingPole > 0, "Images/sortFish") {sorts = new List<Sort>() { new Sort("Pole Power", "Images/sortFish", (x,y)=>x.fishingPole.CompareTo(y.fishingPole)), } },
						new Category("Bait", x=>x.bait>0, "Images/sortBait") {sorts = new List<Sort>() { new Sort("Bait Power", "Images/sortBait", (x,y)=>x.bait.CompareTo(y.bait)), } },
						new Category("Quest Fish", x=>x.questItem, smallQuestFish),
					}
				},
				new Category("Extractinator", x=>ItemID.Sets.ExtractinatorMode[x.type] > -1, smallExtractinator),
				//modCategory,
				new Category("Other", x=>BelongsInOther(x), smallOther),
			};

			foreach (var modCategory in RecipeBrowser.instance.modCategories)
			{
				if (string.IsNullOrEmpty(modCategory.parent))
				{
					categories.Insert(categories.Count - 2, new Category(modCategory.name, modCategory.belongs, modCategory.icon));
				}
				else
				{
					foreach (var item in categories)
					{
						if (item.name == modCategory.parent)
						{
							item.subCategories.Add(new Category(modCategory.name, modCategory.belongs, modCategory.icon));
						}
					}
				}
			}

			foreach (var modCategory in RecipeBrowser.instance.modFilters)
			{
				filters.Add(new Filter(modCategory.name, modCategory.belongs, modCategory.icon));
			}

			foreach (var parent in categories)
			{
				foreach (var child in parent.subCategories)
				{
					child.parent = parent; // 3 levels?
				}
			}

			SelectedSort = sorts[0];
			SelectedCategory = categories[0];
		}

		private bool BelongsInOther(Item item)
		{
			var cats = categories.Skip(1).Take(categories.Count - 2);
			foreach (var category in cats)
			{
				if (category.BelongsRecursive(item))
					return false;
			}
			return true;
		}

		private Texture2D StackResizeImage(Texture2D[] texture2D, int desiredWidth, int desiredHeight)
		{
			float overlap = .5f;
			float totalScale = 1 / (1f + ((1 - overlap) * (texture2D.Length - 1)));
			int newWidth = (int)(desiredWidth * totalScale);
			int newHeight = (int)(desiredHeight * totalScale);
			//var texture2Ds = texture2D.Select(x => ResizeImage(x, newWidth, newHeight));

			RenderTarget2D renderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, desiredWidth, desiredHeight);
			Main.instance.GraphicsDevice.SetRenderTarget(renderTarget);
			Main.instance.GraphicsDevice.Clear(Color.Transparent);
			Main.spriteBatch.Begin();

			int index = 0;
			foreach (var texture in texture2D)
			{
				float scale = 1;
				if (texture.Width > newWidth || texture.Height > newHeight)
				{
					if (texture.Height > texture.Width)
						scale = (float)newHeight / texture.Height;
					else
						scale = (float)newWidth / texture.Width;
				}

				Vector2 position = new Vector2(newWidth / 2, newHeight / 2);
				position += new Vector2(index * (1 - overlap) * newWidth, index * (1 - overlap) * newHeight);
				Main.spriteBatch.Draw(texture, position, null, Color.White, 0f, new Vector2(texture.Width / 2, texture.Height / 2), scale, SpriteEffects.None, 0f);
				index++;
			}
			Main.spriteBatch.End();
			Main.instance.GraphicsDevice.SetRenderTarget(null);

			Texture2D mergedTexture = new Texture2D(Main.instance.GraphicsDevice, desiredWidth, desiredHeight);
			Color[] content = new Color[desiredWidth * desiredHeight];
			renderTarget.GetData<Color>(content);
			mergedTexture.SetData<Color>(content);
			return mergedTexture;
		}

		private Texture2D ResizeImage(Texture2D texture2D, int desiredWidth, int desiredHeight)
		{
			RenderTarget2D renderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, desiredWidth, desiredHeight);
			Main.instance.GraphicsDevice.SetRenderTarget(renderTarget);
			Main.instance.GraphicsDevice.Clear(Color.Transparent);
			Main.spriteBatch.Begin();

			float scale = 1;
			if (texture2D.Width > desiredWidth || texture2D.Height > desiredHeight)
			{
				if (texture2D.Height > texture2D.Width)
					scale = (float)desiredWidth / texture2D.Height;
				else
					scale = (float)desiredWidth / texture2D.Width;
			}

			//new Vector2(texture2D.Width / 2 * scale, texture2D.Height / 2 * scale) desiredWidth/2, desiredHeight/2
			Main.spriteBatch.Draw(texture2D, new Vector2(desiredWidth / 2, desiredHeight / 2), null, Color.White, 0f, new Vector2(texture2D.Width / 2, texture2D.Height / 2), scale, SpriteEffects.None, 0f);

			Main.spriteBatch.End();
			Main.instance.GraphicsDevice.SetRenderTarget(null);

			Texture2D mergedTexture = new Texture2D(Main.instance.GraphicsDevice, desiredWidth, desiredHeight);
			Color[] content = new Color[desiredWidth * desiredHeight];
			renderTarget.GetData<Color>(content);
			mergedTexture.SetData<Color>(content);
			return mergedTexture;
		}
	}

	internal class ItemCatalogueUIPanel : UIPanel
	{
		public override void Recalculate()
		{
			//if (ItemCatalogueUI.instance.sortsAndFiltersPanelGrid != null)
			//{
			//	var size = 12 + ItemCatalogueUI.instance.sortsAndFiltersPanelGrid.GetTotalHeight();
			//	ItemCatalogueUI.instance.sortsAndFiltersPanel.Height.Set(size, 0f);
			//	ItemCatalogueUI.instance.itemGridPanel.Top.Set(size, 0f);
			//	ItemCatalogueUI.instance.itemGridPanel.Height.Set(-size - 16, 1f);
			//}
			base.Recalculate();
		}
		public override void RecalculateChildren()
		{
			base.RecalculateChildren();
		}
	}

	internal class Filter
	{
		internal string name;
		internal Predicate<Item> belongs;
		internal List<Category> subCategories; //
		internal List<Sort> sorts;
		internal UISilentImageButton button;
		internal Category parent;

		public Filter(string name, Predicate<Item> belongs, Texture2D texture)
		{
			this.name = name;
			subCategories = new List<Category>();
			sorts = new List<Sort>();
			this.belongs = belongs;

			this.button = new UISilentImageButton(texture, name);
			button.OnClick += (a, b) =>
			{
				button.selected = !button.selected;
				ItemCatalogueUI.instance.updateNeeded = true;
				//Main.NewText("clicked on " + button.hoverText);
			};
		}
	}

	internal class Sort
	{
		internal Func<Item, Item, int> sort;
		internal UISilentImageButton button;

		public Sort(string hoverText, Texture2D texture, Func<Item, Item, int> sort)
		{
			this.sort = sort;
			button = new UISilentImageButton(texture, hoverText);
			button.OnClick += (a, b) =>
			{
				ItemCatalogueUI.instance.SelectedSort = this;
			};
		}

		public Sort(string hoverText, string textureFileName, Func<Item, Item, int> sort) : this(hoverText, RecipeBrowser.instance.GetTexture(textureFileName), sort)
		{
		}
	}

	// Represents a requested Category or Filter.
	internal class ModCategory
	{
		internal string name;
		internal string parent;
		internal Texture2D icon;
		internal Predicate<Item> belongs;
		public ModCategory(string name, string parent, Texture2D icon, Predicate<Item> belongs)
		{
			this.name = name;
			this.parent = parent;
			this.icon = icon;
			this.belongs = belongs;
		}
	}

	// Can belong to 2 Category? -> ??
	// Separate filter? => yes, but Separate conditional filters?
	// All children belong to parent -> yes.
	internal class Category // Filter
	{
		internal string name;
		internal Predicate<Item> belongs;
		internal List<Category> subCategories;
		internal List<Sort> sorts;
		internal UISilentImageButton button;
		internal Category parent;

		public Category(string name, Predicate<Item> belongs, Texture2D texture = null)
		{
			if (texture == null)
				texture = RecipeBrowser.instance.GetTexture("Images/sortAmmo");
			this.name = name;
			subCategories = new List<Category>();
			sorts = new List<Sort>();
			this.belongs = belongs;

			this.button = new UISilentImageButton(texture, name);
			button.OnClick += (a, b) =>
			{
				//Main.NewText("clicked on " + button.hoverText);
				ItemCatalogueUI.instance.SelectedCategory = this;
			};
		}

		public Category(string name, Predicate<Item> belongs, string textureFileName) : this(name, belongs, RecipeBrowser.instance.GetTexture(textureFileName))
		{
		}

		internal bool BelongsRecursive(Item item)
		{
			if (belongs(item))
				return true;
			return subCategories.Any(x => x.belongs(item));
		}

		internal void ParentAddToSorts(List<Sort> availableSorts)
		{
			if (parent != null)
				parent.ParentAddToSorts(availableSorts);
			availableSorts.AddRange(sorts);
		}
	}
}