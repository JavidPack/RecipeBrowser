using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Graphics;
using Terraria.UI;
using Terraria;
using Terraria.GameContent.UI.Elements;

namespace RecipeBrowser.UIElements
{
	public class UIBottomlessPanel : UIPanel
	{
		private static int CORNER_SIZE = 12;
		private static int BAR_SIZE = 4;
		private static Texture2D _borderTexture;
		private static Texture2D _backgroundTexture;

		public UIBottomlessPanel()
		{
			if (UIBottomlessPanel._borderTexture == null)
			{
				UIBottomlessPanel._borderTexture = TextureManager.Load("Images/UI/PanelBorder");
			}
			if (UIBottomlessPanel._backgroundTexture == null)
			{
				UIBottomlessPanel._backgroundTexture = TextureManager.Load("Images/UI/PanelBackground");
			}
			base.SetPadding((float)UIBottomlessPanel.CORNER_SIZE);
		}

		private void DrawPanel(SpriteBatch spriteBatch, Texture2D texture, Color color)
		{
			CalculatedStyle dimensions = base.GetDimensions();
			Point point = new Point((int)dimensions.X, (int)dimensions.Y);
			Point point2 = new Point(point.X + (int)dimensions.Width - UIBottomlessPanel.CORNER_SIZE, point.Y + (int)dimensions.Height);
			int width = point2.X - point.X - UIBottomlessPanel.CORNER_SIZE;
			int height = point2.Y - point.Y - UIBottomlessPanel.CORNER_SIZE;
			spriteBatch.Draw(texture, new Rectangle(point.X, point.Y, UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.CORNER_SIZE), new Rectangle?(new Rectangle(0, 0, UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.CORNER_SIZE)), color);
			spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y, UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.CORNER_SIZE), new Rectangle?(new Rectangle(UIBottomlessPanel.CORNER_SIZE + UIBottomlessPanel.BAR_SIZE, 0, UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.CORNER_SIZE)), color);
		//	spriteBatch.Draw(texture, new Rectangle(point.X, point2.Y, UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.CORNER_SIZE), new Rectangle?(new Rectangle(0, UIBottomlessPanel.CORNER_SIZE + UIBottomlessPanel.BAR_SIZE, UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.CORNER_SIZE)), color);
		//	spriteBatch.Draw(texture, new Rectangle(point2.X, point2.Y, UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.CORNER_SIZE), new Rectangle?(new Rectangle(UIBottomlessPanel.CORNER_SIZE + UIBottomlessPanel.BAR_SIZE, UIBottomlessPanel.CORNER_SIZE + UIBottomlessPanel.BAR_SIZE, UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.CORNER_SIZE)), color);
			spriteBatch.Draw(texture, new Rectangle(point.X + UIBottomlessPanel.CORNER_SIZE, point.Y, width, UIBottomlessPanel.CORNER_SIZE), new Rectangle?(new Rectangle(UIBottomlessPanel.CORNER_SIZE, 0, UIBottomlessPanel.BAR_SIZE, UIBottomlessPanel.CORNER_SIZE)), color);
		//	spriteBatch.Draw(texture, new Rectangle(point.X + UIBottomlessPanel.CORNER_SIZE, point2.Y, width, UIBottomlessPanel.CORNER_SIZE), new Rectangle?(new Rectangle(UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.CORNER_SIZE + UIBottomlessPanel.BAR_SIZE, UIBottomlessPanel.BAR_SIZE, UIBottomlessPanel.CORNER_SIZE)), color);
			spriteBatch.Draw(texture, new Rectangle(point.X, point.Y + UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.CORNER_SIZE, height), new Rectangle?(new Rectangle(0, UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.BAR_SIZE)), color);
			spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y + UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.CORNER_SIZE, height), new Rectangle?(new Rectangle(UIBottomlessPanel.CORNER_SIZE + UIBottomlessPanel.BAR_SIZE, UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.BAR_SIZE)), color);
			spriteBatch.Draw(texture, new Rectangle(point.X + UIBottomlessPanel.CORNER_SIZE, point.Y + UIBottomlessPanel.CORNER_SIZE, width, height), new Rectangle?(new Rectangle(UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.CORNER_SIZE, UIBottomlessPanel.BAR_SIZE, UIBottomlessPanel.BAR_SIZE)), color);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			this.DrawPanel(spriteBatch, UIBottomlessPanel._backgroundTexture, this.BackgroundColor);
			this.DrawPanel(spriteBatch, UIBottomlessPanel._borderTexture, this.BorderColor);

			//Rectangle hitbox = GetInnerDimensions().ToRectangle();
			//Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.Red * 0.6f);
		}
	}
}