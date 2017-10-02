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
	class UINPCSlot : UIElement
	{
		public static Texture2D selectedBackgroundTexture = Main.inventoryBack15Texture;
		public static Texture2D backgroundTexture = Main.inventoryBack9Texture;
		private float scale = .75f;
		public int npcType;
		public NPC npc;

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
		const int frameDelay = 7;
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
			float availableWidth = (float)backgroundTexture.Width * scale;
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

		public override void Click(UIMouseEvent evt)
		{
			clickIndicatorTime = ClickTime;
			// Calculate
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
			StringBuilder sb = new StringBuilder();

			sb.Append($"{Lang.GetNPCNameValue(npc.type)} drops: ");
			foreach (var item in drops)
			{
				sb.Append($"[i:{item}]");
			}

			Main.NewText(sb.ToString());
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
		}

		internal void DrawAdditionalOverlays(SpriteBatch spriteBatch, Vector2 vector2, float scale)
		{
			if (clickIndicatorTime > 0)
			{
				clickIndicatorTime--;
				spriteBatch.Draw(selectedBackgroundTexture, vector2, null, Color.White * ((float)clickIndicatorTime / ClickTime), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}
		}
	}
}
