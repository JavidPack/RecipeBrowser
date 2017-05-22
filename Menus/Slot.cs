using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Input;
using RecipeBrowser.UI;

namespace RecipeBrowser.Menus
{
	internal class Slot : UIView
	{
		public Item item = new Item();

		public int index = -1;

		public static Texture2D backgroundTexture = Main.inventoryBack9Texture;

		public bool functionalSlot;
		private bool rightClicking;

		public Slot(Vector2 position, int itemNum)
		{
			base.Position = position;
			this.Init(itemNum);
		}

		public Slot(int itemNum)
		{
			this.Init(itemNum);
		}

		private void Init(int itemNum)
		{
			base.Scale = 0.85f;
			this.item.SetDefaults(itemNum, false);
			base.onLeftClick += new EventHandler(this.Slot2_onLeftClick);
			//	base.onRightClick += new EventHandler(this.Slot2_onRightClick);
			base.onMouseDown += new ClickEventHandler(this.Slot2_onMouseDown);
			base.onHover += new EventHandler(this.Slot2_onHover);
		}

		protected override float GetWidth()
		{
			return (float)Slot.backgroundTexture.Width * base.Scale;
		}

		public override void Update()
		{
			if (!UIView.MouseRightButton)
			{
				this.rightClicking = false;
			}
			if (rightClicking)
			{
				Main.playerInventory = true;

				if (Main.stackSplit <= 1 /*&& Main.mouseRight */&& item.type > 0 && (Main.mouseItem.IsTheSameAs(item) || Main.mouseItem.type == 0))
				{
					int num2 = Main.superFastStack + 1;
					for (int j = 0; j < num2; j++)
					{
						if ((Main.mouseItem.stack < Main.mouseItem.maxStack || Main.mouseItem.type == 0) && item.stack > 0)
						{
							if (j == 0)
							{
								Main.PlaySound(18, -1, -1, 1);
							}
							if (Main.mouseItem.type == 0)
							{
								Main.mouseItem.netDefaults(item.netID);
								if (item.prefix != 0)
								{
									Main.mouseItem.Prefix((int)item.prefix);
								}
								Main.mouseItem.stack = 0;
							}
							Main.mouseItem.stack++;
							if (Main.stackSplit == 0)
							{
								Main.stackSplit = 15;
							}
							else
							{
								Main.stackSplit = Main.stackDelay;
							}
						}
					}
				}
			}
		}

		protected override float GetHeight()
		{
			return (float)Slot.backgroundTexture.Height * base.Scale;
		}

		private void Slot2_onHover(object sender, EventArgs e)
		{
			//ErrorLogger.Log("On hover " + this.item.name);
			//UIView.HoverText = this.item.name;
			//UIView.HoverItem = this.item.Clone();

			//Main.craftingHide = true;
			Main.hoverItemName = this.item.name;// + (item.modItem != null ? " " + item.modItem.mod.Name : "???");
			//if (item.stack > 1)
			//{
			//	object hoverItemName = Main.hoverItemName;
			//	Main.hoverItemName = string.Concat(new object[]
			//		{
			//				hoverItemName,
			//				" (",
			//				item.stack,
			//				")"
			//		});
			//}
			Main.toolTip = item.Clone();
			Main.toolTip.name = Main.toolTip.name + (Main.toolTip.modItem != null ? " [" + Main.toolTip.modItem.mod.Name + "]" : "");


		}

		private void Slot2_onLeftClick(object sender, EventArgs e)
		{
			//ErrorLogger.Log("On Slot2_onLeftClick " + this.item.name);
			if (this.functionalSlot)
			{
				Item item = Main.mouseItem.Clone();
				Main.mouseItem = this.item.Clone();
				this.item = item.Clone();
				return;
			}

			if (Main.mouseItem.type == 0)
			{
				if (Main.keyState.IsKeyDown(Keys.LeftShift))
				{
					Main.player[Main.myPlayer].QuickSpawnItem(this.item.type, this.item.maxStack);
					return;
				}
				//	ErrorLogger.Log("On Slot2_onLeftClick Here");
				//Main.mouseItem = this.item.Clone();
				Main.mouseItem.netDefaults(item.netID);
				Main.mouseItem.stack = Main.mouseItem.maxStack;
				Main.playerInventory = true;
				Main.PlaySound(18, -1, -1, 1);
			}
		}

