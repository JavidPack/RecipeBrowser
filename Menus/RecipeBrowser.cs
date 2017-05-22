using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using RecipeBrowser.UI;
using RecipeBrowser.CustomUI;
using Terraria.GameInput;

namespace RecipeBrowser.Menus
{
	enum RecipeBrowserCategories
	{
		AllRecipes,
		ModRecipes
	}

	class RecipeBrowserWindow : UISlideWindow
	{
		private static string[] categNames = new string[]
		{
			"All Recipes",
			"Cycle Mod Specific Recipes"
		};

		private static Texture2D[] categoryIcons = new Texture2D[]
		{
			Main.itemTexture[ItemID.AlphabetStatueA],
			Main.itemTexture[ItemID.AlphabetStatueM],
		};

		internal static RecipeView recipeView;
		public Mod mod;
		//private static List<string> categoryNames = new List<string>();
		private static UIImage[] bCategories = new UIImage[categoryIcons.Length];

		//private static GenericItemSlot[] lookupItem = new GenericItemSlot[1];
		internal static RecipeQuerySlot lookupItemSlot;


		private static GenericItemSlot[] ingredients = new GenericItemSlot[Recipe.maxRequirements];
		private static GenericItemSlot[] tiles = new GenericItemSlot[Recipe.maxRequirements];

		public static List<List<int>> categories = new List<List<int>>();
		private static Color buttonColor = new Color(190, 190, 190);

		private static Color buttonSelectedColor = new Color(209, 142, 13);

		private UITextbox textbox;

		private float spacing = 16f;
		private float halfspacing = 8f;

		public int lastModNameNumber = 0;

		public Recipe selectedRecipe = null;
		internal bool selectedRecipeChanged = false;

		// 270 : 16 40 ?? 16

		public RecipeBrowserWindow(Mod mod)
		{
			categories.Clear();
			recipeView = new RecipeView();
			this.mod = mod;
			this.CanMove = true;
			base.Width = recipeView.Width + this.spacing * 2f;
			base.Height = 420f;
			recipeView.Position = new Vector2(this.spacing, this.spacing + 40);
			this.AddChild(recipeView);
			this.InitializeRecipeCategories();
			Texture2D texture = mod.GetTexture("UI/closeButton");
			UIImage uIImage = new UIImage(texture);
			uIImage.Anchor = AnchorPosition.TopRight;
			uIImage.Position = new Vector2(base.Width - this.spacing, this.spacing);
			uIImage.onLeftClick += new EventHandler(this.bClose_onLeftClick);
			this.AddChild(uIImage);
			this.textbox = new UITextbox();
			this.textbox.Anchor = AnchorPosition.TopRight;
			this.textbox.Position = new Vector2(base.Width - this.spacing * 2f - uIImage.Width, this.spacing /** 2f + uIImage.Height*/);
			this.textbox.KeyPressed += new UITextbox.KeyPressedHandler(this.textbox_KeyPressed);
			this.AddChild(this.textbox);


			//lookupItemSlot = new Slot(0);
			lookupItemSlot = new RecipeQuerySlot();
			lookupItemSlot.Position = new Vector2(spacing, halfspacing);
			lookupItemSlot.Scale = .85f;
			//lookupItemSlot.functionalSlot = true;
			this.AddChild(lookupItemSlot);

			for (int j = 0; j < RecipeBrowserWindow.categoryIcons.Length; j++)
			{
				UIImage uIImage2 = new UIImage(RecipeBrowserWindow.categoryIcons[j]);
				Vector2 position = new Vector2(this.spacing + 48, this.spacing);
				uIImage2.Scale = 32f / Math.Max(categoryIcons[j].Width, categoryIcons[j].Height);

				position.X += (float)(j % 6 * 40);
				position.Y += (float)(j / 6 * 40);

				if (categoryIcons[j].Height > categoryIcons[j].Width)
				{
					position.X += (32 - categoryIcons[j].Width) / 2;
				}
				else if (categoryIcons[j].Height < categoryIcons[j].Width)
				{
					position.Y += (32 - categoryIcons[j].Height) / 2;
				}

				uIImage2.Position = position;
				uIImage2.Tag = j;
				uIImage2.onLeftClick += new EventHandler(this.button_onLeftClick);
				uIImage2.ForegroundColor = RecipeBrowserWindow.buttonColor;
				if (j == 0)
				{
					uIImage2.ForegroundColor = RecipeBrowserWindow.buttonSelectedColor;
				}
				uIImage2.Tooltip = RecipeBrowserWindow.categNames[j];
				RecipeBrowserWindow.bCategories[j] = uIImage2;
				this.AddChild(uIImage2);
			}
			// 15.
			for (int j = 0; j < Recipe.maxRequirements; j++)
			{
				GenericItemSlot genericItemSlot = new GenericItemSlot();
				Vector2 position = new Vector2(this.spacing, this.spacing);

				//position.X += j * 60;
				//position.Y += 250;
				//if (j >= 7)
				//{
				//	position.Y += 60;
				//	position.X -= 7 * 60;
				//}
				position.X += 166 + (j % cols * 51);
				position.Y += 244 + (j / cols * 51);

				genericItemSlot.Position = position;
				genericItemSlot.Tag = j;
				RecipeBrowserWindow.ingredients[j] = genericItemSlot;
				this.AddChild(genericItemSlot, false);
			}

			recipeView.selectedCategory = RecipeBrowserWindow.categories[0].ToArray();
			recipeView.activeSlots = recipeView.selectedCategory;
			recipeView.ReorderSlots();
		}
		const int cols = 5;


		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			if (Visible && IsMouseInside())
			{
				Main.player[Main.myPlayer].mouseInterface = true;
				Main.player[Main.myPlayer].showItemIcon = false;
			}

