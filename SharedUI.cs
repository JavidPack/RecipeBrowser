using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RecipeBrowser.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using static RecipeBrowser.Utilities;
using Terraria.WorldBuilding;
using Terraria.GameContent.ItemDropRules;

namespace RecipeBrowser
{
	class SharedUI
	{
		internal static string RBText(string key, string category = "RecipeCatalogueFilters") => RecipeBrowser.RBText(category, key);

		internal static SharedUI instance;
		internal bool updateNeeded;

		internal UIPanel sortsAndFiltersPanel;
		internal UIHorizontalGrid categoriesGrid;
		internal InvisibleFixedUIHorizontalScrollbar categoriesGridScrollbar;
		internal UIHorizontalGrid subCategorySortsFiltersGrid;
		internal InvisibleFixedUIHorizontalScrollbar lootGridScrollbar2;

		private Sort selectedSort;
		internal Sort SelectedSort {
			get { return selectedSort; }
			set {
				if (selectedSort != value) {
					updateNeeded = true;
					RecipeCatalogueUI.instance.updateNeeded = true;
					ItemCatalogueUI.instance.updateNeeded = true;
				}
				selectedSort = value;
			}
		}

		private Category selectedCategory;
		internal Category SelectedCategory {
			get { return selectedCategory; }
			set {
				if (selectedCategory != value) {
					updateNeeded = true;
					RecipeCatalogueUI.instance.updateNeeded = true;
					ItemCatalogueUI.instance.updateNeeded = true;
				}
				selectedCategory = value;
				if (selectedCategory != null && selectedCategory.sorts.Count > 0)
					SelectedSort = selectedCategory.sorts[0];
				else if (selectedCategory != null && selectedCategory.parent != null && selectedCategory.parent.sorts.Count > 0)
					SelectedSort = selectedCategory.parent.sorts[0];
			}
		}

		public SharedUI() {
			instance = this;
		}

		internal void Initialize() {
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
			sortsAndFiltersPanel.BackgroundColor = Color.CornflowerBlue;//Color.LightSeaGreen;

			//sortsAndFiltersPanel.SetPadding(4);
			//mainPanel.Append(sortsAndFiltersPanel);
			//additionalDragTargets.Add(sortsAndFiltersPanel);
			//SetupSortsAndCategories();
			//PopulateSortsAndFiltersPanel();

			updateNeeded = true;
		}

		internal void Update() {
			if (!updateNeeded) { return; }
			updateNeeded = false;

			// Delay this so we can integrate mod categories.
			if (sorts == null) {
				SetupSortsAndCategories();
			}

			PopulateSortsAndFiltersPanel();
		}

