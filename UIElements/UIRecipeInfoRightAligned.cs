using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace RecipeBrowser
{
	internal class UIRecipeInfoRightAligned : UIElement
	{
		Recipe recipe;
		List<int> tiles;
		bool needWater;
		bool needHoney;
		bool needLava;
		public UIRecipeInfoRightAligned(Recipe recipe, List<int> tiles, bool needWater, bool needHoney, bool needLava)
		{
			this.recipe = recipe;
			this.tiles = tiles;
			this.needWater = needWater;
			this.needHoney = needHoney;
			this.needLava = needLava;
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			foreach (var tile in tiles)
			{
				if (!Utilities.tileTextures.ContainsKey(tile))
				{
					Utilities.GenerateTileTexture(tile);
				}
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (CraftUI.instance.recipeResultItemSlot.item.IsAir) return;

			CalculatedStyle innerDimensions = GetInnerDimensions();
			Vector2 pos = innerDimensions.Position();

			float positionX = pos.X;
			float positionY = pos.Y;
			StringBuilder sbTiles = new StringBuilder();

			bool comma = false;

			foreach (var condition in recipe.Conditions) {
				bool state = condition.IsMet();
				string description = condition.Description.Value;
				DoChatTag(sbTiles, comma, state, description);
				comma = true;
				// Idea: Instead of chat tag, make icons for each condition instead?
			}
			/*
			if (needWater)
			{
				DoChatTag(sbTiles, comma, Main.LocalPlayer.adjWater, Language.GetTextValue("LegacyInterface.53"));
				comma = true;
			}
			if (needHoney)
			{
				DoChatTag(sbTiles, comma, Main.LocalPlayer.adjHoney, Language.GetTextValue("LegacyInterface.58"));
				comma = true;
			}
			if (needLava)
			{
				DoChatTag(sbTiles, comma, Main.LocalPlayer.adjLava, Language.GetTextValue("LegacyInterface.56"));
				comma = true;
			}
			*/
			string message = sbTiles.ToString();
			float stringWidth = ChatManager.GetStringSize(FontAssets.MouseText.Value, message, Vector2.One).X;
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, message, new Vector2(positionX - stringWidth, positionY), Color.White, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
			stringWidth += 2;
			int drawNumber = 0;
			for (int i = 0; i < tiles.Count; i++)
			{
				int tile = tiles[i];
				//if (!Main.LocalPlayer.adjTile[tile]) // Show all, use ✓, X, and ?
				{
					Texture2D texture;
					Utilities.tileTextures.TryGetValue(tile, out texture);
					if (texture != null)
					{
						drawNumber++;
						int height = texture.Height;
						int width = texture.Width;
						float drawScale = 1f;
						float availableWidth = 22;
						if (width > availableWidth || height > availableWidth)
						{
							if (width > height)
							{
								drawScale = availableWidth / width;
							}
							else
							{
								drawScale = availableWidth / height;
							}
						}

						//spriteBatch.Draw(Main.magicPixel, new Rectangle((int)(positionX - stringWidth - tiles.Count * 24 + i * 24), (int)positionY, 22, 22), Color.Red * 0.6f);
						spriteBatch.Draw(texture, new Vector2(positionX - stringWidth - drawNumber * 24 + 11, positionY + 11), null, Color.White, 0f, texture.Size() * 0.5f, drawScale, SpriteEffects.None, 0f);
						ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, Main.LocalPlayer.adjTile[tile] ? "✓" : RecipeBrowserPlayer.seenTiles[tile] ? "X" : "?", new Vector2(positionX - stringWidth - drawNumber * 24, positionY) + new Vector2(14f, 10f), Main.LocalPlayer.adjTile[tile] ? Utilities.yesColor : RecipeBrowserPlayer.seenTiles[tile] ? Utilities.maybeColor : Utilities.noColor, 0f, Vector2.Zero, new Vector2(0.7f));

						Rectangle rectangle = new Rectangle((int)(positionX - stringWidth - drawNumber * 24), (int)(positionY), 22, 22);
						if (rectangle.Contains(Main.MouseScreen.ToPoint()))
						{
							string tileName = Utilities.GetTileName(tile);
							Main.hoverItemName = $"[c/{(Main.LocalPlayer.adjTile[tile] ? Utilities.yesColor : RecipeBrowserPlayer.seenTiles[tile] ? Utilities.maybeColor : Utilities.noColor).Hex3()}:{(Main.LocalPlayer.adjTile[tile] ? "" : RecipeBrowserPlayer.seenTiles[tile] ? "Missing " : "Unseen ")}{tileName}]";
						}
					}
				}
			}
			// TODO: RecipeAvailable Icon or something.
		}

		private void DoChatTag(StringBuilder sb, bool comma, bool state, string text)
		{
			sb.Append($"{(comma ? ", " : "")}[c/{(state ? Utilities.yesColor : Utilities.noColor).Hex3()}:{text}]");
		}
	}
}