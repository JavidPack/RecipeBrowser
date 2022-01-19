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
using static RecipeBrowser.Utilities;

namespace RecipeBrowser
{
	class SharedUI
	{
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
			availableFilters = new List<Filter>(filters);
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
				categoriesGrid.OnScrollWheel += RecipeBrowserUI.OnScrollWheel_FixHotbarScroll;
				sortsAndFiltersPanel.Append(categoriesGrid);
				categoriesGrid.drawArrows = true;

				categoriesGridScrollbar = new InvisibleFixedUIHorizontalScrollbar(RecipeBrowserUI.instance.userInterface);
				categoriesGridScrollbar.SetView(100f, 1000f);
				categoriesGridScrollbar.Width.Set(0, 1f);
				categoriesGridScrollbar.Top.Set(0, 0f);
				sortsAndFiltersPanel.Append(categoriesGridScrollbar);
				categoriesGrid.SetScrollbar(categoriesGridScrollbar);
			}

			subCategorySortsFiltersGrid = new UIHorizontalGrid();
			subCategorySortsFiltersGrid.Width.Set(0, 1f);
			subCategorySortsFiltersGrid.Top.Set(26, 0f);
			subCategorySortsFiltersGrid.Height.Set(26, 0f);
			subCategorySortsFiltersGrid.ListPadding = 2f;
			subCategorySortsFiltersGrid.OnScrollWheel += RecipeBrowserUI.OnScrollWheel_FixHotbarScroll;
			sortsAndFiltersPanel.Append(subCategorySortsFiltersGrid);
			subCategorySortsFiltersGrid.drawArrows = true;

