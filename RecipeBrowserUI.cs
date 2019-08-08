using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RecipeBrowser.UIElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI.Elements;
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
		internal UIHoverImageButton closeButton;

		internal SharedUI sharedUI;
		internal RecipeCatalogueUI recipeCatalogueUI;
		internal CraftUI craftUI;
		internal ItemCatalogueUI itemCatalogueUI;
		internal BestiaryUI bestiaryUI;
		internal HelpUI helpUI;

		internal List<int> localPlayerFavoritedRecipes => Main.LocalPlayer.GetModPlayer<RecipeBrowserPlayer>().favoritedRecipes;
		internal bool[] foundItems;

		internal string[] mods;

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
			var type = Assembly.GetAssembly(typeof(Mod)).GetType("Terraria.ModLoader.Mod");
			FieldInfo loadModsField = type.GetField("items", BindingFlags.Instance | BindingFlags.NonPublic);

			mods = ModLoader.Mods.Where(mod => ((Dictionary<string, ModItem>)loadModsField.GetValue(mod)).Count > 0).Select(mod => mod.Name).ToArray();
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

			sharedUI = new SharedUI();
			recipeCatalogueUI = new RecipeCatalogueUI();
			craftUI = new CraftUI();
			itemCatalogueUI = new ItemCatalogueUI();
			bestiaryUI = new BestiaryUI();
			helpUI = new HelpUI();

			sharedUI.Initialize();

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
			button.OnClick += (a, b) => tabController.SetPanel(RecipeCatalogue);
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
			button.OnClick += (a, b) => { tabController.SetPanel(Craft); };
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
			button.OnClick += (a, b) => { tabController.SetPanel(ItemCatalogue); itemCatalogueUI.updateNeeded = true; };
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
			button.OnClick += (a, b) => tabController.SetPanel(Bestiary);
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
			button.OnClick += (a, b) => tabController.SetPanel(Help);
			button.BackgroundColor = HelpUI.color;

			text = new UIText("Help", 0.85f);
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

			var modFilterButton = new UIHoverImageButtonMod(RecipeBrowser.instance.GetTexture("Images/filterMod"), RBText("ModFilter") + ": " + RBText("All"));
			modFilterButton.Left.Set(-60, 1f);
			modFilterButton.Top.Set(-0, 0f);
			modFilterButton.OnClick += ModFilterButton_OnClick;
			modFilterButton.OnRightClick += ModFilterButton_OnRightClick;
			modFilterButton.OnMiddleClick += ModFilterButton_OnMiddleClick;
			button.Append(modFilterButton);

			Texture2D texture = RecipeBrowser.instance.GetTexture("UIElements/closeButton");
			closeButton = new UIHoverImageButton(texture, RBText("Close"));
			closeButton.OnClick += CloseButtonClicked;
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
		}

		//private void ItemChecklistRadioButton_OnRightClick(UIMouseEvent evt, UIElement listeningElement)
		//{
		//	// Switch modes.
		//	ItemChecklistRadioButton.SetHoverText("Mode: Only All Results");
		//}

		// Vanilla ModLoader mod will act as "all"
		internal static int modIndex;

		private void ModFilterButton_OnClick(UIMouseEvent evt, UIElement listeningElement)
		{
			UIHoverImageButtonMod button = (evt.Target as UIHoverImageButtonMod);
			button.hoverText = RBText("ModFilter") + ": " + GetModFilterTooltip(true);
			UpdateModHoverImage(button);
			AllUpdateNeeded();
		}

		private void ModFilterButton_OnRightClick(UIMouseEvent evt, UIElement listeningElement)
		{
			UIHoverImageButtonMod button = (evt.Target as UIHoverImageButtonMod);
			button.hoverText = RBText("ModFilter") + ": " + GetModFilterTooltip(false);
			UpdateModHoverImage(button);
			AllUpdateNeeded();
		}

		private void ModFilterButton_OnMiddleClick(UIMouseEvent evt, UIElement listeningElement)
		{
			UIHoverImageButtonMod button = (evt.Target as UIHoverImageButtonMod);
			modIndex = 0;
			button.hoverText = RBText("ModFilter") + ": " + RBText("All");
			UpdateModHoverImage(button);
			AllUpdateNeeded();
		}

		private void UpdateModHoverImage(UIHoverImageButtonMod button)
		{
			button.texture = null;
			Mod otherMod = ModLoader.GetMod(mods[modIndex]);
			if (otherMod != null && otherMod.FileExists("icon.png"))
			{
				var modIconTexture = Texture2D.FromStream(Main.instance.GraphicsDevice, new MemoryStream(otherMod.GetFileBytes("icon.png")));
				if (modIconTexture.Width == 80 && modIconTexture.Height == 80)
				{
					button.texture = modIconTexture;
				}
			}
		}

		private string GetModFilterTooltip(bool increment)
		{
			modIndex = increment ? (modIndex + 1) % mods.Length : (mods.Length + modIndex - 1) % mods.Length;
			return modIndex == 0 ? RBText("All") : mods[modIndex];
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

		internal void FavoriteChange(int index, bool favorite)
		{
			RecipeCatalogueUI.instance.recipeSlots[index].favorited = favorite;
			localPlayerFavoritedRecipes.RemoveAll(x => x == index);
			if (favorite)
				localPlayerFavoritedRecipes.Add(index);
			favoritePanelUpdateNeeded = true;
			RecipeCatalogueUI.instance.updateNeeded = true;
		}

		internal bool favoritePanelUpdateNeeded;
		internal void UpdateFavoritedPanel()
		{
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

			ShowFavoritePanel = localPlayerFavoritedRecipes.Count > 0; // TODO: checkbox for force this.
			favoritePanel.RemoveAllChildren();

			UIGrid list = new UIGrid();
			list.Width.Set(0, 1f);
			list.Height.Set(0, 1f);
			list.ListPadding = 5f;
			list.OnScrollWheel += RecipeBrowserUI.OnScrollWheel_FixHotbarScroll;
			favoritePanel.Append(list);
			favoritePanel.AddDragTarget(list);
			favoritePanel.AddDragTarget(list._innerList);
			int width = 1;
			int height = 0;
			int order = 1;

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				if (i != Main.myPlayer && Main.player[i].active)
				{
					foreach (var recipeIndex in Main.player[i].GetModPlayer<RecipeBrowserPlayer>().favoritedRecipes) // Collection was modified potential with receiving other player starred recipes?
					{
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
			favoritePanel.Height.Pixels = height + favoritePanel.PaddingBottom + favoritePanel.PaddingTop - list.ListPadding;
			favoritePanel.Width.Pixels = width;
			favoritePanel.Recalculate();

			var scrollbar = new InvisibleFixedUIScrollbar(userInterface);
			scrollbar.SetView(100f, 1000f);
			scrollbar.Height.Set(0, 1f);
			scrollbar.Left.Set(-20, 1f);
			favoritePanel.Append(scrollbar);
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

			sharedUI.Update();
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

		internal static void OnScrollWheel_FixHotbarScroll(UIScrollWheelEvent evt, UIElement listeningElement)
		{
			Main.LocalPlayer.ScrollHotbar(Terraria.GameInput.PlayerInput.ScrollWheelDelta / 120);
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

				if(panelIndex == RecipeBrowserUI.ItemCatalogue)
				{
					SharedUI.instance.sortsAndFiltersPanel.Top.Set(0, 0f);
					SharedUI.instance.sortsAndFiltersPanel.Width.Set(-272, 1);
					SharedUI.instance.sortsAndFiltersPanel.Height.Set(60, 0f);

					ItemCatalogueUI.instance.mainPanel.Append(SharedUI.instance.sortsAndFiltersPanel);
				}
				else if (panelIndex == RecipeBrowserUI.RecipeCatalogue)
				{
					SharedUI.instance.sortsAndFiltersPanel.Top.Set(60, 0f);
					SharedUI.instance.sortsAndFiltersPanel.Width.Set(-52, 1);
					SharedUI.instance.sortsAndFiltersPanel.Height.Set(60, 0f);

					RecipeCatalogueUI.instance.mainPanel.Append(SharedUI.instance.sortsAndFiltersPanel);
				}
			}
		}
	}
}