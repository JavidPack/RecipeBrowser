using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.UI;

namespace RecipeBrowser.UIElements
{
	internal class UIRecipeCatalogueQueryItemSlot : UIQueryItemSlot
	{
		internal List<int> history = new List<int>();
		internal int historyCursor = 0; // 0 when empty, points to the nth item.
		private bool skipHistory = false; // avoid adding to history when navigating history

		public UIRecipeCatalogueQueryItemSlot(Item item) : base(item) {
		}

		public override void LeftClick(UIMouseEvent evt) {
			base.LeftClick(evt);
			ReplaceWithFake(item.type);
			RecipeCatalogueUI.instance.queryLootItem = (item.type == 0) ? null : item;
			RecipeCatalogueUI.instance.updateNeeded = true;
			SharedUI.instance.SelectedCategory = SharedUI.instance.categories[0];
		}

		internal override void ReplaceWithFake(int type) {
			base.ReplaceWithFake(type);
			RecipeCatalogueUI.instance.queryLootItem = item;
			RecipeCatalogueUI.instance.updateNeeded = true;
			RecipeCatalogueUI.instance.Tile = -1;
			RecipeCatalogueUI.instance.TileLookupRadioButton.Selected = false;
			SharedUI.instance.SelectedCategory = SharedUI.instance.categories[0];
			AddToHistory(type);
		}

		internal void AddToHistory(int type) {
			//LogHistory("AddToHistory start");
			if (!skipHistory && type != 0) {
				// Remove existing duplicate entries
				for (int i = history.Count - 1; i >= 0; i--) {
					if (history[i] == type) {
						history.RemoveAt(i);
						if (i < historyCursor)
							historyCursor--;
					}
				}

				// Truncate future history if in the middle somewhere.
				history.RemoveRange(historyCursor, history.Count - historyCursor);
				history.Add(type);
				historyCursor++;

			}
			skipHistory = false;
		}

		internal void GoBackInHistory() {
			skipHistory = true;
			if (real) { // Real just means the slot was empty
				if (historyCursor > 0) {
					int previous = history[historyCursor - 1];
					ReplaceWithFake(previous);
				}
				else {
					if (history.Count == 0)
						Main.NewText(RecipeCatalogueUI.RBText("HistoryEmpty"));
					else
						Main.NewText(RecipeCatalogueUI.RBText("HistoryReachedStart"));
				}
			}
			else {
				if (historyCursor > 1) {
					historyCursor--;
					int previous = history[historyCursor - 1];
					ReplaceWithFake(previous);
				}
				else if (historyCursor == 1) {
					historyCursor--;
					ReplaceWithFake(0);
				}
				else {
					Main.NewText("Error: GoBackInHistory, not real, historyCursor is 0");
				}
			}
			skipHistory = false;
		}

		internal void GoForwardInHistory() {
			skipHistory = true;
			if (real) {
				// Restore the current history item, or the 1st item if at empty start.
				if (history.Count > 0) {
					if (historyCursor == 0)
						historyCursor++;
					int next = history[historyCursor - 1];
					ReplaceWithFake(next);
				}
				else {
					Main.NewText(RecipeCatalogueUI.RBText("HistoryEmpty"));
				}
			}
			else {
				if (historyCursor < history.Count) {
					int next = history[historyCursor];
					historyCursor++;
					ReplaceWithFake(next);
				}
				else {
					Main.NewText(RecipeCatalogueUI.RBText("HistoryReachedEnd"));
				}
			}
			skipHistory = false;
		}

		private void LogHistory(string message) {
			Main.NewText($"{message}: {string.Join(", ", history.Select(x => ItemID.Search.GetName(x)))} -- Cursor {historyCursor}/{(historyCursor > 0 ? ItemID.Search.GetName(history[historyCursor - 1]) : "None")}");
		}
	}
}