			float oldRow2ViewPosition = lootGridScrollbar2?.ViewPosition ?? 0f;
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
				var image = new UIImage(RecipeBrowser.instance.GetTexture("Images/spacer"));
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
				var image = new UIImage(RecipeBrowser.instance.GetTexture("Images/spacer"));
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
		internal List<Sort> sorts;
		private void SetupSortsAndCategories() {
			//Texture2D terrariaSort = ResizeImage(Main.inventorySortTexture[1], 24, 24);
			Texture2D rarity = ResizeImage(Main.itemTexture[ItemID.MetalDetector], 24, 24);

			// TODO: Implement Badge text as used in Item Checklist.
			sorts = new List<Sort>()
			{
				new Sort("ItemID", "Images/sortItemID", (x,y)=>x.type.CompareTo(y.type)),
				new Sort("Value", "Images/sortValue", (x,y)=>x.value.CompareTo(y.value)),
				new Sort("Alphabetical", "Images/sortAZ", (x,y)=>x.Name.CompareTo(y.Name)),
				new Sort("Rarity", rarity, (x,y)=> x.rare==y.rare ? x.value.CompareTo(y.value) : Math.Abs(x.rare).CompareTo(Math.Abs(y.rare))),
				//new Sort("Terraria Sort", terrariaSort, (x,y)=> -ItemChecklistUI.vanillaIDsInSortOrder[x.type].CompareTo(ItemChecklistUI.vanillaIDsInSortOrder[y.type]), x=>ItemChecklistUI.vanillaIDsInSortOrder[x.type].ToString()),
			};

			Texture2D materialsIcon = Utilities.StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.SpellTome] }, 24, 24);
			Texture2D craftableIcon = ResizeImage(Main.itemTexture[ItemID.IronAnvil], 24, 24);
			Texture2D extendedCraftIcon = ResizeImage(Main.itemTexture[ItemID.MythrilAnvil], 24, 24);
			filters = new List<Filter>()
			{
				new Filter("Materials", x=>x.material, materialsIcon),
				(CraftableFilter = new Filter("Craftable", x=>true, craftableIcon)),
				(ObtainableFilter = new Filter("Extended Craftable (RMB on Recipe to view, Auto-disables to prevent lag)", x=>true, extendedCraftIcon)),
			};

			// TODOS: Vanity armor, grapple, cart, potions buffs
			// 24x24 pixels

			var yoyos = new List<int>();
			for (int i = 0; i < ItemID.Sets.Yoyo.Length; ++i) {
				if (ItemID.Sets.Yoyo[i]) {
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

			var ammoFilters = sortedAmmoTypes.Select(ammoType => new Filter(Lang.GetItemNameValue(ammoType), x => x.ammo == ammoType, ResizeImage(Main.itemTexture[ammoType], 24, 24))).ToList();
			var useAmmoFilters = sortedUseAmmoTypes.Select(ammoType => new Filter(Lang.GetItemNameValue(ammoType), x => x.useAmmo == ammoType, ResizeImage(Main.itemTexture[ammoType], 24, 24))).ToList();

			var ammoFilter = new CycleFilter("Cycle Ammo Types", RecipeBrowser.instance.GetTexture("Images/sortAmmo"), ammoFilters);
			var useAmmoFilter = new CycleFilter("Cycle Used Ammo Types", RecipeBrowser.instance.GetTexture("Images/sortAmmo"), useAmmoFilters);

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
			Texture2D smallVanity = ResizeImage(Main.itemTexture[ItemID.BunnyHood], 24, 24);
			//Texture2D smallVanity2 = ResizeImage(Main.itemTexture[ItemID.HerosHat], 24, 24);
			Texture2D smallNonVanity = ResizeImage(Main.itemTexture[ItemID.GoldHelmet], 24, 24);
			Texture2D smallTiles = ResizeImage(Main.itemTexture[ItemID.Sign], 24, 24);
			Texture2D smallCraftingStation = ResizeImage(Main.itemTexture[ItemID.IronAnvil], 24, 24);
			Texture2D smallWalls = ResizeImage(Main.itemTexture[ItemID.PearlstoneBrickWall], 24, 24);
			Texture2D smallExpert = ResizeImage(Main.itemTexture[ItemID.EoCShield], 24, 24);
			Texture2D smallPets = ResizeImage(Main.itemTexture[ItemID.ZephyrFish], 24, 24);
			Texture2D smallLightPets = ResizeImage(Main.itemTexture[ItemID.FairyBell], 24, 24);
			Texture2D smallBossSummon = ResizeImage(Main.itemTexture[ItemID.MechanicalSkull], 24, 24);
			Texture2D smallMounts = ResizeImage(Main.itemTexture[ItemID.SlimySaddle], 24, 24);
			Texture2D smallHooks = ResizeImage(Main.itemTexture[ItemID.AmethystHook], 24, 24);
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
			//Texture2D smallVanityFilterGroup = StackResizeImage2424(Main.itemTexture[ItemID.BunnyHood], Main.itemTexture[ItemID.GoldHelmet]);
			Texture2D smallPetsLightPets = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.ZephyrFish], Main.itemTexture[ItemID.FairyBell] }, 24, 24);
			Texture2D smallPlaceables = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.Sign], Main.itemTexture[ItemID.PearlstoneBrickWall] }, 24, 24);
			Texture2D smallWeapons = StackResizeImage(new Texture2D[] { smallMelee, smallMagic, smallThrown }, 24, 24);
			Texture2D smallTools = StackResizeImage(new Texture2D[] { RecipeBrowser.instance.GetTexture("Images/sortPick"), RecipeBrowser.instance.GetTexture("Images/sortAxe"), RecipeBrowser.instance.GetTexture("Images/sortHammer") }, 24, 24);
			Texture2D smallFishing = StackResizeImage(new Texture2D[] { RecipeBrowser.instance.GetTexture("Images/sortFish"), RecipeBrowser.instance.GetTexture("Images/sortBait"), Main.itemTexture[ItemID.FallenStarfish] }, 24, 24);
			Texture2D smallPotions = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.HealingPotion], Main.itemTexture[ItemID.ManaPotion], Main.itemTexture[ItemID.RagePotion] }, 24, 24);
			Texture2D smallBothDyes = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.OrangeDye], Main.itemTexture[ItemID.BiomeHairDye] }, 24, 24);
			Texture2D smallSortTiles = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.Candelabra], Main.itemTexture[ItemID.GrandfatherClock] }, 24, 24);

			Texture2D StackResizeImage2424(params Texture2D[] textures) => StackResizeImage(textures, 24, 24);
			Texture2D ResizeImage2424(Texture2D texture) => ResizeImage(texture, 24, 24);

			// Potions, other?
			// should inherit children?
			// should have other category?
			if (WorldGen.statueList == null)
				WorldGen.SetupStatueList();

			var vanity = new MutuallyExclusiveFilter("Vanity", x => x.vanity, smallVanity);
			var armor = new MutuallyExclusiveFilter("Armor", x => !x.vanity, smallNonVanity);
			vanity.SetExclusions(new List<Filter>() { vanity, armor });
			armor.SetExclusions(new List<Filter>() { vanity, armor });

			categories = new List<Category>() {
				new Category("All", x=> true, smallAll),
				// TODO: Filter out tools from weapons. Separate belongs and doesn't belong predicates? How does inheriting work again? Other?
				new Category("Weapons"/*, x=>x.damage>0*/, x=> false, smallWeapons) { //"Images/sortDamage"
					subCategories = new List<Category>() {
						new Category("Melee", x=>x.melee, smallMelee),
						new Category("Yoyo", x=>ItemID.Sets.Yoyo[x.type], smallYoyo),
						new Category("Magic", x=>x.magic, smallMagic),
						new Category("Ranged", x=>x.ranged && x.ammo == 0, smallRanged) // TODO and ammo no
						{
							sorts = new List<Sort>() { new Sort("Use Ammo Type", "Images/sortAmmo", (x,y)=>x.useAmmo.CompareTo(y.useAmmo)), },
							filters = new List<Filter> { useAmmoFilter }
						},
						new Category("Throwing", x=>x.thrown, smallThrown),
						new Category("Summon", x=>x.summon && !x.sentry, smallSummon),
						new Category("Sentry", x=>x.summon && x.sentry, smallSentry),
					},
					sorts = new List<Sort>() { new Sort("Damage", "Images/sortDamage", (x,y)=>x.damage.CompareTo(y.damage)), },
				},
				new Category("Tools"/*,x=>x.pick>0||x.axe>0||x.hammer>0*/, x=>false, smallTools) {
					subCategories = new List<Category>() {
						new Category("Pickaxes", x=>x.pick>0, "Images/sortPick") { sorts = new List<Sort>() { new Sort("Pick Power", "Images/sortPick", (x,y)=>x.pick.CompareTo(y.pick)), } },
						new Category("Axes", x=>x.axe>0, "Images/sortAxe"){ sorts = new List<Sort>() { new Sort("Axe Power", "Images/sortAxe", (x,y)=>x.axe.CompareTo(y.axe)), } },
						new Category("Hammers", x=>x.hammer>0, "Images/sortHammer"){ sorts = new List<Sort>() { new Sort("Hammer Power", "Images/sortHammer", (x,y)=>x.hammer.CompareTo(y.hammer)), } },
					},
				},
				new Category(ArmorSetFeatureHelper.ArmorSetsHoverTest, x => true, "Images/categoryArmorSets") {
					sorts = new List<Sort>() { new Sort("Total Defense", "Images/categoryArmorSets", (x,y)=>x.defense.CompareTo(y.defense)), }, // See ItemCatalogueUI.ItemGridSort for actual implementation
				},
				new Category("Armor"/*,  x=>x.headSlot!=-1||x.bodySlot!=-1||x.legSlot!=-1*/, x => false, smallArmor) {
					subCategories = new List<Category>() {
						new Category("Head", x=>x.headSlot!=-1, smallHead),
						new Category("Body", x=>x.bodySlot!=-1, smallBody),
						new Category("Legs", x=>x.legSlot!=-1, smallLegs),
					},
					sorts = new List<Sort>() { new Sort("Defense", "Images/sortDefense", (x,y)=>x.defense.CompareTo(y.defense)), },
					filters = new List<Filter> {
						//new Filter("Vanity", x=>x.vanity, RecipeBrowser.instance.GetTexture("Images/sortDefense")),
						// Prefer MutuallyExclusiveFilter for this, rather than CycleFilter since there are only 2 options.
						//new CycleFilter("Vanity/Armor", smallVanityFilterGroup, new List<Filter> {
						//	new Filter("Vanity", x=>x.vanity, smallVanity),
						//	new Filter("Armor", x=>!x.vanity, smallNonVanity),
						//}),
						vanity, armor,
						//new DoubleFilter("Vanity", "Armor", smallVanity2, x=>x.vanity),
					}
				},
				new Category("Tiles", x=>x.createTile!=-1, smallTiles)
				{
					subCategories = new List<Category>()
					{
						new Category("Crafting Stations", x=>RecipeCatalogueUI.instance.craftingTiles.Contains(x.createTile), smallCraftingStation),
						new Category("Containers", x=>x.createTile!=-1 && Main.tileContainer[x.createTile], smallContainer),
						new Category("Wiring", x=>ItemID.Sets.SortingPriorityWiring[x.type] > -1, smallWiring),
						new Category("Statues", x=>WorldGen.statueList.Any(point => point.X == x.createTile && point.Y == x.placeStyle), smallStatue),
						new Category("Doors", x=> x.createTile > 0 && TileID.Sets.RoomNeeds.CountsAsDoor.Contains(x.createTile), ResizeImage2424(Main.itemTexture[ItemID.WoodenDoor])),
						new Category("Chairs", x=> x.createTile > 0 && TileID.Sets.RoomNeeds.CountsAsChair.Contains(x.createTile), ResizeImage2424(Main.itemTexture[ItemID.WoodenChair])),
						new Category("Tables", x=> x.createTile > 0 && TileID.Sets.RoomNeeds.CountsAsTable.Contains(x.createTile), ResizeImage2424(Main.itemTexture[ItemID.PalmWoodTable])),
						new Category("Light Sources", x=> x.createTile > 0 && TileID.Sets.RoomNeeds.CountsAsTorch.Contains(x.createTile), ResizeImage2424(Main.itemTexture[ItemID.ChineseLantern])),
						new Category("Torches", x=> x.createTile > 0 && TileLoader.IsTorch(x.createTile), ResizeImage2424(Main.itemTexture[ItemID.RainbowTorch])),
						// Banners => Banner Bonanza mod integration
						//Main.itemTexture[Main.rand.Next(TileID.Sets.RoomNeeds.CountsAsTable)] doesn't work since those are tilesids. yoyo approach?
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
					sorts = new List<Sort>() {
						new Sort("Ammo Type", "Images/sortAmmo", (x,y)=>x.ammo.CompareTo(y.ammo)),
						new Sort("Damage", "Images/sortDamage", (x,y)=>x.damage.CompareTo(y.damage)),
					},
					filters = new List<Filter> { ammoFilter }
					// TODO: Filters/Subcategories for all ammo types? // each click cycles?
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
				new Category("Hooks", x=> Main.projHook[x.shoot], smallHooks){
					sorts = new List<Sort>() {
						new Sort("Grapple Range", smallHooks, (x,y)=> GrappleRange(x.shoot).CompareTo(GrappleRange(y.shoot))),
					},
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
				new Category("Consumables", x=> !(x.createWall > 0 || x.createTile > -1) && !(x.ammo > 0 && !x.notAmmo) && x.consumable, smallConsumables){
					subCategories = new List<Category>() {
						new Category("Captured NPC", x=>x.makeNPC != 0, ResizeImage2424(Main.itemTexture[ItemID.GoldBunny])),
					}
				},
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

			foreach (var modCategory in RecipeBrowser.instance.modCategories) {
				if (string.IsNullOrEmpty(modCategory.parent)) {
					categories.Insert(categories.Count - 2, new Category(modCategory.name, modCategory.belongs, modCategory.icon));
				}
				else {
					foreach (var item in categories) {
						if (item.name == modCategory.parent) {
							item.subCategories.Add(new Category(modCategory.name, modCategory.belongs, modCategory.icon));
						}
					}
				}
			}

			foreach (var modCategory in RecipeBrowser.instance.modFilters) {
				filters.Add(new Filter(modCategory.name, modCategory.belongs, modCategory.icon));
			}

			foreach (var parent in categories) {
				foreach (var child in parent.subCategories) {
					child.parent = parent; // 3 levels?
				}
			}
			SelectedSort = sorts[0];
			SelectedCategory = categories[0];
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

		private bool BelongsInOther(Item item) {
			var cats = categories.Skip(1).Take(categories.Count - 2);
			foreach (var category in cats) {
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
		internal Texture2D texture;
		//internal Category parent;

		public Filter(string name, Predicate<Item> belongs, Texture2D texture) {
			this.name = name;
			this.texture = texture;
			subCategories = new List<Category>();
			sorts = new List<Sort>();
			this.belongs = belongs;

			this.button = new UISilentImageButton(texture, name);
			button.OnClick += (a, b) => {
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

		public MutuallyExclusiveFilter(string name, Predicate<Item> belongs, Texture2D texture) : base(name, belongs, texture) {
			button.OnClick += (a, b) => {
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
		public DoubleFilter(string name, string other, Texture2D texture, Predicate<Item> belongs) : base(name, belongs, texture) {
			this.other = other;
			this.belongs = (item) => {
				return belongs(item) ^ right;
			};
			button = new UIBadgedSilentImageButton(texture, name + " (RMB)");
			button.OnClick += (a, b) => {
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

		public CycleFilter(string name, Texture2D texture, List<Filter> filters) : base(name, (item) => false, texture) {
			this.filters = filters;
			this.belongs = (item) => {
				return index == 0 ? true : filters[index - 1].belongs(item);
			};
			//CycleFilter needs SharedUI.instance.updateNeeded to update image, since each filter acts independently.

			var firstButton = new UISilentImageButton(texture, name);
			firstButton.OnClick += (a, b) => ButtonBehavior(true);
			firstButton.OnRightClick += (a, b) => ButtonBehavior(false);

			buttons.Add(firstButton);

			for (int i = 0; i < filters.Count; i++) {
				var buttonOption = new UISilentImageButton(filters[i].texture, filters[i].name);
				buttonOption.OnClick += (a, b) => ButtonBehavior(true);
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
		internal UISilentImageButton button;

		public Sort(string hoverText, Texture2D texture, Func<Item, Item, int> sort) {
			this.sort = sort;
			button = new UISilentImageButton(texture, hoverText);
			button.OnClick += (a, b) => {
				SharedUI.instance.SelectedSort = this;
			};
		}

		public Sort(string hoverText, string textureFileName, Func<Item, Item, int> sort) : this(hoverText, RecipeBrowser.instance.GetTexture(textureFileName), sort) {
		}
	}

	// Represents a requested Category or Filter.
	internal class ModCategory
	{
		internal string name;
		internal string parent;
		internal Texture2D icon;
		internal Predicate<Item> belongs;
		public ModCategory(string name, string parent, Texture2D icon, Predicate<Item> belongs) {
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

		public Category(string name, Predicate<Item> belongs, Texture2D texture = null) {
			if (texture == null)
				texture = RecipeBrowser.instance.GetTexture("Images/sortAmmo");
			this.name = name;
			subCategories = new List<Category>();
			sorts = new List<Sort>();
			filters = new List<Filter>();
			this.belongs = belongs;

			this.button = new UISilentImageButton(texture, name);
			button.OnClick += (a, b) => {
				//Main.NewText("clicked on " + button.hoverText);
				SharedUI.instance.SelectedCategory = this;
			};
		}

		public Category(string name, Predicate<Item> belongs, string textureFileName) : this(name, belongs, RecipeBrowser.instance.GetTexture(textureFileName)) {
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
