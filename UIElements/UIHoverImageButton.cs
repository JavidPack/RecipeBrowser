using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;

namespace RecipeBrowser
{
	internal class UIHoverImageButton : UIImageButton
	{
		internal string hoverText;

		public UIHoverImageButton(Texture2D texture, string hoverText) : base(texture)
		{
			this.hoverText = hoverText;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			if (IsMouseHovering)
			{
				Main.hoverItemName = hoverText;
			//	Main.toolTip = new Item();
			//	Main.toolTip.name = hoverText;
			}
		}
	}
}
