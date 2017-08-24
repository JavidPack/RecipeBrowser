using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework;

namespace RecipeBrowser.UIElements
{
	// used as a duplicate
	// Todo, temporary?
	class UIMockRecipeSlot : UIItemSlot
	{
		public static Texture2D ableToCraftBackgroundTexture;
		public static Texture2D unableToCraftBackgroundTexture = Main.inventoryBack11Texture;
		UIRecipeSlot slot;
		public UIMockRecipeSlot(UIRecipeSlot slot, float scale = 0.75f) : base(slot.item, scale)
		{
			this.slot = slot;
		}

		public override void Click(UIMouseEvent evt)
		{
			slot.Click(evt);
			RecipeBrowserUI.instance.ShowRecipeBrowser = true;
		}

		public override void RightClick(UIMouseEvent evt)
		{
			RecipeBrowserUI.instance.ShowRecipeBrowser = false;
		}

		internal override void DrawAdditionalOverlays(SpriteBatch spriteBatch, Vector2 vector2, float scale)
		{
			slot.favorited = false;
			slot.DrawAdditionalOverlays(spriteBatch, vector2, scale);
			slot.favorited = true;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (IsMouseHovering)
				if (Main.keyState.IsKeyDown(Main.FavoriteKey))
					Main.cursorOverride = 3;

			backgroundTexture = unableToCraftBackgroundTexture;

			for (int n = 0; n < Main.numAvailableRecipes; n++)
			{
				if (slot.index == Main.availableRecipe[n])
				{
					backgroundTexture = ableToCraftBackgroundTexture;
					break;
				}
			}

			base.DrawSelf(spriteBatch);
		}
	}
}
