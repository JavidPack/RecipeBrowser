using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RecipeBrowser.UIElements;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace RecipeBrowser
{
	internal class UIRecipeInfo : UIElement
	{
		// TODO: Fix order of ingredients to match recipe.
		// TODO: Use Tile images here as well? Optional? // internal List<UITileNoSlot> tileList; 
		internal UIHorizontalGrid craftingIngredientsGrid;

		public UIRecipeInfo()
		{
			UIPanel craftingPanel = new UIPanel();
			craftingPanel.SetPadding(6);
			craftingPanel.Top.Set(-50, 1f);
			craftingPanel.Left.Set(180, 0f);
			craftingPanel.Width.Set(-180 - 2, 1f); //- 50
			craftingPanel.Height.Set(50, 0f);
			craftingPanel.BackgroundColor = Color.CornflowerBlue;
			Append(craftingPanel);

			craftingIngredientsGrid = new UIHorizontalGrid();
			craftingIngredientsGrid.Width.Set(0, 1f);
			craftingIngredientsGrid.Height.Set(0, 1f);
			craftingIngredientsGrid.ListPadding = 2f;
			craftingIngredientsGrid.drawArrows = true;
			craftingPanel.Append(craftingIngredientsGrid);

			var craftingTilesGridScrollbar = new InvisibleFixedUIHorizontalScrollbar(RecipeBrowserUI.instance.userInterface);
			craftingTilesGridScrollbar.SetView(100f, 1000f);
			craftingTilesGridScrollbar.Width.Set(0, 1f);
			craftingTilesGridScrollbar.Top.Set(-20, 1f);
			//craftingPanel.Append(craftingTilesGridScrollbar);
			craftingIngredientsGrid.SetScrollbar(craftingTilesGridScrollbar);

			//tileList = new List<UITileNoSlot>();
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

				sb.Append($"[c/{Utilities.textColor.Hex3()}:{Language.GetTextValue("LegacyInterface.22")}] ");
				Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, Language.GetTextValue("LegacyInterface.22"), new Vector2((float)positionX, (float)(positionY)), Utilities.textColor, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
				int row = 0;
				int tileIndex = 0;
				bool comma = false;
				if(selectedRecipe.requiredTile.Count == 0) {
					sb.Append($"{(comma ? ", " : "")}[c/{Utilities.textColor.Hex3()}:{Language.GetTextValue("LegacyInterface.23")}]");
					sbTiles.Append($"{(comma ? ", " : "")}[c/{Utilities.textColor.Hex3()}:{Language.GetTextValue("LegacyInterface.23")}]");
					comma = true;
				}

				while (tileIndex < selectedRecipe.requiredTile.Count)
				{
					int num63 = (tileIndex + 1) * 26;
					if (selectedRecipe.requiredTile[tileIndex] == -1)
					{
						break;
					}
					else
					{
						row++;
						int tileID = selectedRecipe.requiredTile[tileIndex];
						string tileName = Utilities.GetTileName(tileID);
						//Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, tileName, new Vector2(positionX, positionY + num63), Main.LocalPlayer.adjTile[tileID] ?  yesColor : noColor, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
						DoChatTag(sb, comma, Main.LocalPlayer.adjTile[tileID], tileName);
						DoChatTag(sbTiles, comma, Main.LocalPlayer.adjTile[tileID], tileName);

						tileIndex++;
						comma = true;
					}
				}
				// white if window not open?
				int yAdjust = (row + 1) * 26;
				foreach (var condition in selectedRecipe.Conditions) {
					bool state = condition.IsMet(); //.RecipeAvailable(selectedRecipe);
					string description = condition.Description.Value;
					DoChatTag(sb, comma, state, description);
					DoChatTag(sbTiles, comma, state, description);
					yAdjust += 26;
					comma = true;
				}
				float width = Terraria.UI.Chat.ChatManager.GetStringSize(FontAssets.MouseText.Value, sbTiles.ToString(), Vector2.One).X;
				if (width > 170)
				{
					Vector2 scale = new Vector2(170 / width);
					Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, sbTiles.ToString(), new Vector2(positionX, positionY + 26), Color.White, 0f, Vector2.Zero, scale, -1f, 2f);
				}
				else
				{
					Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, sbTiles.ToString(), new Vector2(positionX, positionY + 26), Color.White, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
				}
				Rectangle rectangle = GetDimensions().ToRectangle();
				rectangle.Width = 180;
				//if (IsMouseHovering)
				if (rectangle.Contains(Main.MouseScreen.ToPoint()) && Terraria.UI.Chat.ChatManager.GetStringSize(FontAssets.MouseText.Value, sbTiles.ToString(), Vector2.One).X > 180)
				{
					Main.hoverItemName = sb.ToString();
					/* Different approach to informing recipe mod source
					ModRecipe modRecipe = selectedRecipe as ModRecipe;
					if (Terraria.UI.Chat.ChatManager.GetStringSize(FontAssets.MouseText.Value, sbTiles.ToString(), Vector2.One).X > 180)
						Main.hoverItemName = sb.ToString() + (modRecipe != null ? $"\n[{modRecipe.mod.DisplayName}]" : "");
					else if (modRecipe != null)
						Main.hoverItemName = $"[{modRecipe.mod.DisplayName}]";
					*/
				}
			}
		}

		private void DoChatTag(StringBuilder sb, bool comma, bool state, string text)
		{
			sb.Append($"{(comma ? ", " : "")}[c/{(state ? Utilities.yesColor : Utilities.noColor).Hex3()}:{text}]");
		}
	}
}