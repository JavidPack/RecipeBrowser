using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace RecipeBrowser.UIElements
{
	internal class UITrackIngredientSlot : UIIngredientSlot
	{
		private int targetStack;
		private int owner;
		private Recipe recipe;

		public UITrackIngredientSlot(Item item, int targetStack, Recipe recipe, int order, int owner, float scale = 0.75f) : base(item, order, scale)
		{
			this.targetStack = targetStack;
			this.recipe = recipe;
			this.owner = owner;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			if (item != null)
			{
				CalculatedStyle dimensions = base.GetInnerDimensions();
				Rectangle rectangle = dimensions.ToRectangle();
				if (!item.IsAir)
				{
					if (IsMouseHovering)
						if (Main.keyState.IsKeyDown(Main.FavoriteKey))
							if (Main.drawingPlayerChat)
								Main.cursorOverride = 2;
							else
								Main.cursorOverride = 3;

					int count = CountItemGroups(Main.player[owner], recipe, item.type, targetStack > 999 ? targetStack : 999 ); // stopping at item.maxStack means you can't see if you can make multiple.
					string progress = count + "/" + targetStack;
					Color progressColor = count >= targetStack ? Color.LightGreen : Color.LightSalmon;
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, progress, dimensions.Position() + new Vector2(10f, 26f) * scale + new Vector2(-4f, 0f), progressColor, 0f, Vector2.Zero, new Vector2(scale), -1f, /*scale*/1);
				}
			}
		}

		// Like Player.CountItem, except caps at stopCountingAt exactly and counts items that satisfy recipe as well.
		public int CountItemGroups(Player player, Recipe recipe, int type, int stopCountingAt = 1)
		{
			int count = 0;
			if (type == 0)
			{
				return 0;
			}

			count += CountItemsInInventory(player.inventory.Take(58), recipe, type);
			
			if (player.useVoidBag() && player.chest != -5)
				count += CountItemsInInventory(player.bank4.item.Take(40), recipe, type);

			if (player.chest != -1)
			{
				var currentInventory = player.chest switch
				{
					> -1 => Main.chest[player.chest].item,
					-2 => player.bank.item,
					-3 => player.bank2.item,
					-4 => player.bank3.item,
					-5 => player.bank4.item,
					_ => null
				};

				if (currentInventory != null)
					count += CountItemsInInventory(currentInventory.Take(40), recipe, type);
			}

			foreach (var (items, _) in PlayerLoader.GetModdedCraftingMaterials(Main.LocalPlayer))
			{
				count += CountItemsInInventory(items, recipe, type);
			}
			
			return count >= stopCountingAt ? stopCountingAt : count;
		}

		private int CountItemsInInventory(IEnumerable<Item> items, Recipe recipe, int type)
		{
			int count = 0;
			foreach (var currentItem in from i in items where !i.IsAir select i)
			{
				if (recipe.AcceptedByItemGroups(currentItem.type, item.type))
				{
					count += currentItem.stack;
				}
				else if (currentItem.type == type)
				{
					count += currentItem.stack;
				}
			}

			return count;
		}

		public override void LeftClick(UIMouseEvent evt) {
			base.LeftClick(evt);

			if (Main.keyState.IsKeyDown(Main.FavoriteKey)) {
				if (Main.drawingPlayerChat) {
					if (ChatManager.AddChatText(FontAssets.MouseText.Value, ItemTagHandler.GenerateTag(item), Vector2.One)) {
						SoundEngine.PlaySound(SoundID.MenuTick);
					}
				}
				else {
					bool found = false;
					for (int i = 0; i < Recipe.numRecipes; i++) {
						Recipe r = Main.recipe[i];
						if (r.createItem.type == itemType) {
							RecipeBrowserUI.instance.FavoriteChange(i, true);
							found = true;
						}
					}
					if (!found) {
						Main.NewText("No recipe found for " + ItemTagHandler.GenerateTag(item));
					}
				}
			}
		}
	}
}