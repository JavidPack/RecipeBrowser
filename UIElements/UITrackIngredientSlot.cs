using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.UI.Chat;

namespace RecipeBrowser.UIElements
{
	internal class UITrackIngredientSlot : UIIngredientSlot
	{
		private int targetStack;

		public UITrackIngredientSlot(Item item, int targetStack) : base(item)
		{
			this.targetStack = targetStack;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			if (item != null)
			{
				CalculatedStyle dimensions = base.GetInnerDimensions();
				Rectangle rectangle = dimensions.ToRectangle();
				if (!item.IsAir)
				{
					int count = Main.LocalPlayer.CountItem(item.type, item.maxStack);
					string progress = count + "/" + targetStack;
					Color progressColor = count >= targetStack ? Color.LightGreen : Color.LightSalmon;
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, progress, dimensions.Position() + new Vector2(10f, 26f) * scale + new Vector2(-4f, 0f), progressColor, 0f, Vector2.Zero, new Vector2(scale), -1f, /*scale*/1);
				}
			}
		}
	}
}