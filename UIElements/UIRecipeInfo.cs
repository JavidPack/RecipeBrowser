using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Map;
using Terraria.UI;
using ReLogic.Graphics;

namespace RecipeBrowser
{
	internal class UIRecipeInfo : UIElement
	{
		// 0 means unchecked
		//static int[] tileToItemCache;
		//public UIRecipeInfo()
		//{
		//	tileToItemCache = new int[Main.tileTexture.Length];
		//}

		//public UIRecipeInfo()
		//{
		//	for (int i = 0; i < Recipe.maxRequirements; i++)
		//	{
		//		var ingredient = new 
		//		Append()
		//	}
		//}

			const int cols = 5;

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			//Rectangle hitbox = GetInnerDimensions().ToRectangle();
			//Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.LightBlue * 0.6f);

			if (RecipeBrowserUI.instance.selectedIndex < 0) return;

			Recipe selectedRecipe = Main.recipe[RecipeBrowserUI.instance.selectedIndex];

			CalculatedStyle innerDimensions = GetInnerDimensions();
			Vector2 pos = innerDimensions.Position();

			float positionX = pos.X;
			float positionY = pos.Y;
			if (selectedRecipe != null)
			{
				Color textColor = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);

				spriteBatch.DrawString(Main.fontMouseText, Lang.inter[22].Value, new Vector2((float)positionX, (float)(positionY)), textColor, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
				int num61 = 0;
				int tileIndex = 0;
				while (tileIndex < Recipe.maxRequirements)
				{
					int num63 = (tileIndex + 1) * 26;
					if (selectedRecipe.requiredTile[tileIndex] == -1)
					{
						if (tileIndex == 0 && !selectedRecipe.needWater && !selectedRecipe.needHoney && !selectedRecipe.needLava)
						{
							spriteBatch.DrawString(Main.fontMouseText, Lang.inter[23].Value, new Vector2((float)positionX, (float)(positionY + num63)), textColor, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
							break;
						}
						break;
					}
					else
					{
						num61++;

						//if (tileToItemCache[selectedRecipe.requiredTile[tileIndex]] == -1)
						//{

						//}

						spriteBatch.DrawString(Main.fontMouseText, Lang.GetMapObjectName(MapHelper.TileToLookup(selectedRecipe.requiredTile[tileIndex], 0)), new Vector2((float)positionX, (float)(positionY + num63)), textColor, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
						tileIndex++;
					}
				}
				if (selectedRecipe.needWater)
				{
					int num64 = (num61 + 1) * 26;
					spriteBatch.DrawString(Main.fontMouseText, Lang.inter[53].Value, new Vector2((float)positionX, (float)(positionY + num64)), textColor, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
				}
				if (selectedRecipe.needHoney)
				{
					int num65 = (num61 + 1) * 26;
					spriteBatch.DrawString(Main.fontMouseText, Lang.inter[58].Value, new Vector2((float)positionX, (float)(positionY + num65)), textColor, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
				}
				if (selectedRecipe.needLava)
				{
					int num66 = (num61 + 1) * 26;
					spriteBatch.DrawString(Main.fontMouseText, Lang.inter[56].Value, new Vector2((float)positionX, (float)(positionY + num66)), textColor, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
				}
			}
		}
	}
}