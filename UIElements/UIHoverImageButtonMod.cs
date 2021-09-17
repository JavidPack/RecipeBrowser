using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace RecipeBrowser
{
	internal class UIHoverImageButtonMod : UIHoverImageButton
	{
		internal Texture2D texture;
		private Vector2 offset = new Vector2(0, 12);

		public UIHoverImageButtonMod(Asset<Texture2D> texture, string hoverText) : base(texture, hoverText)
		{
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			if (IsMouseHovering && texture != null)
			{
				Rectangle hitbox = GetInnerDimensions().ToRectangle();
				spriteBatch.Draw(texture, new Vector2(hitbox.X + hitbox.Width / 2 - 40, hitbox.Y - 80), Color.White);
			}
		}
	}
}