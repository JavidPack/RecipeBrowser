using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.UI;

namespace RecipeBrowser
{
	class UIDragablePanel : UIPanel
	{
		private static Texture2D dragTexture;
		private Vector2 offset;
		private bool dragable;
		private bool dragging;
		private bool resizeableX;
		private bool resizeableY;
		private bool resizeable => resizeableX || resizeableY;
		private bool resizeing;
		//private int minX, minY, maxX, maxY;
		private List<UIElement> additionalDragTargets;

		// TODO, move panel back in if offscreen? prevent drag off screen?
		public UIDragablePanel(bool dragable = true, bool resizeableX = false, bool resizeableY = false)
		{
			this.dragable = dragable;
			this.resizeableX = resizeableX;
			this.resizeableY = resizeableY;
			if (dragTexture == null)
			{
				dragTexture = TextureManager.Load("Images/UI/PanelBorder");
			}
			additionalDragTargets = new List<UIElement>();
		}

		public void AddDragTarget(UIElement element)
		{
			additionalDragTargets.Add(element);
		}

		//public void SetMinMaxWidth(int min, int max)
		//{
		//	this.minX = min;
		//	this.maxX = max;
		//}

		//public void SetMinMaxHeight(int min, int max)
		//{
		//	this.minY = min;
		//	this.maxY = max;
		//}

		public override void MouseDown(UIMouseEvent evt)
		{
			DragStart(evt);
			base.MouseDown(evt);
		}

		public override void MouseUp(UIMouseEvent evt)
		{
			DragEnd(evt);
			base.MouseUp(evt);
		}

		private void DragStart(UIMouseEvent evt)
		{
			CalculatedStyle innerDimensions = GetInnerDimensions();
			if (evt.Target == this || additionalDragTargets.Contains(evt.Target))
			{
				if (resizeable && new Rectangle((int)(innerDimensions.X + innerDimensions.Width - 12), (int)(innerDimensions.Y + innerDimensions.Height - 12), 12 + 6, 12 + 6).Contains(evt.MousePosition.ToPoint()))
				{
					offset = new Vector2(evt.MousePosition.X - innerDimensions.X - innerDimensions.Width - 6, evt.MousePosition.Y - innerDimensions.Y - innerDimensions.Height - 6);
					resizeing = true;
				}
				else if (dragable)
				{
					offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
					dragging = true;
				}
			}
		}

		private void DragEnd(UIMouseEvent evt)
		{
			if (evt.Target == this || additionalDragTargets.Contains(evt.Target))
			{
				dragging = false;
				resizeing = false;
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = base.GetOuterDimensions();
			if (ContainsPoint(Main.MouseScreen))
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.LocalPlayer.showItemIcon = false;
				Main.ItemIconCacheUpdate(0);
			}
			if (dragging)
			{
				Left.Set(Main.MouseScreen.X - offset.X, 0f);
				Top.Set(Main.MouseScreen.Y - offset.Y, 0f);
				Recalculate();
			}
			if (resizeing)
			{
				if (resizeableX)
				{
					//Width.Pixels = Utils.Clamp(Main.MouseScreen.X - dimensions.X - offset.X, minX, maxX);
					Width.Pixels = Main.MouseScreen.X - dimensions.X - offset.X;
				}
				if (resizeableY)
				{
					//Height.Pixels = Utils.Clamp(Main.MouseScreen.Y - dimensions.Y - offset.Y, minY, maxY);
					Height.Pixels = Main.MouseScreen.Y - dimensions.Y - offset.Y;
				}
				Recalculate();
			}
			base.DrawSelf(spriteBatch);
			if (resizeable)
			{
				DrawDragAnchor(spriteBatch, dragTexture, this.BorderColor);
			}
		}

		private void DrawDragAnchor(SpriteBatch spriteBatch, Texture2D texture, Color color)
		{
			CalculatedStyle dimensions = GetDimensions();

			//Rectangle hitbox = new Rectangle((int)(innerDimensions.X + innerDimensions.Width - 12), (int)(innerDimensions.Y + innerDimensions.Height - 12), 12 + 6, 12 + 6);
			//Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.LightBlue * 0.6f);

			Point point = new Point((int)(dimensions.X + dimensions.Width - 12), (int)(dimensions.Y + dimensions.Height - 12));
			spriteBatch.Draw(texture, new Rectangle(point.X - 2, point.Y - 2, 12 - 2, 12 - 2), new Rectangle(12 + 4, 12 + 4, 12, 12), color);
			spriteBatch.Draw(texture, new Rectangle(point.X - 4, point.Y - 4, 12 - 4, 12 - 4), new Rectangle(12 + 4, 12 + 4, 12, 12), color);
			spriteBatch.Draw(texture, new Rectangle(point.X - 6, point.Y - 6, 12 - 6, 12 - 6), new Rectangle(12 + 4, 12 + 4, 12, 12), color);
		}
	}
}
