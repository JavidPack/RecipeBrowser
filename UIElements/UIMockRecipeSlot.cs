using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using Terraria.GameContent;

namespace RecipeBrowser.UIElements
{
	// used as a duplicate
	// Todo, temporary?
	internal class UIMockRecipeSlot : UIItemSlot
	{
		public static Asset<Texture2D> ableToCraftBackgroundTexture;
		public static Asset<Texture2D> unableToCraftBackgroundTexture = TextureAssets.InventoryBack11;
		private UIRecipeSlot slot;

		public UIMockRecipeSlot(UIRecipeSlot slot, float scale = 0.75f) : base(slot.item, scale)
		{
			this.slot = slot;
		}

		public override void Click(UIMouseEvent evt)
		{
			slot.Click(evt);
			if (!Main.keyState.IsKeyDown(Main.FavoriteKey))
			{
				if ((slot.craftPathCalculated || slot.craftPathsCalculated) && slot.craftPaths.Count > 0)
				{
					RecipeBrowserUI.instance.tabController.SetPanel(RecipeBrowserUI.Craft);
					CraftUI.instance.SetRecipe(slot.index);
					if (!RecipeBrowserUI.instance.ShowRecipeBrowser)
						RecipeBrowserUI.instance.ShowRecipeBrowser = true;
				}
				else
				{
					// inherited. RecipeCatalogueUI.instance.SetRecipe(slot.index);
					RecipeBrowserUI.instance.tabController.SetPanel(RecipeBrowserUI.RecipeCatalogue);
					RecipeCatalogueUI.instance.recipeGrid.Goto(delegate (UIElement element)
					{
						UIRecipeSlot itemSlot = element as UIRecipeSlot;
						return itemSlot == slot;
					}, true);
					if (!RecipeBrowserUI.instance.ShowRecipeBrowser)
						RecipeBrowserUI.instance.ShowRecipeBrowser = true;
				}
			}
		}

		public override void RightClick(UIMouseEvent evt)
		{
			RecipeBrowserUI.instance.ShowRecipeBrowser = false;
		}

		internal override void DrawAdditionalOverlays(SpriteBatch spriteBatch, Vector2 vector2, float scale)
		{
			bool favorited = slot.favorited;
			slot.favorited = false;
			slot.DrawAdditionalOverlays(spriteBatch, vector2, scale);
			slot.favorited = favorited;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (IsMouseHovering)
				if (Main.keyState.IsKeyDown(Main.FavoriteKey))
					if (Main.drawingPlayerChat)
						Main.cursorOverride = 2;
					else
						Main.cursorOverride = 3;

			// TODO: Trigger slot.CraftPathsNeeded if RecipePath.extendedCraft

			backgroundTexture = unableToCraftBackgroundTexture;
			if(RecipePath.extendedCraft)
				slot.CraftPathNeeded();
			if ((slot.craftPathCalculated || slot.craftPathsCalculated) && slot.craftPaths.Count > 0)
				backgroundTexture = UIRecipeSlot.ableToCraftExtendedBackgroundTexture;

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