using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;

namespace RecipeBrowser.UIElements
{
	internal class UIRecipeProgress : UIElement
	{
		private int order;

		public UIRecipeProgress(int index, Recipe recipe, int order)
		{
			this.order = order;
			UIMockRecipeSlot create = new UIMockRecipeSlot(RecipeCatalogueUI.instance.recipeSlots[index]);
			create.Recalculate();
			create.Left.Set(-create.Width.Pixels, 1f);
			var b = create.GetOuterDimensions();
			Append(create);
			int x = 0;
			x += (int)b.Width + 2;
			for (int j = 0; j < Recipe.maxRequirements; j++)
			{
				if (recipe.requiredItem[j].type > 0)
				{
					Item item = new Item();
					item.SetDefaults(recipe.requiredItem[j].type);
					UITrackIngredientSlot ingredient = new UITrackIngredientSlot(item, recipe.requiredItem[j].stack, recipe);
					x += (int)b.Width + 2;
					ingredient.Left.Set(-x, 1f);

					RecipeCatalogueUI.OverrideForGroups(recipe, ingredient.item);

					Append(ingredient);
				}
			}
			Height.Pixels = b.Height;
			Width.Pixels = x + 12;
		}

		private bool updateNeeded;

		public override void Update(GameTime gameTime)
		{
			if (!updateNeeded) return;
			updateNeeded = false;
		}

		public override int CompareTo(object obj)
		{
			UIRecipeProgress other = obj as UIRecipeProgress;
			return order.CompareTo(other.order);
		}
	}
}