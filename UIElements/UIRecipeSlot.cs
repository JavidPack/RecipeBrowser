using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Chat;
using Terraria.UI;
using Terraria.UI.Chat;

namespace RecipeBrowser.UIElements
{
	internal class UIRecipeSlot : UIItemSlot
	{
		public static Texture2D selectedBackgroundTexture;
		public static Texture2D recentlyDiscoveredBackgroundTexture = Main.inventoryBack8Texture;
		public static Texture2D favoritedBackgroundTexture;
		public static Texture2D ableToCraftBackgroundTexture;
		public static Texture2D ableToCraftExtendedBackgroundTexture;
		public int index;
		public bool recentlyDiscovered;
		public bool favorited;
		public bool selected;
		public bool craftPathsNeeded; // seen limits craftPaths calculation
		public bool craftPathsCalculated;
		public bool craftPathsCalculationBegun;
		internal CancellationTokenSource cancellationTokenSource;
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
					foreach (var item in Main.recipe[index].requiredItem)
					{
						if (!item.IsAir)
							sb.Append(ItemTagHandler.GenerateTag(item));
					}
					sb.Append("-->");
					sb.Append(ItemTagHandler.GenerateTag(Main.recipe[index].createItem));
					if (ChatManager.AddChatText(Main.fontMouseText, sb.ToString(), Vector2.One))
					{
						Main.PlaySound(12);
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
				craftPathsNeeded = true;
			if (IsMouseHovering)
				if (Main.keyState.IsKeyDown(Main.FavoriteKey))
					if (Main.drawingPlayerChat)
						Main.cursorOverride = 2;
					else
						Main.cursorOverride = 3;

			backgroundTexture = defaultBackgroundTexture;

			if (craftPathsCalculated && craftPaths.Count > 0)
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
				spriteBatch.Draw(favoritedBackgroundTexture, vector2, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			if (selected)
				spriteBatch.Draw(selectedBackgroundTexture, vector2, null, Color.White * Main.essScale, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			// Is this called even when not drawn? Yes. Need Draw flag
			if (craftPathsNeeded && !craftPathsCalculated && !craftPathsCalculationBegun)
			{
				DoGetCraftPaths();
			}
		}

		internal void CraftPathsImmediatelyNeeded()
		{
			craftPathsNeeded = true;
			if (!craftPathsCalculated)
			{
				RecipePath.PrepareGetCraftPaths();
				craftPaths = RecipePath.GetCraftPaths(Main.recipe[index], CancellationToken.None);
				craftPathsCalculated = true;
			}
		}

		internal void CraftPathsNeeded()
		{
			craftPathsNeeded = true;
			if (!craftPathsCalculated && !craftPathsCalculationBegun)
			{
				DoGetCraftPaths();
			}
		}

		private void DoGetCraftPaths()
		{
			cancellationTokenSource = new CancellationTokenSource(500);
			RecipePath.PrepareGetCraftPaths();
			var firstTask = Task.Run(() => craftPaths = RecipePath.GetCraftPaths(Main.recipe[index], cancellationTokenSource.Token), cancellationTokenSource.Token);
			//var firstTask = new Task(() => craftPaths = RecipePath.GetCraftPaths(Main.recipe[index], cancellationTokenSource.Token), cancellationTokenSource.Token);
			var secondTask = firstTask.ContinueWith((t) =>
			{
				if (!cancellationTokenSource.Token.IsCancellationRequested)
				{
					craftPathsCalculated = true;
					//RecipeCatalogueUI.instance.updateNeeded = true;
					if (RecipeCatalogueUI.instance.slowUpdateNeeded == 0)
						RecipeCatalogueUI.instance.slowUpdateNeeded = 5;
				}
				//else
				//	Main.NewText(this.index + " cancelled");
			});
			//firstTask.Start();
			craftPathsCalculationBegun = true;
		}
	}
}