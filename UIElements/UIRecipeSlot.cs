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
	class UIRecipeSlot : UIItemSlot
	{
		public static Texture2D selectedBackgroundTexture = Main.inventoryBack15Texture;
		public static Texture2D recentlyDiscoveredBackgroundTexture = Main.inventoryBack10Texture;
		public int index;
		public bool recentlyDiscovered;

		public UIRecipeSlot(int index) : base(Main.recipe[index].createItem)
		{
			this.index = index;
		}

		public override void Click(UIMouseEvent evt)
		{
			RecipeBrowserUI.instance.SetRecipe(index);
		}

		public override void DoubleClick(UIMouseEvent evt)
		{
			RecipeBrowserUI.instance.itemDescriptionFilter.SetText("");
			RecipeBrowserUI.instance.itemNameFilter.SetText("");
			RecipeBrowserUI.instance.queryItem.ReplaceWithFake(item.type);
		}

		public override int CompareTo(object obj)
		{
			UIRecipeSlot other = obj as UIRecipeSlot;
			if (recentlyDiscovered && !other.recentlyDiscovered)
			{
				return -1;
			}
			if (!recentlyDiscovered && other.recentlyDiscovered)
			{
				return 1;
			}
			return index.CompareTo(other.index);
		}
	}
}
