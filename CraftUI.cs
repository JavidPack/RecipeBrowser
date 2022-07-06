using Microsoft.Xna.Framework;
using RecipeBrowser.UIElements;
using System.Collections.Generic;
using System.Threading;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace RecipeBrowser
{
	class CraftUI
	{
		internal static string RBText(string key, string category = "CraftUI") => RecipeBrowser.RBText(category, key);

		internal static CraftUI instance;
		internal static Color color = new Color(90, 158, 57);// Color.YellowGreen;

		internal UIPanel mainPanel;
		internal UIPanel craftPanel;
		internal UIList craftPathList;
		internal UICraftQueryItemSlot recipeResultItemSlot;
		internal List<UIElement> additionalDragTargets;

		internal List<int> selectedIndexes;
		internal bool craftPathsUpToDate = false;

		public CraftUI()
		{
			instance = this;
			additionalDragTargets = new List<UIElement>();
			selectedIndexes = new List<int>();
		}

		internal UIElement CreateCraftPanel()
		{
			mainPanel = new UIPanel();
			mainPanel.SetPadding(6);
			mainPanel.BackgroundColor = color;

			mainPanel.Top.Set(20, 0f);
			mainPanel.Height.Set(-20, 1f);
			mainPanel.Width.Set(0, 1f);

			recipeResultItemSlot = new UICraftQueryItemSlot(new Item());
			recipeResultItemSlot.emptyHintText = RBText("EmptyQuerySlotHint");
			// Not used, UICraftQueryItemSlot calls SetItem on click.
			// SetItem -> ReplaceWithFake -> OnItemChanged -> SetItem loop issue.
			//recipeResultItemSlot.OnItemChanged += RecipeResultItemSlot_OnItemChanged;
			mainPanel.Append(recipeResultItemSlot);

			UICheckbox extendedCraft = new UICheckbox(RBText("Calc"), RBText("CalcTooltip"));
			extendedCraft.Top.Set(42, 0f);
			extendedCraft.Left.Set(0, 0f);
			extendedCraft.SetText("  " + RBText("Calc"));
			extendedCraft.Selected = RecipePath.extendedCraft;
			extendedCraft.OnSelectedChanged += (s, e) =>
			{
				RecipeCatalogueUI.instance.InvalidateExtendedCraft();
				RecipePath.extendedCraft = extendedCraft.Selected;
			};
			mainPanel.Append(extendedCraft);

			int top = 2;
			int left = 50;

			// 1.3 used NPC.killCount/Banners, 1.4 now uses bestiary unlock state, handles killed, seen, talked to npcs. Tracked per world, synced in multiplayer.
			UICheckbox lootables = new UICheckbox(RBText("Loot"), RBText("LootTooltip"));
			lootables.Top.Set(top, 0f);
			lootables.Left.Set(left, 0f);
			lootables.Selected = RecipePath.allowLoots;
			lootables.OnSelectedChanged += (s, e) =>
			{
				RecipeCatalogueUI.instance.InvalidateExtendedCraft();
				RecipePath.allowLoots = lootables.Selected;
			};
			mainPanel.Append(lootables);
			left += (int)lootables.MinWidth.Pixels + 6;

			UICheckbox npcShopsCheckbox = new UICheckbox(RBText("Shop"), RBText("ShopTooltip"));
			npcShopsCheckbox.SetDisabled(); // TODO: implement correctly
			npcShopsCheckbox.Top.Set(top, 0f);
			npcShopsCheckbox.Left.Set(left, 0f);
			npcShopsCheckbox.Selected = RecipePath.allowPurchasable;
			npcShopsCheckbox.OnSelectedChanged += (s, e) =>
			{
				RecipeCatalogueUI.instance.InvalidateExtendedCraft();
				RecipePath.allowPurchasable = npcShopsCheckbox.Selected;
			};
			mainPanel.Append(npcShopsCheckbox);
			left += (int)npcShopsCheckbox.MinWidth.Pixels + 6;

			// Adjacent, Seen, Unseen. This allows Unseen.
			UICheckbox missingStationsCheckbox = new UICheckbox(RBText("MissingStations"), RBText("MissingStationsTooltip"));
			missingStationsCheckbox.Top.Set(top, 0f);
			missingStationsCheckbox.Left.Set(left, 0f);
			missingStationsCheckbox.Selected = RecipePath.allowMissingStations;
			missingStationsCheckbox.OnSelectedChanged += (s, e) =>
			{
				RecipeCatalogueUI.instance.InvalidateExtendedCraft();
				RecipePath.allowMissingStations = missingStationsCheckbox.Selected;
			};
			mainPanel.Append(missingStationsCheckbox);
			left += (int)missingStationsCheckbox.MinWidth.Pixels + 6;

			// Checkbox for RecipeAvailable?
			// TODO: Handle other invalidations. Button here for forced refresh?

			top += 25;
			left = 50;

			// Option to skip some items? Hotbar? Starred?
			UICheckbox sourceInventoryCheckbox = new UICheckbox(RBText("Inventory"), "");
			sourceInventoryCheckbox.SetDisabled();
			sourceInventoryCheckbox.Top.Set(top, 0f);
			sourceInventoryCheckbox.Left.Set(left, 0f);
			sourceInventoryCheckbox.Selected = RecipePath.sourceInventory;
			sourceInventoryCheckbox.OnSelectedChanged += (s, e) =>
			{
				RecipeCatalogueUI.instance.InvalidateExtendedCraft();
				RecipePath.sourceInventory = sourceInventoryCheckbox.Selected;
			};
			mainPanel.Append(sourceInventoryCheckbox);
			left += (int)sourceInventoryCheckbox.MinWidth.Pixels + 6;

			UICheckbox sourceBanksCheckbox = new UICheckbox(RBText("Banks"), RBText("BanksTooltip"));
			sourceBanksCheckbox.SetDisabled();
			sourceBanksCheckbox.Top.Set(top, 0f);
			sourceBanksCheckbox.Left.Set(left, 0f);
			sourceBanksCheckbox.Selected = RecipePath.sourceBanks;
			sourceBanksCheckbox.OnSelectedChanged += (s, e) =>
			{
				RecipeCatalogueUI.instance.InvalidateExtendedCraft();
				RecipePath.sourceBanks = sourceBanksCheckbox.Selected;
			};
			mainPanel.Append(sourceBanksCheckbox);
			left += (int)sourceBanksCheckbox.MinWidth.Pixels + 6;

			// Need to point to chest on hovering over ingredient, or actually implement taking from chest. Nearby, or siphon remotely?
			UICheckbox sourceChestsCheckbox = new UICheckbox(RBText("Chests"), RBText("ChestsTooltip"));
			sourceChestsCheckbox.SetDisabled();
			sourceChestsCheckbox.Top.Set(top, 0f);
			sourceChestsCheckbox.Left.Set(left, 0f);
			sourceChestsCheckbox.Selected = RecipePath.sourceChests;
			sourceChestsCheckbox.OnSelectedChanged += (s, e) =>
			{
				RecipeCatalogueUI.instance.InvalidateExtendedCraft();
				RecipePath.sourceChests = sourceChestsCheckbox.Selected;
			};
			mainPanel.Append(sourceChestsCheckbox);
			left += (int)sourceChestsCheckbox.MinWidth.Pixels + 6;

			UICheckbox sourceMagicStorageCheckbox = new UICheckbox(RBText("MagicStorage"), RBText("MagicStorageTooltip"));
			sourceMagicStorageCheckbox.SetDisabled();
			sourceMagicStorageCheckbox.Top.Set(top, 0f);
			sourceMagicStorageCheckbox.Left.Set(left, 0f);
			sourceMagicStorageCheckbox.Selected = RecipePath.sourceMagicStorage;
			sourceMagicStorageCheckbox.OnSelectedChanged += (s, e) =>
			{
				RecipeCatalogueUI.instance.InvalidateExtendedCraft();
				RecipePath.sourceMagicStorage = sourceMagicStorageCheckbox.Selected;
			};
			mainPanel.Append(sourceMagicStorageCheckbox);
			left += (int)sourceMagicStorageCheckbox.MinWidth.Pixels + 6;

			top += 37;

			craftPanel = new UIPanel();
			craftPanel.SetPadding(6);
			craftPanel.Top.Pixels = top;
			craftPanel.Left.Set(0, 0f);
			craftPanel.Width.Set(0, 1f);
			craftPanel.Height.Set(-top - 16, 1f);
			craftPanel.BackgroundColor = Color.DarkCyan;

			// TODO: Fix vanilla UIList to properly capture scrollwheel
			craftPathList = new UIList();
			craftPathList.Width.Set(-24f, 1f);
			craftPathList.Height.Set(0, 1f);
			craftPathList.ListPadding = 6f;
			craftPanel.Append(craftPathList);

			var craftPathListScrollbar = new FixedUIScrollbar(RecipeBrowserUI.instance.userInterface);
			craftPathListScrollbar.SetView(100f, 1000f);
			craftPathListScrollbar.Height.Set(0, 1f);
			craftPathListScrollbar.Left.Set(-20, 1f);
			craftPanel.Append(craftPathListScrollbar);
			craftPathList.SetScrollbar(craftPathListScrollbar);
			mainPanel.Append(craftPanel);

			UIText text = new UIText(RBText("BottomInstructions"), 0.85f);
			text.Top.Set(-14, 1f);
			text.HAlign = 0.5f;
			mainPanel.Append(text);
			additionalDragTargets.Add(text);

			return mainPanel;
		}

		private void PopulateList(List<UIRecipeSlot> slots)
		{
			List<UIRecipePath> recipePaths = new List<UIRecipePath>();
			craftPathList.Clear();
			foreach (var slot in slots)
			{
				foreach (var path in slot.craftPaths)
				{
					recipePaths.Add(new UIRecipePath(path));
				}
			}
			foreach (var item in recipePaths)
			{
				craftPathList.Add(item);
			}
			if (recipePaths.Count == 0)
			{
				var message = new UIText(RBText("NoPathsFound"));
				craftPathList.Add(message);
			}
		}

		internal void SetRecipe(int index)
		{
			// SetRecipe is more accessible than SetItem through the UI, but might not match the intended player usage. TODO: Option to not limit search to single recipe.
			selectedIndexes.Clear();
			selectedIndexes.Add(index);
			Recipe recipe = Main.recipe[index];
			recipeResultItemSlot.ReplaceWithFake(recipe.createItem.type);
			craftPathsUpToDate = false;
		}

		internal void SetItem(int itemID)
		{
			List<int> indexes = new List<int>();
			for (int i = 0; i < Recipe.numRecipes; i++)
			{
				Recipe r = Main.recipe[i];
				if (r.createItem.type == itemID)
				{
					indexes.Add(i);
				}
			}
			selectedIndexes.Clear();
			selectedIndexes.AddRange(indexes);

			recipeResultItemSlot.ReplaceWithFake(itemID);
			craftPathsUpToDate = false;
		}

		internal void Update()
		{
			//Main.playerInventory = true;
			if (craftPathsUpToDate)
				return;

			if (!RecipeBrowserUI.instance.ShowRecipeBrowser || RecipeBrowserUI.instance.CurrentPanel != RecipeBrowserUI.Craft)
				return;

			craftPathsUpToDate = true; 
			var slots = new List<UIRecipeSlot>();
			foreach (var selectedIndex in selectedIndexes)
			{
				var slot = RecipeCatalogueUI.instance.recipeSlots[selectedIndex];
				slot.CraftPathsImmediatelyNeeded();
				slots.Add(slot);
			}
			PopulateList(slots);
		}
	}
}
