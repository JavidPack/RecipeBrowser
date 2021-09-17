using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
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
	}
}