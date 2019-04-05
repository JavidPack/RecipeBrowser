using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RecipeBrowser.UIElements;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Map;
using Terraria.UI;

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

		internal UIHorizontalGrid craftingTilesGrid;
		internal List<UITileNoSlot> tileList;

		public UIRecipeInfo()
		{
			UIPanel craftingPanel = new UIPanel();
			craftingPanel.SetPadding(6);
			craftingPanel.Top.Set(-50, 1f);
			craftingPanel.Left.Set(180, 0f);
			craftingPanel.Width.Set(-180 - 4, 1f); //- 50
			craftingPanel.Height.Set(50, 0f);
			craftingPanel.BackgroundColor = Color.CornflowerBlue;
			Append(craftingPanel);

			craftingTilesGrid = new UIHorizontalGrid();
			craftingTilesGrid.Width.Set(0, 1f);
			craftingTilesGrid.Height.Set(0, 1f);
			craftingTilesGrid.ListPadding = 2f;
			craftingTilesGrid.OnScrollWheel += RecipeBrowserUI.OnScrollWheel_FixHotbarScroll;
			craftingPanel.Append(craftingTilesGrid);

			var craftingTilesGridScrollbar = new InvisibleFixedUIHorizontalScrollbar(RecipeBrowserUI.instance.userInterface);
			craftingTilesGridScrollbar.SetView(100f, 1000f);
			craftingTilesGridScrollbar.Width.Set(0, 1f);
			craftingTilesGridScrollbar.Top.Set(-20, 1f);
			craftingPanel.Append(craftingTilesGridScrollbar);
			craftingTilesGrid.SetScrollbar(craftingTilesGridScrollbar);

			tileList = new List<UITileNoSlot>();
		}

		private const int cols = 5;

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			//Rectangle hitbox = GetInnerDimensions().ToRectangle();
			//Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.LightBlue * 0.6f);

			if (RecipeCatalogueUI.instance.selectedIndex < 0) return;

			Recipe selectedRecipe = Main.recipe[RecipeCatalogueUI.instance.selectedIndex];

			CalculatedStyle innerDimensions = GetInnerDimensions();
			Vector2 pos = innerDimensions.Position();

			float positionX = pos.X;
			float positionY = pos.Y;
			if (selectedRecipe != null)
			{
				StringBuilder sb = new StringBuilder();
				StringBuilder sbTiles = new StringBuilder();

				if (!Main.playerInventory) Main.LocalPlayer.AdjTiles(); // force adj tiles to be correct
				Color textColor = Color.White;// new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
				Color noColor = Color.OrangeRed;
				Color yesColor = Color.Green;

				//[c/FF0000:This text is red.]
				sb.Append($"[c/{textColor.Hex3()}:{Lang.inter[22].Value}] ");
				Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, Lang.inter[22].Value, new Vector2((float)positionX, (float)(positionY)), textColor, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
				int row = 0;
				int tileIndex = 0;
				bool comma = false;
				while (tileIndex < Recipe.maxRequirements)
				{
					int num63 = (tileIndex + 1) * 26;
					if (selectedRecipe.requiredTile[tileIndex] == -1)
					{
						if (tileIndex == 0 && !selectedRecipe.needWater && !selectedRecipe.needHoney && !selectedRecipe.needLava)
						{
							// "None"
							sb.Append($"{(comma ? ", " : "")}[c/{textColor.Hex3()}:{Lang.inter[23].Value}]");
							sbTiles.Append($"{(comma ? ", " : "")}[c/{textColor.Hex3()}:{Lang.inter[23].Value}]");
							//Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, Lang.inter[23].Value, new Vector2(positionX, positionY + num63), textColor, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
							comma = true;
							break;
						}
						break;
					}
					else
					{
						row++;
						int tileID = selectedRecipe.requiredTile[tileIndex];
						string tileName = Lang.GetMapObjectName(MapHelper.TileToLookup(tileID, 0));
						if (tileName == "")
						{
							if (tileID < TileID.Count)
								tileName = $"Tile {tileID}";
							else
								tileName = Terraria.ModLoader.TileLoader.GetTile(tileID).Name + " (err no entry)";
						}
						sb.Append($"{(comma ? ", " : "")}[c/{(Main.LocalPlayer.adjTile[tileID] ? yesColor : noColor).Hex3()}:{tileName}]");
						sbTiles.Append($"{(comma ? ", " : "")}[c/{(Main.LocalPlayer.adjTile[tileID] ? yesColor : noColor).Hex3()}:{tileName}]");
						//Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, tileName, new Vector2(positionX, positionY + num63), Main.LocalPlayer.adjTile[tileID] ?  yesColor : noColor, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
						tileIndex++;
						comma = true;
					}
				}
				// white if window not open?
				int yAdjust = (row + 1) * 26;
				if (selectedRecipe.needWater)
				{
					sb.Append($"{(comma ? ", " : "")}[c/{(Main.LocalPlayer.adjWater ? yesColor : noColor).Hex3()}:{Lang.inter[53].Value}]");
					sbTiles.Append($"{(comma ? ", " : "")}[c/{(Main.LocalPlayer.adjWater ? yesColor : noColor).Hex3()}:{Lang.inter[53].Value}]");
					//Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, Lang.inter[53].Value, new Vector2((float)positionX, (float)(positionY + yAdjust)), Main.LocalPlayer.adjWater ? yesColor : noColor, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
					yAdjust += 26;
					comma = true;
				}
				if (selectedRecipe.needHoney)
				{
					sb.Append($"{(comma ? ", " : "")}[c/{(Main.LocalPlayer.adjHoney ? yesColor : noColor).Hex3()}:{Lang.inter[58].Value}]");
					sbTiles.Append($"{(comma ? ", " : "")}[c/{(Main.LocalPlayer.adjHoney ? yesColor : noColor).Hex3()}:{Lang.inter[58].Value}]");
					//Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, Lang.inter[58].Value, new Vector2((float)positionX, (float)(positionY + yAdjust)), Main.LocalPlayer.adjHoney ? yesColor : noColor, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
					yAdjust += 26;
					comma = true;
				}
				if (selectedRecipe.needLava)
				{
					sb.Append($"{(comma ? ", " : "")}[c/{(Main.LocalPlayer.adjLava ? yesColor : noColor).Hex3()}:{Lang.inter[56].Value}]");
					sbTiles.Append($"{(comma ? ", " : "")}[c/{(Main.LocalPlayer.adjLava ? yesColor : noColor).Hex3()}:{Lang.inter[56].Value}]");
					//Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, Lang.inter[56].Value, new Vector2((float)positionX, (float)(positionY + yAdjust)), Main.LocalPlayer.adjLava ? yesColor : noColor, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
					comma = true;
				}
				float width = Terraria.UI.Chat.ChatManager.GetStringSize(Main.fontMouseText, sbTiles.ToString(), Vector2.One).X;
				if (width > 170)
				{
					Vector2 scale = new Vector2(170 / width);
					Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, sbTiles.ToString(), new Vector2(positionX, positionY + 26), Color.White, 0f, Vector2.Zero, scale, -1f, 2f);
				}
				else
				{
					Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, sbTiles.ToString(), new Vector2(positionX, positionY + 26), Color.White, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
				}
				Rectangle rectangle = GetDimensions().ToRectangle();
				rectangle.Width = 180;
				//if (IsMouseHovering)
				if (rectangle.Contains(Main.MouseScreen.ToPoint()) && Terraria.UI.Chat.ChatManager.GetStringSize(Main.fontMouseText, sbTiles.ToString(), Vector2.One).X > 180)
				{
					Main.hoverItemName = sb.ToString();
					/* Different approach to informing recipe mod source
					ModRecipe modRecipe = selectedRecipe as ModRecipe;
					if (Terraria.UI.Chat.ChatManager.GetStringSize(Main.fontMouseText, sbTiles.ToString(), Vector2.One).X > 180)
						Main.hoverItemName = sb.ToString() + (modRecipe != null ? $"\n[{modRecipe.mod.DisplayName}]" : "");
					else if (modRecipe != null)
						Main.hoverItemName = $"[{modRecipe.mod.DisplayName}]";
					*/
				}
			}
		}
	}
}