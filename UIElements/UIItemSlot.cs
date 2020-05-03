using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace RecipeBrowser.UIElements
{
	internal class UIItemSlot : UIElement
	{
		public static Texture2D defaultBackgroundTexture = Main.inventoryBack9Texture;
		public Texture2D backgroundTexture = defaultBackgroundTexture;
		internal float scale = .75f;
		public int itemType;
		public Item item;
		public bool hideSlot = false;
		internal static Item hoveredItem;

		public UIItemSlot(Item item, float scale = .75f)
		{
			this.scale = scale;
			this.item = item;
			this.itemType = item.type;
			this.Width.Set(defaultBackgroundTexture.Width * scale, 0f);
			this.Height.Set(defaultBackgroundTexture.Height * scale, 0f);
		}

		internal int frameCounter = 0;
		internal int frameTimer = 0;
		private const int frameDelay = 7;

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (item != null)
			{
				CalculatedStyle dimensions = base.GetInnerDimensions();
				Rectangle rectangle = dimensions.ToRectangle();
				if (!hideSlot)
				{
					spriteBatch.Draw(backgroundTexture, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
					DrawAdditionalOverlays(spriteBatch, dimensions.Position(), scale);
				}
				if (!item.IsAir)
				{
					Texture2D itemTexture = Main.itemTexture[this.item.type];
					Rectangle rectangle2;
					if (Main.itemAnimations[item.type] != null)
					{
						rectangle2 = Main.itemAnimations[item.type].GetFrame(itemTexture);
					}
					else
					{
						rectangle2 = itemTexture.Frame(1, 1, 0, 0);
					}
					Color newColor = Color.White;
					float pulseScale = 1f;
					ItemSlot.GetItemLight(ref newColor, ref pulseScale, item, false);
					int height = rectangle2.Height;
					int width = rectangle2.Width;
					float drawScale = 1f;
					float availableWidth = (float)defaultBackgroundTexture.Width * scale;
					if (width > availableWidth || height > availableWidth)
					{
						if (width > height)
						{
							drawScale = availableWidth / width;
						}
						else
						{
							drawScale = availableWidth / height;
						}
					}
					drawScale *= scale;
					Vector2 vector = backgroundTexture.Size() * scale;
					Vector2 position2 = dimensions.Position() + vector / 2f - rectangle2.Size() * drawScale / 2f;
					Vector2 origin = rectangle2.Size() * (pulseScale / 2f - 0.5f);
					//Vector2 drawPosition = dimensions.Position();
					//drawPosition.X += defaultBackgroundTexture.Width * scale / 2f - (float)width * drawScale / 2f;
					//drawPosition.Y += defaultBackgroundTexture.Height * scale / 2f - (float)height * drawScale / 2f;

					if (ItemLoader.PreDrawInInventory(item, spriteBatch, position2, rectangle2, item.GetAlpha(newColor),
						item.GetColor(Color.White), origin, drawScale * pulseScale))
					{
						spriteBatch.Draw(itemTexture, position2, new Rectangle?(rectangle2), item.GetAlpha(newColor), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
						if (item.color != Color.Transparent)
						{
							spriteBatch.Draw(itemTexture, position2, new Rectangle?(rectangle2), item.GetColor(Color.White), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
						}
					}
					ItemLoader.PostDrawInInventory(item, spriteBatch, position2, rectangle2, item.GetAlpha(newColor),
						item.GetColor(Color.White), origin, drawScale * pulseScale);
					if (ItemID.Sets.TrapSigned[item.type])
					{
						spriteBatch.Draw(Main.wireTexture, dimensions.Position() + new Vector2(40f, 40f) * scale, new Rectangle?(new Rectangle(4, 58, 8, 8)), Color.White, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);
					}
					DrawAdditionalBadges(spriteBatch, dimensions.Position(), scale);
					if (item.stack > 1)
					{
						ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, item.stack.ToString(), dimensions.Position() + new Vector2(10f, 26f) * scale, Color.White, 0f, Vector2.Zero, new Vector2(scale), -1f, scale);
					}

					//this.item.GetColor(Color.White);
					//spriteBatch.Draw(itemTexture, drawPosition, rectangle2, this.item.GetAlpha(Color.White), 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);
					//if (this.item.color != default(Color))
					//{
					//	spriteBatch.Draw(itemTexture, drawPosition, new Rectangle?(rectangle2), this.item.GetColor(Color.White), 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);
					//}
					//if (this.item.stack > 1)
					//{
					//	spriteBatch.DrawString(Main.fontItemStack, this.item.stack.ToString(), new Vector2(drawPosition.X + 10f * scale, drawPosition.Y + 26f * scale), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
					//}

					if (IsMouseHovering)
					{
						// TODO, should only need 2 of these 3 I think
						Main.HoverItem = item.Clone();
						Main.hoverItemName = Main.HoverItem.Name + (Main.HoverItem.modItem != null && ModContent.GetInstance<RecipeBrowserClientConfig>().ShowItemModSource ? " [" + Main.HoverItem.modItem.mod.Name + "]" : "");

						//	Main.hoverItemName = this.item.name;
						//	Main.toolTip = item.Clone();
						Main.HoverItem.SetNameOverride(Main.HoverItem.Name + (Main.HoverItem.modItem != null && ModContent.GetInstance<RecipeBrowserClientConfig>().ShowItemModSource ? " [" + Main.HoverItem.modItem.mod.Name + "]" : ""));

						hoveredItem = Main.HoverItem;
					}
				}
			}
		}

		internal virtual void DrawAdditionalOverlays(SpriteBatch spriteBatch, Vector2 vector2, float scale)
		{
		}

		internal virtual void DrawAdditionalBadges(SpriteBatch spriteBatch, Vector2 vector2, float scale)
		{
		}
	}

	internal class UIItemCatalogueItemSlot : UIItemSlot
	{
		internal bool selected;

		public UIItemCatalogueItemSlot(Item item, float scale = 0.75F) : base(item, scale)
		{
		}

		public override void Click(UIMouseEvent evt)
		{
			ItemCatalogueUI.instance.SetItem(this);
			CraftUI.instance.SetItem(this.item.type);
		}

		public override void DoubleClick(UIMouseEvent evt)
		{
			RecipeCatalogueUI.instance.itemDescriptionFilter.SetText("");
			RecipeCatalogueUI.instance.itemNameFilter.SetText("");
			RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(item.type);
			RecipeBrowserUI.instance.tabController.SetPanel(RecipeBrowserUI.RecipeCatalogue);
		}

		public override void RightDoubleClick(UIMouseEvent evt)
		{
			BestiaryUI.instance.npcNameFilter.SetText("");
			BestiaryUI.instance.queryItem.ReplaceWithFake(item.type);
			RecipeBrowserUI.instance.tabController.SetPanel(RecipeBrowserUI.Bestiary);
		}

		internal override void DrawAdditionalOverlays(SpriteBatch spriteBatch, Vector2 vector2, float scale)
		{
			base.DrawAdditionalOverlays(spriteBatch, vector2, scale);
			if (selected)
				spriteBatch.Draw(UIElements.UIRecipeSlot.selectedBackgroundTexture, vector2, null, Color.White * Main.essScale, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		}

		internal override void DrawAdditionalBadges(SpriteBatch spriteBatch, Vector2 vector2, float scale)
		{
			base.DrawAdditionalBadges(spriteBatch, vector2, scale);
			if (ItemCatalogueUI.instance.isLoot[item.type])
				spriteBatch.Draw(Main.wire2Texture, vector2 + new Vector2(40f, 10f) * scale, new Rectangle(4, 58, 8, 8), Color.White, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);
			if (ItemCatalogueUI.instance.craftResults[item.type])
				spriteBatch.Draw(Main.wire3Texture, vector2 + new Vector2(10f, 10f) * scale, new Rectangle(4, 58, 8, 8), Color.White, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);
			if (RecipeBrowserUI.instance.foundItems != null && !RecipeBrowserUI.instance.foundItems[item.type])
				spriteBatch.Draw(Main.wire4Texture, vector2 + new Vector2(10f, 40f) * scale, new Rectangle(4, 58, 8, 8), Color.White, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);
		}
	}

	internal class UIBestiaryItemSlot : UIItemSlot
	{
		public UIBestiaryItemSlot(Item item, float scale = 0.75F) : base(item, scale)
		{
		}

		public override void DoubleClick(UIMouseEvent evt)
		{
			RecipeCatalogueUI.instance.itemDescriptionFilter.SetText("");
			RecipeCatalogueUI.instance.itemNameFilter.SetText("");
			RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(item.type);
			RecipeBrowserUI.instance.tabController.SetPanel(RecipeBrowserUI.RecipeCatalogue);
		}

		public override void RightClick(UIMouseEvent evt)
		{
			BestiaryUI.instance.queryItem.ReplaceWithFake(item.type);
		}
	}

	internal class UIItemNoSlot : UIElement
	{
		internal float scale = .75f;
		public int itemType;
		public Item item;
		public UIItemNoSlot(Item item, float scale = .75f)
		{
			this.scale = scale;
			this.item = item;
			this.itemType = item.type;
			this.Width.Set(32f * scale * 0.65f, 0f);
			this.Height.Set(32f * scale * 0.65f, 0f);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			Vector2 position = GetInnerDimensions().Position();
			float num = 1f;
			float num2 = 1f;
			if (Main.netMode != NetmodeID.Server && !Main.dedServ)
			{
				Texture2D texture2D = Main.itemTexture[item.type];
				Rectangle rectangle;
				if (Main.itemAnimations[item.type] != null)
				{
					rectangle = Main.itemAnimations[item.type].GetFrame(texture2D);
				}
				else
				{
					rectangle = texture2D.Frame(1, 1, 0, 0);
				}
				if (rectangle.Height > 32)
				{
					num2 = 32f / (float)rectangle.Height;
				}
			}
			num2 *= scale;
			num *= num2;
			if (num > 0.75f)
			{
				num = 0.75f;
			}
			{
				float inventoryScale = Main.inventoryScale;
				Main.inventoryScale = scale * num;
				ItemSlot.Draw(spriteBatch, ref item, 14, position - new Vector2(10f) * scale * num, Color.White);
				Main.inventoryScale = inventoryScale;
			}

			if (IsMouseHovering)
			{
				//Main.HoverItem = item.Clone();
				//Main.instance.MouseText(item.Name, item.rare, 0, -1, -1, -1, -1);

				Main.hoverItemName = item.Name;
			}
		}
	}

	//internal class UIHoverText : UIText
	//{
	//	string hover;
	//	public UIHoverText(string hover, string text, float textScale = 1f, bool large = false) : base(text, textScale, large)
	//	{
	//		this.hover = hover;
	//	}

	//	protected override void DrawSelf(SpriteBatch spriteBatch)
	//	{
	//		base.DrawSelf(spriteBatch);

	//		if (IsMouseHovering)
	//		{
	//			Main.hoverItemName = hover;
	//		}
	//	}
	//}
}