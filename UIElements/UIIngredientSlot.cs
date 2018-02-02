using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace RecipeBrowser.UIElements
{
	internal class UIIngredientSlot : UIItemSlot
	{
		public static Texture2D selectedBackgroundTexture = Main.inventoryBack15Texture;
		private int clickIndicatorTime = 0;
		private const int ClickTime = 30;

		public UIIngredientSlot(Item item, float scale = 0.75f) : base(item, scale)
		{
		}

		public override void Click(UIMouseEvent evt)
		{
			base.Click(evt);
			//RecipeBrowserUI.instance.SetRecipe(index);
			RecipeCatalogueUI.instance.queryLootItem = this.item;
			RecipeCatalogueUI.instance.updateNeeded = true;
			clickIndicatorTime = ClickTime;
		}

		public override void DoubleClick(UIMouseEvent evt)
		{
			RecipeCatalogueUI.instance.itemDescriptionFilter.SetText("");
			RecipeCatalogueUI.instance.itemNameFilter.SetText("");
			RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(item.type);
		}

		internal override void DrawAdditionalOverlays(SpriteBatch spriteBatch, Vector2 vector2, float scale)
		{
			if (clickIndicatorTime > 0)
			{
				clickIndicatorTime--;
				spriteBatch.Draw(selectedBackgroundTexture, vector2, null, Color.White * ((float)clickIndicatorTime / ClickTime), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}
		}
	}
}