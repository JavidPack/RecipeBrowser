using Terraria;
using Terraria.ModLoader;

namespace RecipeBrowser
{
	class RecipeBrowserGlobalItem : GlobalItem
	{
		// OnPIckup only called on LocalPlayer: I think
		public override void OnCraft(Item item, Recipe recipe)
		{
			ItemReceived(item);
		}

		// OnPIckup only called on LocalPlayer: i == Main.myPlayer
		public override bool OnPickup(Item item, Player player)
		{
			ItemReceived(item);
			return true;
		}

		internal void ItemReceived(Item item)
		{
			RecipeBrowserUI.instance.ItemReceived(item);
		}
	}
}
