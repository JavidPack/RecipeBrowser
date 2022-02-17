using Microsoft.Xna.Framework;
using RecipeBrowser.UIElements;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace RecipeBrowser
{
	internal class RecipeBrowserGlobalItem : GlobalItem
	{
		// OnPickup only called on LocalPlayer: I think
		public override void OnCreate(Item item, ItemCreationContext context)
		{
			ItemReceived(item);
		}

		// OnPickup only called on LocalPlayer: i == Main.myPlayer
		public override bool OnPickup(Item item, Player player)
		{
			ItemReceived(item);
			return true;
		}

		internal void ItemReceived(Item item)
		{
			RecipeBrowserUI.instance.ItemReceived(item);
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (Main.npcShop <= 0 && item == UIItemSlot.hoveredItem && SharedUI.instance.SelectedSort?.button.hoverText == "Value") {
				int storeValue = Main.HoverItem.value;
				string text = "";
				int plat = 0;
				int gold = 0;
				int silver = 0;
				int copper = 0;
				int total = storeValue * Main.HoverItem.stack;

				// convert to sell price
				total = storeValue / 5;
				if (total < 1 && storeValue > 0) {
					total = 1;
				}
				total *= Main.HoverItem.stack;

				if (total < 1) {
					total = 1;
				}
				if (total >= 1000000) {
					plat = total / 1000000;
					total -= plat * 1000000;
				}
				if (total >= 10000) {
					gold = total / 10000;
					total -= gold * 10000;
				}
				if (total >= 100) {
					silver = total / 100;
					total -= silver * 100;
				}
				if (total >= 1) {
					copper = total;
				}
				if (plat > 0) {
					text = string.Concat(text, plat, " ", Lang.inter[15].Value, " ");
				}
				if (gold > 0) {
					text = string.Concat(text, gold, " ", Lang.inter[16].Value, " ");
				}
				if (silver > 0) {
					text = string.Concat(text, silver, " ", Lang.inter[17].Value, " ");
				}
				if (copper > 0) {
					text = string.Concat(text, copper, " ", Lang.inter[18].Value, " ");
				}
				//	array[num4] = Lang.tip[49].Value + " " + text;
				//array[num4] = Lang.tip[50].Value + " " + text;

				Color color = Color.White;
				float mouseFade = Main.mouseTextColor / 255f;
				if (plat > 0) {
					color = new Color((byte)(220f * mouseFade), (byte)(220f * mouseFade), (byte)(198f * mouseFade), Main.mouseTextColor);
				}
				else if (gold > 0) {
					color = new Color((byte)(224f * mouseFade), (byte)(201f * mouseFade), (byte)(92f * mouseFade), Main.mouseTextColor);
				}
				else if (silver > 0) {
					color = new Color((byte)(181f * mouseFade), (byte)(192f * mouseFade), (byte)(193f * mouseFade), Main.mouseTextColor);
				}
				else if (copper > 0) {
					color = new Color((byte)(246f * mouseFade), (byte)(138f * mouseFade), (byte)(96f * mouseFade), Main.mouseTextColor);
				}
				var valueTooltip = new TooltipLine(Mod, "RecipeBrowserValue", Lang.tip[49].Value + " " + text);
				if (storeValue == 0) {
					valueTooltip = new TooltipLine(Mod, "RecipeBrowserValue", Lang.tip[51].Value);
					color = new Color((byte)(120f * mouseFade), (byte)(120f * mouseFade), (byte)(120f * mouseFade), Main.mouseTextColor);
				}
				valueTooltip.overrideColor = color;

				tooltips.Add(valueTooltip);
			}


			// TODO: Config option to show always
			if (RecipeCatalogueUI.instance.hoveredIndex < 0) return;

			var selectedModRecipe = Main.recipe[RecipeCatalogueUI.instance.hoveredIndex];
			if (selectedModRecipe.Mod != null && ModContent.GetInstance<RecipeBrowserClientConfig>().ShowRecipeModSource && item.type == selectedModRecipe.createItem.type)
			{
				var line = new TooltipLine(Mod, "RecipeBrowser:RecipeOriginHint", "Recipe added by " + selectedModRecipe.Mod.DisplayName)
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