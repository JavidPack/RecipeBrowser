using Terraria;
using Terraria.UI;

namespace RecipeBrowser.UIElements
{
	class UICraftQueryItemSlot : UIQueryItemSlot
	{
		public UICraftQueryItemSlot(Item item) : base(item)
		{
		}

		public override void Click(UIMouseEvent evt)
		{
			base.Click(evt);
			CraftUI.instance.SetItem(item.type);
		}
	}
}