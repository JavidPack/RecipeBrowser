using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Map;
using Terraria.UI;
using ReLogic.Graphics;
using Terraria.ID;

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
				if(!Main.playerInventory) Main.LocalPlayer.AdjTiles(); // force adj tiles to be correct
				Color textColor = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
				Color noColor = Color.OrangeRed;
				Color yesColor = Color.Green;

				Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, Lang.inter[22].Value, new Vector2((float)positionX, (float)(positionY)), textColor, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
				int row = 0;
				int tileIndex = 0;
				while (tileIndex < Recipe.maxRequirements)
				{
					int num63 = (tileIndex + 1) * 26;
					if (selectedRecipe.requiredTile[tileIndex] == -1)
					{
						if (tileIndex == 0 && !selectedRecipe.needWater && !selectedRecipe.needHoney && !selectedRecipe.needLava)
						{
							// "None"
							Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, Lang.inter[23].Value, new Vector2(positionX, positionY + num63), textColor, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
							break;
						}
						break;
					}
					else
					{
						row++;
						int tileID = selectedRecipe.requiredTile[tileIndex];
						string tileName = Lang.GetMapObjectName(MapHelper.TileToLookup(tileID, 0));
						if(tileName == "")
						{
							if(tileID < TileID.Count)
								tileName = $"Tile {tileID}";
							else 
								tileName = Terraria.ModLoader.TileLoader.GetTile(tileID).Name + " (err no entry)";
						}
						Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, tileName, new Vector2(positionX, positionY + num63), Main.LocalPlayer.adjTile[tileID] ?  yesColor : noColor, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
						tileIndex++;
					}
				}
				// white if window not open?
				int yAdjust = (row + 1) * 26;
				if (selectedRecipe.needWater)
				{
					Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, Lang.inter[53].Value, new Vector2((float)positionX, (float)(positionY + yAdjust)), Main.LocalPlayer.adjWater ? yesColor : noColor, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
					yAdjust += 26;
				}
				if (selectedRecipe.needHoney)
				{
					Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, Lang.inter[58].Value, new Vector2((float)positionX, (float)(positionY + yAdjust)), Main.LocalPlayer.adjHoney ? yesColor : noColor, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
					yAdjust += 26;
				}
				if (selectedRecipe.needLava)
				{
					Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, Lang.inter[56].Value, new Vector2((float)positionX, (float)(positionY + yAdjust)), Main.LocalPlayer.adjLava ? yesColor : noColor, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
				}
			}
		}
	}
}