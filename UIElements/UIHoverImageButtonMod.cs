using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.UI;

namespace RecipeBrowser
{
	internal class UIHoverImageButtonMod : UIHoverImageButton
	{
		internal Texture2D texture;
		private Asset<Texture2D> textureColorable;
		private Vector2 offset = new Vector2(0, 12);

		public UIHoverImageButtonMod(Asset<Texture2D> texture, Asset<Texture2D> textureColorable, string hoverText) : base(texture, hoverText)
		{
			this.textureColorable = textureColorable;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{ 
			if (RecipeBrowserUI.modIndex != 0) {
				CalculatedStyle dimensions = GetDimensions();
				spriteBatch.Draw(textureColorable.Value, dimensions.Position(), Main.DiscoColor);
			}
			else {
				base.DrawSelf(spriteBatch);
			}
			if (IsMouseHovering && texture != null)
			{
				Rectangle hitbox = GetInnerDimensions().ToRectangle();
				spriteBatch.Draw(texture, new Vector2(hitbox.X + hitbox.Width / 2 - 40, hitbox.Y - 80), Color.White);
			}
		}
	}
}