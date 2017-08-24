using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework;
using System;

namespace RecipeBrowser.UIElements
{
	class UIRecipeSlot : UIItemSlot
	{
		public static Texture2D selectedBackgroundTexture;
		public static Texture2D recentlyDiscoveredBackgroundTexture = Main.inventoryBack8Texture;
		public static Texture2D favoritedBackgroundTexture;
		public static Texture2D ableToCraftBackgroundTexture;
		public int index;
		public bool recentlyDiscovered;
		public bool favorited;
		public bool selected;

		public UIRecipeSlot(int index, float scale = 0.75f) : base(Main.recipe[index].createItem, scale)
		{
			this.index = index;
		}

		public override void Click(UIMouseEvent evt)
		{
			if (Main.keyState.IsKeyDown(Main.FavoriteKey))
			{
				favorited = !favorited;
				// trigger update since this reorders things
				RecipeBrowserUI.instance.FavoriteChange(this);
				RecipeBrowserUI.instance.updateNeeded = true;
			}
			else
			{
				RecipeBrowserUI.instance.SetRecipe(index);
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
		}

		public override void DoubleClick(UIMouseEvent evt)
		{
			if (Main.keyState.IsKeyDown(Main.FavoriteKey))
				return;
			RecipeBrowserUI.instance.itemDescriptionFilter.SetText("");
			RecipeBrowserUI.instance.itemNameFilter.SetText("");
			RecipeBrowserUI.instance.queryItem.ReplaceWithFake(item.type);
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
				return RecipeBrowserUI.instance.favoritedRecipes.IndexOf(this).CompareTo(RecipeBrowserUI.instance.favoritedRecipes.IndexOf(other));
			}
			return index.CompareTo(other.index);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (IsMouseHovering)
				if (Main.keyState.IsKeyDown(Main.FavoriteKey))
					Main.cursorOverride = 3;

			backgroundTexture = defaultBackgroundTexture;

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
	}
}
