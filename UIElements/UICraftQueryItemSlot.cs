using Terraria;
using Terraria.UI;

namespace RecipeBrowser.UIElements
{
	class UICraftQueryItemSlot : UIQueryItemSlot
	{
		public UICraftQueryItemSlot(Item item) : base(item)
		{
		}

		public override void LeftClick(UIMouseEvent evt)
		{
			base.LeftClick(evt);
			CraftUI.instance.SetItem(item.type);
		}
	}
}