			if (Visible && Recipe.numRecipes > recipeView.allRecipeSlot.Length)
			{
				//			ErrorLogger.Log("New " + Recipe.numRecipes + " " + recipeView.allRecipeSlot.Length);

				recipeView.allRecipeSlot = new RecipeSlot[Recipe.numRecipes];
				for (int i = 0; i < recipeView.allRecipeSlot.Length; i++)
				{
					recipeView.allRecipeSlot[i] = new RecipeSlot(i);
				}

				this.InitializeRecipeCategories();

				recipeView.selectedCategory = RecipeBrowserWindow.categories[0].ToArray();
				recipeView.activeSlots = recipeView.selectedCategory;
				recipeView.ReorderSlots();
			}

			float x = Main.fontMouseText.MeasureString(UIView.HoverText).X;
			Vector2 vector = new Vector2((float)Main.mouseX, (float)Main.mouseY) + new Vector2(16f);
			if (vector.Y > (float)(Main.screenHeight - 30))
			{
				vector.Y = (float)(Main.screenHeight - 30);
			}
			if (vector.X > (float)Main.screenWidth - x)
			{
				vector.X = (float)(Main.screenWidth - 460);
			}
			Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, UIView.HoverText, vector.X, vector.Y, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, Vector2.Zero, 1f);


