using Microsoft.Xna.Framework;
using RecipeBrowser.UIElements;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace RecipeBrowser
{
	internal class ItemCatalogueUI
	{
		internal static ItemCatalogueUI instance;
		internal static Color color = Color.DarkGreen;

		internal UIPanel mainPanel;
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

		public ItemCatalogueUI()
		{
			instance = this;
			itemSlots = new List<UIItemCatalogueItemSlot>();
			additionalDragTargets = new List<UIElement>();
		}

		internal UIElement CreateItemCataloguePanel()
		{
			mainPanel = new UIPanel();
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

			itemNameFilter = new NewUITextBox("Filter by Name");
			itemNameFilter.OnTextChanged += () => { ValidateItemFilter(); updateNeeded = true; };
			itemNameFilter.OnTabPressed += () => { itemDescriptionFilter.Focus(); };
			itemNameFilter.Top.Pixels = 0f;
			itemNameFilter.Left.Set(-152, 1f);
			itemNameFilter.Width.Set(150, 0f);
			itemNameFilter.Height.Set(25, 0f);
			mainPanel.Append(itemNameFilter);

			itemDescriptionFilter = new NewUITextBox("Filter by tooltip");
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

			CraftedRadioButton = new UICheckbox("Crafted", "Only show crafted items");
			CraftedRadioButton.Top.Set(0, 0f);
			CraftedRadioButton.Left.Set(-270, 1f);
			CraftedRadioButton.OnSelectedChanged += (a, b) => updateNeeded = true;
			mainPanel.Append(CraftedRadioButton);

			LootRadioButton = new UICheckbox("Loot", "Show only loot items");
			LootRadioButton.Top.Set(20, 0f);
			LootRadioButton.Left.Set(-270, 1f);
			LootRadioButton.OnSelectedChanged += (a, b) => updateNeeded = true;
			mainPanel.Append(LootRadioButton);

			UnobtainedRadioButton = new UICheckbox("Unobtained", "???");
			UnobtainedRadioButton.Top.Set(40, 0f);
			UnobtainedRadioButton.Left.Set(-270, 1f);
			UnobtainedRadioButton.OnSelectedChanged += (a, b) => { updateNeeded = true; /*HasLootRadioButton.Selected = true;*/ };
			mainPanel.Append(UnobtainedRadioButton);

			if (RecipeBrowser.itemChecklistInstance != null)
			{
				UnobtainedRadioButton.OnSelectedChanged += UnobtainedRadioButton_OnSelectedChanged;
				UnobtainedRadioButton.SetHoverText("Only unobtained items");
			}
			else
			{
				UnobtainedRadioButton.SetDisabled();
				UnobtainedRadioButton.SetHoverText("Install Item Checklist to use");
			}

			//updateNeeded = true;

			UIPanel itemGridPanel = new UIPanel();
			itemGridPanel.SetPadding(6);
			itemGridPanel.Top.Pixels = 60;
			itemGridPanel.Width.Set(0, 1f);
			itemGridPanel.Left.Set(0, 0f);
			itemGridPanel.Height.Set(-76, 1f);
			itemGridPanel.BackgroundColor = Color.CornflowerBlue;
			mainPanel.Append(itemGridPanel);

			itemGrid = new UIGrid();
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

			UIText text = new UIText("2x LMB: View Recipes  ---  2x RMB: See dropping NPCs", 0.85f);
			text.Top.Set(-14, 1f);
			text.HAlign = 0.5f;
			mainPanel.Append(text);
			additionalDragTargets.Add(text);

			return mainPanel;
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
	}
}