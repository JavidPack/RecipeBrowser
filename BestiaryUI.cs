using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RecipeBrowser.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace RecipeBrowser
{
	// TODO:
	// Button to open vanilla bestiary?
	// Show vanilla bestiary elements?
	// Reuse vanilla bestiary search and filters?
	internal class BestiaryUI
	{
		internal static string RBText(string key, string category = "BestiaryUI") => RecipeBrowser.RBText(category, key);

		// Idea: Auto select/show loot from last npc hit.
		internal static BestiaryUI instance;

		internal static Color color = new Color(28, 187, 180);

		internal UIPanel npcGridPanel;
		internal UIGrid npcGrid;
		internal UIHorizontalGrid lootGrid;
		internal bool updateNeeded;
		internal UIPanel mainPanel;
		internal UIBestiaryQueryItemSlot queryItem;
		internal NewUITextBox npcNameFilter;
		internal bool EncounteredRadioButtonIsUnencountered;
		internal UICheckbox EncounteredRadioButton;
		internal UICheckbox HasLootRadioButton;
		internal UICheckbox NewLootOnlyRadioButton;

		internal UIRadioButtonGroup RadioButtonGroup;
		internal UIRadioButton BestiarySortRadioButton;
		internal UIRadioButton IDSortRadioButton;

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

			npcGridPanel = new UIPanel();
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
			npcGrid.alternateSort = CustomSort;
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
			lootGrid.drawArrows = true;
			lootPanel.Append(lootGrid);

			var lootGridScrollbar = new InvisibleFixedUIHorizontalScrollbar(RecipeBrowserUI.instance.userInterface);
			lootGridScrollbar.SetView(100f, 1000f);
			lootGridScrollbar.Width.Set(0, 1f);
			lootGridScrollbar.Top.Set(-20, 1f);
			//lootPanel.Append(lootGridScrollbar);
			lootGrid.SetScrollbar(lootGridScrollbar);

			queryItem = new UIBestiaryQueryItemSlot(new Item());
			queryItem.emptyHintText = RBText("EmptyQuerySlotHint");
			mainPanel.Append(queryItem);

			RadioButtonGroup = new UIRadioButtonGroup();
			RadioButtonGroup.Left.Pixels = 45;
			RadioButtonGroup.Width.Set(180, 0f);
			BestiarySortRadioButton = new UIRadioButton(Language.GetTextValue("BestiaryInfo.Sort_BestiaryID"), "");
			IDSortRadioButton = new UIRadioButton(Language.GetTextValue("BestiaryInfo.Sort_ID"), "");
			RadioButtonGroup.Add(BestiarySortRadioButton);
			RadioButtonGroup.Add(IDSortRadioButton);
			mainPanel.Append(RadioButtonGroup);
			BestiarySortRadioButton.Selected = true;
			
			BestiarySortRadioButton.OnSelectedChanged += (a, b) => updateNeeded = true;
			IDSortRadioButton.OnSelectedChanged += (a, b) => updateNeeded = true;

			npcNameFilter = new NewUITextBox(RBText("FilterByName", "Common"));
			npcNameFilter.OnTextChanged += () => { ValidateNPCFilter(); updateNeeded = true; };
			npcNameFilter.Top.Set(0, 0f);
			npcNameFilter.Left.Set(-150, 1f);
			npcNameFilter.Width.Set(150, 0f);
			npcNameFilter.Height.Set(25, 0f);
			mainPanel.Append(npcNameFilter);

			EncounteredRadioButton = new UICheckbox(RBText("Encountered"), RBText("ShowOnlyNPCKilledAlready"));
			EncounteredRadioButton.TextOriginX = 0; // fixes issue with changing text later
			EncounteredRadioButton.Top.Set(-40, 1f);
			EncounteredRadioButton.Left.Set(6, .5f);
			EncounteredRadioButton.OnSelectedChanged += (a, b) => updateNeeded = true;
			EncounteredRadioButton.OnRightClick += (a, b) => {
				EncounteredRadioButtonIsUnencountered = !EncounteredRadioButtonIsUnencountered;
				if (EncounteredRadioButtonIsUnencountered) {
					EncounteredRadioButton.SetText("   " + RBText("Unencountered"));
					EncounteredRadioButton.SetHoverText(RBText("ShowOnlyNPCNotKilled"));
				}
				else {
					EncounteredRadioButton.SetText("   " + RBText("Encountered"));
					EncounteredRadioButton.SetHoverText(RBText("ShowOnlyNPCKilledAlready"));
				}
				updateNeeded = true;
			};
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

		private int CustomSort(UIElement x, UIElement y) {
			if (x is UINPCSlot a && y is UINPCSlot b) {
				if (BestiarySortRadioButton.Selected) {
					bool aHasSort = ContentSamples.NpcBestiarySortingId.TryGetValue(a.npcType, out int aSortValue);
					bool bHasSort = ContentSamples.NpcBestiarySortingId.TryGetValue(b.npcType, out int bSortValue);

					if (aHasSort && bHasSort)
						return aSortValue.CompareTo(bSortValue);

					if (aHasSort)
						return -1;

					if (bHasSort)
						return 1;
				}

				// This should work with negatives, but they aren't displayed yet anyway. Vanilla, Negative (reversed), Modded
				int aFallbackOrder = a.npc.netID switch {
					< 0 => -a.npc.netID,
					< 688 => a.npc.netID - 1000, // NPCID.Count
					_ => a.npc.netID,
				};
				int bFallbackOrder = b.npc.netID switch {
					< 0 => -b.npc.netID,
					< 688 => b.npc.netID - 1000,
					_ => b.npc.netID,
				};

				return aFallbackOrder.CompareTo(bFallbackOrder);
			}

			return x.CompareTo(y);
		}

		private void ValidateNPCFilter()
		{
			if (npcNameFilter.currentString.Length > 0)
			{
				bool found = false;
				for (int type = NPCID.NegativeIDCount + 1; type < NPCLoader.NPCCount; type++) {
					if (type == 0)
						continue;
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
			if (NPCLoader.NPCCount - 2 + -NPCID.NegativeIDCount != npcSlots.Count)
			{
				// should only happen once
				npcSlots.Clear();
				for (int type = NPCID.NegativeIDCount + 1; type < NPCLoader.NPCCount; type++)
				{
					if (type == 0)
						continue;

					NPC npc = new NPC();
					npc.SetDefaults(type);
					var slot = new UINPCSlot(npc);
					npcSlots.Add(slot);
				}
			}

			if (!updateNeeded) { return; }
			updateNeeded = false;

			npcGrid.Clear();
			foreach (var slot in npcSlots) {
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
				if (!EncounteredRadioButtonIsUnencountered == !RecipePath.NPCUnlocked(slot.npc.netID)) {
					return false;
				}
			}

			if (HasLootRadioButton.Selected)
			{
				// Slow, AnyDrops or Cache results.
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

			if (RecipeBrowserUI.ModIndex != 0)
			{
				if (slot.npc.ModNPC == null)
				{
					return false;
				}
				if (slot.npc.ModNPC.Mod.Name != RecipeBrowserUI.instance.mods[RecipeBrowserUI.ModIndex])
				{
					return false;
				}
			}

			if (!queryItem.item.IsAir)
			{
				if (!slot.GetDrops().Contains(queryItem.item.type))
					return false;
			}

			if (Lang.GetNPCNameValue(slot.npc.netID).IndexOf(npcNameFilter.currentString, StringComparison.OrdinalIgnoreCase) == -1)
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