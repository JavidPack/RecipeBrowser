using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.ID;

namespace RecipeBrowser.UIElements
{
	internal class UICraftButton : UIElement
	{
		CraftPath.RecipeNode recipeNode;
		Recipe recipe;
		int index = -1;
		public UICraftButton(CraftPath.RecipeNode recipeNode, Recipe recipe)
		{
			this.recipe = recipe;
			this.recipeNode = recipeNode;
			this.Width.Set(TextureAssets.Reforge[0].Width(), 0f);
			this.Height.Set(TextureAssets.Reforge[0].Height(), 0f);
			for (int i = 0; i < Recipe.numRecipes; i++)
			{
				if (recipe == Main.recipe[i])
				{
					index = i;
					break;
				}
			}
			if (index == -1)
				throw new System.Exception("Index is -1??");
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			bool ableToCraft = AbleToCraft();
			//if (AbleToCraft())
			{
				CalculatedStyle dimensions = base.GetDimensions();
				spriteBatch.Draw(TextureAssets.Reforge[IsMouseHovering && ableToCraft ? 1 : 0].Value, dimensions.Position(), null, Color.White, 0, Vector2.Zero, 0.75f, SpriteEffects.None, 0);
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, ableToCraft ? "✓" : "X", dimensions.Position() + new Vector2(14f, 10f), ableToCraft ? Utilities.yesColor : Color.LightSalmon, 0f, Vector2.Zero, new Vector2(0.7f));
			}
		}

		public override void MouseOver(UIMouseEvent evt)
		{
			base.MouseOver(evt);
			if (AbleToCraft())
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			base.Click(evt);
			for (int i = 0; i < recipeNode.multiplier; i++)
			{
				Recipe.FindRecipes();
				if (AbleToCraft())
				{
					//Main.CraftItem(recipe);
					Item result = recipe.createItem.Clone();
					result.Prefix(-1);
					// Consumes the items. Does not actually check required items or tiles.
					recipe.Create();
					// TODO: Alternate recipe.Create that takes from all sources.
					result.position = Main.LocalPlayer.Center - result.Size; // needed for ItemText

					RecipeLoader.OnCraft(result, recipe);
					ItemLoader.OnCreate(result, new RecipeCreationContext { recipe = recipe });

					Item itemIfNoSpace = Main.LocalPlayer.GetItem(Main.myPlayer, result, GetItemSettings.PickupItemFromWorld);
					if (itemIfNoSpace.stack > 0)
					{
						Main.LocalPlayer.QuickSpawnClonedItem(itemIfNoSpace, itemIfNoSpace.stack);
					}
				}
				else
				{
					//Main.NewText("Oops, I couldn't actually craft that for you.");
				}
			}
		}

		// Buggy if inventory not open.
		bool AbleToCraft()
		{
			bool flag = Main.guideItem.type > 0 && Main.guideItem.stack > 0 && Main.guideItem.Name != "";
			if (flag)
				return false; // Potential bug with guideItem affecting availableRecipe
			for (int n = 0; n < Main.numAvailableRecipes; n++)
			{
				if (index == Main.availableRecipe[n])
				{
					return true;
				}
			}
			return false;
		}
	}
}