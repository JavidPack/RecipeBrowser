using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using System;
using Terraria.ID;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace RecipeBrowser.UIElements
{
	internal class UINPCSlot : UIElement
	{
		public static Texture2D selectedBackgroundTexture = Main.inventoryBack15Texture;
		public static Texture2D backgroundTexture = Main.inventoryBack9Texture;
		private float scale = .75f;
		public int npcType;
		public NPC npc;
		public bool selected;

		private int clickIndicatorTime = 0;
		private const int ClickTime = 30;

		//public UINPCSlot(int npcType)
		//{
		//	this.npcType = npcType;
		//	this.Width.Set(backgroundTexture.Width * scale, 0f);
		//	this.Height.Set(backgroundTexture.Height * scale, 0f);
		//}

		public UINPCSlot(NPC npc)
		{
			this.npc = npc;
			this.npcType = npc.type;
			this.Width.Set(backgroundTexture.Width * scale, 0f);
			this.Height.Set(backgroundTexture.Height * scale, 0f);
		}

		internal int frameCounter = 0;
		internal int frameTimer = 0;
		private const int frameDelay = 7;

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			Main.instance.LoadNPC(npcType);
			Texture2D npcTexture = Main.npcTexture[npcType];

			if (++frameTimer > frameDelay)
			{
				frameCounter = frameCounter + 1;
				frameTimer = 0;
				if (frameCounter > Main.npcFrameCount[npcType] - 1)
				{
					frameCounter = 0;
				}
			}

			Rectangle npcDrawRectangle = new Rectangle(0, (Main.npcTexture[npcType].Height / Main.npcFrameCount[npcType]) * frameCounter, Main.npcTexture[npcType].Width, Main.npcTexture[npcType].Height / Main.npcFrameCount[npcType]);

			CalculatedStyle dimensions = base.GetInnerDimensions();
			spriteBatch.Draw(backgroundTexture, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			DrawAdditionalOverlays(spriteBatch, dimensions.Position(), scale);
			Rectangle rectangle = dimensions.ToRectangle();

			int height = npcTexture.Height / Main.npcFrameCount[npcType];
			int width = npcTexture.Width;

			float drawScale = 2f;
			float availableWidth = (float)backgroundTexture.Width * scale - 6;
			if (width * drawScale > availableWidth || height * drawScale > availableWidth)
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
			Vector2 drawPosition = dimensions.Position();
			drawPosition.X += backgroundTexture.Width * scale / 2f - (float)width * drawScale / 2f;
			drawPosition.Y += backgroundTexture.Height * scale / 2f - (float)height * drawScale / 2f;

			Color color = (npc.color != new Color(byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue)) ? new Color(npc.color.R, npc.color.G, npc.color.B, 255f) : new Color(1f, 1f, 1f);

			Main.spriteBatch.Draw(npcTexture, drawPosition, npcDrawRectangle, color, 0, Vector2.Zero, drawScale, SpriteEffects.None, 0);

			if (IsMouseHovering)
			{
				Main.hoverItemName = Lang.GetNPCNameValue(npc.type) + (npc.modNPC != null ? " [" + npc.modNPC.mod.Name + "]" : "");
			}
		}

		public override int CompareTo(object obj)
		{
			UINPCSlot other = obj as UINPCSlot;
			return /*-1 * */npcType.CompareTo(other.npcType);
		}

		public SortedSet<int> GetDrops()
		{
			SortedSet<int> drops = new SortedSet<int>();
			foreach (var kvp in LootCache.instance.lootInfos)
			{
				foreach (var npc in kvp.Value)
				{
					if (npc.GetID() == this.npc.type)
					{
						drops.Add(kvp.Key.GetID());
					}
				}
			}
			drops.Remove(0);
			return drops;
		}

		public override void Click(UIMouseEvent evt)
		{
			clickIndicatorTime = ClickTime;
			// Calculate
			var drops = GetDrops();

			if (RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.RecipeCatalogue)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append($"{Lang.GetNPCNameValue(npc.type)} drops: ");
				foreach (var item in drops)
				{
					sb.Append($"[i:{item}]");
				}

				Main.NewText(sb.ToString());
			}
			else if (RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.Bestiary)
			{
				// Ug. double click calls click again after double click, leading to this being called on the wrong set.
				if (BestiaryUI.instance.npcSlots.Contains(this))
				{
					BestiaryUI.instance.queryLootNPC = this;
					BestiaryUI.instance.updateNeeded = true;
					BestiaryUI.instance.SetNPC(this);
				}
			}
		}

		public override void DoubleClick(UIMouseEvent evt)
		{
			// Open up bestiary tab
			// Large grid for npc
			//
			// Small drops grid

			// Catalogue double click? recipe ingredient?
			// quickly visiting catalogue not really needed...just share type so scroll to it/select it when switch tabs

			// Make
			if (RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.RecipeCatalogue)
			{
				RecipeBrowserUI.instance.tabController.SetPanel(RecipeBrowserUI.Bestiary);
				BestiaryUI.instance.npcNameFilter.SetText("");
				BestiaryUI.instance.queryItem.ReplaceWithFake(0);
				// Need update before Goto
				BestiaryUI.instance.updateNeeded = true;
				BestiaryUI.instance.Update();
				BestiaryUI.instance.npcGrid.Recalculate();
				BestiaryUI.instance.npcGrid.Goto((element) =>
				{
					UINPCSlot slot = element as UINPCSlot;
					if (slot != null)
					{
						if (slot.npcType == this.npcType)
						{
							BestiaryUI.instance.queryLootNPC = slot;
							BestiaryUI.instance.updateNeeded = true;
							BestiaryUI.instance.SetNPC(slot);
							return true;
						}
						return false;
					}
					return false;
				}, true);
			}
		}

		internal void DrawAdditionalOverlays(SpriteBatch spriteBatch, Vector2 vector2, float scale)
		{
			if (selected)
				spriteBatch.Draw(selectedBackgroundTexture, vector2, null, Color.White * Main.essScale, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

			if (clickIndicatorTime > 0)
			{
				clickIndicatorTime--;
				spriteBatch.Draw(selectedBackgroundTexture, vector2, null, Color.White * ((float)clickIndicatorTime / ClickTime), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}
		}
	}
}