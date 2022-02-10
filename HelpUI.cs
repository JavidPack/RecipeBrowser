using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;

namespace RecipeBrowser
{
	internal class HelpUIPanel : UIPanel
	{
		public override void Recalculate()
		{
			//if (ItemCatalogueUI.instance.sortsAndFiltersPanelGrid != null)
			//{
			//	var size = 12 + ItemCatalogueUI.instance.sortsAndFiltersPanelGrid.GetTotalHeight();
			//	ItemCatalogueUI.instance.sortsAndFiltersPanel.Height.Set(size, 0f);
			//	ItemCatalogueUI.instance.itemGridPanel.Top.Set(size, 0f);
			//	ItemCatalogueUI.instance.itemGridPanel.Height.Set(-size - 16, 1f);
			//}
			base.Recalculate();
		}
		public override void RecalculateChildren()
		{
			HelpUI.instance.message.heightNeedsRecalculating = true;
			base.RecalculateChildren();
		}
	}

	class HelpUI
	{
		internal static HelpUI instance;
		internal static Color color = Color.LightPink;

		internal UIPanel mainPanel;
		internal UIMessageBox message;

		public HelpUI()
		{
			instance = this;
		}

		internal UIElement CreateHelpPanel()
		{
			mainPanel = new HelpUIPanel();
			mainPanel.SetPadding(6);
			mainPanel.BackgroundColor = color;

			mainPanel.Top.Set(20, 0f);
			mainPanel.Height.Set(-20, 1f);
			mainPanel.Width.Set(0, 1f);
			//mainPanel.OverflowHidden = true;

			//UIText text = new UIText("Help will be here.", 0.85f);
			//text.Top.Set(4, 0f);
			//text.HAlign = 0.5f;
			//mainPanel.Append(text);

			// UIList of expandable subheadings? possible with TextSnippet?
			//[image:Terraria/UI/ButtonCloudActive]

			StringBuilder sb = new StringBuilder();

			string headingColor = Color.CornflowerBlue.Hex3();
			// Overhaul warning
			if(Terraria.ModLoader.ModLoader.TryGetMod("TerrariaOverhaul", out _))
				sb.Append($"[c/{headingColor}:Jumbled Text?]\n    If the text below looks wrong, you'll need to disable Terraria Overhaul to fix it.\n\n");
			// Recipes
			sb.Append($"[c/{headingColor}:Recipes]\n    Place items in the query slot to find recipes using or resulting in that item. You can also search and filter with the various options in the window. The Nearby Chests option filters the recipes to show only recipes that you have at least 1 of each ingredient. The [c/{Color.Goldenrod.Hex3()}:Item Checklist Only] option limits the recipes to only show recipes that result in an item you have never seen using items you have seen. The Tile checkbox brings up a list of crafting stations sorted by most often required. Clicking on a tile limits recipes to recipes using that crafting station. The [image:RecipeBrowser/Images/Help/uniqueTile] Show Unique Recipes option toggles showing inherited recipes. For example, Hellforge inherits all of the Forge recipes, but has the unique Hellstone Bar recipes. Crafting stations for a selected recipe are listed at the bottom. The color will be red or green showing if the player is near one or not. The right side will show any NPC that drops the selected item. Double Click on an NPC to visit the Bestiary. Double Clicking on a Recipe or Ingredient will populate the query slot with the item, great for traversing crafting trees for items with multiple levels of crafting. Alt-click on a recipe to favorite a recipe, see Favorite section below.\n");
			// Craft
			sb.Append($"[c/{headingColor}:Craft]\n    This tool can be used to craft recipes. In the Recipes tool, right click on any recipe to bring up this tool. Recipes with the [image/s0.5:RecipeBrowser/Images/CanCraftExtendedBackground] background can be crafted via multiple steps. You can also left click on items in the Items tool and then visit this tool. This tool shows extended crafting options. For example, if you have gold and iron ore and click on the Gold Watch recipe, this tool will show you the multiple steps needed to craft gold and iron ore, followed by crafting chains, and finally the gold watch itself. You can also enable the Loot option to allow farming as part of the multi-step process. Simply find and kill the specified enemies to farm the needed ingredient. By default, all crafting stations that you have seen will be available for the extended crafting paths. If you enable the Missing Stations option, unseen crafting stations will also be shown. [c/{Utilities.yesColor.Hex3()}:✓], [c/{Utilities.maybeColor.Hex3()}:X], and [c/{Utilities.noColor.Hex3()}:?] icons over the tiles indicate if you are near the crafting station, not near it, or have never seen the station. Use the [image/s0.7:RecipeBrowser/Images/Help/CraftIcon] button to craft individual steps. By default the Recipes tool won't automatically calculate multiple step crafting unless the Calc checkbox is checked. You can also use the Extended Craftable filter to quickly view all extended crafting options. Note that you may experience some lag with this feature.\n");
			// Items
			sb.Append($"[c/{headingColor}:Items]\n    Here you can explore all items in the game. Use the filters and sorts to find what you want. Double click an item to query it in the Recipe panel. Double right click an item to query the item in the Bestiary. The Unobtained option shows only items not collected yet with [c/{Color.Goldenrod.Hex3()}:Item Checklist].\n");
			// Bestiary
			sb.Append($"[c/{headingColor}:Bestiary]\n    Here you can explore all NPC in the game. Use the filters and sorts to find what you want. Clicking on an NPC shows the loot from that NPC. Placing an item in the query slot filters the grid to show only NPC that drop that item. Double click an item to query it in the Recipe panel. Right click an item to populate the query slot, great for seeing who else drops the same item. The New Loot ([c/{Color.Goldenrod.Hex3()}:Item Checklist only]) option shows only NPC with items not collected yet, great for knowing which NPC are still hiding valuable loot.\n");
			// Borders
			sb.Append($"[c/{headingColor}:Borders]\n    Sometimes items will display with different borders, here is what they mean:\n[image/s0.5:RecipeBrowser/Images/FavoritedOverlay] This Recipe is Favorited, read Favorite section below.\n[image/s0.5:RecipeBrowser/Images/SelectedOverlay] This Recipe/Item/NPC is Selected. Information shown relates to this thing.\n[image/s0.5:RecipeBrowser/Images/CanCraftBackground] This Recipe can currently be crafted. Click on it to open the crafting menu directly.\n[image/s0.5:RecipeBrowser/Images/CanCraftExtendedBackground] This Recipe can currently be crafted via multiple steps. Right Click on it to open the Craft tool to view crafting options.\n[image/s0.5:RecipeBrowser/Images/Help/Inventory_Back8] [c/{Color.Goldenrod.Hex3()}:Item Checklist integration:] These items use the most recent new item you have collected.\n");
			// TODO ItemCatelog Badges
			// TODO Delete RecipeBrowser/Images/Help/Inventory_Back8 and 2 after 0.10.1.6 fix
			// Favorite
			sb.Append($"[c/{headingColor}:Favorite]\n    In the Recipe panel, you can favorite recipes by alt-clicking on the recipe. If you have favorited recipes, the favorites panel will appear on screen. This panel will help you track progress towards collecting all ingredients. In multiplayer, recipes favorited by other players will also appear in the favorites panel. You can track the progress of other players and cooperate efficiently. Item counts shown correspond to the player who favorited the recipe.\n[image:RecipeBrowser/Images/Help/mpFavorited]\n");
			// Query Hovered Item
			sb.Append($"[c/{headingColor}:Query Hovered Item]\n By assigning a hotkey in the keybindings menu, you can use this feature to quickly bring the item into one of the tools. You can use anytime you are hovering over any item anywhere in the game. Depending on the last panel you were on, the behavior changes. The Recipe tool will populate the query slot, the Craft tool will display options to craft the item, the Item tool will scroll and select the item, and the Bestiary tool will populate the loot query slot.\n");
			// Categories, Sub Categories, Sorts, and Filters
			sb.Append($"[c/{headingColor}:Categories, Sub Categories, Sorts, and Filters]\n    The Recipe and Item panels both have many ways to filter and sort items and recipes. You can type to search tooltips and item names and use the [image:RecipeBrowser/Images/filterMod] button to filter by source mod. You can also click on Categories such as [image:RecipeBrowser/Images/Help/Item_2430] and [image:RecipeBrowser/Images/Help/Item_54] to filter by category. Some Categories have Sub-Categories. You'll also notice on the 2nd row various Sorts such as [image:RecipeBrowser/Images/sortValue] and [image:RecipeBrowser/Images/sortAZ]. There are also Filters like [image:RecipeBrowser/Images/Help/Item_531]. Many Sorts and Filters are visible only within specific Categories. Some Filters, like the Ammo Filter, will cycle through many options. For these Filters, use left and right click to go forward and back, and middle click to clear the Filter.\n[image:RecipeBrowser/Images/Help/sortsExplanation]\n");
			// Item Checklist Integration
			sb.Append($"[c/{headingColor}:Item Checklist Integration]\n    By using the Item Checklist mod at the same time, you can narrow down your browsing to new items you've never obtained before from items you've collected so far. This helps you focus on items at the current stage of your game. Item Checklist also affects the Items and Bestiary tools. [l/www.gfycat.com/insidiousblandgoitered:Item Checklist video].");

			message = new UIMessageBox(sb.ToString());

			//message = new UIMessageBox($"[i:{ItemID.Safe}][i:{ItemID.Safe}][i:{ItemID.Safe}][i:{ItemID.Safe}] [i:{ItemID.Safe}][i:{ItemID.Safe}] [i:{ItemID.Safe}][i:{ItemID.Safe}] [i:{ItemID.Safe}][i:{ItemID.Safe}][i:{ItemID.Safe}] [i:{ItemID.Safe}]. [i:{ItemID.Safe}][i:{ItemID.Safe}][i:{ItemID.Safe}][i:{ItemID.Safe}] [i:{ItemID.Safe}][i:{ItemID.Safe}] [i:{ItemID.Safe}][i:{ItemID.Safe}] [i:{ItemID.Safe}][i:{ItemID.Safe}][i:{ItemID.Safe}] [i:{ItemID.Safe}] [i:{ItemID.Safe}][i:{ItemID.Safe}][i:{ItemID.Safe}][i:{ItemID.Safe}] [i:{ItemID.Safe}][i:{ItemID.Safe}] [i:{ItemID.Safe}][i:{ItemID.Safe}] [i:{ItemID.Safe}][i:{ItemID.Safe}][i:{ItemID.Safe}] [i:{ItemID.Safe}] Wow.");
			//message = new UIMessageBox($"Just Text word word rap wrap rappdfhdhdfgfdhhghgddafadsfsdfdsfararpfghffhfgh par parr Just Text word word rap wrap rapp ararp par parrJust Text word word rap wrap rapp ararp par parr Just Text word word rap wrap rapp ararp par parr");
			//message = new UIMessageBox($"s a s d f f f f f f f f f f f f f f f f f f f f f f f f f f f a f a d s f s df  d s f a rarp a parr Just Text word word rap wrap rapp ararp par parrJust Text word word rap wrap rapp ararp par parr Just Text word word rap wrap rapp ararp par parr");

			// text: 28. newlines too. mult newlines ignored? ==> mess up "no carrige return.
			message.Width.Set(-25f, 1f);
			message.Height.Set(0f, 1f);
			//message.Top.Set(20f, 0f);
			mainPanel.Append(message);

			FixedUIScrollbar uIScrollbar = new FixedUIScrollbar(RecipeBrowserUI.instance.userInterface);
			uIScrollbar.SetView(100f, 1000f);
			//uIScrollbar.Top.Set(25f, 0f);
			uIScrollbar.Top.Set(5f, 0f);
			uIScrollbar.Height.Set(-30, 1f);
			//uIScrollbar.VAlign = 0.5f;
			uIScrollbar.HAlign = 1f;
			mainPanel.Append(uIScrollbar);

			message.SetScrollbar(uIScrollbar);

			return mainPanel;
		}
	}

