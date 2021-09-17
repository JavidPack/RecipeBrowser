using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace RecipeBrowser.UIElements
{
	internal class UIIngredientSlot : UIItemSlot
	{
		public static Asset<Texture2D> selectedBackgroundTexture = TextureAssets.InventoryBack15;
		private int clickIndicatorTime = 0;
		private const int ClickTime = 30;
		private int order; // Recipe Ingredient Order

		public UIIngredientSlot(Item item, int order, float scale = 0.75f) : base(item, scale)
		{
			this.order = order;
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
			RecipeBrowserUI.instance.tabController.SetPanel(RecipeBrowserUI.RecipeCatalogue);
			if (!RecipeBrowserUI.instance.ShowRecipeBrowser)
				RecipeBrowserUI.instance.ShowRecipeBrowser = true;
			RecipeCatalogueUI.instance.itemDescriptionFilter.SetText("");
			RecipeCatalogueUI.instance.itemNameFilter.SetText("");
			RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(item.type);
		}

		internal override void DrawAdditionalOverlays(SpriteBatch spriteBatch, Vector2 vector2, float scale)
		{
			if (clickIndicatorTime > 0)
			{
				clickIndicatorTime--;
				spriteBatch.Draw(selectedBackgroundTexture.Value, vector2, null, Color.White * ((float)clickIndicatorTime / ClickTime), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}
		}

		public override int CompareTo(object obj)
		{
			UIIngredientSlot other = obj as UIIngredientSlot;
			return order.CompareTo(other.order);
		}
	}
}