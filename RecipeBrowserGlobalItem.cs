using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace RecipeBrowser
{
	internal class RecipeBrowserGlobalItem : GlobalItem
	{
		// OnPickup only called on LocalPlayer: I think
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

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			// TODO: Config option to show always
			if (RecipeCatalogueUI.instance.hoveredIndex < 0) return;

			var selectedModRecipe = Main.recipe[RecipeCatalogueUI.instance.hoveredIndex] as ModRecipe;
			if (selectedModRecipe != null && item.IsTheSameAs(selectedModRecipe.createItem))
			{
				var line = new TooltipLine(mod, "RecipeBrowser:RecipeOriginHint", "Recipe added by " + selectedModRecipe.mod.DisplayName)
				{
					overrideColor = Color.Goldenrod
				};
				int index = tooltips.FindIndex(x => x.Name == "ItemName");
				if (index == -1)
					tooltips.Add(line);
				else
					tooltips.Insert(index + 1, line);
			}
		}
	}
}