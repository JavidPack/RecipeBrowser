using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.ID;

namespace RecipeBrowser.UIElements
{
	internal class UIRecipeSlot : UIItemSlot
	{
		public static Asset<Texture2D> selectedBackgroundTexture;
		public static Asset<Texture2D> recentlyDiscoveredBackgroundTexture = TextureAssets.InventoryBack8;
		public static Asset<Texture2D> favoritedBackgroundTexture;
		public static Asset<Texture2D> ableToCraftBackgroundTexture;
		public static Asset<Texture2D> ableToCraftExtendedBackgroundTexture;
		public int index;
		public bool recentlyDiscovered;
		public bool favorited;
		public bool selected;

		// Single vs All needed.
		public bool craftPathNeeded;
		public bool craftPathCalculated;
		public bool craftPathCalculationBegun;
		internal CancellationTokenSource craftPathCancellationTokenSource;

		// seen limits craftPaths calculation
		public bool craftPathsCalculated;
		public List<CraftPath> craftPaths;

		public UIRecipeSlot(int index, float scale = 0.75f) : base(Main.recipe[index].createItem, scale)
		{
			this.index = index;
		}

		public override void Click(UIMouseEvent evt)
		{
			if (Main.keyState.IsKeyDown(Main.FavoriteKey))
			{
				if (Main.drawingPlayerChat)
				{
					StringBuilder sb = new StringBuilder();
					foreach (var reqItem in Main.recipe[index].requiredItem)
					{
						sb.Append(ItemTagHandler.GenerateTag(reqItem));
					}
					sb.Append("-->");
					sb.Append(ItemTagHandler.GenerateTag(Main.recipe[index].createItem));
					if (ChatManager.AddChatText(FontAssets.MouseText.Value, sb.ToString(), Vector2.One))
					{
						SoundEngine.PlaySound(SoundID.MenuTick);
					}
				}
				else
					RecipeBrowserUI.instance.FavoriteChange(this.index, !favorited);
			}
			else
			{
				RecipeCatalogueUI.instance.SetRecipe(index);
				RecipeCatalogueUI.instance.queryLootItem = Main.recipe[index].createItem;
				RecipeCatalogueUI.instance.updateNeeded = true;
			}

			for (int n = 0; n < Main.numAvailableRecipes; n++)
			{
				if (index == Main.availableRecipe[n])
				{
					Main.playerInventory = true;
					Main.focusRecipe = n;
					Main.recFastScroll = true;
					break;
				}
			}

			// Idea: Glint the CraftUI tab if extended craft available?
		}

		public override void DoubleClick(UIMouseEvent evt)
		{
			if (Main.keyState.IsKeyDown(Main.FavoriteKey))
				return;
			RecipeCatalogueUI.instance.itemDescriptionFilter.SetText("");
			RecipeCatalogueUI.instance.itemNameFilter.SetText("");
			RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(item.type);
		}

		public override void RightClick(UIMouseEvent evt)
		{
			// TODO: Idea. Draw hinted craft path and suggest right click to view it?
			base.RightClick(evt);

			RecipeCatalogueUI.instance.SetRecipe(index);
			RecipeCatalogueUI.instance.queryLootItem = Main.recipe[index].createItem;
			RecipeCatalogueUI.instance.updateNeeded = true;

			RecipeBrowserUI.instance.tabController.SetPanel(RecipeBrowserUI.Craft);
			CraftUI.instance.SetRecipe(index);
			//Main.NewText($"{craftPaths.Count} path(s) found:");
		}

		public override int CompareTo(object obj)
		{
			UIRecipeSlot other = obj as UIRecipeSlot;
			if (favorited && !other.favorited)
			{
				return -1;
			}
			if (!favorited && other.favorited)
			{
				return 1;
			}
			if (recentlyDiscovered && !other.recentlyDiscovered)
			{
				return -1;
			}
			if (!recentlyDiscovered && other.recentlyDiscovered)
			{
				return 1;
			}
			if (favorited && other.favorited)
			{
				return RecipeBrowserUI.instance.localPlayerFavoritedRecipes.IndexOf(this.index).CompareTo(RecipeBrowserUI.instance.localPlayerFavoritedRecipes.IndexOf(other.index));
			}
			return index.CompareTo(other.index);
		}