	//list = Utils.WordwrapStringSmart(text, c, FontAssets.MouseText.Value, WidthLimit, 10);
	internal class UIMessageBox : UIPanel
	{
		private string text;
		protected UIScrollbar _scrollbar;
		private float height;
		internal bool heightNeedsRecalculating;
		//private List<Tuple<string, float>> drawtexts = new List<Tuple<string, float>>();
		private List<List<TextSnippet>> drawTextSnippets = new List<List<TextSnippet>>();

		public UIMessageBox(string text)
		{
			this.text = text;
			if (this._scrollbar != null)
			{
				this._scrollbar.ViewPosition = 0;
				heightNeedsRecalculating = true;
			}
			OverflowHidden = true; // DrawChildren for OverflowHidden, lazy.
		}

		//protected override void DrawChildren(SpriteBatch spriteBatch)
		//{
		//	base.DrawChildren(spriteBatch);

		//	Vector2 position = this.Parent.GetDimensions().Position();
		//	Vector2 dimensions = new Vector2(this.Parent.GetDimensions().Width, this.Parent.GetDimensions().Height);
		//	foreach (UIElement current in this.Elements)
		//	{
		//		Vector2 position2 = current.GetDimensions().Position();
		//		Vector2 dimensions2 = new Vector2(current.GetDimensions().Width, current.GetDimensions().Height);
		//		if (Collision.CheckAABBvAABBCollision(position, dimensions, position2, dimensions2))
		//		{
		//			current.Draw(spriteBatch);
		//		}
		//	}
		//}

