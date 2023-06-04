using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.ID;
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
			for (int i = 0; i <= 58; i++)
			{
				if (!player.inventory[i].IsAir)
				{
					int current = player.inventory[i].type;
					if (recipe.AcceptedByItemGroups(current, item.type))
					{
						count += player.inventory[i].stack;
					}
					else if (current == type)
					{
						count += player.inventory[i].stack;
					}
					//if (count >= stopCountingAt)
					//{
					//	return stopCountingAt;
					//}
				}
			}
			return count >= stopCountingAt ? stopCountingAt : count;
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