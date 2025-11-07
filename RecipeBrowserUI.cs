using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RecipeBrowser.UIElements;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

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

	internal class RecipeBrowserUI : UIModState
	{
		internal static RecipeBrowserUI instance;

		internal const int RecipeCatalogue = 0;
		internal const int Craft = 1;
		internal const int ItemCatalogue = 2;
		internal const int Bestiary = 3;
		internal const int Help = 4;

		internal TabController tabController;
		internal UIDragableElement mainPanel;
		internal UIDragablePanel favoritePanel;
		internal UIElements.UICycleImage HideUnlessInventoryToggle;
		internal UIElements.UICycleImage ShowOtherPlayersFavoritesToggle;
		internal UIHoverImageButton closeFavoritePanelButton;
		internal UIHoverImageButton closeButton;
		internal UIHoverImageButtonMod modFilterButton;
		private BlockInputElement blockInput;
		private UIElement activeDialog;

		//internal SharedUI sharedUI;
		internal RecipeCatalogueUI recipeCatalogueUI;
		internal CraftUI craftUI;
		internal ItemCatalogueUI itemCatalogueUI;
		internal BestiaryUI bestiaryUI;
		internal HelpUI helpUI;

		internal List<int> localPlayerFavoritedRecipes => Main.LocalPlayer.GetModPlayer<RecipeBrowserPlayer>().favoritedRecipes;
		internal bool[] foundItems;

		internal string[] mods;
		internal ModFilterDropdown ModFilterDropdown;

		public bool ForceShowFavoritePanel;
		public bool ForceHideFavoritePanel; // Could save to config on exit world to preserve, but if users want that they should just not favorite recipes.

		private bool showFavoritePanel;
		public bool ShowFavoritePanel
		{
			get { return showFavoritePanel; }
			set
			{
				if (value)
				{
					Append(favoritePanel);
				}
				else
				{
					RemoveChild(favoritePanel);
				}
				if (value)
					ForceHideFavoritePanel = false;
				if (!value)
					ForceShowFavoritePanel = false;

				showFavoritePanel = value;
			}
		}

		private bool showRecipeBrowser;

		public bool ShowRecipeBrowser
		{
			get { return showRecipeBrowser; }
			set
			{
				if (value)
				{
					Recipe.FindRecipes();
					Append(mainPanel);
				}
				else
				{
					RemoveChild(mainPanel);
				}
				showRecipeBrowser = value;
			}
		}

		public int CurrentPanel => tabController.currentPanel;

		internal static string RBText(string key, string category = "RecipeBrowserUI") => RecipeBrowser.RBText(category, key);

		public RecipeBrowserUI(UserInterface ui) : base(ui)
		{
			instance = this;
			mods = new string[] { "ModLoader" };
		}

		// Technically since RecipeBrowserUI ctor happens in Mod.Load, we could miss mods that add during Load that happen after me.
		public void PostSetupContent()
		{
			mods = ModLoader.Mods.Where(mod => mod.GetContent<ModItem>().Any()).Select(mod => mod.Name).ToArray();
			modIndex = 0;
		}

		public override void OnInitialize()
		{
			mainPanel = new UIDragableElement(true, true, true);
			//mainPanel.SetPadding(0);
			//mainPanel.PaddingTop = 4;
			mainPanel.Left.Set(400f, 0f);
			mainPanel.Top.Set(400f, 0f);
			mainPanel.Width.Set(475f, 0f); // + 30
			mainPanel.MinWidth.Set(415f, 0f);
			mainPanel.MaxWidth.Set(884f, 0f);
			mainPanel.Height.Set(350, 0f);
			mainPanel.MinHeight.Set(263, 0f);
			mainPanel.MaxHeight.Set(1000, 0f);
			//mainPanel.BackgroundColor = Color.LightBlue;

			var config = ModContent.GetInstance<RecipeBrowserClientConfig>();
			mainPanel.Left.Set(config.RecipeBrowserPosition.X, 0f);
			mainPanel.Top.Set(config.RecipeBrowserPosition.Y, 0f);
			mainPanel.Width.Set(config.RecipeBrowserSize.X, 0f);
			mainPanel.Height.Set(config.RecipeBrowserSize.Y, 0f);

			new SharedUI();
			recipeCatalogueUI = new RecipeCatalogueUI();
			craftUI = new CraftUI();
			itemCatalogueUI = new ItemCatalogueUI();
			bestiaryUI = new BestiaryUI();
			helpUI = new HelpUI();

			SharedUI.instance.Initialize();

			var recipePanel = recipeCatalogueUI.CreateRecipeCataloguePanel();
			mainPanel.Append(recipePanel);

			var craftPanel = craftUI.CreateCraftPanel();
			mainPanel.Append(craftPanel);

			var cataloguePanel = itemCatalogueUI.CreateItemCataloguePanel();
			mainPanel.Append(cataloguePanel);

			var bestiaryPanel = bestiaryUI.CreateBestiaryPanel();
			mainPanel.Append(bestiaryPanel);

			var helpPanel = helpUI.CreateHelpPanel();
			//mainPanel.Append(helpPanel); // does this do anything?

			tabController = new TabController(mainPanel);
			tabController.AddPanel(recipePanel);
			tabController.AddPanel(craftPanel);
			tabController.AddPanel(cataloguePanel);
			tabController.AddPanel(bestiaryPanel);
			tabController.AddPanel(helpPanel);

			mainPanel.AddDragTarget(recipePanel);
			mainPanel.AddDragTarget(recipeCatalogueUI.recipeInfo);
			mainPanel.AddDragTarget(recipeCatalogueUI.RadioButtonGroup);
			mainPanel.AddDragTarget(craftPanel);
			craftUI.additionalDragTargets.ForEach(x => mainPanel.AddDragTarget(x));
			mainPanel.AddDragTarget(cataloguePanel);
			itemCatalogueUI.additionalDragTargets.ForEach(x => mainPanel.AddDragTarget(x));
			mainPanel.AddDragTarget(bestiaryPanel);
			mainPanel.AddDragTarget(helpPanel);
			mainPanel.AddDragTarget(helpUI.message);

			UIPanel button = new UIBottomlessPanel();
			button.SetPadding(0);
			button.Left.Set(10, 0);
			button.Width.Set(80, 0);
			button.Height.Set(22, 0);
			button.OnLeftClick += (a, b) => tabController.SetPanel(RecipeCatalogue);
			button.BackgroundColor = RecipeCatalogueUI.color;

			UIText text = new UIText(RBText("Recipes"), 0.85f);
			text.HAlign = 0.5f;
			text.VAlign = 0.5f;
			button.Append(text);
			mainPanel.Append(button);
			tabController.AddButton(button);

			button = new UIBottomlessPanel();
			button.SetPadding(0);
			button.Left.Set(85, 0);
			button.Width.Set(80, 0);
			button.Height.Set(22, 0);
			button.OnLeftClick += (a, b) => { tabController.SetPanel(Craft); };
			button.BackgroundColor = CraftUI.color;

			text = new UIText(RBText("Craft"), 0.85f);
			text.HAlign = 0.5f;
			text.VAlign = 0.5f;
			button.Append(text);
			mainPanel.Append(button);
			tabController.AddButton(button);

			button = new UIBottomlessPanel();
			button.SetPadding(0);
			button.Left.Set(160, 0);
			button.Width.Set(80, 0);
			button.Height.Set(22, 0);
			button.OnLeftClick += (a, b) => { tabController.SetPanel(ItemCatalogue); itemCatalogueUI.updateNeeded = true; };
			button.BackgroundColor = ItemCatalogueUI.color;

			text = new UIText(RBText("Items"), 0.85f);
			text.HAlign = 0.5f;
			text.VAlign = 0.5f;
			button.Append(text);
			mainPanel.Append(button);
			tabController.AddButton(button);

			button = new UIBottomlessPanel();
			button.SetPadding(0);
			button.Left.Set(235, 0);
			button.Width.Set(80, 0);
			button.Height.Set(22, 0);
			button.OnLeftClick += (a, b) => tabController.SetPanel(Bestiary);
			button.BackgroundColor = BestiaryUI.color;

			text = new UIText(RBText("Bestiary"), 0.85f);
			text.HAlign = 0.5f;
			text.VAlign = 0.5f;
			button.Append(text);
			mainPanel.Append(button);
			tabController.AddButton(button);

			button = new UIBottomlessPanel();
			button.SetPadding(0);
			button.Left.Set(-155, 1);
			button.Width.Set(80, 0);
			button.Height.Set(22, 0);
			button.OnLeftClick += (a, b) => tabController.SetPanel(Help);
			button.BackgroundColor = HelpUI.color;

			text = new UIText(RBText("Help"), 0.85f);
			text.HAlign = 0.5f;
			text.VAlign = 0.5f;
			button.Append(text);
			mainPanel.Append(button);
			tabController.AddButton(button);

			// TODO: Help panel with expandable help topics.

			button = new UIBottomlessPanel();
			button.SetPadding(0);
			button.Left.Set(-80, 1);
			button.Width.Set(70, 0);
			button.Height.Set(22, 0);
			button.BackgroundColor = Color.DarkRed;

			Asset<Texture2D> filterModTexture = RecipeBrowser.instance.Assets.Request<Texture2D>(RBText("FilterMod", "ImagePaths"), AssetRequestMode.ImmediateLoad);
			Asset<Texture2D> filterModColorableTexture = RecipeBrowser.instance.Assets.Request<Texture2D>(RBText("FilterModColorable", "ImagePaths"), AssetRequestMode.ImmediateLoad);
			modFilterButton = new UIHoverImageButtonMod(filterModTexture, filterModColorableTexture, RBText("ModFilter") + ": " + RBText("All"));
			modFilterButton.Left.Set(-60, 1f);
			modFilterButton.Top.Set(-0, 0f);
			modFilterButton.OnLeftClick += ModFilterButton_OnClick;
			modFilterButton.OnRightClick += ModFilterButton_OnRightClick;
			modFilterButton.OnMiddleClick += ModFilterButton_OnMiddleClick;
			button.Append(modFilterButton);

			Asset<Texture2D> closeButtonTexture = RecipeBrowser.instance.Assets.Request<Texture2D>("UIElements/closeButton", AssetRequestMode.ImmediateLoad);
			closeButton = new UIHoverImageButton(closeButtonTexture, RBText("Close"));
			closeButton.OnLeftClick += CloseButtonClicked;
			closeButton.Left.Set(-26, 1f);
			closeButton.VAlign = 0.5f;
			button.Append(closeButton);
			mainPanel.Append(button);

			tabController.SetPanel(0);

			//favoritedRecipes = new List<int>();
			favoritePanel = new UIDragablePanel();
			favoritePanel.SetPadding(6);
			favoritePanel.Left.Set(-310f, 0f);
			favoritePanel.HAlign = 1f;
			favoritePanel.Top.Set(90f, 0f);
			favoritePanel.Width.Set(415f, 0f);
			favoritePanel.MinWidth.Set(50f, 0f);
			favoritePanel.MaxWidth.Set(600f, 0f);
			favoritePanel.Height.Set(350, 0f);
			favoritePanel.MinHeight.Set(50, 0f);
			favoritePanel.MaxHeight.Set(300, 0f);
			//favoritePanel.BackgroundColor = new Color(73, 94, 171);
			favoritePanel.BackgroundColor = Color.Transparent;
			//Append(favoritePanel);

			favoritePanel.Left.Set(config.FavoritedRecipePanelPosition.X, 0f);
			favoritePanel.Top.Set(config.FavoritedRecipePanelPosition.Y, 0f);

			string closeFavoritePanelButtonHoverText = string.Format(RBText("Close", "FavoritedUI"), RBText("ToggleUnboundHint", "FavoritedUI"));
			closeFavoritePanelButton = new UIHoverImageButton(closeButtonTexture, closeFavoritePanelButtonHoverText);
			closeFavoritePanelButton.OnLeftClick += CloseFavoritePanelButtonClicked;
			closeFavoritePanelButton.Top.Set(0, 0f);
			closeFavoritePanelButton.Left.Set(-15, 1f);
			favoritePanel.Append(closeFavoritePanelButton);

			HideUnlessInventoryToggle = new UIElements.UICycleImage(RecipeBrowser.instance.Assets.Request<Texture2D>("UIElements/TickOnOff", AssetRequestMode.ImmediateLoad), 2, new string[] { RBText("AlwaysShow", "FavoritedUI"), RBText("ShowWhenInventory", "FavoritedUI") }, 16, 12);
			HideUnlessInventoryToggle.Top.Set(20, 0f);
			HideUnlessInventoryToggle.Left.Set(-15, 1f);
			HideUnlessInventoryToggle.CurrentState = config.OnlyShowFavoritedWhileInInventory ? 1 : 0;
			HideUnlessInventoryToggle.OnStateChanged += (s, e) => {
				favoritePanelUpdateNeeded = true;
				config.OnlyShowFavoritedWhileInInventory = HideUnlessInventoryToggle.CurrentState == 1;
				RecipeBrowserClientConfig.SaveConfig();
			};
			favoritePanel.Append(HideUnlessInventoryToggle);

			ShowOtherPlayersFavoritesToggle = new UICycleImage(RecipeBrowser.instance.Assets.Request<Texture2D>("UIElements/ShowOtherPlayersRecipesToggle", AssetRequestMode.ImmediateLoad), 3, [RBText("ShowOtherPlayersFavorited", "FavoritedUI"), RBText("ShowTeammatesFavorited", "FavoritedUI"), RBText("HideOtherPlayersFavorited", "FavoritedUI")], 16, 12);
			ShowOtherPlayersFavoritesToggle.Top.Set(40, 0f);
			ShowOtherPlayersFavoritesToggle.Left.Set(-15, 1f);
			ShowOtherPlayersFavoritesToggle.CurrentState = (int)config.ShowOtherPlayersFavoritedRecipes;
			ShowOtherPlayersFavoritesToggle.OnStateChanged += (s, e) => {
				favoritePanelUpdateNeeded = true;
				config.ShowOtherPlayersFavoritedRecipes = (ShowOtherPlayersFavoritedRecipesOption)ShowOtherPlayersFavoritesToggle.CurrentState;
				RecipeBrowserClientConfig.SaveConfig();
			};
			favoritePanel.Append(ShowOtherPlayersFavoritesToggle);
		}

		//private void ItemChecklistRadioButton_OnRightClick(UIMouseEvent evt, UIElement listeningElement)
		//{
		//	// Switch modes.
		//	ItemChecklistRadioButton.SetHoverText("Mode: Only All Results");
		//}

		// Vanilla ModLoader mod will act as "all"
		internal static int modIndex; // Selected mod
		internal static int modIndexPrevious; // Last icon calculated
		internal static int modHoverIndex = -1; // Currently hovering option

		private void ModFilterButton_OnClick(UIMouseEvent evt, UIElement listeningElement)
		{
			if (mods.Length < 4)
			{
				ChangeModIndex(true);
				UpdateModFilterUI();
				return;
			}

			var host = listeningElement.Parent?.Parent;
			if (host == null)
			{
				return;
			}
			
			if (ModFilterDropdown == null)
			{
				ModFilterDropdown = new ModFilterDropdown(
					mods,
					modIndex,
					GetDisplayName
				);

				ModFilterDropdown.SelectedIndexChanged += (_, selectedIndex) =>
				{
					modIndex = selectedIndex;
					UpdateModFilterUI();
					UnblockInput(evt, listeningElement);
				};
			}

			if(ModFilterDropdown.Parent == null)
				BlockInput(ModFilterDropdown);
			else
				UnblockInput(evt, listeningElement);
		}

		private void ModFilterButton_OnRightClick(UIMouseEvent evt, UIElement listeningElement)
		{
			ChangeModIndex(false);
			ModFilterDropdown?.SelectIndex(modIndex);
			UpdateModFilterUI();
		}

		private void ModFilterButton_OnMiddleClick(UIMouseEvent evt, UIElement listeningElement)
		{
			modIndex = 0;
			ModFilterDropdown?.SelectIndex(modIndex);
			UpdateModFilterUI();
		}

		private void UpdateModFilterUI()
		{
			modFilterButton.hoverText = RBText("ModFilter") + ": " + GetDisplayName(modIndex);
			UpdateModHoverImage();
			AllUpdateNeeded();
		}

		private void ChangeModIndex(bool increment)
		{
			if (mods.Length <= 1)
			{
				modIndex = 0;
				return;
			}
			
			modIndex = (modIndex + (increment ? 1 : mods.Length - 1)) % mods.Length;
		}

		private string GetDisplayName(int index)
		{
			return index == 0 ? RBText("All") : ModLoader.GetMod(mods[index]).DisplayName;
		}

		internal void UpdateModHoverImage()
		{
			int indexToDisplay = modHoverIndex > -1 ? modHoverIndex : modIndex;
			if (indexToDisplay == modIndexPrevious)
				return;

			modIndexPrevious = indexToDisplay;
			modFilterButton.texture = null;
			Mod otherMod = ModLoader.GetMod(mods[indexToDisplay]);
			if (otherMod == null || !otherMod.FileExists("icon.png"))
			{
				return;
			}
			
			using var ms = new MemoryStream(otherMod.GetFileBytes("icon.png"));
			var modIconTexture = Texture2D.FromStream(Main.instance.GraphicsDevice, ms);
			if (modIconTexture.Width == 80 && modIconTexture.Height == 80)
			{
				modFilterButton.texture = modIconTexture;
			}
		}

		internal void AllUpdateNeeded()
		{
			recipeCatalogueUI.updateNeeded = true;
			itemCatalogueUI.updateNeeded = true;
			bestiaryUI.updateNeeded = true;
		}

		public void NewItemFound(int type)
		{
			recipeCatalogueUI.newestItem = type;
			recipeCatalogueUI.updateNeeded = true;
		}

		internal void CloseButtonClicked(UIMouseEvent evt, UIElement listeningElement)
		{
			RecipeBrowserUI.instance.ShowRecipeBrowser = !RecipeBrowserUI.instance.ShowRecipeBrowser;

			recipeCatalogueUI.CloseButtonClicked();
			bestiaryUI.CloseButtonClicked();
		}
		
		internal void CloseFavoritePanelButtonClicked(UIMouseEvent evt, UIElement listeningElement) {
			RecipeBrowserUI.instance.ForceHideFavoritePanel = true;
			RecipeBrowserUI.instance.ShowFavoritePanel = false;
		}

		internal void FavoriteChange(int index, bool favorite)
		{
			if(Recipe.numRecipes == RecipeCatalogueUI.instance.recipeSlots.Count) // might not be initialized yet first time entering world.
				RecipeCatalogueUI.instance.recipeSlots[index].favorited = favorite;
			localPlayerFavoritedRecipes.RemoveAll(x => x == index);
			if (favorite)
				localPlayerFavoritedRecipes.Add(index);
			favoritePanelUpdateNeeded = true;
			if (favorite) {
				ShowFavoritePanel = true;
			}
			RecipeCatalogueUI.instance.updateNeeded = true;
		}

		internal bool lastMainPlayerInventory = false;
		internal bool favoritePanelUpdateNeeded;
		internal void UpdateFavoritedPanel()
		{
			HideUnlessInventoryToggle.CurrentState = RecipeBrowserClientConfig.Instance.OnlyShowFavoritedWhileInInventory ? 1 : 0;
			ShowOtherPlayersFavoritesToggle.CurrentState = (int)RecipeBrowserClientConfig.Instance.ShowOtherPlayersFavoritedRecipes;

			if (HideUnlessInventoryToggle.CurrentState == 1 && lastMainPlayerInventory != Main.playerInventory && !ForceHideFavoritePanel/* && !ShouldShowFavoritePanel.HasValue*/) {
				ShowFavoritePanel = Main.playerInventory;
			}
			lastMainPlayerInventory = Main.playerInventory;
			if (!favoritePanelUpdateNeeded)
				return;
			favoritePanelUpdateNeeded = false;

			// Reset All
			foreach (var recipeSlot in RecipeCatalogueUI.instance.recipeSlots)
			{
				recipeSlot.favorited = false;
			}
			foreach (var recipeIndex in localPlayerFavoritedRecipes)
			{
				RecipeCatalogueUI.instance.recipeSlots[recipeIndex].favorited = true;
			}

			if(localPlayerFavoritedRecipes.Count == 0 && !ForceShowFavoritePanel) {
				ShowFavoritePanel = false;
				ForceHideFavoritePanel = true;
			}
			favoritePanel.RemoveAllChildren();

			if (Main.netMode != NetmodeID.SinglePlayer)
				favoritePanel.Append(ShowOtherPlayersFavoritesToggle);
			favoritePanel.Append(HideUnlessInventoryToggle);
			favoritePanel.Append(closeFavoritePanelButton);
			if (Main.GameUpdateCount > 0) {
				// TODO: Could make this update via an IL edit or new tmod hook, AssignedKeysChanged
				var keys = RecipeBrowser.instance.ToggleFavoritedPanelHotKey.GetAssignedKeys();
				if (keys.Count != 0) {
					closeFavoritePanelButton.hoverText = string.Format(RBText("Close", "FavoritedUI"), string.Format(RBText("ToggleHint", "FavoritedUI"), string.Join(", ", keys)));
				}
				else {
					closeFavoritePanelButton.hoverText = string.Format(RBText("Close", "FavoritedUI"), RBText("ToggleUnboundHint", "FavoritedUI"));
				}
			}

			UIGrid list = new UIGrid();
			list.Width.Set(-18, 1f);
			list.Height.Set(0, 1f);
			list.ListPadding = 5f;
			list.drawArrows = true;
			favoritePanel.Append(list);
			favoritePanel.AddDragTarget(list);
			favoritePanel.AddDragTarget(list._innerList);
			int width = 1;
			int height = 0;
			int order = 1;

			// TODO: support non-recipe paths, favorite from Craft Path entries: Farm Item from Enemy X,Y,Z
			// Support setting recipe to a desired count. (Alt click on ingredient that is required X times, the favorited recipe will be multiplied by X) Or scroll to increase? Buttons?
			if (RecipeBrowserClientConfig.Instance.ShowOtherPlayersFavoritedRecipes != ShowOtherPlayersFavoritedRecipesOption.Hide) {
				for (int i = 0; i < Main.maxPlayers; i++) {
					if (i != Main.myPlayer && Main.player[i].active) {
						if (RecipeBrowserClientConfig.Instance.ShowOtherPlayersFavoritedRecipes == ShowOtherPlayersFavoritedRecipesOption.ShowTeamOnly && Main.player[Main.myPlayer].team > 0 && Main.player[i].team != Main.player[Main.myPlayer].team)
							continue;
						foreach (var recipeIndex in Main.player[i].GetModPlayer<RecipeBrowserPlayer>().favoritedRecipes) { // Collection was modified potential with receiving other player favorited recipes?
							Recipe r = Main.recipe[recipeIndex];
							UIRecipeProgress s = new UIRecipeProgress(recipeIndex, r, order, i);
							order++;
							s.Recalculate();
							var a = s.GetInnerDimensions();
							s.Width.Precent = 1;
							list.Add(s);
							height += (int)(a.Height + list.ListPadding);
							width = Math.Max(width, (int)a.Width);
							favoritePanel.AddDragTarget(s);
						}
					}
				}
			}

			foreach (var recipeIndex in localPlayerFavoritedRecipes)
			{
				Recipe r = Main.recipe[recipeIndex];
				UIRecipeProgress s = new UIRecipeProgress(recipeIndex, r, order, Main.myPlayer);
				order++;
				s.Recalculate();
				var a = s.GetInnerDimensions();
				s.Width.Precent = 1;
				list.Add(s);
				height += (int)(a.Height + list.ListPadding);
				width = Math.Max(width, (int)a.Width);
				favoritePanel.AddDragTarget(s);
			}
			if(height == 0) {
				UIText text = new UIText(RBText("NoFavoritedRecipes"));
				list.Add(text);
				var a = text.GetInnerDimensions();
				text.Recalculate();
				a = text.GetInnerDimensions();
				height += (int)(a.Height + list.ListPadding);
				width = Math.Max(width, (int)a.Width + 20);
				favoritePanel.AddDragTarget(text);
			}
			favoritePanel.Height.Pixels = height + favoritePanel.PaddingBottom + favoritePanel.PaddingTop - list.ListPadding;
			if (favoritePanel.Height.Pixels < 64 && Main.netMode != NetmodeID.SinglePlayer)
				favoritePanel.Height.Pixels = 64; // Room for extra toggle in MP.
			favoritePanel.Width.Pixels = width + 18;
			favoritePanel.Recalculate();

			var scrollbar = new InvisibleFixedUIScrollbar(userInterface);
			scrollbar.SetView(100f, 1000f);
			scrollbar.Height.Set(0, 1f);
			scrollbar.Left.Set(-20, 1f);
			//favoritePanel.Append(scrollbar);
			list.SetScrollbar(scrollbar);

			Recipe.FindRecipes();
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			// additional updates.
			if (!mainPanel.GetDimensions().ToRectangle().Intersects(GetDimensions().ToRectangle()))
			{
				var parentSpace = GetDimensions().ToRectangle();
				mainPanel.Left.Pixels = Utils.Clamp(mainPanel.Left.Pixels, 0, parentSpace.Right - mainPanel.Width.Pixels);
				mainPanel.Top.Pixels = Utils.Clamp(mainPanel.Top.Pixels, 0, parentSpace.Bottom - mainPanel.Height.Pixels);
				mainPanel.Recalculate();
			}

			SharedUI.instance.Update();
			recipeCatalogueUI.Update();
			craftUI.Update();
			itemCatalogueUI.Update();
			bestiaryUI.Update();
			UpdateFavoritedPanel();
		}

		internal void ItemReceived(Item item)
		{
			var removes = localPlayerFavoritedRecipes.Where(x => Main.recipe[x].createItem.type == item.type && Main.recipe[x].createItem.maxStack == 1).ToList();

			foreach (var recipeIndex in removes)
			{
				FavoriteChange(recipeIndex, false);
			}

			if (item.createTile > -1) {
				List<int> adjTiles = Utilities.PopulateAdjTilesForTile(item.createTile);
				foreach (var tile in adjTiles) {
					RecipeBrowserPlayer.seenTiles[tile] = true;
				}
			}
		}

		internal void QueryItemChecklist()
		{
			object result = RecipeBrowser.itemChecklistInstance.Call("RequestFoundItems");
			if (result is string)
			{
				Main.NewText("Error, ItemChecklist said: " + result);
			}
			else if (result is bool[])
			{
				RecipeBrowserUI.instance.foundItems = (result as bool[]);
			}
		}

		internal int npcArrow = -1;
		public void HandleArrow() {
			// Copied over npc locating arrows from Census, changed arrow from starting at player to starting at cursor
			if(npcArrow != -1) {
				var npc = Main.npc[npcArrow];
				Terraria.GameInput.PlayerInput.SetZoom_World();
				Vector2 playerCenter = Main.MouseWorld;
				Terraria.GameInput.PlayerInput.SetZoom_UI();
				var vector = npc.Center - playerCenter;
				var distance = vector.Length();
				if (distance > 40) {
					var offset = Vector2.Normalize(vector) * Math.Min(40, distance - 20);
					float rotation = vector.ToRotation() + (float)(3 * Math.PI / 4);
					var drawPosition = Main.MouseScreen + offset;
					float fade = Math.Min(1f, (distance - 20) / 40);

					Main.spriteBatch.Draw(TextureAssets.Cursors[0].Value, drawPosition, null, new Color(173, 255, 47, 255) * fade, rotation, TextureAssets.Cursors[1].Value.Size() / 2, new Vector2(1.5f), SpriteEffects.None, 0);
				}
			}
			npcArrow = -1;
		}

		internal void BlockInput(UIElement dialog) {
			blockInput = new BlockInputElement(mainPanel, 20);
			blockInput.OnLeftMouseDown += UnblockInput;
			mainPanel.Append(blockInput);
			mainPanel.Append(activeDialog = dialog);
		}

		internal void UnblockInput(UIMouseEvent evt, UIElement listeningElement) {
			blockInput?.Remove();
			activeDialog?.Remove();

			UpdateModHoverImage();
		}
	}

	internal class TabController
	{
		private UIElement parent;
		private List<UIElement> panels;
		private List<UIElement> buttons;
		internal int currentPanel;

		public TabController(UIElement parent)
		{
			this.parent = parent;
			panels = new List<UIElement>();
			buttons = new List<UIElement>();
		}

		public void AddPanel(UIElement element)
		{
			panels.Add(element);
		}

		public void AddButton(UIElement element)
		{
			buttons.Add(element);
		}

		public void SetPanel(int panelIndex)
		{
			if (panelIndex >= 0 && panelIndex < panels.Count)
			{
				currentPanel = panelIndex;

				// TODO: OnRemove event maybe? Public children?
				panels.ForEach(panel => { if (parent.HasChild(panel)) parent.RemoveChild(panel); });

				for (int i = buttons.Count - 1; i >= 0; i--)
				{
					var button = buttons[i];
					parent.RemoveChild(button); parent.Append(button);
				}

				parent.RemoveChild(buttons[panelIndex]);

				parent.Append(panels[panelIndex]);
				parent.Append(buttons[panelIndex]);

				if (RecipeBrowserUI.instance.ModFilterDropdown?.Parent == parent)
				{
					parent.RemoveChild(RecipeBrowserUI.instance.ModFilterDropdown);
					parent.Append(RecipeBrowserUI.instance.ModFilterDropdown);
				}
				
				if(panelIndex == RecipeBrowserUI.ItemCatalogue)
				{
					SharedUI.instance.sortsAndFiltersPanel.Top.Set(0, 0f);
					SharedUI.instance.sortsAndFiltersPanel.Width.Set(-272, 1);
					SharedUI.instance.sortsAndFiltersPanel.Height.Set(60, 0f);

					SharedUI.instance.updateNeeded = true;
					ItemCatalogueUI.instance.mainPanel.Append(SharedUI.instance.sortsAndFiltersPanel);
				}
				else if (panelIndex == RecipeBrowserUI.RecipeCatalogue)
				{
					SharedUI.instance.sortsAndFiltersPanel.Top.Set(60, 0f);
					SharedUI.instance.sortsAndFiltersPanel.Width.Set(-52, 1);
					SharedUI.instance.sortsAndFiltersPanel.Height.Set(60, 0f);

					RecipeCatalogueUI.instance.mainPanel.Append(SharedUI.instance.sortsAndFiltersPanel);

					SharedUI.instance.updateNeeded = true;
					if (SharedUI.instance.SelectedCategory?.name == ArmorSetFeatureHelper.ArmorSetsInternalName) {
						SharedUI.instance.SelectedCategory = SharedUI.instance.categories[0];
					}
				}
			}
		}
	}
}
