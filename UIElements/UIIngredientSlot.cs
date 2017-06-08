using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.UI;

namespace RecipeBrowser.UIElements
{
	class UIIngredientSlot : UIItemSlot
	{
		public static Texture2D selectedBackgroundTexture = Main.inventoryBack15Texture;

		public UIIngredientSlot(Item item) : base(item)
		{
		}

		public override void Click(UIMouseEvent evt)
		{
			//RecipeBrowserUI.instance.SetRecipe(index);
		}

		public override void DoubleClick(UIMouseEvent evt)
		{
			RecipeBrowserUI.instance.itemDescriptionFilter.SetText("");
			RecipeBrowserUI.instance.itemNameFilter.SetText("");
			RecipeBrowserUI.instance.queryItem.ReplaceWithFake(item.type);
		}
	}
}
