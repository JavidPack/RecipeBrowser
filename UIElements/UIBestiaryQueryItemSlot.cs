using Terraria;
using Terraria.UI;

namespace RecipeBrowser.UIElements
{
	internal class UIBestiaryQueryItemSlot : UIQueryItemSlot
	{
		public UIBestiaryQueryItemSlot(Item item) : base(item)
		{
		}

		public override void Click(UIMouseEvent evt)
		{
			base.Click(evt);
			//BestiaryUI.instance.queryLootItem = (item.type == 0) ? null : item;
			ReplaceWithFake(item.type);
			BestiaryUI.instance.updateNeeded = true;
		}

		internal override void ReplaceWithFake(int type)
		{
			base.ReplaceWithFake(type);
			//BestiaryUI.instance.queryLootItem = item;
			BestiaryUI.instance.updateNeeded = true;
		}
	}
}