		private void Slot2_onMouseDown(object sender, byte button)
		{
			if (button == 0)
			{
				return;
			}

			rightClicking = true;

			//ErrorLogger.Log("1");

			//if (Main.stackSplit <= 1 /*&& Main.mouseRight */&& item.type > 0 && (Main.mouseItem.IsTheSameAs(item) || Main.mouseItem.type == 0))
			//{
			//	ErrorLogger.Log("2");

			//	int num2 = Main.superFastStack + 1;
			//	for (int j = 0; j < num2; j++)
			//	{
			//		if ((Main.mouseItem.stack < Main.mouseItem.maxStack || Main.mouseItem.type == 0) && item.stack > 0)
			//		{
			//			ErrorLogger.Log("3");

			//			if (j == 0)
			//			{
			//				Main.PlaySound(18, -1, -1, 1);
			//			}
			//			if (Main.mouseItem.type == 0)
			//			{
			//				ErrorLogger.Log("4");

			//				Main.mouseItem.netDefaults(item.netID);
			//				if (item.prefix != 0)
			//				{
			//					ErrorLogger.Log("??");
			//					Main.mouseItem.Prefix((int)item.prefix);
			//				}
			//				Main.mouseItem.stack = 0;
			//			}
			//			Main.mouseItem.stack++;
			//			if (Main.stackSplit == 0)
			//			{
			//				Main.stackSplit = 15;
			//			}
			//			else
			//			{
			//				Main.stackSplit = Main.stackDelay;
			//			}
			//		}
			//	}
			//}
		}

		private void Slot2_onRightClick(object sender, EventArgs e)
		{
			//if (Main.mouseItem.type == 0)
			//{
			//	if (Main.keyState.IsKeyDown(Keys.LeftShift))
			//	{
			//		Main.player[Main.myPlayer].QuickSpawnItem(this.item.type, this.item.maxStack);
			//		return;
			//	}
			//	//	ErrorLogger.Log("On Slot2_onLeftClick Here");
			//	Main.mouseItem = this.item.Clone();
			//	Main.mouseItem.stack = Main.mouseItem.maxStack;
			//	Main.playerInventory = true;
			//}
			//ErrorLogger.Log("1");

			if (Main.stackSplit <= 1 /*&& Main.mouseRight */&& item.type > 0 && (Main.mouseItem.IsTheSameAs(item) || Main.mouseItem.type == 0))
			{
				////ErrorLogger.Log("2");

				int num2 = Main.superFastStack + 1;
				for (int j = 0; j < num2; j++)
				{
					if ((Main.mouseItem.stack < Main.mouseItem.maxStack || Main.mouseItem.type == 0) && item.stack > 0)
					{
					//	ErrorLogger.Log("3");

						if (j == 0)
						{
							Main.PlaySound(18, -1, -1, 1);
						}
						if (Main.mouseItem.type == 0)
						{
						//	ErrorLogger.Log("4");

							Main.mouseItem.netDefaults(item.netID);
							if (item.prefix != 0)
							{
//ErrorLogger.Log("??");
								Main.mouseItem.Prefix((int)item.prefix);
							}
							Main.mouseItem.stack = 0;
						}
						Main.mouseItem.stack++;
						if (Main.stackSplit == 0)
						{
							Main.stackSplit = 15;
						}
						else
						{
							Main.stackSplit = Main.stackDelay;
						}
					}
				}
			}


		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Slot.backgroundTexture, base.DrawPosition, null, Color.White, 0f, Vector2.Zero, base.Scale, SpriteEffects.None, 0f);
			Texture2D texture2D = Main.itemTexture[this.item.type];
			Rectangle rectangle2;
			if (Main.itemAnimations[item.type] != null)
			{
				rectangle2 = Main.itemAnimations[item.type].GetFrame(texture2D);
			}
			else
			{
				rectangle2 = texture2D.Frame(1, 1, 0, 0);
			}
			float num = 1f;
			float num2 = (float)Slot.backgroundTexture.Width * base.Scale * 0.6f;
			if ((float)rectangle2.Width > num2 || (float)rectangle2.Height > num2)
			{
				if (rectangle2.Width > rectangle2.Height)
				{
					num = num2 / (float)rectangle2.Width;
				}
				else
				{
					num = num2 / (float)rectangle2.Height;
				}
			}
			Vector2 drawPosition = base.DrawPosition;
			drawPosition.X += (float)Slot.backgroundTexture.Width * base.Scale / 2f - (float)rectangle2.Width * num / 2f;
			drawPosition.Y += (float)Slot.backgroundTexture.Height * base.Scale / 2f - (float)rectangle2.Height * num / 2f;
			this.item.GetColor(Color.White);
			spriteBatch.Draw(texture2D, drawPosition, new Rectangle?(rectangle2), this.item.GetAlpha(Color.White), 0f, Vector2.Zero, num, SpriteEffects.None, 0f);
			if (this.item.color != default(Color))
			{
				spriteBatch.Draw(texture2D, drawPosition, new Rectangle?(rectangle2), this.item.GetColor(Color.White), 0f, Vector2.Zero, num, SpriteEffects.None, 0f);
			}
			if (this.item.stack > 1)
			{
				spriteBatch.DrawString(Main.fontItemStack, this.item.stack.ToString(), new Vector2(base.DrawPosition.X + 10f * base.Scale, base.DrawPosition.Y + 26f * base.Scale), Color.White, 0f, Vector2.Zero, base.Scale, SpriteEffects.None, 0f);
			}
			base.Draw(spriteBatch);
		}
	}
}
