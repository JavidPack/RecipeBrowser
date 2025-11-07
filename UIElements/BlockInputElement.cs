using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace RecipeBrowser.UIElements
{
	internal class BlockInputElement : UIElement
	{
		private UIElement elementToBlock;
		private int top;

		public BlockInputElement(UIElement elementToBlock, int top) {
			Width.Set(0, 1);
			Height.Set(0, 1);
			//Height.Set(-top, 1);
			//Top.Set(top, 0);

			this.top = top;
			this.elementToBlock = elementToBlock;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			var drawArea = elementToBlock.GetDimensions().ToRectangle();
			drawArea.Y += top;
			drawArea.Height -= top;
			//spriteBatch.Draw(TextureAssets.MagicPixel.Value, drawArea, Color.Black * 0.5f);
			Utils.DrawInvBG(spriteBatch, drawArea, Color.Black * 0.5f);
		}
	}
}
