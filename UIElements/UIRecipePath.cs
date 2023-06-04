using Microsoft.Xna.Framework;
using RecipeBrowser.TagHandlers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;

namespace RecipeBrowser.UIElements
{
	class UIRecipePath : UIPanel
	{
		private CraftPath path;
		const int verticalSpace = 24;
		const int HorizontalTab = 20;

		// Using Chat Tags is cool, but I can't assign individual code to them.
		// They are first and foremost just Text, made to look like things.
		// Word Wrap is possible with UIMessageBox code, but scrollbar is needed? With UIGrid, I could probably do this better
		public UIRecipePath(CraftPath path)
		{
			this.path = path;

			SetPadding(6);
			Width.Set(0, 1f);
			int top = 0;
			// Craftable
			// Total Money Cost
			var totalItemCost = new Dictionary<int, int>();
			// Does all the other lines.
			int count = Traverse(path.root, 0, ref top, totalItemCost);

			var neededTiles = new HashSet<int>(path.root.GetAllChildrenPreOrder().OfType<CraftPath.RecipeNode>().SelectMany(x => x.recipe.requiredTile));
			neededTiles.Remove(-1);
			var needWater = path.root.GetAllChildrenPreOrder().OfType<CraftPath.RecipeNode>().Any(x => x.recipe.HasCondition(Condition.NearWater));
			var needHoney = path.root.GetAllChildrenPreOrder().OfType<CraftPath.RecipeNode>().Any(x => x.recipe.HasCondition(Condition.NearHoney));
			var needLava = path.root.GetAllChildrenPreOrder().OfType<CraftPath.RecipeNode>().Any(x => x.recipe.HasCondition(Condition.NearLava));

			var missingTiles = neededTiles.Where(x => !Main.LocalPlayer.adjTile[x]);

			StringBuilder sb = new StringBuilder();
			sb.Append("Cost: ");
			foreach (var data in totalItemCost)
			{
				sb.Append(ItemHoverFixTagHandler.GenerateTag(data.Key, data.Value));
			}

			// Maybe have a summary tiles needed if Full Craft implemented

			var drawTextSnippets = UIMessageBox.WordwrapStringSmart(sb.ToString(), Color.White, FontAssets.MouseText.Value, 300, -1);
			foreach (var textSnippet in drawTextSnippets)
			{
				string s = string.Concat(textSnippet.Select(x => x.TextOriginal));
				UITextSnippet snippet = new UITextSnippet(s);
				snippet.Top.Set(top, 0);
				snippet.Left.Set(0, 0);
				Append(snippet);
				top += verticalSpace;
				count++;
			}

			Height.Set(count * verticalSpace + PaddingBottom + PaddingTop, 0f);

			// TODO: Full Craft Button
			//var craftButton = new UITextPanel<string>("Craft");
			//craftButton.Top.Set(-38, 1f);
			//craftButton.Left.Set(-63, 1f);
			//craftButton.OnClick += CraftButton_OnClick;
			//Append(craftButton);
		}

		private int Traverse(CraftPath.CraftPathNode node, int left, ref int top, Dictionary<int, int> totalItemCost)
		{
			int count = 1;

			StringBuilder sb = new StringBuilder();
			var recipeNode = node as CraftPath.RecipeNode;
			if (recipeNode != null)
			{
				sb.Append(ItemHoverFixTagHandler.GenerateTag(recipeNode.recipe.createItem.type, recipeNode.recipe.createItem.stack * recipeNode.multiplier));
				sb.Append('<');
				for (int i = 0; i < recipeNode.recipe.requiredItem.Count; i++)
				{
					Item item = recipeNode.recipe.requiredItem[i];
					bool check = recipeNode.children[i] is CraftPath.HaveItemNode;

					string nameOverride = RecipeCatalogueUI.OverrideForGroups(recipeNode.recipe, item.type);
					sb.Append(ItemHoverFixTagHandler.GenerateTag(item.type, item.stack * recipeNode.multiplier, nameOverride, check));
				}
			}
			else
			{
				if (node is CraftPath.HaveItemNode)
				{
					count--;
				}
				else
				{
					sb.Append(node.ToUITextString());
				}
			}

			var haveItemNode = node as CraftPath.HaveItemNode;
			if (haveItemNode != null)
			{
				totalItemCost.Adjust(haveItemNode.itemid, haveItemNode.stack);
			}
			var haveItemsNode = node as CraftPath.HaveItemsNode;
			if (haveItemsNode != null)
			{
				foreach (var item in haveItemsNode.listOfItems)
				{
					totalItemCost.Adjust(item.Item1, item.Item2);
				}
			}

			if (sb.Length > 0)
			{
				var snippet = new UITextSnippet(sb.ToString());
				snippet.Top.Set(top, 0);
				snippet.Left.Set(left, 0);
				Append(snippet);

				if (recipeNode != null)
				{
					var neededTiles = new HashSet<int>(recipeNode.recipe.requiredTile);
					neededTiles.Remove(-1);
					var needWater = recipeNode.recipe.HasCondition(Condition.NearWater);
					var needHoney = recipeNode.recipe.HasCondition(Condition.NearHoney);
					var needLava = recipeNode.recipe.HasCondition(Condition.NearLava);

					UIRecipeInfoRightAligned simpleRecipeInfo = new UIRecipeInfoRightAligned(neededTiles.ToList(), needWater, needHoney, needLava);
					simpleRecipeInfo.Top.Set(top, 0);
					simpleRecipeInfo.Left.Set(-30, 1f);
					Append(simpleRecipeInfo);

					UICraftButton craftButton = new UICraftButton(recipeNode, recipeNode.recipe);
					craftButton.Top.Set(top, 0);
					craftButton.Left.Set(-26, 1f);
					Append(craftButton);
				}
				top += verticalSpace;
			}

			if (node.children != null)
				foreach (var child in node.children)
				{
					if (child != null)
						count += Traverse(child, left + HorizontalTab, ref top, totalItemCost);
				}
			return count;
		}
	}
}
