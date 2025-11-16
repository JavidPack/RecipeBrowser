using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;

namespace RecipeBrowser
{
	internal class UIHoverImageButton : UIImageButton
	{
		internal string hoverText;

		public UIHoverImageButton(Asset<Texture2D> texture, string hoverText) : base(texture)
		{
			this.hoverText = hoverText;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			if (IsMouseHovering)
			{
				// Main.hoverItemName = hoverText;
				if (!string.IsNullOrWhiteSpace(hoverText))
					Terraria.ModLoader.UI.UICommon.TooltipMouseText(hoverText);
				//	Main.toolTip = new Item();
				//	Main.toolTip.name = hoverText;
			}
		}
	}
}