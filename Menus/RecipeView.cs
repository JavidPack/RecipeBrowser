using RecipeBrowser.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace RecipeBrowser.Menus
{
	internal class RecipeView : UIScrollView
	{
		private float spacing = 8f;

		public RecipeSlot[] allRecipeSlot;

		private int[] _selectedCategory;

		public int[] activeSlots;

		private int slotSpace = 4;

		private int slotColumns = 8;

		private float slotSize = (float)Slot.backgroundTexture.Width * 0.85f;

		private int slotRows = 6;

		public int[] selectedCategory
		{
			get
			{
				return this._selectedCategory;
			}
			set
			{
				List<int> list = value.ToList<int>();
				for (int i = 0; i < list.Count; i++)
				{
					//NPCSlot slot = this.allNPCSlot[list[i]];
					//if (slot.npcType == 0)
					//{
					//	list.RemoveAt(i);
					//	i--;
					//}
				}
				this._selectedCategory = list.ToArray();
			}
		}

		public RecipeView()
		{
			base.Width = (this.slotSize + (float)this.slotSpace) * (float)this.slotColumns + (float)this.slotSpace + 20f;
			base.Height = 200f;
			this.allRecipeSlot = new RecipeSlot[Recipe.numRecipes];
			for (int i = 0; i < this.allRecipeSlot.Length; i++)
			{
				this.allRecipeSlot[i] = new RecipeSlot(i);
			}
		//	this.allNPCSlot = (from s in this.allNPCSlot
		//					   select s).ToArray<NPCSlot>();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
		}

		public void ReorderSlots()
		{
			int type = 0;
			List<int> groups = new List<int>();
			if (RecipeBrowserWindow.lookupItemSlot.item.stack > 0)
			{
				type = RecipeBrowserWindow.lookupItemSlot.item.type;

				foreach (var group in RecipeGroup.recipeGroups)
				{
					if (group.Value.ValidItems.Contains(type))
					{
						groups.Add(group.Key);
					}
				}
			}
			//foreach (var item in groups)
			//{
			//	Main.NewText("Group " + item);
			//}

			base.ScrollPosition = 0f;
			base.ClearContent();
			int slotNum = 0;
			for (int i = 0; i < this.activeSlots.Length; i++)
			{
				if (type != 0)
				{
					Recipe curRecipe = allRecipeSlot[activeSlots[i]].recipe;
					//Main.NewText("Recipe " + i);
					// if my item is in a recipe group, i should add that to required items.
					bool inGroup = allRecipeSlot[activeSlots[i]].recipe.acceptedGroups.Intersect(groups).Any();

					inGroup |= curRecipe.useWood(type,type) || curRecipe.useSand(type,type) || curRecipe.useFragment(type,type) || curRecipe.useIronBar(type,type) || curRecipe.usePressurePlate(type,type);

					//if (inGroup)
					//{
					//	foreach (var item in allRecipeSlot[activeSlots[i]].recipe.acceptedGroups.Intersect(groups))
					//	{
					//		Main.NewText(i + " " + item.ToString());
					//	}
					//}

					if (!inGroup)
					{
					// contine if neither result or ingredients contains item
					if(!(allRecipeSlot[activeSlots[i]].recipe.createItem.type == type || allRecipeSlot[activeSlots[i]].recipe.requiredItem.Any(ing=>ing.type == type)))
					{
						continue;
						}
					}
				}
				//int num = i;
				RecipeSlot slot = this.allRecipeSlot[this.activeSlots[i]];
				int num2 = slotNum % this.slotColumns;
				int num3 = slotNum / this.slotColumns;
				float x = (float)this.slotSpace + (float)num2 * (slot.Width + (float)this.slotSpace);
				float y = (float)this.slotSpace + (float)num3 * (slot.Height + (float)this.slotSpace);
				slot.Position = new Vector2(x, y);
				slot.Offset = Vector2.Zero;
				this.AddChild(slot);
				slotNum++;
			}
			base.ContentHeight = base.GetLastChild().Y + base.GetLastChild().Height + this.spacing;
		}
	}
}