		public override void OnActivate()
		{
			base.OnActivate();
			heightNeedsRecalculating = true;
		}

		internal void SetText(string text)
		{
			this.text = text;
			if (this._scrollbar != null)
			{
				this._scrollbar.ViewPosition = 0;
				heightNeedsRecalculating = true;
			}
		}

		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			base.DrawChildren(spriteBatch);

			CalculatedStyle space = GetInnerDimensions();
			//Main.spriteBatch.Draw(Main.magicPixel, space.ToRectangle(), Color.Yellow * .7f);
			//Main.spriteBatch.Draw(Main.magicPixel, GetOuterDimensions().ToRectangle(), Color.Red * .7f);
			DynamicSpriteFont font = FontAssets.MouseText.Value;
			float position = 0f;
			if (this._scrollbar != null)
			{
				position = -this._scrollbar.GetValue();
			}
			//foreach (var drawtext in drawtexts)
			//{
			//	if (position + drawtext.Item2 > space.Height)
			//		break;
			//	if (position >= 0)
			//		Utils.DrawBorderString(spriteBatch, drawtext.Item1, new Vector2(space.X, space.Y + position), Color.White, 1f);
			//	position += drawtext.Item2;
			//}
			//float offset = 0;
			TextSnippet[] texts;
			foreach (var snippetList in drawTextSnippets)
			{
				texts = snippetList.ToArray();
				float snippetListHeight = ChatManager.GetStringSize(font, texts, Vector2.One).Y;
				//Main.NewText($"Y: {ChatManager.GetStringSize(font, texts, Vector2.One).X}");
				if (position > -snippetListHeight)
				{
					//foreach (var snippet in snippetList)
					//{
					int hoveredSnippet = -1;
					ChatManager.ConvertNormalSnippets(texts);
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, texts, new Vector2(space.X, space.Y + position /*+ offset*/), 0f, Vector2.Zero, Vector2.One, out hoveredSnippet);
					//offset += 20;
					//offset += texts.Max(t => (int)ChatManager.GetStringSize(FontAssets.MouseText.Value, texts, Vector2.One).X);
					if (hoveredSnippet > -1 && IsMouseHovering)
					{
						texts[hoveredSnippet].OnHover();
						if (Main.mouseLeft && Main.mouseLeftRelease/* && Terraria.GameInput.PlayerInput.Triggers.JustReleased.MouseLeft*/)
						{
							texts[hoveredSnippet].OnClick();
						}
					}
				}
				position += snippetListHeight;
				if (position > space.Height)
					break;
				//}
			}
			//if (drawTextSnippets.Count > 0)
			//{
			//	//ChatManager.DrawColorCodedStringShadow()
			//	int hoveredSnippet = -1;
			//	ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, drawTextSnippets[0].ToArray(), new Vector2(space.X, space.Y + position), 0f, Vector2.Zero, Vector2.One, out hoveredSnippet);
			//	if (hoveredSnippet > -1)
			//	{
			//		//array[hoveredSnippet].OnHover();
			//	}
			//}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			this.Recalculate();
		}

		public override void RecalculateChildren()
		{
			base.RecalculateChildren();
			if (!heightNeedsRecalculating)
			{
				return;
			}
			CalculatedStyle space = GetInnerDimensions();
			if (space.Width <= 0 || space.Height <= 0)
			{
				return;
			}
			DynamicSpriteFont font = FontAssets.MouseText.Value;

			drawTextSnippets = WordwrapStringSmart(text, Color.White, font, (int)space.Width, -1);
			//height = ChatManager.GetStringSize(font, text, Vector2.One, space.Width).Y;

			//Main.NewText($"h: {height} ");
			//Main.NewText($"a: {drawTextSnippets[0][0].GetStringLength(font)} ");
			//// 18* 28 == 504
			//// 18 * ? == 588
			//float hsum = 0;
			height = 0;
			foreach (var snippetList in drawTextSnippets)
			{
				var texts = snippetList.ToArray();
				height += ChatManager.GetStringSize(font, texts, Vector2.One).Y;
				//Main.NewText($"calc Y: {ChatManager.GetStringSize(font, texts, Vector2.One).Y}");
			}

			//Main.NewText($"Count: {drawTextSnippets.Count}  Height:{height}    sum: {hsum}");

			//height = hsum;

			//height = ChatManager.GetStringSize(font, "1", Vector2.One).Y;

			/*
			drawtexts.Clear();
			float position = 0f;
			float textHeight = font.MeasureString("A").Y;
			string[] lines = text.Split('\n');
			foreach (string line in lines)
			{
				string drawString = line;
				if (drawString.Length == 0)
				{
					position += textHeight;
				}
				while (drawString.Length > 0)
				{
					string remainder = "";
					while (font.MeasureString(drawString).X > space.Width)
					{
						remainder = drawString[drawString.Length - 1] + remainder;
						drawString = drawString.Substring(0, drawString.Length - 1);
					}
					if (remainder.Length > 0)
					{
						int index = drawString.LastIndexOf(' ');
						if (index >= 0)
						{
							remainder = drawString.Substring(index + 1) + remainder;
							drawString = drawString.Substring(0, index);
						}
					}
					drawtexts.Add(new Tuple<string, float>(drawString, textHeight));
					position += textHeight;
					drawString = remainder;
				}
			}
			height = position;
			*/
			heightNeedsRecalculating = false;
		}

		public override void Recalculate()
		{
			base.Recalculate();
			this.UpdateScrollbar();
		}

		public override void ScrollWheel(UIScrollWheelEvent evt)
		{
			base.ScrollWheel(evt);
			if (this._scrollbar != null)
			{
				this._scrollbar.ViewPosition -= (float)evt.ScrollWheelValue;
			}
		}

		public void SetScrollbar(UIScrollbar scrollbar)
		{
			this._scrollbar = scrollbar;
			this.UpdateScrollbar();
			this.heightNeedsRecalculating = true;
		}

		private void UpdateScrollbar()
		{
			if (this._scrollbar == null)
			{
				return;
			}
			this._scrollbar.SetView(base.GetInnerDimensions().Height, this.height);
		}

		// Attempt at fix: problems: long words crash it, spaces seem to be miscounted.
		public static List<List<TextSnippet>> WordwrapStringSmart(string text, Color c, DynamicSpriteFont font, int maxWidth, int maxLines)
		{
			TextSnippet[] array = ChatManager.ParseMessage(text, c).ToArray();
			List<List<TextSnippet>> finalList = new List<List<TextSnippet>>();
			List<TextSnippet> list2 = new List<TextSnippet>();
			for (int i = 0; i < array.Length; i++)
			{
				TextSnippet textSnippet = array[i];
				string[] array2 = textSnippet.Text.Split(new char[]
					{
						'\n'
					});
				for (int j = 0; j < array2.Length - 1; j++)
				{
					list2.Add(textSnippet.CopyMorph(array2[j]));
					finalList.Add(list2);
					list2 = new List<TextSnippet>();
				}
				list2.Add(textSnippet.CopyMorph(array2[array2.Length - 1]));
			}
			finalList.Add(list2);
			if (maxWidth != -1)
			{
				for (int k = 0; k < finalList.Count; k++)
				{
					List<TextSnippet> currentLine = finalList[k];
					float usedWidth = 0f;
					for (int l = 0; l < currentLine.Count; l++)
					{
						//float stringLength = list3[l].GetStringLength(font); // GetStringLength doesn't match UniqueDraw
						float stringLength = ChatManager.GetStringSize(font, new TextSnippet[] { currentLine[l] }, Vector2.One).X;
						//float stringLength2 = ChatManager.GetStringSize(font, " ", Vector2.One).X;
						//float stringLength3 = ChatManager.GetStringSize(font, "1", Vector2.One).X;

						if (stringLength + usedWidth > (float)maxWidth)
						{
							int num2 = maxWidth - (int)usedWidth;
							if (usedWidth > 0f)
							{
								num2 -= 16;
							}
							float toFill = num2;
							bool filled = false;
							int successfulIndex = -1;
							int index = 0;
							while (index < currentLine[l].Text.Length && !filled)
							{
								if (currentLine[l].Text[index] == ' ' || isChinese(currentLine[l].Text[index])) {
									if (ChatManager.GetStringSize(font, currentLine[l].Text.Substring(0, index), Vector2.One).X < toFill)
										successfulIndex = index;
									else
									{
										filled = true;
										//if (successfulIndex == 0)
										//	successfulIndex = index;
									}
								}
								index++;
							}
							if (currentLine[l].Text.Length == 0)
							{
								filled = true;
							}
							int num4 = successfulIndex;

							if (successfulIndex == -1) // last item is too big
							{
								if (l == 0) // 1st item in list, keep it and move on
								{
									//list2 = new List<TextSnippet>{currentLine[l]};
									list2 = new List<TextSnippet>();
									for (int m = l + 1; m < currentLine.Count; m++)
									{
										list2.Add(currentLine[m]);
									}
									finalList[k] = finalList[k].Take(/*l + */ 1).ToList<TextSnippet>(); // take 1
									finalList.Insert(k + 1, list2);
								}
								else // midway through list, keep previous and move this to next
								{
									list2 = new List<TextSnippet>();
									for (int m = l; m < currentLine.Count; m++)
									{
										list2.Add(currentLine[m]);
									}
									finalList[k] = finalList[k].Take(l).ToList<TextSnippet>(); // take previous ones
									finalList.Insert(k + 1, list2);
								}
							}
							else
							{
								string newText = currentLine[l].Text.Substring(0, num4);
								string newText2 = currentLine[l].Text.Substring(num4).TrimStart();
								list2 = new List<TextSnippet>
								{
									currentLine[l].CopyMorph(newText2)
								};
								for (int m = l + 1; m < currentLine.Count; m++)
								{
									list2.Add(currentLine[m]);
								}
								currentLine[l] = currentLine[l].CopyMorph(newText);
								finalList[k] = finalList[k].Take(l + 1).ToList<TextSnippet>();
								finalList.Insert(k + 1, list2);
							}
							break;
						}
						usedWidth += stringLength;
					}
				}
			}
			if (maxLines != -1)
			{
				while (finalList.Count > 10)
				{
					finalList.RemoveAt(10);
				}
			}
			return finalList;
		}

		private static readonly List<char> cnPuncs = new List<char>() {
			'–', '—', '‘', '’', '“', '”',
			'…', '、', '。', '〈', '〉', '《',
			'》', '「', '」', '『', '』', '【',
			'】', '〔', '〕', '！', '（', '）',
			'，', '．', '：', '；', '？'
		};
		public static bool isChinese(char a) {
			return (a >= 0x4E00 && a <= 0x9FA5) || cnPuncs.Contains(a);
		}
	}
}