			float positionX = this.X + spacing;
			float positionY = this.Y + 270;// 320;
			string text4;
			if (selectedRecipe != null && Visible)
			{
				Color color3 = new Color((int)((byte)((float)Main.mouseTextColor)), (int)((byte)((float)Main.mouseTextColor)), (int)((byte)((float)Main.mouseTextColor)), (int)((byte)((float)Main.mouseTextColor)));


				text4 = Lang.inter[21] + " " + Main.guideItem.name; // guideItem???
				spriteBatch.DrawString(Main.fontMouseText, Lang.inter[22], new Vector2((float)positionX, (float)(positionY)), color3, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
				//	int num60 = Main.focusRecipe;
				int num61 = 0;
				int num62 = 0;
				while (num62 < Recipe.maxRequirements)
				{
					int num63 = (num62 + 1) * 26;
					if (selectedRecipe.requiredTile[num62] == -1)
					{
						if (num62 == 0 && !selectedRecipe.needWater && !selectedRecipe.needHoney && !selectedRecipe.needLava)
						{
							spriteBatch.DrawString(Main.fontMouseText, Lang.inter[23], new Vector2((float)positionX, (float)(positionY + num63)), color3, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
							break;
						}
						break;
					}
					else
					{
						num61++;
						spriteBatch.DrawString(Main.fontMouseText, Lang.mapLegend.FromType(selectedRecipe.requiredTile[num62]), new Vector2((float)positionX, (float)(positionY + num63)), color3, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
						num62++;
					}
				}
				if (selectedRecipe.needWater)
				{
					int num64 = (num61 + 1) * 26;
					spriteBatch.DrawString(Main.fontMouseText, Lang.inter[53], new Vector2((float)positionX, (float)(positionY + num64)), color3, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
				}
				if (selectedRecipe.needHoney)
				{
					int num65 = (num61 + 1) * 26;
					spriteBatch.DrawString(Main.fontMouseText, Lang.inter[58], new Vector2((float)positionX, (float)(positionY + num65)), color3, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
				}
				if (selectedRecipe.needLava)
				{
					int num66 = (num61 + 1) * 26;
					spriteBatch.DrawString(Main.fontMouseText, Lang.inter[56], new Vector2((float)positionX, (float)(positionY + num66)), color3, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
				}
			}
			//else
			//{
			//	text4 = Lang.inter[24];
			//}
			//spriteBatch.DrawString(Main.fontMouseText, text4, new Vector2((float)(positionX + 50), (float)(positionY + 12)), new Microsoft.Xna.Framework.Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
		}

		public override void Update()
		{
			UIView.MousePrevLeftButton = UIView.MouseLeftButton;
			UIView.MouseLeftButton = Main.mouseLeft;
			UIView.MousePrevRightButton = UIView.MouseRightButton;
			UIView.MouseRightButton = Main.mouseRight;
			UIView.ScrollAmount = PlayerInput.ScrollWheelDeltaForUI;
			//UIView.ScrollAmount = (Main.mouseState.ScrollWheelValue - Main.oldMouseState.ScrollWheelValue) / 2;
			UIView.HoverItem = UIView.EmptyItem;
			UIView.HoverText = "";
			UIView.HoverOverridden = false;

			//	UIView.MousePrevLeftButton = UIView.MouseLeftButton;
			//	UIView.MouseLeftButton = Main.mouseLeft;
			//	UIView.MousePrevRightButton = UIView.MouseRightButton;
			//	UIView.MouseRightButton = Main.mouseRight;
			//	UIView.ScrollAmount = (Main.mouseState.ScrollWheelValue - Main.oldMouseState.ScrollWheelValue) / 2;
			//	UIView.HoverItem = UIView.EmptyItem;
			//	UIView.HoverText = "";
			//	UIView.HoverOverridden = false;

			if (selectedRecipeChanged)
			{
				//ErrorLogger.Log("1");
				//foreach(var a in CheatSheet.ButtonClicked)
				//{
				//	Main.NewText(">");
				//	ErrorLogger.Log("button pressing");

				//	a(selectedRecipe.requiredItem[0].type);
				//	Main.NewText("<");
				//}

				selectedRecipeChanged = false;
				string oldname = Main.toolTip.name;
				for (int i = 0; i < Recipe.maxRequirements; i++)
				{
					if (selectedRecipe.requiredItem[i].type > 0)
					{
						ingredients[i].item = selectedRecipe.requiredItem[i];

						string name;
						if (selectedRecipe.ProcessGroupsForText(selectedRecipe.requiredItem[i].type, out name))
						{
							Main.toolTip.name = name;
						}
						if (selectedRecipe.anyIronBar && selectedRecipe.requiredItem[i].type == 22)
						{
							Main.toolTip.name = Lang.misc[37] + " " + Main.itemName[22];
						}
						else if (selectedRecipe.anyWood && selectedRecipe.requiredItem[i].type == 9)
						{
							Main.toolTip.name = Lang.misc[37] + " " + Main.itemName[9];
						}
						else if (selectedRecipe.anySand && selectedRecipe.requiredItem[i].type == 169)
						{
							Main.toolTip.name = Lang.misc[37] + " " + Main.itemName[169];
						}
						else if (selectedRecipe.anyFragment && selectedRecipe.requiredItem[i].type == 3458)
						{
							Main.toolTip.name = Lang.misc[37] + " " + Lang.misc[51];
						}
						else if (selectedRecipe.anyPressurePlate && selectedRecipe.requiredItem[i].type == 542)
						{
							Main.toolTip.name = Lang.misc[37] + " " + Lang.misc[38];
						}
						//else
						//{
						//	ModRecipe recipe = selectedRecipe as ModRecipe;
						//	if (recipe != null)
						//	{
						//		recipe.CraftGroupDisplayName(i);
						//	}
						//}

						if (Main.toolTip.name != oldname)
						{
							ingredients[i].item.name = Main.toolTip.name;
							Main.toolTip.name = oldname;
						}
					}
					else
					{
						ingredients[i].item = null;
					}

					//				if (selectedRecipe.requiredTile[i] > -1)
					//				{
					//					tiles[i].item = selectedRecipe.requiredItem[i]
					//;
					//				}
					//				else
					//				{
					//					ingredients[i].item = null;
					//				}
					//				this.requiredItem[i] = new Item();
					//				this.requiredTile[i] = -1;
				}
			}

			base.Update();
		}

		private void bClose_onLeftClick(object sender, EventArgs e)
		{
			if (lookupItemSlot.real && lookupItemSlot.item.stack > 0)
			{
				// This causes items to get a new modifier. Oops
				//Main.player[Main.myPlayer].QuickSpawnItem(lookupItemSlot.item.type, lookupItemSlot.item.stack);
				//lookupItemSlot.item.SetDefaults(0);

				Player player = Main.player[Main.myPlayer];
				lookupItemSlot.item.position = player.Center;
				Item item = player.GetItem(player.whoAmI, lookupItemSlot.item, false, true);
				if (item.stack > 0)
				{
					int num = Item.NewItem((int)player.position.X, (int)player.position.Y, player.width, player.height, item.type, item.stack, false, (int)lookupItemSlot.item.prefix, true, false);
					Main.item[num].newAndShiny = false;
					if (Main.netMode == 1)
					{
						NetMessage.SendData(21, -1, -1, "", num, 1f, 0f, 0f, 0, 0, 0);
					}
				}
				lookupItemSlot.item = new Item();

				recipeView.ReorderSlots();
			}

			Hide();
			//base.Visible = false;
		}

		private void button_onLeftClick(object sender, EventArgs e)
		{
			UIImage uIImage = (UIImage)sender;
			int num = (int)uIImage.Tag;
			if (num == (int)RecipeBrowserCategories.ModRecipes)
			{
				string[] mods = ModLoader.GetLoadedMods();
				string currentMod = mods[lastModNameNumber];
				lastModNameNumber = (lastModNameNumber + 1) % mods.Length;
				if (currentMod == "ModLoader")
				{
					currentMod = mods[lastModNameNumber];
					lastModNameNumber = (lastModNameNumber + 1) % mods.Length;
				}
				recipeView.selectedCategory = RecipeBrowserWindow.categories[0].Where(x => recipeView.allRecipeSlot[x].recipe as ModRecipe != null && (recipeView.allRecipeSlot[x].recipe as ModRecipe).mod.Name == currentMod).ToArray();
				recipeView.activeSlots = recipeView.selectedCategory;
				recipeView.ReorderSlots();
				bCategories[num].Tooltip = RecipeBrowserWindow.categNames[num] + ": " + currentMod;
			}
			else
			{
				recipeView.selectedCategory = RecipeBrowserWindow.categories[num].ToArray();
				recipeView.activeSlots = recipeView.selectedCategory;
				recipeView.ReorderSlots();
			}
			this.textbox.Text = "";
			UIImage[] array = RecipeBrowserWindow.bCategories;
			for (int j = 0; j < array.Length; j++)
			{
				UIImage uIImage2 = array[j];
				uIImage2.ForegroundColor = RecipeBrowserWindow.buttonColor;
			}
			uIImage.ForegroundColor = RecipeBrowserWindow.buttonSelectedColor;
		}

		private void textbox_KeyPressed(object sender, char key)
		{
			if (this.textbox.Text.Length <= 0)
			{
				recipeView.activeSlots = recipeView.selectedCategory;
				recipeView.ReorderSlots();
				return;
			}
			List<int> list = new List<int>();
			int[] category = recipeView.selectedCategory;
			for (int i = 0; i < category.Length; i++)
			{
				int num = category[i];
				RecipeSlot slot = recipeView.allRecipeSlot[num];
				if (slot.recipe.createItem.name.ToLower().IndexOf(this.textbox.Text.ToLower(), StringComparison.Ordinal) != -1)
				{
					list.Add(num);
				}
				//else
				//{
				//	for (int j = 0; j < slot.recipe.requiredItem.Length; i++)
				//	{
				//		if (slot.recipe.requiredItem[j].type > 0 && slot.recipe.requiredItem[j].name.ToLower().IndexOf(this.textbox.Text.ToLower(), StringComparison.Ordinal) != -1)
				//		{
				//			list.Add(num);
				//			break;
				//		}
				//	}
				//}
			}
			if (list.Count > 0)
			{
				recipeView.activeSlots = list.ToArray();
				recipeView.ReorderSlots();
				return;
			}
			this.textbox.Text = this.textbox.Text.Substring(0, this.textbox.Text.Length - 1);
		}

		private void InitializeRecipeCategories()
		{
			//	RecipeBrowser.categoryNames = RecipeBrowser.categNames.ToList<string>();
			for (int i = 0; i < RecipeBrowserWindow.categNames.Length; i++)
			{
				RecipeBrowserWindow.categories.Add(new List<int>());
				for (int j = 0; j < recipeView.allRecipeSlot.Length; j++)
				{
					if (i == 0)
					{
						RecipeBrowserWindow.categories[i].Add(j);
					}
					//else if (i == 1 && recipeView.allNPCSlot[j].npc.boss)
					//{
					//	RecipeBrowser.categories[i].Add(j);
					//}
					//else if (i == 2 && recipeView.allNPCSlot[j].npc.townNPC)
					//{
					//	RecipeBrowser.categories[i].Add(j);
					//}
				}
			}
			recipeView.selectedCategory = RecipeBrowserWindow.categories[0].ToArray();
		}
	}
}