		internal List<Filter> availableFilters;
		private void PopulateSortsAndFiltersPanel() {
			var availableSorts = new List<Sort>(sorts);
			availableSorts.RemoveAll(x => !x.sortAvailable?.Invoke() ?? false);
			availableFilters = new List<Filter>(filters);

			if (!Main.GameModeInfo.IsJourneyMode)
				availableFilters.Remove(SharedUI.instance.UnresearchedFilter);

			//sortsAndFiltersPanel.RemoveAllChildren();
			if (subCategorySortsFiltersGrid != null) {
				sortsAndFiltersPanel.RemoveChild(subCategorySortsFiltersGrid);
				sortsAndFiltersPanel.RemoveChild(lootGridScrollbar2);
			}

			if (categoriesGrid == null) {
				categoriesGrid = new UIHorizontalGrid();
				categoriesGrid.Width.Set(0, 1f);
				categoriesGrid.Height.Set(26, 0f);
				categoriesGrid.ListPadding = 2f;
				categoriesGrid.drawArrows = true;

				categoriesGridScrollbar = new InvisibleFixedUIHorizontalScrollbar(RecipeBrowserUI.instance.userInterface);
				categoriesGridScrollbar.SetView(100f, 1000f);
				categoriesGridScrollbar.Width.Set(0, 1f);
				categoriesGridScrollbar.Top.Set(0, 0f);
				sortsAndFiltersPanel.Append(categoriesGridScrollbar);
				categoriesGrid.SetScrollbar(categoriesGridScrollbar);
				sortsAndFiltersPanel.Append(categoriesGrid); // This is after so it gets the mouse events.
			}

			subCategorySortsFiltersGrid = new UIHorizontalGrid();
			subCategorySortsFiltersGrid.Width.Set(0, 1f);
			subCategorySortsFiltersGrid.Top.Set(26, 0f);
			subCategorySortsFiltersGrid.Height.Set(26, 0f);
			subCategorySortsFiltersGrid.ListPadding = 2f;
			subCategorySortsFiltersGrid.drawArrows = true;

			float oldRow2ViewPosition = lootGridScrollbar2?.ViewPosition ?? 0f;
			lootGridScrollbar2 = new InvisibleFixedUIHorizontalScrollbar(RecipeBrowserUI.instance.userInterface);
			lootGridScrollbar2.SetView(100f, 1000f);
			lootGridScrollbar2.Width.Set(0, 1f);
			lootGridScrollbar2.Top.Set(28, 0f);
			sortsAndFiltersPanel.Append(lootGridScrollbar2);
			subCategorySortsFiltersGrid.SetScrollbar(lootGridScrollbar2);
			sortsAndFiltersPanel.Append(subCategorySortsFiltersGrid);

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
			foreach (var category in categories) {
				category.button.selected = false;
				visibleCategories.Add(category);
				bool meOrChildSelected = SelectedCategory == category;
				foreach (var subcategory in category.subCategories) {
					subcategory.button.selected = false;
					meOrChildSelected |= subcategory == SelectedCategory;
				}
				if (meOrChildSelected) {
					visibleSubCategories.AddRange(category.subCategories);
					category.button.selected = true;
				}
				if (RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.RecipeCatalogue && category.name == ArmorSetFeatureHelper.ArmorSetsHoverTest)
					visibleCategories.Remove(category);
			}

			float oldTopRowViewPosition = categoriesGridScrollbar?.ViewPosition ?? 0f;
			categoriesGrid.Clear();
			foreach (var category in visibleCategories) {
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

			foreach (var category in visibleSubCategories) {
				var container = new UISortableElement(++count);
				container.Width.Set(24, 0);
				container.Height.Set(24, 0);
				container.Append(category.button);
				subCategorySortsFiltersGrid.Add(container);
				left += 26;
			}

			if (visibleSubCategories.Count > 0) {
				var container2 = new UISortableElement(++count);
				container2.Width.Set(24, 0);
				container2.Height.Set(24, 0);
				var image = new UIImage(RecipeBrowser.instance.Assets.Request<Texture2D>("Images/spacer"));
				//image.Left.Set(6, 0);
				image.HAlign = 0.5f;
				container2.Append(image);
				subCategorySortsFiltersGrid.Add(container2);
			}

			// add to sorts and filters here
			if (SelectedCategory != null) {
				SelectedCategory.button.selected = true;
				SelectedCategory.ParentAddToSorts(availableSorts);
				SelectedCategory.ParentAddToFilters(availableFilters);
			}

			left = 0;
			foreach (var sort in availableSorts) {
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
			if (!availableSorts.Contains(SharedUI.instance.SelectedSort)) {
				availableSorts[0].button.selected = true;
				SharedUI.instance.SelectedSort = availableSorts[0];
				updateNeeded = false;
			}

			if (availableFilters.Count > 0) {
				var container2 = new UISortableElement(++count);
				container2.Width.Set(24, 0);
				container2.Height.Set(24, 0);
				var image = new UIImage(RecipeBrowser.instance.Assets.Request<Texture2D>("Images/spacer"));
				image.HAlign = 0.5f;
				container2.Append(image);
				subCategorySortsFiltersGrid.Add(container2);

				foreach (var item in availableFilters) {
					var container = new UISortableElement(++count);
					container.Width.Set(24, 0);
					container.Height.Set(24, 0);
					container.Append(item.button);
					subCategorySortsFiltersGrid.Add(container);
				}
			}

			// Restore view position after CycleFilter changes current filters.
			subCategorySortsFiltersGrid.Recalculate();
			lootGridScrollbar2.ViewPosition = oldRow2ViewPosition;
			categoriesGrid.Recalculate();
			//categoriesGridScrollbar.ViewPosition = oldTopRowViewPosition; // And after category disappears, not really needed since only 1 will disappear, unlike 2nd row. Test more if more special categories are added
		}

		internal List<Category> categories;
		internal List<Filter> filters;
		internal Filter CraftableFilter;
		internal Filter ObtainableFilter;
		internal Filter DisabledFilter;
		internal Filter UnresearchedFilter;
		internal List<Sort> sorts;

		// Items whose textures are resized used during setup
		// If they aren't loaded, some buttons doesn't have an icon
		// TODO: A better way to do this?
		private int[] itemTexturePreload =
		{
			ItemID.MetalDetector, ItemID.SpellTome, ItemID.IronAnvil, ItemID.MythrilAnvil, ItemID.Blindfold, ItemID.GoldBroadsword, ItemID.GoldenShower, ItemID.FlintlockPistol,
			ItemID.Shuriken, ItemID.SlimeStaff, ItemID.BlandWhip, ItemID.DD2LightningAuraT1Popper, ItemID.SilverHelmet, ItemID.SilverChainmail, ItemID.SilverGreaves,
			ItemID.BunnyHood, ItemID.HerosHat, ItemID.GoldHelmet, ItemID.Sign, ItemID.IronAnvil, ItemID.PearlstoneBrickWall, ItemID.EoCShield, ItemID.KingSlimeMasterTrophy,
			ItemID.ZephyrFish, ItemID.FairyBell, ItemID.MechanicalSkull, ItemID.SlimySaddle, ItemID.AmethystHook, ItemID.OrangeDye, ItemID.BiomeHairDye,
			ItemID.FallenStarfish, ItemID.FishingBobber, ItemID.HermesBoots, ItemID.LeafWings, ItemID.Minecart, ItemID.HealingPotion, ItemID.ManaPotion, ItemID.RagePotion,
			ItemID.AlphabetStatueA, ItemID.GoldChest, ItemID.PaintingMartiaLisa, ItemID.HeartStatue, ItemID.Wire, ItemID.PurificationPowder,
			ItemID.Extractinator, ItemID.UnicornonaStick, ItemID.SilverHelmet, ItemID.BunnyHood, ItemID.ZephyrFish, ItemID.Sign, ItemID.FallenStarfish,
			ItemID.HealingPotion, ItemID.OrangeDye, ItemID.Candelabra, ItemID.GrandfatherClock, ItemID.WoodenDoor, ItemID.WoodenChair, ItemID.PalmWoodTable, ItemID.ChineseLantern,
			ItemID.RainbowTorch, ItemID.GoldBunny, ItemID.WoodenDoor, ItemID.WoodenChair, ItemID.PalmWoodTable, ItemID.ChineseLantern, ItemID.RainbowTorch,
			ItemID.KingSlimeBossBag, ItemID.WoodenCrate, ItemID.WoodenCrateHard, ItemID.EyeOfCthulhuBossBag, ItemID.PlanteraBossBag, ItemID.HerbBag
		};

		private void SetupSortsAndCategories() {
			foreach (int type in itemTexturePreload)
				Main.instance.LoadItem(type); // needs ImmediateLoad. Could do this setup in Load if determined to be slow.

			Asset<Texture2D> creativeSort = ResizeImage(TextureAssets.InventorySort[0], 24, 24);
			Asset<Texture2D> recipeOrderSort = ResizeImage(TextureAssets.CraftToggle[2], 24, 24);
			Asset<Texture2D> rarity = ResizeImage(TextureAssets.Item[ItemID.MetalDetector], 24, 24);

			// TODO: Implement Badge text as used in Item Checklist.
			sorts = new List<Sort>()
			{
				new Sort(RBText("RecipeOrder"), recipeOrderSort, (x, y) => 0) {
					recipeSort = (x, y) => x.RecipeIndex.CompareTo(y.RecipeIndex),
					sortAvailable = () => RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.RecipeCatalogue,
				},
				new Sort(RBText("CreativeSort"), creativeSort, ByCreativeSortingId) {
					sortAvailable = () => RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.ItemCatalogue,
				},
				new Sort(RBText("ItemID"), "Images/sortItemID", (x,y)=>x.type.CompareTo(y.type)),
				new Sort(RBText("Value"), "Images/sortValue", (x,y)=>x.value.CompareTo(y.value)),
				new Sort(RBText("Alphabetical"), "Images/sortAZ", (x,y)=>x.Name.CompareTo(y.Name)),
				new Sort(RBText("Rarity"), rarity, (x,y)=> x.rare==y.rare ? x.value.CompareTo(y.value) : Math.Abs(x.rare).CompareTo(Math.Abs(y.rare))),
			};

			Asset<Texture2D> materialsIcon = Utilities.StackResizeImage(new[] { TextureAssets.Item[ItemID.SpellTome] }, 24, 24);
			Asset<Texture2D> craftableIcon = ResizeImage(TextureAssets.Item[ItemID.IronAnvil], 24, 24);
			Asset<Texture2D> extendedCraftIcon = ResizeImage(TextureAssets.Item[ItemID.MythrilAnvil], 24, 24);
			Asset<Texture2D> unresearchedIcon = Utilities.StackResizeImage(new[] { Main.Assets.Request<Texture2D>("Images/UI/WorldCreation/IconDifficultyCreative") }, 24, 24);
			Asset<Texture2D> disabledIcon = ResizeImage(TextureAssets.Item[ItemID.Blindfold], 24, 24);
			filters = new List<Filter>()
			{
				new Filter(RBText("Materials"), x=>x.material, materialsIcon),
				(CraftableFilter = new Filter(RBText("Craftable"), x=>true, craftableIcon)),
				(ObtainableFilter = new Filter(RBText("ExtendedCraftable"), x=>true, extendedCraftIcon)),
				(DisabledFilter = new Filter(RBText("DisabledRecipes"), x=>true, disabledIcon)),
				(UnresearchedFilter = new Filter(RBText("Unresearched"), x=>{
					return Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId.ContainsKey(x.type) && !RecipePath.ItemFullyResearched(x.type);
				}, unresearchedIcon)),
			};

			// TODOS: Vanity armor, grapple, cart, potions buffs
			// 24x24 pixels

			var yoyos = new List<int>();
			for (int i = 0; i < ItemID.Sets.Yoyo.Length; ++i) {
				if (ItemID.Sets.Yoyo[i]) {
					Main.instance.LoadItem(i);
					yoyos.Add(i);
				}
			}

			var useAmmoTypes = new Dictionary<int, int>();
			var ammoTypes = new Dictionary<int, int>();
			var testItem = new Item();
			for (int i = 0; i < ItemLoader.ItemCount; i++) {
				testItem.SetDefaults(i);
				if (testItem.useAmmo >= ItemLoader.ItemCount || testItem.ammo >= ItemLoader.ItemCount || testItem.useAmmo < 0 || testItem.ammo < 0)
					continue; // Some mods misuse useAmmo
				if (testItem.useAmmo > 0) {
					useAmmoTypes.TryGetValue(testItem.useAmmo, out var currentCount);
					useAmmoTypes[testItem.useAmmo] = currentCount + 1;
				}
				if (testItem.ammo > 0) {
					ammoTypes.TryGetValue(testItem.ammo, out var currentCount);
					ammoTypes[testItem.ammo] = currentCount + 1;
				}
			}
			var sortedUseAmmoTypes = from pair in useAmmoTypes orderby pair.Value descending select pair.Key;
			var sortedAmmoTypes = from pair in ammoTypes orderby pair.Value descending select pair.Key;

			foreach (int type in sortedUseAmmoTypes)
				Main.instance.LoadItem(type);
			foreach (int type in sortedAmmoTypes)
				Main.instance.LoadItem(type);

			var ammoFilters = sortedAmmoTypes.Select(ammoType => new Filter(Lang.GetItemNameValue(ammoType), x => x.ammo == ammoType, ResizeImage(TextureAssets.Item[ammoType], 24, 24))).ToList();
			var useAmmoFilters = sortedUseAmmoTypes.Select(ammoType => new Filter(Lang.GetItemNameValue(ammoType), x => x.useAmmo == ammoType, ResizeImage(TextureAssets.Item[ammoType], 24, 24))).ToList();

			// TODO: Filter Conditions CycleFilter: Graveyard, etc.
			var ammoFilter = new CycleFilter(RBText("Ammo.CycleAmmoTypes"), "Images/sortAmmo", ammoFilters);
			var useAmmoFilter = new CycleFilter(RBText("Weapons.CycleUsedAmmoTypes"), "Images/sortAmmo", useAmmoFilters);

			Asset<Texture2D> smallMelee = ResizeImage(TextureAssets.Item[ItemID.GoldBroadsword], 24, 24);
			Asset<Texture2D> smallYoyo = ResizeImage(TextureAssets.Item[Main.rand.Next(yoyos)], 24, 24); //Main.rand.Next(ItemID.Sets.Yoyo) ItemID.Yelets
			Asset<Texture2D> smallMagic = ResizeImage(TextureAssets.Item[ItemID.GoldenShower], 24, 24);
			Asset<Texture2D> smallRanged = ResizeImage(TextureAssets.Item[ItemID.FlintlockPistol], 24, 24);
			Asset<Texture2D> smallThrown = ResizeImage(TextureAssets.Item[ItemID.Shuriken], 24, 24);
			Asset<Texture2D> smallSummon = ResizeImage(TextureAssets.Item[ItemID.SlimeStaff], 24, 24);
			Asset<Texture2D> smallWhip = ResizeImage(TextureAssets.Item[ItemID.BlandWhip], 24, 24);
			Asset<Texture2D> smallSentry = ResizeImage(TextureAssets.Item[ItemID.DD2LightningAuraT1Popper], 24, 24);
			Asset<Texture2D> smallHead = ResizeImage(TextureAssets.Item[ItemID.SilverHelmet], 24, 24);
			Asset<Texture2D> smallBody = ResizeImage(TextureAssets.Item[ItemID.SilverChainmail], 24, 24);
			Asset<Texture2D> smallLegs = ResizeImage(TextureAssets.Item[ItemID.SilverGreaves], 24, 24);
			Asset<Texture2D> smallVanity = ResizeImage(TextureAssets.Item[ItemID.BunnyHood], 24, 24);
			//Asset<Texture2D> smallVanity2 = ResizeImage(TextureAssets.Item[ItemID.HerosHat], 24, 24);
			Asset<Texture2D> smallNonVanity = ResizeImage(TextureAssets.Item[ItemID.GoldHelmet], 24, 24);
			Asset<Texture2D> smallTiles = ResizeImage(TextureAssets.Item[ItemID.Sign], 24, 24);
			Asset<Texture2D> smallCraftingStation = ResizeImage(TextureAssets.Item[ItemID.IronAnvil], 24, 24);
			Asset<Texture2D> smallWalls = ResizeImage(TextureAssets.Item[ItemID.PearlstoneBrickWall], 24, 24);
			Asset<Texture2D> smallExpert = ResizeImage(TextureAssets.Item[ItemID.EoCShield], 24, 24);
			Asset<Texture2D> smallMaster = ResizeImage(TextureAssets.Item[ItemID.KingSlimeMasterTrophy], 24, 24);
			Asset<Texture2D> smallPets = ResizeImage(TextureAssets.Item[ItemID.ZephyrFish], 24, 24);
			Asset<Texture2D> smallLightPets = ResizeImage(TextureAssets.Item[ItemID.FairyBell], 24, 24);
			Asset<Texture2D> smallBossSummon = ResizeImage(TextureAssets.Item[ItemID.MechanicalSkull], 24, 24);
			Asset<Texture2D> smallMounts = ResizeImage(TextureAssets.Item[ItemID.SlimySaddle], 24, 24);
			Asset<Texture2D> smallHooks = ResizeImage(TextureAssets.Item[ItemID.AmethystHook], 24, 24);
			Asset<Texture2D> smallDyes = ResizeImage(TextureAssets.Item[ItemID.OrangeDye], 24, 24);
			Asset<Texture2D> smallHairDye = ResizeImage(TextureAssets.Item[ItemID.BiomeHairDye], 24, 24);
			Asset<Texture2D> smallQuestFish = ResizeImage(TextureAssets.Item[ItemID.FallenStarfish], 24, 24);
			Asset<Texture2D> smallFishingBobber = ResizeImage(TextureAssets.Item[ItemID.FishingBobber], 24, 24);
			Asset<Texture2D> smallAccessories = ResizeImage(TextureAssets.Item[ItemID.HermesBoots], 24, 24);
			Asset<Texture2D> smallWings = ResizeImage(TextureAssets.Item[ItemID.LeafWings], 24, 24);
			Asset<Texture2D> smallCarts = ResizeImage(TextureAssets.Item[ItemID.Minecart], 24, 24);
			Asset<Texture2D> smallHealth = ResizeImage(TextureAssets.Item[ItemID.HealingPotion], 24, 24);
			Asset<Texture2D> smallMana = ResizeImage(TextureAssets.Item[ItemID.ManaPotion], 24, 24);
			Asset<Texture2D> smallBuff = ResizeImage(TextureAssets.Item[ItemID.RagePotion], 24, 24);
			Asset<Texture2D> smallAll = ResizeImage(TextureAssets.Item[ItemID.AlphabetStatueA], 24, 24);
			Asset<Texture2D> smallContainer = ResizeImage(TextureAssets.Item[ItemID.GoldChest], 24, 24);
			Asset<Texture2D> smallPaintings = ResizeImage(TextureAssets.Item[ItemID.PaintingMartiaLisa], 24, 24);
			Asset<Texture2D> smallStatue = ResizeImage(TextureAssets.Item[ItemID.HeartStatue], 24, 24);
			Asset<Texture2D> smallWiring = ResizeImage(TextureAssets.Item[ItemID.Wire], 24, 24);
			Asset<Texture2D> smallConsumables = ResizeImage(TextureAssets.Item[ItemID.PurificationPowder], 24, 24);
			Asset<Texture2D> smallGrabBags = ResizeImage(TextureAssets.Item[ItemID.KingSlimeBossBag], 24, 24);
			Asset<Texture2D> smallExtractinator = ResizeImage(TextureAssets.Item[ItemID.Extractinator], 24, 24);
			Asset<Texture2D> smallOther = ResizeImage(TextureAssets.Item[ItemID.UnicornonaStick], 24, 24);

			Asset<Texture2D> smallArmor = StackResizeImage(new[] { TextureAssets.Item[ItemID.SilverHelmet], TextureAssets.Item[ItemID.SilverChainmail], TextureAssets.Item[ItemID.SilverGreaves] }, 24, 24);
			//Texture2D smallVanityFilterGroup = StackResizeImage2424(TextureAssets.Item[ItemID.BunnyHood], TextureAssets.Item[ItemID.GoldHelmet]);
			Asset<Texture2D> smallPetsLightPets = StackResizeImage(new[] { TextureAssets.Item[ItemID.ZephyrFish], TextureAssets.Item[ItemID.FairyBell] }, 24, 24);
			Asset<Texture2D> smallPlaceables = StackResizeImage(new[] { TextureAssets.Item[ItemID.Sign], TextureAssets.Item[ItemID.PearlstoneBrickWall] }, 24, 24);
			Asset<Texture2D> smallWeapons = StackResizeImage(new[] { smallMelee, smallMagic, smallThrown }, 24, 24);
			Asset<Texture2D> smallTools = StackResizeImage(new[] { RecipeBrowser.instance.Assets.Request<Texture2D>("Images/sortPick"), RecipeBrowser.instance.Assets.Request<Texture2D>("Images/sortAxe"), RecipeBrowser.instance.Assets.Request<Texture2D>("Images/sortHammer") }, 24, 24);
			Asset<Texture2D> smallFishing = StackResizeImage(new[] { RecipeBrowser.instance.Assets.Request<Texture2D>("Images/sortFish"), RecipeBrowser.instance.Assets.Request<Texture2D>("Images/sortBait"), TextureAssets.Item[ItemID.FallenStarfish] }, 24, 24);
			Asset<Texture2D> smallPotions = StackResizeImage(new[] { TextureAssets.Item[ItemID.HealingPotion], TextureAssets.Item[ItemID.ManaPotion], TextureAssets.Item[ItemID.RagePotion] }, 24, 24);
			Asset<Texture2D> smallBothDyes = StackResizeImage(new[] { TextureAssets.Item[ItemID.OrangeDye], TextureAssets.Item[ItemID.BiomeHairDye] }, 24, 24);
			Asset<Texture2D> smallSortTiles = StackResizeImage(new[] { TextureAssets.Item[ItemID.Candelabra], TextureAssets.Item[ItemID.GrandfatherClock] }, 24, 24);

			Asset<Texture2D> StackResizeImage2424(params Asset<Texture2D>[] textures) => StackResizeImage(textures, 24, 24);
			Asset<Texture2D> ResizeImage2424(Asset<Texture2D> texture) => ResizeImage(texture, 24, 24);

			// Potions, other?
			// should inherit children?
			// should have other category?
			if (GenVars.statueList == null)
				WorldGen.SetupStatueList();

			var vanity = new MutuallyExclusiveFilter(RBText("Armor.Vanity"), x => x.vanity, smallVanity);
			var armor = new MutuallyExclusiveFilter(RBText("Armor.ArmorOnly"), x => !x.vanity, smallNonVanity);
			vanity.SetExclusions(new List<Filter>() { vanity, armor });
			armor.SetExclusions(new List<Filter>() { vanity, armor });

			categories = new List<Category>() {
				new Category(RBText("All"), x=> true, smallAll),
				// TODO: Filter out tools from weapons. Separate belongs and doesn't belong predicates? How does inheriting work again? Other?
				new Category(RBText("Weapons.Name")/*, x=>x.damage>0*/, x=> false && x.type != ItemID.WorldGlobe, smallWeapons) { //"Images/sortDamage"
					subCategories = new List<Category>() {
						new Category(RBText("Weapons.Melee"), x=>x.CountsAsClass(DamageClass.Melee) && !(x.pick>0 || x.axe>0 || x.hammer>0), smallMelee),
						new Category(RBText("Weapons.Yoyo"), x=>ItemID.Sets.Yoyo[x.type], smallYoyo),
						new Category(RBText("Weapons.Magic"), x=>x.CountsAsClass(DamageClass.Magic), smallMagic),
						new Category(RBText("Weapons.Ranged"), x=>x.CountsAsClass(DamageClass.Ranged) && x.ammo == 0, smallRanged) // TODO and ammo no
						{
							sorts = new List<Sort>() { new Sort(RBText("Weapons.UseAmmoType"), "Images/sortAmmo", (x,y)=>x.useAmmo.CompareTo(y.useAmmo)), },
							filters = new List<Filter> { useAmmoFilter }
						},
						new Category(RBText("Weapons.Throwing"), x=>x.CountsAsClass(DamageClass.Throwing), smallThrown),
						new Category(RBText("Weapons.Summon"), x=>x.CountsAsClass(DamageClass.Summon) && !x.sentry && !ProjectileID.Sets.IsAWhip[x.shoot], smallSummon),

						new Category(RBText("Weapons.Whip"), x=>x.CountsAsClass(DamageClass.Summon) && !x.sentry && ProjectileID.Sets.IsAWhip[x.shoot] , smallWhip),
						new Category(RBText("Weapons.Sentry"), x=>x.CountsAsClass(DamageClass.Summon) && x.sentry, smallSentry),

					},
					sorts = new List<Sort>() { new Sort(RBText("Damage"), "Images/sortDamage", (x,y)=>x.damage.CompareTo(y.damage)) },
				},
				new Category(RBText("Tools.Name")/*,x=>x.pick>0||x.axe>0||x.hammer>0*/, x=>false, smallTools) {
					subCategories = new List<Category>() {
						new Category(RBText("Tools.Pickaxes"), x=>x.pick>0, "Images/sortPick") { sorts = new List<Sort>() { new Sort(RBText("Tools.PickPower"), "Images/sortPick", (x,y)=>x.pick.CompareTo(y.pick)), } },
						new Category(RBText("Tools.Axes"), x=>x.axe>0, "Images/sortAxe"){ sorts = new List<Sort>() { new Sort(RBText("Tools.AxePower"), "Images/sortAxe", (x,y)=>x.axe.CompareTo(y.axe)), } },
						new Category(RBText("Tools.Hammers"), x=>x.hammer>0, "Images/sortHammer"){ sorts = new List<Sort>() { new Sort(RBText("Tools.HammerPower"), "Images/sortHammer", (x,y)=>x.hammer.CompareTo(y.hammer)), } },
					},
				},
				new Category(ArmorSetFeatureHelper.ArmorSetsHoverTest, x => true, "Images/categoryArmorSets") {
					sorts = new List<Sort>() { new Sort(RBText("Armor.TotalDefense"), "Images/categoryArmorSets", (x,y)=>x.defense.CompareTo(y.defense)), }, // See ItemCatalogueUI.ItemGridSort for actual implementation
				},
				new Category(RBText("Armor.Name")/*,  x=>x.headSlot!=-1||x.bodySlot!=-1||x.legSlot!=-1*/, x => false, smallArmor) {
					subCategories = new List<Category>() {
						new Category(RBText("Armor.Head"), x=>x.headSlot!=-1, smallHead),
						new Category(RBText("Armor.Body"), x=>x.bodySlot!=-1, smallBody),
						new Category(RBText("Armor.Legs"), x=>x.legSlot!=-1, smallLegs),
					},
					sorts = new List<Sort>() { new Sort(RBText("Armor.Defense"), "Images/sortDefense", (x,y)=>x.defense.CompareTo(y.defense)), },
					filters = new List<Filter> {
						//new Filter("Vanity", x=>x.vanity, RecipeBrowser.instance.Assets.Request<Texture2D>("Images/sortDefense")),
						// Prefer MutuallyExclusiveFilter for this, rather than CycleFilter since there are only 2 options.
						//new CycleFilter("Vanity/Armor", smallVanityFilterGroup, new List<Filter> {
						//	new Filter("Vanity", x=>x.vanity, smallVanity),
						//	new Filter("Armor", x=>!x.vanity, smallNonVanity),
						//}),
						vanity, armor,
						//new DoubleFilter("Vanity", "Armor", smallVanity2, x=>x.vanity),
					}
				},
				new Category(RBText("Tiles.Name"), x=>x.createTile!=-1, smallTiles)
				{
					subCategories = new List<Category>()
					{
						new Category(RBText("Tiles.CraftingStations"), x=>RecipeCatalogueUI.instance.craftingTiles.Contains(x.createTile), smallCraftingStation),
						new Category(RBText("Tiles.Containers"), x=>x.createTile!=-1 && Main.tileContainer[x.createTile], smallContainer),
						new Category(RBText("Tiles.Wiring"), x=>ItemID.Sets.SortingPriorityWiring[x.type] > -1, smallWiring),
						new Category(RBText("Tiles.Statues"), x=>GenVars.statueList.Any(point => point.X == x.createTile && point.Y == x.placeStyle), smallStatue), // Alphabet statues not here, should they be included?
						new Category(RBText("Tiles.Doors"), x=> x.createTile > 0 && TileID.Sets.RoomNeeds.CountsAsDoor.Contains(x.createTile), ResizeImage2424(TextureAssets.Item[ItemID.WoodenDoor])),
						new Category(RBText("Tiles.Chairs"), x=> x.createTile > 0 && TileID.Sets.RoomNeeds.CountsAsChair.Contains(x.createTile), ResizeImage2424(TextureAssets.Item[ItemID.WoodenChair])),
						new Category(RBText("Tiles.Tables"), x=> x.createTile > 0 && TileID.Sets.RoomNeeds.CountsAsTable.Contains(x.createTile), ResizeImage2424(TextureAssets.Item[ItemID.PalmWoodTable])),
						new Category(RBText("Tiles.LightSources"), x=> x.createTile > 0 && TileID.Sets.RoomNeeds.CountsAsTorch.Contains(x.createTile), ResizeImage2424(TextureAssets.Item[ItemID.ChineseLantern])),
						new Category(RBText("Tiles.Torches"), x=> x.createTile > 0 && TileID.Sets.Torch[x.createTile], ResizeImage2424(TextureAssets.Item[ItemID.RainbowTorch])),
						// Banners => Banner Bonanza mod integration
						//TextureAssets.Item[Main.rand.Next(TileID.Sets.RoomNeeds.CountsAsTable)] doesn't work since those are tilesids. yoyo approach?
						// todo: music box
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
					sorts = new List<Sort>() {
						new Sort(RBText("Tiles.PlaceTile"), smallSortTiles, (x,y)=> x.createTile == y.createTile ? x.placeStyle.CompareTo(y.placeStyle) : x.createTile.CompareTo(y.createTile)),
					}
				},
				new Category(RBText("Walls"), x=>x.createWall!=-1, smallWalls),
				new Category(RBText("Accessories"), x=>x.accessory, smallAccessories)
				{
					subCategories = new List<Category>()
					{
						new Category(RBText("Wings"), x=>x.wingSlot > 0, smallWings)
					}
				},
				new Category(RBText("Ammo.Name"), x=>x.ammo!=0, "Images/sortAmmo")
				{
					sorts = new List<Sort>() {
						new Sort(RBText("Ammo.AmmoType"), "Images/sortAmmo", (x,y)=>x.ammo.CompareTo(y.ammo)),
						new Sort(RBText("Damage"), "Images/sortDamage", (x,y)=>x.damage.CompareTo(y.damage)),
					},
					filters = new List<Filter> { ammoFilter }
					// TODO: Filters/Subcategories for all ammo types? // each click cycles?
				},
				new Category(RBText("Potions.Name"), x=> (x.UseSound?.IsTheSameAs(SoundID.Item3) == true), smallPotions)
				{
					subCategories = new List<Category>() {
						new Category(RBText("Potions.HealthPotions"), x=>x.healLife > 0, smallHealth) { sorts = new List<Sort>() { new Sort(RBText("Potions.HealLife"), smallHealth, (x,y)=>x.healLife.CompareTo(y.healLife)), } },
						new Category(RBText("Potions.ManaPotions"), x=>x.healMana > 0, smallMana) { sorts = new List<Sort>() { new Sort(RBText("Potions.HealMana"), smallMana, (x,y)=>x.healMana.CompareTo(y.healMana)),   }},
						new Category(RBText("Potions.BuffPotions"), x=>(x.UseSound?.IsTheSameAs(SoundID.Item3) == true) && x.buffType > 0 && x.buffType != BuffID.WellFed && x.buffType != BuffID.WellFed2 && x.buffType != BuffID.WellFed3, smallBuff),
						new Category(RBText("Potions.Food"), x=>x.buffType == BuffID.WellFed || x.buffType == BuffID.WellFed2 || x.buffType == BuffID.WellFed3, "Images/sortFood"),
						// Todo: Automatic other category?
					}
				},
				new Category(RBText("Expert"), x=>x.expert, smallExpert),
				new Category(RBText("Master"), x=>x.master, smallMaster),
				new Category(RBText("Pets.Name")/*, x=> x.buffType > 0 && (Main.vanityPet[x.buffType] || Main.lightPet[x.buffType])*/, x=>false, smallPetsLightPets){
					subCategories = new List<Category>() {
						new Category(RBText("Pets.CommonPets"), x=>Main.vanityPet[x.buffType], smallPets),
						new Category(RBText("Pets.LightPets"), x=>Main.lightPet[x.buffType], smallLightPets),
					}
				},
				new Category(RBText("Mounts"), x=>x.mountType != -1, smallMounts)
				{
					subCategories = new List<Category>()
					{
						new Category(RBText("Carts"), x=>x.mountType != -1 && MountID.Sets.Cart[x.mountType], smallCarts) // TODO: need mountType check? inherited parent logic or parent unions children?
					}
				},
				new Category(RBText("Hooks"), x=> Main.projHook[x.shoot], smallHooks){
					sorts = new List<Sort>() {
						new Sort(RBText("GrappleRange"), smallHooks, (x,y)=> GrappleRange(x.shoot).CompareTo(GrappleRange(y.shoot))),
					},
				},
				new Category(RBText("Dyes.Name"), x=>false, smallBothDyes)
				{
					subCategories = new List<Category>()
					{
						new Category(RBText("Dyes.CommonDyes"), x=>x.dye != 0, smallDyes),
						new Category(RBText("Dyes.HairDyes"), x=>x.hairDye != -1, smallHairDye),
					}
				},
				new Category(RBText("BossSummons.Name"), x=>ItemID.Sets.SortingPriorityBossSpawns[x.type] != -1 && x.type != ItemID.LifeCrystal && x.type != ItemID.ManaCrystal && x.type != ItemID.ShellphoneDummy && x.type != ItemID.Shellphone && x.type != ItemID.ShellphoneSpawn && x.type != ItemID.ShellphoneOcean && x.type != ItemID.ShellphoneHell && x.type != ItemID.MagicConch && x.type != ItemID.DemonConch && x.type != ItemID.CellPhone && x.type != ItemID.CellPhone && x.type != ItemID.IceMirror && x.type != ItemID.MagicMirror && x.type != ItemID.LifeFruit && x.netID != ItemID.TreasureMap || x.netID == ItemID.PirateMap, smallBossSummon) { // vanilla bug.
					sorts = new List<Sort>() { new Sort(RBText("BossSummons.ProgressionOrder"), "Images/sortDamage", (x,y)=>ItemID.Sets.SortingPriorityBossSpawns[x.type].CompareTo(ItemID.Sets.SortingPriorityBossSpawns[y.type])), }
				},
				new Category(RBText("Consumables.Name"), x=> !(x.createWall > 0 || x.createTile > -1) && !(x.ammo > 0 && !x.notAmmo) && x.consumable, smallConsumables){
					subCategories = new List<Category>() {
						new Category(RBText("Consumables.CapturedNPC"), x=>x.makeNPC != 0, ResizeImage2424(TextureAssets.Item[ItemID.GoldBunny])),
					}
				},
				new Category(RBText("GrabBags.Name"), x=> Main.ItemDropsDB.GetRulesForItemID(x.type).Any(), smallGrabBags){
					subCategories = new List<Category>() {
						new Category(RBText("GrabBags.FishingCrate"), x=>ItemID.Sets.IsFishingCrate[x.type] && !ItemID.Sets.IsFishingCrateHardmode[x.type], ResizeImage2424(TextureAssets.Item[ItemID.WoodenCrate])),
						new Category(RBText("GrabBags.FishingCrateHardmode"), x=>ItemID.Sets.IsFishingCrateHardmode[x.type], ResizeImage2424(TextureAssets.Item[ItemID.WoodenCrateHard])),
						new Category(RBText("GrabBags.BossBag"), x=>ItemID.Sets.BossBag[x.type] && ItemID.Sets.PreHardmodeLikeBossBag[x.type] && x.type != ItemID.QueenSlimeBossBag, ResizeImage2424(TextureAssets.Item[ItemID.EyeOfCthulhuBossBag])),
						new Category(RBText("GrabBags.BossBagHardmode"), x=>ItemID.Sets.BossBag[x.type] && !ItemID.Sets.PreHardmodeLikeBossBag[x.type] || x.type == ItemID.QueenSlimeBossBag, ResizeImage2424(TextureAssets.Item[ItemID.PlanteraBossBag])),
						new Category(RBText("Other"), x => Main.ItemDropsDB.GetRulesForItemID(x.type).Any() && !ItemID.Sets.BossBag[x.type] && !ItemID.Sets.IsFishingCrate[x.type], ResizeImage2424(TextureAssets.Item[ItemID.HerbBag])),
						// TODO: need to document or streamline "Other" subcategories. Automatically derive from parent belongs?
						// TODO: Golden Lock Box is from Dungeon Crate, but no way for user to know that from UI. Could mention if an item comes from a non-NPC source somehow.
					},
					sorts = new List<Sort>() {
						new Sort(RBText("GrabBags.ExpectedValue"), "Images/sortValue", (x,y)=> ExpectedValue(x.type).CompareTo(ExpectedValue(y.type))),
					},
				},
				new Category(RBText("Fishing.Name")/*, x=> x.fishingPole > 0 || x.bait>0|| x.questItem*/, x=>false, smallFishing){
					subCategories = new List<Category>() {
						new Category(RBText("Fishing.Poles"), x=>x.fishingPole > 0, "Images/sortFish") {sorts = new List<Sort>() { new Sort(RBText("Fishing.PolePower"), "Images/sortFish", (x,y)=>x.fishingPole.CompareTo(y.fishingPole)), } },
						new Category(RBText("Fishing.Bait"), x=>x.bait>0, "Images/sortBait") {sorts = new List<Sort>() { new Sort(RBText("Fishing.BaitPower"), "Images/sortBait", (x,y)=>x.bait.CompareTo(y.bait)), } },
						new Category(RBText("Fishing.Bobbers"), x=>x.type >= ItemID.FishingBobber && x.type <= ItemID.FishingBobberGlowingRainbow, smallFishingBobber),
						new Category(RBText("Fishing.QuestFish"), x=>x.questItem, smallQuestFish),
					}
				},
				new Category(RBText("Extractinator"), x=>ItemID.Sets.ExtractinatorMode[x.type] > -1, smallExtractinator),
				//modCategory,
				new Category(RBText("Other"), x=>BelongsInOther(x), smallOther),
			};

			foreach (var modCategory in RecipeBrowser.instance.modCategories) {
				if (string.IsNullOrEmpty(modCategory.parent)) {
					categories.Insert(categories.Count - 2, new Category(modCategory.name, modCategory.belongs, modCategory.icon));
				}
				else {
					bool placed = false;
					foreach (var item in categories) {
						if (item.name == modCategory.parent) {
							item.subCategories.Add(new Category(modCategory.name, modCategory.belongs, modCategory.icon));
							placed = true;
						}
					}
					if (!placed)
						RecipeBrowser.instance.Logger.Warn($"Parent '{modCategory.parent}' for '{modCategory.name}' category not found. The category will not show up in-game");
				}
			}

			// Filter per mod instead of Mod filter? Expanding filter button?
			foreach (var modFilter in RecipeBrowser.instance.modFilters) {
				if (string.IsNullOrEmpty(modFilter.parent)) {
					filters.Add(new Filter(modFilter.name, modFilter.belongs, modFilter.icon));
				}
				else {
					bool placed = false;
					foreach (var item in categories) {
						if (item.name == modFilter.parent) {
							item.filters.Add(new Filter(modFilter.name, modFilter.belongs, modFilter.icon));
							placed = true;
						}
						foreach (var subCategory in item.subCategories) {
							if (subCategory.name == modFilter.parent) {
								subCategory.filters.Add(new Filter(modFilter.name, modFilter.belongs, modFilter.icon));
								placed = true;
							}
						}
					}
					if (!placed)
						RecipeBrowser.instance.Logger.Warn($"Parent '{modFilter.parent}' for '{modFilter.name}' filter not found. The filter will not show up in-game");
				}
			}

			foreach (var parent in categories) {
				foreach (var child in parent.subCategories) {
					child.parent = parent; // 3 levels?
				}
			}
			SelectedSort = sorts[0];
			SelectedCategory = categories[0];
		}

		private int ByCreativeSortingId(Item x, Item y) {
			ContentSamples.CreativeHelper.ItemGroupAndOrderInGroup itemGroupAndOrderInGroup = ContentSamples.ItemCreativeSortingId[x.type];
			ContentSamples.CreativeHelper.ItemGroupAndOrderInGroup itemGroupAndOrderInGroup2 = ContentSamples.ItemCreativeSortingId[y.type];
			int num = itemGroupAndOrderInGroup.Group.CompareTo(itemGroupAndOrderInGroup2.Group);
			if (num == 0)
				num = itemGroupAndOrderInGroup.OrderInGroup.CompareTo(itemGroupAndOrderInGroup2.OrderInGroup);

			// Fallback to alphabetical for ties.
			if (num == 0)
				num = x.Name.CompareTo(y.Name);

			return num;
		}

		// TODO: Update with new 1.4 values.
		Dictionary<int, float> vanillaGrappleRanges = new Dictionary<int, float>() {
			[13] = 300f,
			[32] = 400f,
			[73] = 440f,
			[74] = 440f,
			[165] = 250f,
			[256] = 350f,
			[315] = 500f,
			[322] = 550f,
			[13] = 300f,
			[331] = 400f,
			[332] = 550f,
			[372] = 400f,
			[396] = 300f,
			[446] = 500f,
			[652] = 600f,
			[646] = 550f,
			[647] = 550f,
			[648] = 550f,
			[649] = 550f,
			[486] = 480f,
			[487] = 480f,
			[488] = 480f,
			[489] = 480f,
			[230] = 300f,
			[231] = 330f,
			[232] = 360f,
			[233] = 390f,
			[234] = 420f,
			[235] = 450f,
		};

		private float GrappleRange(int type) {
			if (vanillaGrappleRanges.ContainsKey(type))
				return vanillaGrappleRanges[type];
			if (type > ProjectileID.Count)
				return ProjectileLoader.GetProjectile(type).GrappleRange();
			return 0;
		}

		internal static bool ShouldShowItemDrop(DropRateInfo dropRateInfo) {
			bool result = true;
			if (dropRateInfo.conditions != null && dropRateInfo.conditions.Count > 0) {
				for (int i = 0; i < dropRateInfo.conditions.Count; i++) {
					if (!dropRateInfo.conditions[i].CanShowItemDropInUI()) {
						result = false;
						break;
					}
				}
			}

			return result;
		}

		private int ExpectedValue(int type) {
			// Could cache for performance, but need to see how drop conditions being later satisfied would affect things. We wouldn't want to cache a value that would later be inaccurate.
			int expectedValue = 0;

			// ItemDropViewer
			List<IItemDropRule> dropRules = Main.ItemDropsDB.GetRulesForItemID(type);
			List<DropRateInfo> list = new List<DropRateInfo>();
			DropRateInfoChainFeed ratesInfo = new DropRateInfoChainFeed(1f);
			foreach (IItemDropRule item in dropRules) {
				item.ReportDroprates(list, ratesInfo);
			}

			foreach (DropRateInfo dropRateInfo in list) {
				bool flag = ShouldShowItemDrop(dropRateInfo);
				if (!flag)
					continue;

				// TODO: Does dropRate already take care of if an item can't drop in specific world conditions? I think not, ShouldShowItem does I think.
				expectedValue += (int)((dropRateInfo.stackMin + dropRateInfo.stackMax) / 2f * dropRateInfo.dropRate * ContentSamples.ItemsByType[dropRateInfo.itemId].value * 0.2f);
			}
			return expectedValue;
		}

		private bool BelongsInOther(Item item) {
			var cats = categories.Skip(1).Take(categories.Count - 2);
			foreach (var category in cats) {
				if(category.name == ArmorSetFeatureHelper.ArmorSetsHoverTest)
					continue;
				if (category.BelongsRecursive(item))
					return false;
			}
			return true;
		}
	}

	internal class Filter
	{
		internal string name;
		internal Predicate<Item> belongs;
		internal List<Category> subCategories;
		internal List<Sort> sorts;
		internal UISilentImageButton button;
		internal Asset<Texture2D> texture;
		//internal Category parent;

		public Filter(string name, Predicate<Item> belongs, Asset<Texture2D> texture) {
			this.name = name;
			this.texture = texture;
			subCategories = new List<Category>();
			sorts = new List<Sort>();
			this.belongs = belongs;

			this.button = new UISilentImageButton(texture, name);
			button.OnLeftClick += (a, b) => {
				button.selected = !button.selected;
				ItemCatalogueUI.instance.updateNeeded = true;
				RecipeCatalogueUI.instance.updateNeeded = true;
				//Main.NewText("clicked on " + button.hoverText);
			};
		}
	}

	internal class MutuallyExclusiveFilter : Filter
	{
		List<Filter> exclusives;

		public MutuallyExclusiveFilter(string name, Predicate<Item> belongs, Asset<Texture2D> texture) : base(name, belongs, texture) {
			button.OnLeftClick += (a, b) => {
				if (button.selected) {
					foreach (var item in exclusives) {
						if (item != this)
							item.button.selected = false;
					}
				}
			};
		}

		internal void SetExclusions(List<Filter> exclusives) {
			this.exclusives = exclusives;
		}
	}

	// A bit confusing, don't use.
	internal class DoubleFilter : Filter
	{
		bool right;
		string other;
		public DoubleFilter(string name, string other, Asset<Texture2D> texture, Predicate<Item> belongs) : base(name, belongs, texture) {
			this.other = other;
			this.belongs = (item) => {
				return belongs(item) ^ right;
			};
			button = new UIBadgedSilentImageButton(texture, name + " (RMB)");
			button.OnLeftClick += (a, b) => {
				button.selected = !button.selected;
				ItemCatalogueUI.instance.updateNeeded = true;
				RecipeCatalogueUI.instance.updateNeeded = true;
				//Main.NewText("clicked on " + button.hoverText);
			};
			button.OnRightClick += (a, b) => {
				right = !right;
				(button as UIBadgedSilentImageButton).drawX = right;
				button.hoverText = (right ? other : name) + " (RMB)";
				ItemCatalogueUI.instance.updateNeeded = true;
				RecipeCatalogueUI.instance.updateNeeded = true;
			};
		}
	}

	internal class CycleFilter : Filter
	{
		int index = 0; // different images? different backgrounds?
		List<Filter> filters;
		List<UISilentImageButton> buttons = new List<UISilentImageButton>();

		public CycleFilter(string name, string textureFileName, List<Filter> filters) :
			this(name, RecipeBrowser.instance.Assets.Request<Texture2D>(textureFileName, AssetRequestMode.ImmediateLoad), filters) {
		}

		public CycleFilter(string name, Asset<Texture2D> texture, List<Filter> filters) : base(name, (item) => false, texture) {
			this.filters = filters;
			this.belongs = (item) => {
				return index == 0 ? true : filters[index - 1].belongs(item);
			};
			//CycleFilter needs SharedUI.instance.updateNeeded to update image, since each filter acts independently.

			var firstButton = new UISilentImageButton(texture, name);
			firstButton.OnLeftClick += (a, b) => ButtonBehavior(true);
			firstButton.OnRightClick += (a, b) => ButtonBehavior(false);

			buttons.Add(firstButton);

			for (int i = 0; i < filters.Count; i++) {
				var buttonOption = new UISilentImageButton(filters[i].texture, filters[i].name);
				buttonOption.OnLeftClick += (a, b) => ButtonBehavior(true);
				buttonOption.OnRightClick += (a, b) => ButtonBehavior(false);
				buttonOption.OnMiddleClick += (a, b) => ButtonBehavior(false, true);
				buttons.Add(buttonOption);
			}

			button = buttons[0];

			void ButtonBehavior(bool increment, bool zero = false) {
				button.selected = false;

				index = zero ? 0 : (increment ? (index + 1) % buttons.Count : (buttons.Count + index - 1) % buttons.Count);
				button = buttons[index];
				if (index != 0)
					button.selected = true;
				ItemCatalogueUI.instance.updateNeeded = true;
				RecipeCatalogueUI.instance.updateNeeded = true;
				SharedUI.instance.updateNeeded = true;
			}
		}
	}

	internal class Sort
	{
		internal Func<Item, Item, int> sort;
		internal Func<Recipe, Recipe, int> recipeSort;
		internal Func<bool> sortAvailable;
		internal UISilentImageButton button;

		public Sort(string hoverText, Asset<Texture2D> texture, Func<Item, Item, int> sort) {
			this.sort = sort;
			button = new UISilentImageButton(texture, hoverText);
			button.OnLeftClick += (a, b) => {
				SharedUI.instance.SelectedSort = this;
			};
		}

		public Sort(string hoverText, string textureFileName, Func<Item, Item, int> sort) :
			this(hoverText, RecipeBrowser.instance.Assets.Request<Texture2D>(textureFileName, AssetRequestMode.ImmediateLoad), sort) {
		}
	}

	// Represents a requested Category or Filter.
	internal class ModCategory
	{
		internal string name;
		internal string parent;
		internal Asset<Texture2D> icon;
		internal Predicate<Item> belongs;
		public ModCategory(string name, string parent, Asset<Texture2D> icon, Predicate<Item> belongs) {
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
		internal List<Filter> filters;
		internal UISilentImageButton button;
		internal Category parent;
		// Pass in other Category to exclude?

		public Category(string name, Predicate<Item> belongs, Asset<Texture2D> texture = null) {
			if (texture == null)
				texture = RecipeBrowser.instance.Assets.Request<Texture2D>("Images/sortAmmo", AssetRequestMode.ImmediateLoad);
			this.name = name;
			subCategories = new List<Category>();
			sorts = new List<Sort>();
			filters = new List<Filter>();
			this.belongs = belongs;

			texture.Wait();
			this.button = new UISilentImageButton(texture, name);
			button.OnLeftClick += (a, b) => {
				//Main.NewText("clicked on " + button.hoverText);
				SharedUI.instance.SelectedCategory = this;
			};
		}

		public Category(string name, Predicate<Item> belongs, string textureFileName) :
			this(name, belongs, RecipeBrowser.instance.Assets.Request<Texture2D>(textureFileName, AssetRequestMode.ImmediateLoad)) {
		}

		internal bool BelongsRecursive(Item item) {
			if (belongs(item))
				return true;
			return subCategories.Any(x => x.belongs(item));
		}

		internal void ParentAddToSorts(List<Sort> availableSorts) {
			if (parent != null)
				parent.ParentAddToSorts(availableSorts);
			availableSorts.AddRange(sorts);
		}

		internal void ParentAddToFilters(List<Filter> availableFilters) {
			if (parent != null)
				parent.ParentAddToFilters(availableFilters);
			availableFilters.AddRange(filters);
		}
	}
}
