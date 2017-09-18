using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;

namespace RecipeBrowser
{
	internal class UIHoverImageButtonMod : UIHoverImageButton
	{
		internal Texture2D texture;
		Vector2 offset = new Vector2(0, 12);

		public UIHoverImageButtonMod(Texture2D texture, string hoverText) : base(texture, hoverText)
		{
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			if (IsMouseHovering && texture != null)
			{
				Rectangle hitbox = GetInnerDimensions().ToRectangle();
				spriteBatch.Draw(texture, new Vector2(hitbox.X + hitbox.Width/2 - 40, hitbox.Y - 80), Color.White);
			}
		}
	}
}