		public int CompareToIgnoreIndex(UIRecipeSlot other)
		{
			if (favorited && !other.favorited)
			{
				return -1;
			}
			if (!favorited && other.favorited)
			{
				return 1;
			}
			if (recentlyDiscovered && !other.recentlyDiscovered)
			{
				return -1;
			}
			if (!recentlyDiscovered && other.recentlyDiscovered)
			{
				return 1;
			}
			if (favorited && other.favorited)
			{
				return RecipeBrowserUI.instance.localPlayerFavoritedRecipes.IndexOf(this.index).CompareTo(RecipeBrowserUI.instance.localPlayerFavoritedRecipes.IndexOf(other.index));
			}
			return 0;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (RecipePath.extendedCraft)
				craftPathNeeded = true;
			if (IsMouseHovering)
			{
				if (Main.keyState.IsKeyDown(Main.FavoriteKey))
					if (Main.drawingPlayerChat)
						Main.cursorOverride = 2;
					else
						Main.cursorOverride = 3;
				RecipeCatalogueUI.instance.hoveredIndex = index;
			}

			backgroundTexture = defaultBackgroundTexture;

			if ((craftPathCalculated || craftPathsCalculated) && craftPaths.Count > 0)
				backgroundTexture = ableToCraftExtendedBackgroundTexture;

			for (int n = 0; n < Main.numAvailableRecipes; n++)
			{
				if (index == Main.availableRecipe[n])
				{
					backgroundTexture = ableToCraftBackgroundTexture;
					break;
				}
			}

			if (recentlyDiscovered)
				backgroundTexture = recentlyDiscoveredBackgroundTexture;

			base.DrawSelf(spriteBatch);
		}

		internal override void DrawAdditionalOverlays(SpriteBatch spriteBatch, Vector2 vector2, float scale)
		{
			if (favorited)
				spriteBatch.Draw(favoritedBackgroundTexture.Value, vector2, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			if (selected)
				spriteBatch.Draw(selectedBackgroundTexture.Value, vector2, null, Color.White * Main.essScale, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			// Is this called even when not drawn? Yes. Need Draw flag
			if (craftPathNeeded && !(craftPathCalculated || craftPathsCalculated) && !craftPathCalculationBegun)
			{
				DoGetCraftPath();
			}
		}

		internal void CraftPathsImmediatelyNeeded()
		{
			if (!craftPathsCalculated)
			{
				RecipePath.PrepareGetCraftPaths();
				craftPaths = RecipePath.GetCraftPaths(Main.recipe[index], CancellationToken.None, false);
				craftPathsCalculated = true;
			}
		}

		internal void CraftPathNeeded()
		{
			craftPathNeeded = true;
			if (!(craftPathCalculated || craftPathsCalculated) && !craftPathCalculationBegun)
			{
				DoGetCraftPath();
			}
		}

		private void DoGetCraftPath()
		{
			//var firstTask = Task.Run(() => craftPaths = RecipePath.GetCraftPaths(Main.recipe[index], cancellationTokenSource.Token), cancellationTokenSource.Token);
			RecipePath.PrepareGetCraftPaths();
			var task = new Task(TaskAction);
			RecipeBrowser.instance.concurrentTasks.Enqueue(task);
			//firstTask.Start();
			craftPathCalculationBegun = true;
		}

		private void TaskAction() {
			craftPathCancellationTokenSource = new CancellationTokenSource(1); // TODO: Configurable
			// could a full override a simple?
			craftPaths = RecipePath.GetCraftPaths(Main.recipe[index], craftPathCancellationTokenSource.Token, true);
			if (!craftPathCancellationTokenSource.Token.IsCancellationRequested) {
				craftPathCalculated = true;
				//RecipeCatalogueUI.instance.updateNeeded = true;
				if (RecipeCatalogueUI.instance.slowUpdateNeeded == 0)
					RecipeCatalogueUI.instance.slowUpdateNeeded = 5;
				if (ItemCatalogueUI.instance.slowUpdateNeeded == 0)
					ItemCatalogueUI.instance.slowUpdateNeeded = 30;
			}
		}
	}
}