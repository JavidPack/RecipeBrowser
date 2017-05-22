using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace RecipeBrowser.UI
{
	internal class UIScrollBar : UIView
	{
		private static Texture2D ScrollbarTexture = Terraria.ModLoader.ModLoader.GetTexture("RecipeBrowser/UI/Images.UIKit.scrollbarEdge");//UIView.GetEmbeddedTexture("Images.UIKit.scrollbarEdge.png");

		private static Texture2D scrollbarFill;

		private float height = 100f;

		private static Texture2D ScrollbarFill
		{
			get
			{
				if (UIScrollBar.scrollbarFill == null)
				{
					Color[] array = new Color[UIScrollBar.ScrollbarTexture.Width * UIScrollBar.ScrollbarTexture.Height];
					UIScrollBar.ScrollbarTexture.GetData<Color>(array);
					Color[] array2 = new Color[UIScrollBar.ScrollbarTexture.Width];
					for (int i = 0; i < array2.Length; i++)
					{
						array2[i] = array[i + (UIScrollBar.ScrollbarTexture.Height - 1) * UIScrollBar.ScrollbarTexture.Width];
					}
					UIScrollBar.scrollbarFill = new Texture2D(UIView.graphics, array2.Length, 1);
					UIScrollBar.scrollbarFill.SetData<Color>(array2);
				}
				return UIScrollBar.scrollbarFill;
			}
		}

		protected override float GetHeight()
		{
			return this.height;
		}

		protected override void SetHeight(float height)
		{
			this.height = height;
		}

		protected override float GetWidth()
		{
			return (float)UIScrollBar.ScrollbarTexture.Width;
		}

		private void DrawScrollBar(SpriteBatch spriteBatch)
		{
			float num = base.Height - (float)(UIScrollBar.ScrollbarTexture.Height * 2);
			Vector2 drawPosition = base.DrawPosition;
			spriteBatch.Draw(UIScrollBar.ScrollbarTexture, drawPosition, null, Color.White, 0f, base.Origin, 1f, SpriteEffects.None, 0f);
			drawPosition.Y += (float)UIScrollBar.ScrollbarTexture.Height;
			spriteBatch.Draw(UIScrollBar.ScrollbarFill, drawPosition - base.Origin, null, Color.White, 0f, Vector2.Zero, new Vector2(1f, num), SpriteEffects.None, 0f);
			drawPosition.Y += num;
			spriteBatch.Draw(UIScrollBar.ScrollbarTexture, drawPosition, null, Color.White, 0f, base.Origin, 1f, SpriteEffects.FlipVertically, 0f);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			this.DrawScrollBar(spriteBatch);
			base.Draw(spriteBatch);
		}
	}
}
