using Microsoft.Xna.Framework;
using RecipeBrowser.UIElements;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace RecipeBrowser
{
	internal class BestiaryUI
	{
		internal static string RBText(string key, string category = "BestiaryUI") => RecipeBrowser.RBText(category, key);

		// Idea: Auto select/show loot from last npc hit.
		internal static BestiaryUI instance;

		internal static Color color = new Color(28, 187, 180);

		internal UIGrid npcGrid;
		internal UIHorizontalGrid lootGrid;
		internal bool updateNeeded;
		internal UIPanel mainPanel;
		internal UIBestiaryQueryItemSlot queryItem;
		internal NewUITextBox npcNameFilter;
		internal UICheckbox EncounteredRadioButton;
		internal UICheckbox HasLootRadioButton;
		internal UICheckbox NewLootOnlyRadioButton;

		internal List<UINPCSlot> npcSlots;
		internal UINPCSlot queryLootNPC;

		public BestiaryUI()
		{
			instance = this;
			npcSlots = new List<UINPCSlot>();
		}

		internal UIElement CreateBestiaryPanel()
		{
			mainPanel = new UIPanel();
			mainPanel.SetPadding(6);
			mainPanel.BackgroundColor = color;

			mainPanel.Top.Set(20, 0f);
			mainPanel.Height.Set(-20, 1f);
			mainPanel.Width.Set(0, 1f);

			UIPanel npcGridPanel = new UIPanel();
			npcGridPanel.SetPadding(6);
			npcGridPanel.Top.Pixels = 46;
			npcGridPanel.Width.Set(0, 1f);
			npcGridPanel.Left.Set(0, 0f);
			npcGridPanel.Height.Set(-52 - 46, 1f);
			npcGridPanel.BackgroundColor = Color.CornflowerBlue;
			mainPanel.Append(npcGridPanel);

			npcGrid = new UIGrid();
			npcGrid.Width.Set(-20, 1f);
			npcGrid.Height.Set(0, 1f);
			npcGrid.ListPadding = 2f;
			npcGrid.OnScrollWheel += RecipeBrowserUI.OnScrollWheel_FixHotbarScroll;
			npcGridPanel.Append(npcGrid);

			var npcGridScrollbar = new FixedUIScrollbar(RecipeBrowserUI.instance.userInterface);
			npcGridScrollbar.SetView(100f, 1000f);
			npcGridScrollbar.Height.Set(0, 1f);
			npcGridScrollbar.Left.Set(-20, 1f);
			npcGridPanel.Append(npcGridScrollbar);
			npcGrid.SetScrollbar(npcGridScrollbar);

			UIPanel lootPanel = new UIPanel();
			lootPanel.SetPadding(6);
			lootPanel.Top.Set(-50, 1f);
			lootPanel.Width.Set(0, .5f);
			lootPanel.Height.Set(50, 0f);
			lootPanel.BackgroundColor = Color.CornflowerBlue;
			mainPanel.Append(lootPanel);

			lootGrid = new UIHorizontalGrid();
			lootGrid.Width.Set(0, 1f);
			lootGrid.Height.Set(0, 1f);
			lootGrid.ListPadding = 2f;
			lootGrid.OnScrollWheel += RecipeBrowserUI.OnScrollWheel_FixHotbarScroll;
			lootPanel.Append(lootGrid);

			var lootGridScrollbar = new InvisibleFixedUIHorizontalScrollbar(RecipeBrowserUI.instance.userInterface);
			lootGridScrollbar.SetView(100f, 1000f);
			lootGridScrollbar.Width.Set(0, 1f);
			lootGridScrollbar.Top.Set(-20, 1f);
			lootPanel.Append(lootGridScrollbar);
			lootGrid.SetScrollbar(lootGridScrollbar);

			queryItem = new UIBestiaryQueryItemSlot(new Item());
			queryItem.emptyHintText = RBText("EmptyQuerySlotHint");
			mainPanel.Append(queryItem);

			npcNameFilter = new NewUITextBox(RBText("FilterByName", "Common"));
			npcNameFilter.OnTextChanged += () => { ValidateNPCFilter(); updateNeeded = true; };
			npcNameFilter.Top.Set(0, 0f);
			npcNameFilter.Left.Set(-150, 1f);
			npcNameFilter.Width.Set(150, 0f);
			npcNameFilter.Height.Set(25, 0f);
			mainPanel.Append(npcNameFilter);

			EncounteredRadioButton = new UICheckbox(RBText("Encountered"), RBText("ShowOnlyNPCKilledAlready"));
			EncounteredRadioButton.Top.Set(-40, 1f);
			EncounteredRadioButton.Left.Set(6, .5f);
			EncounteredRadioButton.OnSelectedChanged += (a, b) => updateNeeded = true;
			mainPanel.Append(EncounteredRadioButton);

			HasLootRadioButton = new UICheckbox(RBText("HasLoot"), RBText("ShowOnlyNPCWithLoot"));
			HasLootRadioButton.Top.Set(-20, 1f);
			HasLootRadioButton.Left.Set(6, .5f);
			HasLootRadioButton.OnSelectedChanged += (a, b) => updateNeeded = true;
			mainPanel.Append(HasLootRadioButton);

			NewLootOnlyRadioButton = new UICheckbox(RBText("NewLoot"), "???");
			NewLootOnlyRadioButton.Top.Set(-20, 1f);
			NewLootOnlyRadioButton.Left.Set(110, .5f);
			NewLootOnlyRadioButton.OnSelectedChanged += (a, b) => { updateNeeded = true; /*HasLootRadioButton.Selected = true;*/ };
			mainPanel.Append(NewLootOnlyRadioButton);

			if (RecipeBrowser.itemChecklistInstance != null)
			{
				NewLootOnlyRadioButton.OnSelectedChanged += ItemChecklistNewLootOnlyFilter_SelectedChanged;
				NewLootOnlyRadioButton.SetHoverText(RBText("ShowOnlyNPCWithNeverBeforeSeenLoot"));
			}
			else
			{
				NewLootOnlyRadioButton.SetDisabled();
				NewLootOnlyRadioButton.SetHoverText(RBText("InstallItemChecklistToUse", "Common"));
			}

			updateNeeded = true;

			return mainPanel;
		}

		private void ValidateNPCFilter()
		{
			if (npcNameFilter.currentString.Length > 0)
			{
				bool found = false;
				for (int type = 1; type < NPCLoader.NPCCount; type++)
				{
					string name = Lang.GetNPCNameValue(type);
					if (name.IndexOf(npcNameFilter.currentString, StringComparison.OrdinalIgnoreCase) != -1)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					npcNameFilter.SetText(npcNameFilter.currentString.Substring(0, npcNameFilter.currentString.Length - 1));
				}
			}
			updateNeeded = true;
		}

		internal void Update()
		{
			if (NPCLoader.NPCCount - 1 != npcSlots.Count)
			{
				// should only happen once
				npcSlots.Clear();
				for (int type = 1; type < NPCLoader.NPCCount; type++)
				{
					NPC npc = new NPC();
					npc.SetDefaults(type);
					var slot = new UINPCSlot(npc);
					npcSlots.Add(slot);
				}
			}

			if (!updateNeeded) { return; }
			updateNeeded = false;

			npcGrid.Clear();
			for (int type = 1; type < NPCLoader.NPCCount; type++)
			{
				var slot = npcSlots[type - 1];
				if (PassNPCFilters(slot))
				{
					npcGrid._items.Add(slot);
					npcGrid._innerList.Append(slot);
				}
			}
			npcGrid.UpdateOrder();
			npcGrid._innerList.Recalculate();

			lootGrid.Clear();
			if (queryLootNPC != null)
			{
				var drops = queryLootNPC.GetDrops();
				if (NewLootOnlyRadioButton.Selected && RecipeBrowserUI.instance.foundItems != null)
					drops.RemoveWhere(x => RecipeBrowserUI.instance.foundItems[x]);
				foreach (var dropitem in drops)
				{
					Item item = new Item();
					item.SetDefaults(dropitem, false);
					var slot = new UIBestiaryItemSlot(item);
					lootGrid._items.Add(slot);
					lootGrid._innerList.Append(slot);
				}
			}
			lootGrid.UpdateOrder();
			lootGrid._innerList.Recalculate();
		}

		internal void SetNPC(UINPCSlot slot)
		{
			foreach (var npc in npcSlots)
			{
				npc.selected = false;
			}
			slot.selected = true;
		}

		internal void CloseButtonClicked()
		{
			if (queryItem.real && queryItem.item.stack > 0)
			{
				queryItem.ReplaceWithFake(0);
			}
			updateNeeded = true;
		}

		private bool PassNPCFilters(UINPCSlot slot)
		{
			if (EncounteredRadioButton.Selected)
			{
				int bannerID = Item.NPCtoBanner(slot.npc.BannerID());
				if (bannerID > 0)
				{
					if (NPC.killCount[bannerID] <= 0)
						return false;
				}
				else return false;
			}

			if (HasLootRadioButton.Selected)
			{
				if (slot.GetDrops().Count == 0)
				{
					return false;
				}
			}

			if (NewLootOnlyRadioButton.Selected)
			{
				// Item Checklist integration
				if (RecipeBrowserUI.instance.foundItems != null)
				{
					bool hasNewItem = false;
					var drops = slot.GetDrops();
					foreach (var item in drops)
					{
						if (!RecipeBrowserUI.instance.foundItems[item])
						{
							hasNewItem = true;
							break;
						}
					}
					if (!hasNewItem) return false;
				}
				else
				{
					Main.NewText("How is this happening?");
				}
			}

			if (RecipeBrowserUI.modIndex != 0)
			{
				if (slot.npc.modNPC == null)
				{
					return false;
				}
				if (slot.npc.modNPC.mod.Name != RecipeBrowserUI.instance.mods[RecipeBrowserUI.modIndex])
				{
					return false;
				}
			}

			if (!queryItem.item.IsAir)
			{
				if (!slot.GetDrops().Contains(queryItem.item.type))
					return false;
			}

			if (Lang.GetNPCNameValue(slot.npcType).IndexOf(npcNameFilter.currentString, StringComparison.OrdinalIgnoreCase) == -1)
				return false;

			return true;
		}

		private void ItemChecklistNewLootOnlyFilter_SelectedChanged(object sender, EventArgs e)
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
	}
}