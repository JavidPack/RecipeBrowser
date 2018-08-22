using Terraria;
using Terraria.UI;

namespace RecipeBrowser.UIElements
{
	internal class UIRecipeCatalogueQueryItemSlot : UIQueryItemSlot
	{
		public UIRecipeCatalogueQueryItemSlot(Item item) : base(item)
		{
		}

		public override void Click(UIMouseEvent evt)
		{
			base.Click(evt);
			RecipeCatalogueUI.instance.queryLootItem = (item.type == 0) ? null : item;
			RecipeCatalogueUI.instance.updateNeeded = true;
			SharedUI.instance.SelectedCategory = SharedUI.instance.categories[0];
		}

		internal override void ReplaceWithFake(int type)
		{
			base.ReplaceWithFake(type);
			RecipeCatalogueUI.instance.queryLootItem = item;
			RecipeCatalogueUI.instance.updateNeeded = true;
			RecipeCatalogueUI.instance.Tile = -1;
			RecipeCatalogueUI.instance.TileLookupRadioButton.Selected = false;
			SharedUI.instance.SelectedCategory = SharedUI.instance.categories[0];
		}
	}
}