using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace RecipeBrowser.UIElements
{
	class UIIngredientSlot : UIItemSlot
	{
		public static Texture2D selectedBackgroundTexture = Main.inventoryBack15Texture;
		private int clickIndicatorTime = 0;
		private const int ClickTime = 30;

		public UIIngredientSlot(Item item) : base(item)
		{
		}

		public override void Click(UIMouseEvent evt)
		{
			base.Click(evt);
			//RecipeBrowserUI.instance.SetRecipe(index);
			RecipeBrowserUI.instance.queryLootItem = this.item;
			RecipeBrowserUI.instance.updateNeeded = true;
			clickIndicatorTime = ClickTime;
		}

		public override void DoubleClick(UIMouseEvent evt)
		{
			RecipeBrowserUI.instance.itemDescriptionFilter.SetText("");
			RecipeBrowserUI.instance.itemNameFilter.SetText("");
			RecipeBrowserUI.instance.queryItem.ReplaceWithFake(item.type);
		}

		internal override void DrawAdditionalOverlays(SpriteBatch spriteBatch, Vector2 vector2, float scale)
		{
			if(clickIndicatorTime > 0)
			{
				clickIndicatorTime--;
				spriteBatch.Draw(selectedBackgroundTexture, vector2, null, Color.White * ((float)clickIndicatorTime/ClickTime), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}
		}
	}
}
