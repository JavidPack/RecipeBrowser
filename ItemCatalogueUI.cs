using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RecipeBrowser.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI;
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
		internal UIPanel itemGridPanel;
		internal UIGrid itemGrid;

		const int ItemDropViewerWidth = 230;
		internal UIPanel itemDropViewerPanel;
		internal UIGrid itemDropViewerGrid;

		internal bool updateNeeded;
		internal int slowUpdateNeeded;
		internal NewUITextBox itemNameFilter;
		internal NewUITextBox itemDescriptionFilter;
		internal List<UIItemCatalogueItemSlot> itemSlots;
		internal bool[] craftResults;
		internal bool[] isLoot;
		internal List<UIElement> additionalDragTargets;
		internal UICheckbox CraftedRadioButton;
		internal UICheckbox LootRadioButton;
		internal UICheckbox UnobtainedRadioButton;
		// TODO: Purchaseable checkbox.

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
			itemNameFilter.Left.Set(-150, 1f);
			itemNameFilter.Width.Set(150, 0f);
			itemNameFilter.Height.Set(25, 0f);
			mainPanel.Append(itemNameFilter);

			itemDescriptionFilter = new NewUITextBox(RBText("FilterByTooltip", "Common"));
			itemDescriptionFilter.OnTextChanged += () => { ValidateItemDescription(); updateNeeded = true; };
			itemDescriptionFilter.OnTabPressed += () => { itemNameFilter.Focus(); };
			itemDescriptionFilter.Top.Pixels = 30f;
			itemDescriptionFilter.Left.Set(-150, 1f);
			itemDescriptionFilter.Width.Set(150, 0f);
			itemDescriptionFilter.Height.Set(25, 0f);
			mainPanel.Append(itemDescriptionFilter);

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
			itemGridPanel.Append(itemGrid);

			var itemGridScrollbar = new FixedUIScrollbar(RecipeBrowserUI.instance.userInterface);
			itemGridScrollbar.SetView(100f, 1000f);
			itemGridScrollbar.Height.Set(0, 1f);
			itemGridScrollbar.Left.Set(-20, 1f);
			itemGridPanel.Append(itemGridScrollbar);
			itemGrid.SetScrollbar(itemGridScrollbar);

			// Bag Drop Viewer
			itemDropViewerPanel = new UIPanel();
			itemDropViewerPanel.SetPadding(6);
			itemDropViewerPanel.Top.Pixels = 60;
			itemDropViewerPanel.Width.Set(ItemDropViewerWidth, 0f);
			itemDropViewerPanel.Height.Set(-76, 1f);
			itemDropViewerPanel.Left.Set(-ItemDropViewerWidth, 1f);
			itemDropViewerPanel.BackgroundColor = Color.CornflowerBlue;

			itemDropViewerGrid = new UIGrid();
			itemDropViewerGrid.Width.Set(0, 1f);
			itemDropViewerGrid.Height.Set(0, 1f);
			itemDropViewerGrid.ListPadding = 2f;
			itemDropViewerGrid.drawArrows = true;
			itemDropViewerGrid.alternateSort = ItemDropViewerGridSort; 
			itemDropViewerPanel.Append(itemDropViewerGrid);

			var itemDropViewerScrollbar = new InvisibleFixedUIScrollbar(RecipeBrowserUI.instance.userInterface);
			itemDropViewerScrollbar.SetView(100f, 1000f);
			itemDropViewerScrollbar.Height.Set(0, 1f);
			itemDropViewerScrollbar.Left.Set(-20, 1f);
			itemDropViewerGrid.SetScrollbar(itemDropViewerScrollbar);

			//"2x LMB: View Recipes  ---  2x RMB: See dropping NPCs"
			UIText text = new UIText(RBText("BottomInstructions"), 0.85f);
			text.Top.Set(-14, 1f);
			text.HAlign = 0.5f;
			mainPanel.Append(text);
			additionalDragTargets.Add(text);

			additionalDragTargets.Add(SharedUI.instance.sortsAndFiltersPanel);

			return mainPanel;
		}

		private int ItemGridSort(UIElement x, UIElement y)
		{
			if (x is UIPanel)
				return -1;
			if (y is UIPanel)
				return 1;
			UIItemCatalogueItemSlot a = x as UIItemCatalogueItemSlot;
			UIItemCatalogueItemSlot b = y as UIItemCatalogueItemSlot;
			if(x is Delimiter delimiter && b != null) {
				//return Math.Abs(b.item.rare).CompareTo(Math.Abs(delimiter.order));
				return (Math.Abs(delimiter.order) - 0.5f).CompareTo(Math.Abs(b.item.rare));
			}
			if (y is Delimiter delimiter2 && a != null) {
				// TODO: separate -11, -12, -13 between 11 and 12
				return Math.Abs((float)a.item.rare).CompareTo(Math.Abs(delimiter2.order) - 0.5f);
				//return Math.Abs(delimiter2.order).CompareTo(Math.Abs(a.item.rare));
			}
			if(x is Delimiter d1 && y is Delimiter d2) {
				return d1.order.CompareTo(d2.order);
			}
			if (a == null || b == null) {
				return x.UniqueId.CompareTo(y.UniqueId);
			}
			if (SharedUI.instance.SelectedSort.button.hoverText == "Total Defense" && x is UIArmorSetCatalogueItemSlot armorA && y is UIArmorSetCatalogueItemSlot armorB)
				return armorA.set.Item5.CompareTo(armorB.set.Item5); // Total Hack
			if (SharedUI.instance.SelectedSort != null)
				return SharedUI.instance.SelectedSort.sort(a.item, b.item);
			return a.itemType.CompareTo(b.itemType);
		}

		private int ItemDropViewerGridSort(UIElement x, UIElement y) {
			// Works since uniqueID is in creation order, but will need to be updated later if filters are implemented.
			return x.UniqueId.CompareTo(y.UniqueId);
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
			// TODO: investigate why this Update is slower than RecipeCatalogueUI

			if (!RecipeBrowserUI.instance.ShowRecipeBrowser || RecipeBrowserUI.instance.CurrentPanel != RecipeBrowserUI.ItemCatalogue)
				return;

			if (slowUpdateNeeded > 0) {
				slowUpdateNeeded--;
				if (slowUpdateNeeded == 0)
					updateNeeded = true;
			}

			if (!updateNeeded) { return; }
			updateNeeded = false;
			slowUpdateNeeded = 0;

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
					int id = kvp.Key;
					if (id > 0)
						isLoot[id] = true;
				}
			}

			itemGrid.Clear();
			List<UIItemCatalogueItemSlot> slotsToUse = itemSlots;

			if (SharedUI.instance.SelectedCategory.name == ArmorSetFeatureHelper.ArmorSetsHoverTest) {
				if (ArmorSetFeatureHelper.armorSetSlots == null)
					ArmorSetFeatureHelper.CalculateArmorSets();
				slotsToUse = ArmorSetFeatureHelper.armorSetSlots.Cast<UIItemCatalogueItemSlot>().ToList();
				ArmorSetFeatureHelper.AppendSpecialUI(itemGrid);
			}

			foreach (var slot in slotsToUse)
			{
				if (PassItemFilters(slot))
				{
					itemGrid._items.Add(slot);
					itemGrid._innerList.Append(slot);
				}
			}

			//if(SharedUI.instance.SelectedSort)
			{
				for (int i = 0; i < RarityLoader.RarityCount; i++) {
					Delimiter delimiter = new Delimiter(i, "Rare " + i); 
					// rarity name too?
					// Filter/Ignore missing delimiters? (A-Z might be too much with only a few results.)
					// Toggle delimiters button somewhere maybe?
					delimiter.TextColor = ItemRarity.GetColor(i);
					delimiter.Width.Set(0, 1f);
					itemGrid._items.Add(delimiter); // ?
					itemGrid._innerList.Append(delimiter); // ?
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
			if (RecipeBrowserUI.modIndex != 0)
			{
				if (slot.item.ModItem == null)
				{
					return false;
				}
				if (slot.item.ModItem.Mod.Name != RecipeBrowserUI.instance.mods[RecipeBrowserUI.modIndex])
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

			if (SharedUI.instance.SelectedCategory != null)
			{
				if (!SharedUI.instance.SelectedCategory.belongs(slot.item) && !SharedUI.instance.SelectedCategory.subCategories.Any(x => x.belongs(slot.item)))
					return false;
			}


			foreach (var filter in SharedUI.instance.availableFilters)
			{
				if (filter.button.selected)
				{
					if (!filter.belongs(slot.item))
						return false;
					if (filter == SharedUI.instance.ObtainableFilter)
					{
						bool ableToCraft = false;
						for (int i = 0; i < Recipe.numRecipes; i++) // Optimize with non-trimmed RecipePath.recipeDictionary
						{
							Recipe recipe = Main.recipe[i];
							if (recipe.createItem.type == slot.item.type)
							{
								UIRecipeSlot recipeSlot = RecipeCatalogueUI.instance.recipeSlots[i];
								recipeSlot.CraftPathNeeded();
								//recipeSlot.CraftPathsImmediatelyNeeded();
								if ((recipeSlot.craftPathCalculated || recipeSlot.craftPathsCalculated) && recipeSlot.craftPaths.Count > 0) {
									ableToCraft = true;
									break;
								}
							}
						}
						if (!ableToCraft)
							return false;
					}
					if (filter == SharedUI.instance.CraftableFilter)
					{
						bool ableToCraft = false;
						for (int n = 0; n < Main.numAvailableRecipes; n++)
						{
							if (Main.recipe[Main.availableRecipe[n]].createItem.type == slot.item.type)
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

			if (slot.item.Name.IndexOf(itemNameFilter.currentString, StringComparison.OrdinalIgnoreCase) == -1)
				return false;

			if (itemDescriptionFilter.currentString.Length > 0)
			{
				if (SharedUI.instance.SelectedCategory.name == ArmorSetFeatureHelper.ArmorSetsHoverTest) {
					if (slot is UIArmorSetCatalogueItemSlot setCatalogueItemSlot)
						return setCatalogueItemSlot.set.Item4.IndexOf(itemDescriptionFilter.currentString, StringComparison.OrdinalIgnoreCase) != -1;
				}
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

		internal void ToggleItemDropViewer(bool show) {
			if (show) {
				itemGridPanel.Width.Set(-ItemDropViewerWidth - 2, 1f);
				mainPanel.Append(itemDropViewerPanel);
			}
			else {
				itemGridPanel.Width.Set(0, 1f);
				mainPanel.RemoveChild(itemDropViewerPanel);
			}
			itemGridPanel.Recalculate();
		}

		internal void PopulateItemDropViewerPanel(int type) {
			// ItemDropViewer
			List<IItemDropRule> dropRules = Main.ItemDropsDB.GetRulesForItemID(type);
			List<DropRateInfo> list = new List<DropRateInfo>();
			DropRateInfoChainFeed ratesInfo = new DropRateInfoChainFeed(1f);
			foreach (IItemDropRule item in dropRules) {
				item.ReportDroprates(list, ratesInfo);
			}

			itemDropViewerGrid.Clear();
			ToggleItemDropViewer(list.Any());

			int expectedValue = 0;
			var expectedValueText = new UIText("Expected Value: ?"); // Move above the grid maybe?
			expectedValueText.SetPadding(6);
			itemDropViewerGrid.Add(expectedValueText);

			foreach (DropRateInfo dropRateInfo in list) {
				// Main.NewText($"{ItemHoverFixTagHandler.GenerateTag(dropRateInfo.itemId, 1)} {dropRateInfo.stackMin}-{dropRateInfo.stackMax}, {dropRateInfo.dropRate:P2}");

				var infoElement = new ItemDropBestiaryInfoElement(dropRateInfo);
				bool flag = SharedUI.ShouldShowItemDrop(dropRateInfo);
				var info = new BestiaryUICollectionInfo();
				info.UnlockState = BestiaryEntryUnlockState.CanShowDropsWithDropRates_4;
				info.OwnerEntry = null; // shouldn't error, I think

				var element = new UIBestiaryInfoItemLine(dropRateInfo, info);
				if (!flag)
					element.BackgroundColor = Color.Red;
				else {
					expectedValue += (int)((dropRateInfo.stackMin + dropRateInfo.stackMax) / 2f * dropRateInfo.dropRate * ContentSamples.ItemsByType[dropRateInfo.itemId].value * 0.2f);
				}

				itemDropViewerGrid.Add(element);
			}
			if (expectedValue > 1000000)
				expectedValue = expectedValue - expectedValue % 100; // only room for 3, so get rid of copper coins if platinum.
			expectedValueText.SetText("Expected Value: " + CraftPath.BuyItemNode.GetTotalCostAsTags(expectedValue));
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

	// Make this prettier, taller, left justified?
	internal class Delimiter : UIText
	{
		public int order;
		public Delimiter(int order, string text, float textScale = 1, bool large = false) : base(text, textScale, large) {
			this.order = order;
		}
	}
}