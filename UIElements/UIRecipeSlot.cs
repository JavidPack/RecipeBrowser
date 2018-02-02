using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework;
using System;
using Terraria.UI.Chat;
using Terraria.GameContent.UI.Chat;
using System.Text;

namespace RecipeBrowser.UIElements
{
	internal class UIRecipeSlot : UIItemSlot
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
		}

		public override void DoubleClick(UIMouseEvent evt)
		{
			if (Main.keyState.IsKeyDown(Main.FavoriteKey))
				return;
			RecipeCatalogueUI.instance.itemDescriptionFilter.SetText("");
			RecipeCatalogueUI.instance.itemNameFilter.SetText("");
			RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(item.type);
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

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (IsMouseHovering)
				if (Main.keyState.IsKeyDown(Main.FavoriteKey))
					if (Main.drawingPlayerChat)
						Main.cursorOverride = 2;
					else
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